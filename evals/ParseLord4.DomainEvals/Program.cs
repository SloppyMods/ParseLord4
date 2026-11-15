using System.Text.Json;
using System.Text.Json.Serialization;
using ParseLord4.Rotation.Jobs.Melee;
using ParseLord4.Rotation.Jobs.Ranged;

var repoRoot = "";
var fixtureRoot = "";

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--repo-root" when i + 1 < args.Length:
            repoRoot = args[++i];
            break;
        case "--fixture-root" when i + 1 < args.Length:
            fixtureRoot = args[++i];
            break;
    }
}

if (string.IsNullOrWhiteSpace(repoRoot))
{
    Console.Error.WriteLine("Missing --repo-root");
    return 2;
}

if (string.IsNullOrWhiteSpace(fixtureRoot))
    fixtureRoot = Path.Combine(repoRoot, "evals", "rotation");

if (!Directory.Exists(fixtureRoot))
{
    Console.Error.WriteLine($"Missing fixture root: {fixtureRoot}");
    return 2;
}

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
};

var fixturePaths = Directory.GetFiles(fixtureRoot, "*.json", SearchOption.AllDirectories)
    .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
    .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (fixturePaths.Length == 0)
{
    Console.Error.WriteLine($"No rotation fixtures found under {fixtureRoot}");
    return 2;
}

var fixtures = new List<RotationFixture>();
var incomplete = 0;

foreach (var fixturePath in fixturePaths)
{
    try
    {
        var fixture = JsonSerializer.Deserialize<RotationFixture>(File.ReadAllText(fixturePath), options);
        if (fixture is null)
        {
            incomplete++;
            Console.WriteLine($"INCOMPLETE {Path.GetFileName(fixturePath)}: empty fixture");
            continue;
        }

        fixture.Path = fixturePath;
        fixtures.Add(fixture);
    }
    catch (Exception ex)
    {
        incomplete++;
        Console.WriteLine($"INCOMPLETE {Path.GetFileName(fixturePath)}: invalid json: {ex.Message}");
    }
}

var passed = 0;
var failed = 0;
var skipped = 0;
var negativePassed = 0;
var perJob = new Dictionary<string, JobSummary>(StringComparer.OrdinalIgnoreCase);

Console.WriteLine("ParseLord4 rotation domain evals");
Console.WriteLine("Scope: production-backed rotation decision adapters");
Console.WriteLine($"Fixture root: {fixtureRoot}");

foreach (var group in fixtures.GroupBy(fixture => fixture.Job.ToUpperInvariant()).OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
{
    var job = string.IsNullOrWhiteSpace(group.Key) ? "UNKNOWN" : group.Key;
    var jobFixtures = group.OrderBy(fixture => fixture.Id, StringComparer.OrdinalIgnoreCase).ToArray();
    var summary = new JobSummary(job, AdapterFor(job), jobFixtures.Length);
    perJob[job] = summary;

    if (!AdapterImplemented(job))
    {
        skipped += jobFixtures.Length;
        summary.Skipped = jobFixtures.Length;
        summary.Result = "SKIPPED_WITH_REASON";
        Console.WriteLine($"{job}: adapter=not-implemented result=SKIPPED_WITH_REASON fixtures={jobFixtures.Length} reason=production adapter not implemented yet");
        continue;
    }

    var sawNegativeControl = false;
    foreach (var fixture in jobFixtures)
    {
        var result = job switch
        {
            "SAM" => EvaluateSamurai(fixture, options),
            "VPR" => EvaluateViper(fixture, options),
            "RPR" => EvaluateReaper(fixture, options),
            "MCH" => EvaluateMachinist(fixture, options),
            "NIN" => EvaluateNinja(fixture, options),
            _ => EvalResult.Incomplete(fixture.Id, "adapter dispatch missing"),
        };

        switch (result.Status)
        {
            case EvalStatus.Pass:
                passed++;
                summary.Passed++;
                Console.WriteLine($"PASS {fixture.Id}: expected {fixture.ExpectedAction}, got {result.ActualAction}");
                break;
            case EvalStatus.PassNegative:
                sawNegativeControl = true;
                negativePassed++;
                summary.NegativeControls++;
                Console.WriteLine($"PASS_NEGATIVE {fixture.Id}: actual {result.ActualAction} rejected intentional wrong expectation {fixture.ExpectedAction}");
                break;
            case EvalStatus.Fail:
                failed++;
                summary.Failed++;
                Console.WriteLine($"FAIL {fixture.Id}: {result.Message}");
                break;
            case EvalStatus.FailNegative:
                sawNegativeControl = true;
                failed++;
                summary.Failed++;
                Console.WriteLine($"FAIL_NEGATIVE {fixture.Id}: {result.Message}");
                break;
            case EvalStatus.Incomplete:
                incomplete++;
                summary.Incomplete++;
                Console.WriteLine($"INCOMPLETE {fixture.Id}: {result.Message}");
                break;
        }
    }

    if (!sawNegativeControl)
    {
        incomplete++;
        summary.Incomplete++;
        Console.WriteLine($"INCOMPLETE {job}: missing negative-control fixture");
    }

    summary.Result = summary.Failed > 0 ? "FAIL" : summary.Incomplete > 0 ? "INCOMPLETE" : "PASS";
}

Console.WriteLine("Per-job summary:");
foreach (var summary in perJob.Values.OrderBy(summary => summary.Job, StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(
        $"{summary.Job}: adapter={summary.Adapter} fixtures={summary.Fixtures} result={summary.Result} passed={summary.Passed} negative_controls={summary.NegativeControls} failed={summary.Failed} incomplete={summary.Incomplete} skipped_with_reason={summary.Skipped}");
}

Console.WriteLine($"Summary: passed={passed} negative_controls={negativePassed} failed={failed} incomplete={incomplete} skipped_with_reason={skipped} total={fixtures.Count}");

if (failed > 0)
    return 1;

if (incomplete > 0)
    return 2;

return 0;

static bool AdapterImplemented(string job) =>
    job.Equals("SAM", StringComparison.OrdinalIgnoreCase) ||
    job.Equals("VPR", StringComparison.OrdinalIgnoreCase) ||
    job.Equals("RPR", StringComparison.OrdinalIgnoreCase) ||
    job.Equals("MCH", StringComparison.OrdinalIgnoreCase) ||
    job.Equals("NIN", StringComparison.OrdinalIgnoreCase);

static string AdapterFor(string job) =>
    job.ToUpperInvariant() switch
    {
            "SAM" => "samurai-production",
            "VPR" => "viper-production",
            "RPR" => "reaper-production",
            "MCH" => "machinist-production",
            "NIN" => "ninja-production",
            _ => "not-implemented",
    };

static EvalResult EvaluateSamurai(RotationFixture fixture, JsonSerializerOptions options)
{
    var validation = ValidateFixture(fixture, "SAM", "samurai-production");
    if (!string.IsNullOrEmpty(validation))
        return EvalResult.Incomplete(fixture.Id, validation);

    SamuraiFixtureInput input;
    try
    {
        input = fixture.Input.Deserialize<SamuraiFixtureInput>(options) ?? new SamuraiFixtureInput();
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"invalid Samurai input: {ex.Message}");
    }

    SamuraiDecision? decision;
    try
    {
        decision = SamuraiDecisionModel.Decide(BuildSamuraiState(input), input.IncludeAbilities);
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"adapter error: {ex.Message}");
    }

    if (decision is null)
        return EvalResult.Incomplete(fixture.Id, "production model returned no action");

    return CompareDecision(
        fixture,
        decision.Value.ActionId,
        SamuraiDecisionModel.DescribeAction,
        SamuraiActionIdFor);
}

