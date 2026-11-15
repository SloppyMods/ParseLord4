---
tags:
  - type/doc
  - project/parselord4
  - status/active
type: doc
project: parselord4
status: active
aliases: []
---
# Dalamud Patterns (Shared Reference)

Common boilerplate patterns used across all ParseLord4 stages.
Max 200 lines. Do not duplicate content from _config/.

## Hook Pattern

```csharp
// 1. Declare delegate matching the game function signature
private delegate ReturnType FunctionNameDelegate(ArgType* self, ...);

// 2. Declare hook field
private Hook<FunctionNameDelegate>? _hook;

// 3. Create in constructor
_hook = Plugin.GameInteropProvider.HookFromAddress<FunctionNameDelegate>(
    ClassName.Addresses.FunctionName.Value,
    Detour);

// 4. Enable/Disable
public void Enable() { _hook?.Enable(); }
public void Disable() { _hook?.Disable(); }

// 5. Always call Original first (or at end for UseAction)
private ReturnType Detour(ArgType* self, ...) {
    var original = _hook!.Original(self, ...);
    try { /* your logic */ }
    catch (Exception ex) { Plugin.Log.Error(ex, "Detour threw"); }
    return original;
}

// 6. Dispose
public void Dispose() { _hook?.Dispose(); _hook = null; }
```

## Plugin Service Injection

```csharp
public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IUiBuilder UiBuilder { get; private set; } = null!; // via PluginInterface
}
```

## EzThrottler (ECommons)

```csharp
using ECommons.Throttlers;

// Returns true only if enough time has passed since last call with same key
if (EzThrottler.Throttle("UniqueKey_" + actionId, milliseconds))
{
    // do the work
}
```

## Reading Job Gauge (example: WAR)

```csharp
var gauge = Plugin.ClientState.LocalPlayer?.GetGaugeData<WarriorGauge>();
int beastGauge = gauge?.BeastGauge ?? 0;
```

## ActionManager Status Check

```csharp
unsafe {
    var am = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
    // 0 = can use, non-zero = cannot (see ActionManagerError enum)
    uint status = am->GetActionStatus(ActionType.Action, actionId, targetId);
    bool ready = status == 0;
}
```

## UseAction Execution

```csharp
unsafe {
    var am = ActionManager.Instance();
    // Always check status before calling UseAction
    if (am->GetActionStatus(ActionType.Action, actionId, targetId) == 0)
        am->UseAction(ActionType.Action, actionId, targetId);
}
```

## IGameObject Null Safety

```csharp
// Use IObjectTable, not IClientState.LocalPlayer (deprecated)
var player = Plugin.ObjectTable.FirstOrDefault(o =>
    o is IPlayerCharacter p && p.IsValid()) as IPlayerCharacter;

// Always null-check before accessing
if (player == null) return;
```

## ImGui Window (Dalamud pattern)

```csharp
Plugin.PluginInterface.UiBuilder.Draw += DrawUI;
Plugin.PluginInterface.UiBuilder.OpenMainUi += OpenMainUI;

private void DrawUI()
{
    if (!_visible) return;
    ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);
    if (ImGui.Begin("Parse Lord 4", ref _visible))
    {
        // draw content
    }
    ImGui.End();
}
```