static EvalResult EvaluateViper(RotationFixture fixture, JsonSerializerOptions options)
{
    var validation = ValidateFixture(fixture, "VPR", "viper-production");
    if (!string.IsNullOrEmpty(validation))
        return EvalResult.Incomplete(fixture.Id, validation);

    ViperFixtureInput input;
    try
    {
        input = fixture.Input.Deserialize<ViperFixtureInput>(options) ?? new ViperFixtureInput();
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"invalid Viper input: {ex.Message}");
    }

    ViperDecision? decision;
    try
    {
        var blocked = new HashSet<uint>(
            input.BlockedActions.Select(ViperActionIdFor).Where(id => id != 0),
            EqualityComparer<uint>.Default);
        decision = ViperDecisionModel.Decide(
            BuildViperState(input),
            input.IncludeAbilities,
            actionId => actionId != 0 && !blocked.Contains(actionId));
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"adapter error: {ex.Message}");
    }

    if (decision is null)
        return EvalResult.Incomplete(fixture.Id, "production model returned no action");

    return CompareDecision(
        fixture,
        decision.Value.ActionId,
        ViperDecisionModel.DescribeAction,
        ViperActionIdFor);
}

static EvalResult EvaluateReaper(RotationFixture fixture, JsonSerializerOptions options)
{
    var validation = ValidateFixture(fixture, "RPR", "reaper-production");
    if (!string.IsNullOrEmpty(validation))
        return EvalResult.Incomplete(fixture.Id, validation);

    ReaperFixtureInput input;
    try
    {
        input = fixture.Input.Deserialize<ReaperFixtureInput>(options) ?? new ReaperFixtureInput();
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"invalid Reaper input: {ex.Message}");
    }

    ReaperDecision? decision;
    try
    {
        var blocked = new HashSet<uint>(
            input.BlockedActions.Select(ReaperActionIdFor).Where(id => id != 0),
            EqualityComparer<uint>.Default);
        decision = ReaperDecisionModel.Decide(
            BuildReaperState(input),
            input.IncludeAbilities,
            actionId => actionId != 0 && !blocked.Contains(actionId));
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"adapter error: {ex.Message}");
    }

    if (decision is null)
        return EvalResult.Incomplete(fixture.Id, "production model returned no action");

    return CompareDecision(
        fixture,
        decision.Value.ActionId,
        ReaperDecisionModel.DescribeAction,
        ReaperActionIdFor);
}

static string? ValidateFixture(RotationFixture fixture, string job, string adapter)
{
    if (string.IsNullOrWhiteSpace(fixture.Id))
        return "missing id";
    if (!string.Equals(fixture.Job, job, StringComparison.OrdinalIgnoreCase))
        return $"job must be {job}";
    if (!string.Equals(fixture.Adapter, adapter, StringComparison.OrdinalIgnoreCase))
        return $"adapter must be {adapter}";
    if (fixture.Input.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        return "missing input";
    if (string.IsNullOrWhiteSpace(fixture.ExpectedAction))
        return "missing expected_action";
    return null;
}

static EvalResult CompareDecision(
    RotationFixture fixture,
    uint actualActionId,
    Func<uint, string> describeAction,
    Func<string, uint> actionIdFor)
{
    var actualActionName = describeAction(actualActionId);
    var expectedActionId = actionIdFor(fixture.ExpectedAction);
    var shouldNotActions = fixture.ShouldNotActions
        .Select(actionIdFor)
        .Where(id => id != 0)
        .ToHashSet();

    if (fixture.NegativeControl)
    {
        if (actualActionId == expectedActionId)
            return EvalResult.FailNegative(actualActionName, $"actual {actualActionName} matched intentional wrong expectation {fixture.ExpectedAction}");

        return EvalResult.PassNegative(actualActionName);
    }

    if (actualActionId != expectedActionId)
        return EvalResult.Fail(actualActionName, $"expected {fixture.ExpectedAction}, got {actualActionName}");

    if (shouldNotActions.Contains(actualActionId))
        return EvalResult.Fail(actualActionName, $"actual {actualActionName} is forbidden by should_not_actions");

    return EvalResult.Pass(actualActionName);
}

static SamuraiDecisionState BuildSamuraiState(SamuraiFixtureInput input)
{
    var useAoE = input.TargetCount >= 3;
    var starterAction = useAoE
        ? (input.Level >= 86 ? SamuraiDecisionModel.Fuko : SamuraiDecisionModel.Fuga)
        : (input.Level >= 100 ? SamuraiDecisionModel.Gyofu : SamuraiDecisionModel.Hakaze);

    var readyTsubameAction = input.ReadyTsubameAction is { Length: > 0 }
        ? SamuraiActionIdFor(input.ReadyTsubameAction)
        : input.HasTendoKaeshiReady
            ? SamuraiDecisionModel.TendoKaeshiSetsugekka
            : 0U;

    uint readyOgiAction = 0;
    if (!string.IsNullOrWhiteSpace(input.ReadyOgiAction))
        readyOgiAction = SamuraiActionIdFor(input.ReadyOgiAction);
    else if (input.HasOgiReady)
        readyOgiAction = SamuraiDecisionModel.OgiNamikiri;

    uint readyIaijutsuAction = 0;
    if (!string.IsNullOrWhiteSpace(input.ReadyIaijutsuAction))
    {
        readyIaijutsuAction = SamuraiActionIdFor(input.ReadyIaijutsuAction);
    }
    else if (useAoE && input.Sen >= 2)
    {
        readyIaijutsuAction = input.HasTendo ? SamuraiDecisionModel.TendoGoken : SamuraiDecisionModel.TenkaGoken;
    }
    else if (input.Sen >= 3)
    {
        readyIaijutsuAction = input.HasTendo ? SamuraiDecisionModel.TendoSetsugekka : SamuraiDecisionModel.MidareSetsugekka;
    }

    var comboAction = SamuraiComboActionFor(input.ComboStep, input.Level, useAoE);
    var hasGetsu = input.HasGetsu ?? input.Sen >= 1;
    var hasSetsu = input.HasSetsu ?? input.Sen >= 2;
    var hasKa = input.HasKa ?? input.Sen >= 3;
    var blocked = new HashSet<string>(input.BlockedActions ?? [], StringComparer.OrdinalIgnoreCase);
    bool CanUse(string actionName) => !blocked.Contains(actionName);

    return new SamuraiDecisionState(
        UseAoE: useAoE,
        ComboAction: comboAction,
        HasGetsu: hasGetsu,
        HasSetsu: hasSetsu,
        HasKa: hasKa,
        SenCount: input.Sen,
        Kenki: input.Kenki,
        MeditationStacks: input.MeditationStacks,
        HasMeikyo: input.HasMeikyo,
        HasFugetsu: input.HasFugetsu,
        HasFuka: input.HasFuka,
        HasZanshinReady: input.HasZanshinReady,
        HasOgiReady: input.HasOgiReady || readyOgiAction != 0,
        FugetsuRemaining: input.FugetsuRemaining,
        FukaRemaining: input.FukaRemaining,
        HiganbanaRemaining: input.HiganbanaRemaining,
        ReadyTsubameAction: readyTsubameAction,
        ReadyOgiAction: readyOgiAction,
        ReadyIaijutsuAction: readyIaijutsuAction,
        AdjustedStarterAction: starterAction,
        CanUseStarter: CanUse(SamuraiDecisionModel.DescribeAction(starterAction)),
        CanUseEnpi: CanUse("Enpi"),
        CanUseMeikyo: CanUse("Meikyo Shisui"),
        CanUseIkishoten: CanUse("Ikishoten"),
        CanUseZanshin: CanUse("Zanshin"),
        CanUseShoha: CanUse("Shoha"),
        CanUseSenei: CanUse("Senei"),
        CanUseGuren: CanUse("Guren"),
        CanUseShinten: CanUse("Shinten"),
        CanUseKyuten: CanUse("Kyuten"));
}

static ViperDecisionState BuildViperState(ViperFixtureInput input)
{
    uint Adjusted(string actionName)
    {
        var baseId = ViperActionIdFor(actionName);
        if (input.AdjustedActions.TryGetValue(actionName, out var adjustedName) ||
            input.AdjustedActions.TryGetValue(actionName.Replace("'", ""), out adjustedName))
        {
            var adjustedId = ViperActionIdFor(adjustedName);
            return adjustedId == 0 ? baseId : adjustedId;
        }

        return baseId;
    }

    var hasTwinfangWeave = input.HasTwinfangWeave || input.PoisedForTwinfang || input.HuntersVenom || input.FellhuntersVenom;
    var hasTwinbloodWeave = input.HasTwinbloodWeave || input.PoisedForTwinblood || input.SwiftskinsVenom || input.FellskinsVenom;
    var hasHindVenom = input.HindstungVenom || input.HindsbaneVenom;
    var hasFlankVenom = input.FlankstungVenom || input.FlanksbaneVenom;

    return new ViperDecisionState(
        UseAoE: input.TargetCount >= 3,
        CanWeave: input.CanWeave,
        ComboAction: ViperComboActionFor(input.ComboStep),
        DreadCombo: ViperDreadComboFor(input.DreadCombo),
        SerpentOffering: input.SerpentOffering ?? input.Generation,
        RattlingCoilStacks: input.RattlingCoilStacks ?? input.Coils,
        HasReawakened: input.Reawakened,
        HasReadyToReawaken: input.ReadyToReawaken || input.ReawakenReady,
        HasBothBuffs: input.HasBothBuffs || input.HuntersInstinct && input.Swiftscaled,
        HasTwinWeave: hasTwinfangWeave || hasTwinbloodWeave,
        HasSerpentTailWeave: input.SerpentsTailReady || IsSerpentTailCombo(input.SerpentCombo),
        HasTwinfangWeave: hasTwinfangWeave,
        HasTwinbloodWeave: hasTwinbloodWeave,
        HasHindVenom: hasHindVenom,
        HasFlankVenom: hasFlankVenom,
        HasBasicComboVenom: hasHindVenom || hasFlankVenom,
        HasFlanksbaneVenom: input.FlanksbaneVenom,
        HasHindsbaneVenom: input.HindsbaneVenom,
        HasFlankstungVenom: input.FlankstungVenom,
        HasHindstungVenom: input.HindstungVenom,
        HasGrimhuntersVenom: input.GrimhuntersVenom,
        HasGrimskinsVenom: input.GrimskinsVenom,
        HasSwiftscaled: input.Swiftscaled,
        HasHuntersInstinct: input.HuntersInstinct,
        HasHonedReavers: input.HonedReavers,
        HasHonedSteel: input.HonedSteel,
        AdjustedSteelFangs: Adjusted("Steel Fangs"),
        AdjustedReavingFangs: Adjusted("Reaving Fangs"),
        AdjustedSteelMaw: Adjusted("Steel Maw"),
        AdjustedReavingMaw: Adjusted("Reaving Maw"),
        AdjustedReawaken: Adjusted("Reawaken"),
        AdjustedVicewinder: Adjusted("Vicewinder"),
        AdjustedVicepit: Adjusted("Vicepit"),
        AdjustedUncoiledFury: Adjusted("Uncoiled Fury"),
        AdjustedWrithingSnap: Adjusted("Writhing Snap"),
        AdjustedSerpentsTail: Adjusted("Serpent's Tail"),
        AdjustedTwinfang: Adjusted("Twinfang"),
        AdjustedTwinblood: Adjusted("Twinblood"),
        CanUseSerpentsIre: !input.BlockedActions.Contains("Serpent's Ire", StringComparer.OrdinalIgnoreCase));
}

static ReaperDecisionState BuildReaperState(ReaperFixtureInput input)
{
    var blockedNames = new HashSet<string>(input.BlockedActions, StringComparer.OrdinalIgnoreCase);

    uint Adjusted(string actionName)
    {
        var baseId = ReaperActionIdFor(actionName);
        foreach (var entry in input.AdjustedActions)
        {
            if (string.Equals(entry.Key, actionName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entry.Key.Replace("'", ""), actionName.Replace("'", ""), StringComparison.OrdinalIgnoreCase))
            {
                var adjustedId = ReaperActionIdFor(entry.Value);
                return adjustedId == 0 ? baseId : adjustedId;
            }
        }

        return baseId;
    }

    bool HasBlocked(string actionName) => blockedNames.Contains(actionName);
    var hasEnshrouded = input.HasEnshrouded || input.Enshrouded;
    var hasExecutioner = input.HasExecutioner || input.ExecutionerReady;
    var hasSoulReaver = input.HasSoulReaver || input.SoulReaverReady;

    return new ReaperDecisionState(
        UseAoE: input.TargetCount >= 3,
        CanWeave: input.CanWeave,
        ComboAction: ReaperComboActionFor(input.ComboStep),
        Soul: input.Soul,
        Shroud: input.Shroud,
        LemureShroud: input.LemureShroud,
        VoidShroud: input.VoidShroud,
        DeathsDesignRemaining: input.DeathsDesignRemaining,
        HasDeathsDesign: input.HasDeathsDesign,
        HasSoulReaver: hasSoulReaver,
        HasExecutioner: hasExecutioner,
        HasEnshrouded: hasEnshrouded,
        HasImmortalSacrifice: input.HasImmortalSacrifice,
        HasPerfectioParata: input.HasPerfectioParata,
        HasIdealHost: input.HasIdealHost,
        HasOblatio: input.HasOblatio,
        HasEnhancedGibbet: input.HasEnhancedGibbet,
        HasEnhancedGallows: input.HasEnhancedGallows,
        HasEnhancedVoidReaping: input.HasEnhancedVoidReaping,
        HasEnhancedCrossReaping: input.HasEnhancedCrossReaping,
        HasSoulsow: input.HasSoulsow,
        InCombat: input.InCombat,
        PreferGibbetNext: input.PreferGibbetNext,
        AdjustedCommunio: Adjusted("Communio"),
        AdjustedPlentifulHarvest: Adjusted("Plentiful Harvest"),
        AdjustedShadowOfDeath: Adjusted("Shadow of Death"),
        AdjustedWhorlOfDeath: Adjusted("Whorl of Death"),
        AdjustedSacrificium: Adjusted("Sacrificium"),
        AdjustedLemuresSlice: Adjusted("Lemures Slice"),
        AdjustedLemuresScythe: Adjusted("Lemures Scythe"),
        AdjustedBloodStalk: Adjusted("Blood Stalk"),
        AdjustedGrimSwathe: Adjusted("Grim Swathe"),
        AdjustedEnshroud: Adjusted("Enshroud"),
        AdjustedGluttony: Adjusted("Gluttony"),
        AdjustedSoulSlice: Adjusted("Soul Slice"),
        AdjustedSoulScythe: Adjusted("Soul Scythe"),
        AdjustedHarvestMoon: Adjusted("Harvest Moon"),
        AdjustedSoulsow: Adjusted("Soulsow"),
        AdjustedGibbet: Adjusted("Gibbet"),
        AdjustedGallows: Adjusted("Gallows"),
        AdjustedGuillotine: Adjusted("Guillotine"),
        AdjustedSlice: Adjusted("Slice"),
        AdjustedSpinningScythe: Adjusted("Spinning Scythe"),
        AdjustedNightmareScythe: Adjusted("Nightmare Scythe"),
        AdjustedWaxingSlice: Adjusted("Waxing Slice"),
        AdjustedInfernalSlice: Adjusted("Infernal Slice"),
        AdjustedHarpe: Adjusted("Harpe"),
        CanUseArcaneCircle: !HasBlocked("Arcane Circle"),
        CanUseEnshroud: !HasBlocked("Enshroud"),
        CanUseSoulsow: !HasBlocked("Soulsow"));
}

static uint SamuraiComboActionFor(string comboStep, int level, bool useAoE) =>
    comboStep.ToLowerInvariant() switch
    {
        "" or "none" => 0,
        "hakaze-ready" => level >= 100 ? SamuraiDecisionModel.Gyofu : SamuraiDecisionModel.Hakaze,
        "gyofu-ready" => SamuraiDecisionModel.Gyofu,
        "jinpu-ready" => SamuraiDecisionModel.Jinpu,
        "shifu-ready" => SamuraiDecisionModel.Shifu,
        "fuga-ready" => level >= 86 ? SamuraiDecisionModel.Fuko : SamuraiDecisionModel.Fuga,
        "fuko-ready" => SamuraiDecisionModel.Fuko,
        _ => throw new InvalidOperationException($"Unsupported Samurai combo_step '{comboStep}'"),
    };

static uint ViperComboActionFor(string comboStep) =>
    comboStep.ToLowerInvariant() switch
    {
        "" or "none" => 0,
        "steel-fangs" => ViperDecisionModel.SteelFangs,
        "reaving-fangs" => ViperDecisionModel.ReavingFangs,
        "hunters-sting" => ViperDecisionModel.HuntersSting,
        "swiftskins-sting" => ViperDecisionModel.SwiftskinsSting,
        "flanksting-strike" => ViperDecisionModel.FlankstingStrike,
        "flanksbane-fang" => ViperDecisionModel.FlanksbaneFang,
        "hindsting-strike" => ViperDecisionModel.HindstingStrike,
        "hindsbane-fang" => ViperDecisionModel.HindsbaneFang,
        "steel-maw" => ViperDecisionModel.SteelMaw,
        "reaving-maw" => ViperDecisionModel.ReavingMaw,
        "hunters-bite" => ViperDecisionModel.HuntersBite,
        "swiftskins-bite" => ViperDecisionModel.SwiftskinsBite,
        "jagged-maw" => ViperDecisionModel.JaggedMaw,
        "bloodied-maw" => ViperDecisionModel.BloodiedMaw,
        _ => throw new InvalidOperationException($"Unsupported Viper combo_step '{comboStep}'"),
    };

static uint ReaperComboActionFor(string comboStep) =>
    comboStep.ToLowerInvariant() switch
    {
        "" or "none" => 0,
        "slice" => ReaperDecisionModel.Slice,
        "waxing-slice" => ReaperDecisionModel.WaxingSlice,
        "infernal-slice" => ReaperDecisionModel.InfernalSlice,
        "spinning-scythe" => ReaperDecisionModel.SpinningScythe,
        _ => throw new InvalidOperationException($"Unsupported Reaper combo_step '{comboStep}'"),
    };

static ViperDreadComboStep ViperDreadComboFor(string value) =>
    value.ToLowerInvariant() switch
    {
        "" or "none" => ViperDreadComboStep.None,
        "dreadwinder" => ViperDreadComboStep.Dreadwinder,
        "hunters-coil" => ViperDreadComboStep.HuntersCoil,
        "swiftskins-coil" => ViperDreadComboStep.SwiftskinsCoil,
        "pit-of-dread" => ViperDreadComboStep.PitOfDread,
        "hunters-den" => ViperDreadComboStep.HuntersDen,
        "swiftskins-den" => ViperDreadComboStep.SwiftskinsDen,
        "first-generation" => ViperDreadComboStep.FirstGeneration,
        "second-generation" => ViperDreadComboStep.SecondGeneration,
        "third-generation" => ViperDreadComboStep.ThirdGeneration,
        "fourth-generation" => ViperDreadComboStep.FourthGeneration,
        _ => throw new InvalidOperationException($"Unsupported Viper dread_combo '{value}'"),
    };

static bool IsSerpentTailCombo(string value) =>
    value.ToLowerInvariant() is "death-rattle" or "last-lash" or "first-legacy" or "second-legacy" or "third-legacy" or "fourth-legacy";

static uint SamuraiActionIdFor(string actionName) =>
    actionName.Trim() switch
    {
        "Hakaze" => SamuraiDecisionModel.Hakaze,
        "Jinpu" => SamuraiDecisionModel.Jinpu,
        "Shifu" => SamuraiDecisionModel.Shifu,
        "Yukikaze" => SamuraiDecisionModel.Yukikaze,
        "Gekko" => SamuraiDecisionModel.Gekko,
        "Kasha" => SamuraiDecisionModel.Kasha,
        "Enpi" => SamuraiDecisionModel.Enpi,
        "Gyofu" => SamuraiDecisionModel.Gyofu,
        "Fuga" => SamuraiDecisionModel.Fuga,
        "Fuko" => SamuraiDecisionModel.Fuko,
        "Mangetsu" => SamuraiDecisionModel.Mangetsu,
        "Oka" => SamuraiDecisionModel.Oka,
        "Higanbana" => SamuraiDecisionModel.Higanbana,
        "Midare Setsugekka" => SamuraiDecisionModel.MidareSetsugekka,
        "Tenka Goken" => SamuraiDecisionModel.TenkaGoken,
        "Tendo Goken" => SamuraiDecisionModel.TendoGoken,
        "Tendo Setsugekka" => SamuraiDecisionModel.TendoSetsugekka,
        "Kaeshi Higanbana" => SamuraiDecisionModel.KaeshiHiganbana,
        "Kaeshi Goken" => SamuraiDecisionModel.KaeshiGoken,
        "Kaeshi Setsugekka" => SamuraiDecisionModel.KaeshiSetsugekka,
        "Tendo Kaeshi Goken" => SamuraiDecisionModel.TendoKaeshiGoken,
        "Tendo Kaeshi Setsugekka" => SamuraiDecisionModel.TendoKaeshiSetsugekka,
        "Ogi Namikiri" => SamuraiDecisionModel.OgiNamikiri,
        "Kaeshi Namikiri" => SamuraiDecisionModel.KaeshiNamikiri,
        "Shinten" => SamuraiDecisionModel.Shinten,
        "Kyuten" => SamuraiDecisionModel.Kyuten,
        "Guren" => SamuraiDecisionModel.Guren,
        "Senei" => SamuraiDecisionModel.Senei,
        "Meikyo Shisui" => SamuraiDecisionModel.MeikyoShisui,
        "Ikishoten" => SamuraiDecisionModel.Ikishoten,
        "Shoha" => SamuraiDecisionModel.Shoha,
        "Zanshin" => SamuraiDecisionModel.Zanshin,
        _ => throw new InvalidOperationException($"Unknown Samurai action name '{actionName}'"),
    };

static uint ViperActionIdFor(string actionName) =>
    actionName.Trim() switch
    {
        "Steel Fangs" => ViperDecisionModel.SteelFangs,
        "Reaving Fangs" => ViperDecisionModel.ReavingFangs,
        "Hunter's Sting" => ViperDecisionModel.HuntersSting,
        "Swiftskin's Sting" => ViperDecisionModel.SwiftskinsSting,
        "Flanksting Strike" => ViperDecisionModel.FlankstingStrike,
        "Flanksbane Fang" => ViperDecisionModel.FlanksbaneFang,
        "Hindsting Strike" => ViperDecisionModel.HindstingStrike,
        "Hindsbane Fang" => ViperDecisionModel.HindsbaneFang,
        "Steel Maw" => ViperDecisionModel.SteelMaw,
        "Reaving Maw" => ViperDecisionModel.ReavingMaw,
        "Hunter's Bite" => ViperDecisionModel.HuntersBite,
        "Swiftskin's Bite" => ViperDecisionModel.SwiftskinsBite,
        "Jagged Maw" => ViperDecisionModel.JaggedMaw,
        "Bloodied Maw" => ViperDecisionModel.BloodiedMaw,
        "Vicewinder" => ViperDecisionModel.Vicewinder,
        "Hunter's Coil" => ViperDecisionModel.HuntersCoil,
        "Swiftskin's Coil" => ViperDecisionModel.SwiftskinsCoil,
        "Vicepit" => ViperDecisionModel.Vicepit,
        "Hunter's Den" => ViperDecisionModel.HuntersDen,
        "Swiftskin's Den" => ViperDecisionModel.SwiftskinsDen,
        "Reawaken" => ViperDecisionModel.Reawaken,
        "First Generation" => ViperDecisionModel.FirstGeneration,
        "Second Generation" => ViperDecisionModel.SecondGeneration,
        "Third Generation" => ViperDecisionModel.ThirdGeneration,
        "Fourth Generation" => ViperDecisionModel.FourthGeneration,
        "Ouroboros" => ViperDecisionModel.Ouroboros,
        "Writhing Snap" => ViperDecisionModel.WrithingSnap,
        "Uncoiled Fury" => ViperDecisionModel.UncoiledFury,
        "Death Rattle" => ViperDecisionModel.DeathRattle,
        "Last Lash" => ViperDecisionModel.LastLash,
        "First Legacy" => ViperDecisionModel.FirstLegacy,
        "Second Legacy" => ViperDecisionModel.SecondLegacy,
        "Third Legacy" => ViperDecisionModel.ThirdLegacy,
        "Fourth Legacy" => ViperDecisionModel.FourthLegacy,
        "Serpent's Ire" => ViperDecisionModel.SerpentsIre,
        "Serpent's Tail" => ViperDecisionModel.SerpentsTail,
        "Twinfang" => ViperDecisionModel.Twinfang,
        "Twinblood" => ViperDecisionModel.Twinblood,
        _ => throw new InvalidOperationException($"Unknown Viper action name '{actionName}'"),
    };

static uint ReaperActionIdFor(string actionName) =>
    actionName.Trim() switch
    {
        "Slice" => ReaperDecisionModel.Slice,
        "Waxing Slice" => ReaperDecisionModel.WaxingSlice,
        "Infernal Slice" => ReaperDecisionModel.InfernalSlice,
        "Spinning Scythe" => ReaperDecisionModel.SpinningScythe,
        "Nightmare Scythe" => ReaperDecisionModel.NightmareScythe,
        "Shadow of Death" => ReaperDecisionModel.ShadowOfDeath,
        "Whorl of Death" => ReaperDecisionModel.WhorlOfDeath,
        "Soul Slice" => ReaperDecisionModel.SoulSlice,
        "Soul Scythe" => ReaperDecisionModel.SoulScythe,
        "Gibbet" => ReaperDecisionModel.Gibbet,
        "Gallows" => ReaperDecisionModel.Gallows,
        "Guillotine" => ReaperDecisionModel.Guillotine,
        "Plentiful Harvest" => ReaperDecisionModel.PlentifulHarvest,
        "Harpe" => ReaperDecisionModel.Harpe,
        "Soulsow" => ReaperDecisionModel.Soulsow,
        "Harvest Moon" => ReaperDecisionModel.HarvestMoon,
        "Blood Stalk" => ReaperDecisionModel.BloodStalk,
        "Grim Swathe" => ReaperDecisionModel.GrimSwathe,
        "Gluttony" => ReaperDecisionModel.Gluttony,
        "Enshroud" => ReaperDecisionModel.Enshroud,
        "Void Reaping" => ReaperDecisionModel.VoidReaping,
        "Cross Reaping" => ReaperDecisionModel.CrossReaping,
        "Grim Reaping" => ReaperDecisionModel.GrimReaping,
        "Communio" => ReaperDecisionModel.Communio,
        "Lemures Slice" => ReaperDecisionModel.LemuresSlice,
        "Lemures Scythe" => ReaperDecisionModel.LemuresScythe,
        "Arcane Circle" => ReaperDecisionModel.ArcaneCircle,
        "Sacrificium" => ReaperDecisionModel.Sacrificium,
        "Executioner's Gibbet" => ReaperDecisionModel.ExecutionersGibbet,
        "Executioner's Gallows" => ReaperDecisionModel.ExecutionersGallows,
        "Executioner's Guillotine" => ReaperDecisionModel.ExecutionersGuillotine,
        "Perfectio" => ReaperDecisionModel.Perfectio,
        _ => throw new InvalidOperationException($"Unknown Reaper action name '{actionName}'"),
    };

static EvalResult EvaluateMachinist(RotationFixture fixture, JsonSerializerOptions options)
{
    var validation = ValidateFixture(fixture, "MCH", "machinist-production");
    if (!string.IsNullOrEmpty(validation))
        return EvalResult.Incomplete(fixture.Id, validation);

    MachinistFixtureInput input;
    try
    {
        input = fixture.Input.Deserialize<MachinistFixtureInput>(options) ?? new MachinistFixtureInput();
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"invalid Machinist input: {ex.Message}");
    }

    MachinistDecision? decision;
    try
    {
        var blocked = new HashSet<uint>(
            input.BlockedActions.Select(MachinistActionIdFor).Where(id => id != 0),
            EqualityComparer<uint>.Default);
        decision = MachinistDecisionModel.Decide(
            BuildMachinistState(input),
            input.IncludeAbilities,
            actionId => actionId != 0 && !blocked.Contains(actionId));
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"adapter error: {ex.Message}");
    }

    if (decision is null)
        return EvalResult.Incomplete(fixture.Id, "production model returned no action");

    return CompareDecision(
        fixture,
        decision.Value.ActionId,
        MachinistDecisionModel.DescribeAction,
        MachinistActionIdFor);
}

static EvalResult EvaluateNinja(RotationFixture fixture, JsonSerializerOptions options)
{
    var validation = ValidateFixture(fixture, "NIN", "ninja-production");
    if (!string.IsNullOrEmpty(validation))
        return EvalResult.Incomplete(fixture.Id, validation);

    NinjaFixtureInput input;
    try
    {
        input = fixture.Input.Deserialize<NinjaFixtureInput>(options) ?? new NinjaFixtureInput();
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"invalid Ninja input: {ex.Message}");
    }

    NinjaDecision? decision;
    try
    {
        var blocked = new HashSet<uint>(
            input.BlockedActions.Select(NinjaActionIdFor).Where(id => id != 0),
            EqualityComparer<uint>.Default);
        decision = NinjaDecisionModel.Decide(
            BuildNinjaState(input),
            input.IncludeAbilities,
            actionId => actionId != 0 && !blocked.Contains(actionId));
    }
    catch (Exception ex)
    {
        return EvalResult.Incomplete(fixture.Id, $"adapter error: {ex.Message}");
    }

    if (decision is null)
        return EvalResult.Incomplete(fixture.Id, "production model returned no action");

    return CompareDecision(
        fixture,
        decision.Value.ActionId,
        NinjaDecisionModel.DescribeAction,
        NinjaActionIdFor);
}

static MachinistDecisionState BuildMachinistState(MachinistFixtureInput input)
{
    uint Adjusted(string actionName)
    {
        var baseId = MachinistActionIdFor(actionName);
        if (input.AdjustedActions.TryGetValue(actionName, out var adjustedName) ||
            input.AdjustedActions.TryGetValue(actionName.Replace("'", ""), out adjustedName))
        {
            var adjustedId = MachinistActionIdFor(adjustedName);
            return adjustedId == 0 ? baseId : adjustedId;
        }
        return baseId;
    }

    var blockedNames = new HashSet<string>(input.BlockedActions, StringComparer.OrdinalIgnoreCase);
    bool HasBlocked(string actionName) => blockedNames.Contains(actionName);

    return new MachinistDecisionState(
        UseAoE: input.TargetCount >= 3,
        CanWeave: input.CanWeave,
        ComboAction: MachinistComboActionFor(input.ComboStep),
        Heat: input.Heat,
        Battery: input.Battery,
        QueenActive: input.QueenActive,
        WildfireReady: input.WildfireReady,
        FullMetalFieldReady: input.FullMetalFieldReady,
        Overheated: input.Overheated,
        Reassembled: input.Reassembled,
        HasDrillBioblaster: input.HasDrillBioblaster,
        HasAirAnchor: input.HasAirAnchor,
        HasChainSaw: input.HasChainSaw,
        AdjustedSplitShot: Adjusted("Split Shot"),
        AdjustedSlugShot: Adjusted("Slug Shot"),
        AdjustedCleanShot: Adjusted("Clean Shot"),
        AdjustedHeatedSplitShot: Adjusted("Heated Split Shot"),
        AdjustedHeatedSlugShot: Adjusted("Heated Slug Shot"),
        AdjustedHeatedCleanShot: Adjusted("Heated Clean Shot"),
        AdjustedScattergun: Adjusted("Scattergun"),
        AdjustedSpreadShot: Adjusted("Spread Shot"),
        AdjustedAutoCrossbow: Adjusted("Auto Crossbow"),
        AdjustedHeatBlast: Adjusted("Heat Blast"),
        AdjustedDrill: Adjusted("Drill"),
        AdjustedBioblaster: Adjusted("Bioblaster"),
        AdjustedAirAnchor: Adjusted("Air Anchor"),
        AdjustedChainSaw: Adjusted("Chain Saw"),
        AdjustedHotShot: Adjusted("Hot Shot"),
        AdjustedFullMetalField: Adjusted("Full Metal Field"),
        AdjustedWildfire: Adjusted("Wildfire"),
        AdjustedHypercharge: Adjusted("Hypercharge"),
        AdjustedGaussRound: Adjusted("Gauss Round"),
        AdjustedRicochet: Adjusted("Ricochet"),
        AdjustedRookTurret: Adjusted("Rook Autoturret"),
        AdjustedQueen: Adjusted("Automaton Queen"),
        CanUseReassemble: !HasBlocked("Reassemble"),
        CanUseWildfire: !HasBlocked("Wildfire"),
        CanUseHypercharge: !HasBlocked("Hypercharge"),
        CanUseGaussRound: !HasBlocked("Gauss Round"),
        CanUseRicochet: !HasBlocked("Ricochet"));
}

static NinjaDecisionState BuildNinjaState(NinjaFixtureInput input)
{
    uint Adjusted(string actionName)
    {
        var baseId = NinjaActionIdFor(actionName);
        if (input.AdjustedActions.TryGetValue(actionName, out var adjustedName) ||
            input.AdjustedActions.TryGetValue(actionName.Replace("'", ""), out adjustedName))
        {
            var adjustedId = NinjaActionIdFor(adjustedName);
            return adjustedId == 0 ? baseId : adjustedId;
        }
        return baseId;
    }

    var blockedNames = new HashSet<string>(input.BlockedActions, StringComparer.OrdinalIgnoreCase);
    bool HasBlocked(string actionName) => blockedNames.Contains(actionName);

    return new NinjaDecisionState(
        UseAoE: input.TargetCount >= 3,
        CanWeave: input.CanWeave,
        ComboAction: NinjaComboActionFor(input.ComboStep),
        Ninki: input.Ninki,
        InBurst: input.InBurst,
        DokumoriReady: input.DokumoriReady,
        RaijuReady: input.RaijuReady,
        MudraSequence: input.MudraSequence ?? [],
        HutonRemaining: input.HutonRemaining,
        HasHuton: input.HasHuton,
        HasTrickAttack: input.HasTrickAttack,
        HasKassatsu: input.HasKassatsu,
        HasTenChiJin: input.HasTenChiJin,
        HasMeisui: input.HasMeisui,
        AdjustedSpinningEdge: Adjusted("Spinning Edge"),
        AdjustedGustSlash: Adjusted("Gust Slash"),
        AdjustedAeolianEdge: Adjusted("Aeolian Edge"),
        AdjustedArmorCrush: Adjusted("Armor Crush"),
        AdjustedDeathBlossom: Adjusted("Death Blossom"),
        AdjustedHakkeMujinsatsu: Adjusted("Hakke Mujinsatsu"),
        AdjustedRaiton: Adjusted("Raiton"),
        AdjustedKaton: Adjusted("Katon"),
        AdjustedHyoton: Adjusted("Hyoton"),
        AdjustedHuton: Adjusted("Huton"),
        AdjustedDoton: Adjusted("Doton"),
        AdjustedSuiton: Adjusted("Suiton"),
        AdjustedDokumori: Adjusted("Dokumori"),
        AdjustedKunaisBane: Adjusted("Kunai's Bane"),
        AdjustedForkedRaiju: Adjusted("Forked Raiju"),
        AdjustedFleetingRaiju: Adjusted("Fleeting Raiju"),
        AdjustedTrickAttack: Adjusted("Trick Attack"),
        AdjustedKassatsu: Adjusted("Kassatsu"),
        AdjustedTenChiJin: Adjusted("Ten Chi Jin"),
        AdjustedMeisui: Adjusted("Meisui"),
        AdjustedBhavacakra: Adjusted("Bhavacakra"),
        AdjustedHellfrogMedium: Adjusted("Hellfrog Medium"),
        AdjustedBunshin: Adjusted("Bunshin"),
        AdjustedPhantomKamaitachi: Adjusted("Phantom Kamaitachi"),
        AdjustedThrowingDagger: Adjusted("Throwing Dagger"),
        CanUseDokumori: !HasBlocked("Dokumori"),
        CanUseTrickAttack: !HasBlocked("Trick Attack"),
        CanUseKassatsu: !HasBlocked("Kassatsu"),
        CanUseTenChiJin: !HasBlocked("Ten Chi Jin"),
        CanUseMeisui: !HasBlocked("Meisui"),
        CanUseBunshin: !HasBlocked("Bunshin"),
        CanUseBhavacakra: !HasBlocked("Bhavacakra"));
}

static uint MachinistComboActionFor(string comboStep) =>
    comboStep.ToLowerInvariant() switch
    {
        "" or "none" => 0,
        "split-shot" => MachinistDecisionModel.SplitShot,
        "heated-split-shot" => MachinistDecisionModel.HeatedSplitShot,
        "slug-shot" => MachinistDecisionModel.SlugShot,
        "heated-slug-shot" => MachinistDecisionModel.HeatedSlugShot,
        "clean-shot" => MachinistDecisionModel.CleanShot,
        "heated-clean-shot" => MachinistDecisionModel.HeatedCleanShot,
        _ => throw new InvalidOperationException($"Unsupported Machinist combo_step '{comboStep}'"),
    };

static uint NinjaComboActionFor(string comboStep) =>
    comboStep.ToLowerInvariant() switch
    {
        "" or "none" => 0,
        "spinning-edge" => NinjaDecisionModel.SpinningEdge,
        "gust-slash" => NinjaDecisionModel.GustSlash,
        "aeolian-edge" => NinjaDecisionModel.AeolianEdge,
        "armor-crush" => NinjaDecisionModel.ArmorCrush,
        _ => throw new InvalidOperationException($"Unsupported Ninja combo_step '{comboStep}'"),
    };

static uint MachinistActionIdFor(string actionName) =>
    actionName.Trim() switch
    {
        "Split Shot" => MachinistDecisionModel.SplitShot,
        "Slug Shot" => MachinistDecisionModel.SlugShot,
        "Clean Shot" => MachinistDecisionModel.CleanShot,
        "Heated Split Shot" => MachinistDecisionModel.HeatedSplitShot,
        "Heated Slug Shot" => MachinistDecisionModel.HeatedSlugShot,
        "Heated Clean Shot" => MachinistDecisionModel.HeatedCleanShot,
        "Scattergun" => MachinistDecisionModel.Scattergun,
        "Spread Shot" => MachinistDecisionModel.SpreadShot,
        "Auto Crossbow" => MachinistDecisionModel.AutoCrossbow,
        "Hot Shot" => MachinistDecisionModel.HotShot,
        "Full Metal Field" => MachinistDecisionModel.FullMetalField,
        "Drill" => MachinistDecisionModel.Drill,
        "Bioblaster" => MachinistDecisionModel.Bioblaster,
        "Air Anchor" => MachinistDecisionModel.AirAnchor,
        "Chain Saw" => MachinistDecisionModel.ChainSaw,
        "Heat Blast" => MachinistDecisionModel.HeatBlast,
        "Wildfire" => MachinistDecisionModel.Wildfire,
        "Hypercharge" => MachinistDecisionModel.Hypercharge,
        "Gauss Round" => MachinistDecisionModel.GaussRound,
        "Ricochet" => MachinistDecisionModel.Ricochet,
        "Rook Autoturret" => MachinistDecisionModel.RookAutoturret,
        "Automaton Queen" => MachinistDecisionModel.AutomatonQueen,
        "Reassemble" => MachinistDecisionModel.Reassemble,
        _ => throw new InvalidOperationException($"Unknown Machinist action name '{actionName}'"),
    };

static uint NinjaActionIdFor(string actionName) =>
    actionName.Trim() switch
    {
        "Spinning Edge" => NinjaDecisionModel.SpinningEdge,
        "Gust Slash" => NinjaDecisionModel.GustSlash,
        "Aeolian Edge" => NinjaDecisionModel.AeolianEdge,
        "Armor Crush" => NinjaDecisionModel.ArmorCrush,
        "Death Blossom" => NinjaDecisionModel.DeathBlossom,
        "Hakke Mujinsatsu" => NinjaDecisionModel.HakkeMujinsatsu,
        "Throwing Dagger" => NinjaDecisionModel.ThrowingDagger,
        "Fuma Shuriken" => NinjaDecisionModel.FumaShuriken,
        "Raiton" => NinjaDecisionModel.Raiton,
        "Katon" => NinjaDecisionModel.Katon,
        "Hyoton" => NinjaDecisionModel.Hyoton,
        "Huton" => NinjaDecisionModel.Huton,
        "Doton" => NinjaDecisionModel.Doton,
        "Suiton" => NinjaDecisionModel.Suiton,
        "Trick Attack" => NinjaDecisionModel.TrickAttack,
        "Kassatsu" => NinjaDecisionModel.Kassatsu,
        "Ten Chi Jin" => NinjaDecisionModel.TenChiJin,
        "Meisui" => NinjaDecisionModel.Meisui,
        "Bhavacakra" => NinjaDecisionModel.Bhavacakra,
        "Hellfrog Medium" => NinjaDecisionModel.HellfrogMedium,
        "Dokumori" => NinjaDecisionModel.Dokumori,
        "Kunai's Bane" => NinjaDecisionModel.KunaisBane,
        "Forked Raiju" => NinjaDecisionModel.ForkedRaiju,
        "Fleeting Raiju" => NinjaDecisionModel.FleetingRaiju,
        "Bunshin" => NinjaDecisionModel.Bunshin,
        "Phantom Kamaitachi" => NinjaDecisionModel.PhantomKamaitachi,
        _ => throw new InvalidOperationException($"Unknown Ninja action name '{actionName}'"),
    };

internal enum EvalStatus
{
    Pass,
    PassNegative,
    Fail,
    FailNegative,
    Incomplete,
}

internal sealed record EvalResult(EvalStatus Status, string ActualAction, string Message)
{
    public static EvalResult Pass(string actualAction) => new(EvalStatus.Pass, actualAction, "");
    public static EvalResult PassNegative(string actualAction) => new(EvalStatus.PassNegative, actualAction, "");
    public static EvalResult Fail(string actualAction, string message) => new(EvalStatus.Fail, actualAction, message);
    public static EvalResult FailNegative(string actualAction, string message) => new(EvalStatus.FailNegative, actualAction, message);
    public static EvalResult Incomplete(string fixtureId, string message) => new(EvalStatus.Incomplete, "", message);
}

internal sealed class JobSummary(string job, string adapter, int fixtures)
{
    public string Job { get; } = job;
    public string Adapter { get; } = adapter;
    public int Fixtures { get; } = fixtures;
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Incomplete { get; set; }
    public int Skipped { get; set; }
    public int NegativeControls { get; set; }
    public string Result { get; set; } = "UNKNOWN";
}

internal sealed class RotationFixture
{
    public string Path { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("job")]
    public string Job { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("input")]
    public JsonElement Input { get; set; }

    [JsonPropertyName("expected_action")]
    public string ExpectedAction { get; set; } = "";

    [JsonPropertyName("should_not_actions")]
    public List<string> ShouldNotActions { get; set; } = [];

    [JsonPropertyName("negative_control")]
    public bool NegativeControl { get; set; }

    [JsonPropertyName("adapter")]
    public string Adapter { get; set; } = "";
}

internal sealed class SamuraiFixtureInput
{
    [JsonPropertyName("sen")]
    public int Sen { get; set; }

    [JsonPropertyName("kenki")]
    public int Kenki { get; set; }

    [JsonPropertyName("has_tendo")]
    public bool HasTendo { get; set; }

    [JsonPropertyName("has_tendo_kaeshi_ready")]
    public bool HasTendoKaeshiReady { get; set; }

    [JsonPropertyName("has_ogi_ready")]
    public bool HasOgiReady { get; set; }

    [JsonPropertyName("has_meikyo")]
    public bool HasMeikyo { get; set; }

    [JsonPropertyName("has_fugetsu")]
    public bool HasFugetsu { get; set; }

    [JsonPropertyName("has_fuka")]
    public bool HasFuka { get; set; }

    [JsonPropertyName("has_zanshin_ready")]
    public bool HasZanshinReady { get; set; }

    [JsonPropertyName("has_getsu")]
    public bool? HasGetsu { get; set; }

    [JsonPropertyName("has_setsu")]
    public bool? HasSetsu { get; set; }

    [JsonPropertyName("has_ka")]
    public bool? HasKa { get; set; }

    [JsonPropertyName("target_count")]
    public int TargetCount { get; set; } = 1;

    [JsonPropertyName("combo_step")]
    public string ComboStep { get; set; } = "none";

    [JsonPropertyName("level")]
    public int Level { get; set; } = 100;

    [JsonPropertyName("include_abilities")]
    public bool IncludeAbilities { get; set; }

    [JsonPropertyName("blocked_actions")]
    public List<string> BlockedActions { get; set; } = [];

    [JsonPropertyName("ready_tsubame_action")]
    public string ReadyTsubameAction { get; set; } = "";

    [JsonPropertyName("ready_ogi_action")]
    public string ReadyOgiAction { get; set; } = "";

    [JsonPropertyName("ready_iaijutsu_action")]
    public string ReadyIaijutsuAction { get; set; } = "";

    [JsonPropertyName("fugetsu_remaining")]
    public float FugetsuRemaining { get; set; }

    [JsonPropertyName("fuka_remaining")]
    public float FukaRemaining { get; set; }

    [JsonPropertyName("higanbana_remaining")]
    public float HiganbanaRemaining { get; set; }

    [JsonPropertyName("meditation_stacks")]
    public int MeditationStacks { get; set; }
}

internal sealed class ViperFixtureInput
{
    [JsonPropertyName("target_count")]
    public int TargetCount { get; set; } = 1;

    [JsonPropertyName("combo_step")]
    public string ComboStep { get; set; } = "none";

    [JsonPropertyName("level")]
    public int Level { get; set; } = 100;

    [JsonPropertyName("include_abilities")]
    public bool IncludeAbilities { get; set; }

    [JsonPropertyName("can_weave")]
    public bool CanWeave { get; set; } = true;

    [JsonPropertyName("generation")]
    public int Generation { get; set; }

    [JsonPropertyName("serpent_offering")]
    public int? SerpentOffering { get; set; }

    [JsonPropertyName("rattling_coil_stacks")]
    public int? RattlingCoilStacks { get; set; }

    [JsonPropertyName("coils")]
    public int Coils { get; set; }

    [JsonPropertyName("reawaken_ready")]
    public bool ReawakenReady { get; set; }

    [JsonPropertyName("ready_to_reawaken")]
    public bool ReadyToReawaken { get; set; }

    [JsonPropertyName("reawakened")]
    public bool Reawakened { get; set; }

    [JsonPropertyName("dread_combo")]
    public string DreadCombo { get; set; } = "none";

    [JsonPropertyName("serpent_combo")]
    public string SerpentCombo { get; set; } = "none";

    [JsonPropertyName("serpents_tail_ready")]
    public bool SerpentsTailReady { get; set; }

    [JsonPropertyName("has_both_buffs")]
    public bool HasBothBuffs { get; set; }

    [JsonPropertyName("hunters_instinct")]
    public bool HuntersInstinct { get; set; }

    [JsonPropertyName("swiftscaled")]
    public bool Swiftscaled { get; set; }

    [JsonPropertyName("hunters_venom")]
    public bool HuntersVenom { get; set; }

    [JsonPropertyName("swiftskins_venom")]
    public bool SwiftskinsVenom { get; set; }

    [JsonPropertyName("fellhunters_venom")]
    public bool FellhuntersVenom { get; set; }

    [JsonPropertyName("fellskins_venom")]
    public bool FellskinsVenom { get; set; }

    [JsonPropertyName("poised_for_twinfang")]
    public bool PoisedForTwinfang { get; set; }

    [JsonPropertyName("poised_for_twinblood")]
    public bool PoisedForTwinblood { get; set; }

    [JsonPropertyName("has_twinfang_weave")]
    public bool HasTwinfangWeave { get; set; }

    [JsonPropertyName("has_twinblood_weave")]
    public bool HasTwinbloodWeave { get; set; }

    [JsonPropertyName("hindstung_venom")]
    public bool HindstungVenom { get; set; }

    [JsonPropertyName("hindsbane_venom")]
    public bool HindsbaneVenom { get; set; }

    [JsonPropertyName("flankstung_venom")]
    public bool FlankstungVenom { get; set; }

    [JsonPropertyName("flanksbane_venom")]
    public bool FlanksbaneVenom { get; set; }

    [JsonPropertyName("grimhunters_venom")]
    public bool GrimhuntersVenom { get; set; }

    [JsonPropertyName("grimskins_venom")]
    public bool GrimskinsVenom { get; set; }

    [JsonPropertyName("honed_reavers")]
    public bool HonedReavers { get; set; }

    [JsonPropertyName("honed_steel")]
    public bool HonedSteel { get; set; }

    [JsonPropertyName("adjusted_actions")]
    public Dictionary<string, string> AdjustedActions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("blocked_actions")]
    public List<string> BlockedActions { get; set; } = [];
}

internal sealed class ReaperFixtureInput
{
    [JsonPropertyName("target_count")]
    public int TargetCount { get; set; } = 1;

    [JsonPropertyName("combo_step")]
    public string ComboStep { get; set; } = "none";

    [JsonPropertyName("level")]
    public int Level { get; set; } = 100;

    [JsonPropertyName("include_abilities")]
    public bool IncludeAbilities { get; set; }

    [JsonPropertyName("can_weave")]
    public bool CanWeave { get; set; } = true;

    [JsonPropertyName("soul")]
    public int Soul { get; set; }

    [JsonPropertyName("shroud")]
    public int Shroud { get; set; }

    [JsonPropertyName("lemure_shroud")]
    public int LemureShroud { get; set; }

    [JsonPropertyName("void_shroud")]
    public int VoidShroud { get; set; }

    [JsonPropertyName("deaths_design_remaining")]
    public float DeathsDesignRemaining { get; set; }

    [JsonPropertyName("has_deaths_design")]
    public bool HasDeathsDesign { get; set; }

    [JsonPropertyName("has_soul_reaver")]
    public bool HasSoulReaver { get; set; }

    [JsonPropertyName("soul_reaver_ready")]
    public bool SoulReaverReady { get; set; }

    [JsonPropertyName("has_executioner")]
    public bool HasExecutioner { get; set; }

    [JsonPropertyName("executioner_ready")]
    public bool ExecutionerReady { get; set; }

    [JsonPropertyName("has_enshrouded")]
    public bool HasEnshrouded { get; set; }

    [JsonPropertyName("enshrouded")]
    public bool Enshrouded { get; set; }

    [JsonPropertyName("has_immortal_sacrifice")]
    public bool HasImmortalSacrifice { get; set; }

    [JsonPropertyName("has_perfectio_parata")]
    public bool HasPerfectioParata { get; set; }

    [JsonPropertyName("has_ideal_host")]
    public bool HasIdealHost { get; set; }

    [JsonPropertyName("has_oblatio")]
    public bool HasOblatio { get; set; }

    [JsonPropertyName("has_enhanced_gibbet")]
    public bool HasEnhancedGibbet { get; set; }

    [JsonPropertyName("has_enhanced_gallows")]
    public bool HasEnhancedGallows { get; set; }

    [JsonPropertyName("has_enhanced_void_reaping")]
    public bool HasEnhancedVoidReaping { get; set; }

    [JsonPropertyName("has_enhanced_cross_reaping")]
    public bool HasEnhancedCrossReaping { get; set; }

    [JsonPropertyName("has_soulsow")]
    public bool HasSoulsow { get; set; }

    [JsonPropertyName("in_combat")]
    public bool InCombat { get; set; } = true;

    [JsonPropertyName("prefer_gibbet_next")]
    public bool PreferGibbetNext { get; set; } = true;

    [JsonPropertyName("adjusted_actions")]
    public Dictionary<string, string> AdjustedActions { get; set; } = [];

    [JsonPropertyName("blocked_actions")]
    public List<string> BlockedActions { get; set; } = [];
}

internal sealed class MachinistFixtureInput
{
    [JsonPropertyName("full_metal_field_ready")]
    public bool FullMetalFieldReady { get; set; }
    [JsonPropertyName("heat")]
    public int Heat { get; set; }
    [JsonPropertyName("battery")]
    public int Battery { get; set; }
    [JsonPropertyName("queen_active")]
    public bool QueenActive { get; set; }
    [JsonPropertyName("wildfire_ready")]
    public bool WildfireReady { get; set; }
    [JsonPropertyName("target_count")]
    public int TargetCount { get; set; } = 1;
    [JsonPropertyName("level")]
    public int Level { get; set; } = 100;
    [JsonPropertyName("include_abilities")]
    public bool IncludeAbilities { get; set; } = true;
    [JsonPropertyName("can_weave")]
    public bool CanWeave { get; set; } = true;
    [JsonPropertyName("combo_step")]
    public string ComboStep { get; set; } = "none";
    [JsonPropertyName("overheated")]
    public bool Overheated { get; set; }
    [JsonPropertyName("reassembled")]
    public bool Reassembled { get; set; }
    [JsonPropertyName("has_drill_bioblaster")]
    public bool HasDrillBioblaster { get; set; }
    [JsonPropertyName("has_air_anchor")]
    public bool HasAirAnchor { get; set; }
    [JsonPropertyName("has_chain_saw")]
    public bool HasChainSaw { get; set; }
    [JsonPropertyName("adjusted_actions")]
    public Dictionary<string, string> AdjustedActions { get; set; } = [];
    [JsonPropertyName("blocked_actions")]
    public List<string> BlockedActions { get; set; } = [];
}

internal sealed class NinjaFixtureInput
{
    [JsonPropertyName("dokumori_ready")]
    public bool DokumoriReady { get; set; }
    [JsonPropertyName("in_burst")]
    public bool InBurst { get; set; }
    [JsonPropertyName("ninki")]
    public int Ninki { get; set; }
    [JsonPropertyName("mudra_sequence")]
    public string[] MudraSequence { get; set; } = [];
    [JsonPropertyName("raiju_ready")]
    public bool RaijuReady { get; set; }
    [JsonPropertyName("target_count")]
    public int TargetCount { get; set; } = 1;
    [JsonPropertyName("level")]
    public int Level { get; set; } = 100;
    [JsonPropertyName("include_abilities")]
    public bool IncludeAbilities { get; set; } = true;
    [JsonPropertyName("can_weave")]
    public bool CanWeave { get; set; } = true;
    [JsonPropertyName("combo_step")]
    public string ComboStep { get; set; } = "none";
    [JsonPropertyName("huton_remaining")]
    public int HutonRemaining { get; set; }
    [JsonPropertyName("has_huton")]
    public bool HasHuton { get; set; } = true;
    [JsonPropertyName("has_trick_attack")]
    public bool HasTrickAttack { get; set; }
    [JsonPropertyName("has_kassatsu")]
    public bool HasKassatsu { get; set; }
    [JsonPropertyName("has_ten_chi_jin")]
    public bool HasTenChiJin { get; set; }
    [JsonPropertyName("has_meisui")]
    public bool HasMeisui { get; set; }
    [JsonPropertyName("adjusted_actions")]
    public Dictionary<string, string> AdjustedActions { get; set; } = [];
    [JsonPropertyName("blocked_actions")]
    public List<string> BlockedActions { get; set; } = [];
}
