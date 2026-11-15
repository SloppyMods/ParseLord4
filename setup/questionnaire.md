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
# ParseLord4 Setup Questionnaire

One-time onboarding. Answer all questions, then answers propagate to stage references.
Already answered — preserved here for reference.

---

**Q: What is the plugin name and internal name?**
A: Display name "Parse Lord 4", InternalName "ParseLord4", AssemblyName "ParseLord4"

**Q: What is the target FFXIV patch and Dalamud API version?**
A: FFXIV Patch 7.5, Dalamud API 15, .NET 10, Dalamud.NET.Sdk 15.0.0

**Q: What license?**
A: GPL-3.0

**Q: What are the slash commands?**
A: `/pl`, `/parselord`

**Q: What is the default operation mode?**
A: Assist (Layer 1 + Layer 2, no auto-execution)

**Q: Should ECommons be included?**
A: Yes — needed for EzThrottler (Layer 1 throttling) and IPC utilities

**Q: Should rotations be bundled or external?**
A: Bundled for v1. External loader deferred to v2.

**Q: Which pilot jobs to implement first in Layer 3?**
A: PLD (Paladin), WAR (Warrior), WHM (White Mage) — one per role category

**Q: What AoE threshold?**
A: 3 targets in splash range

**Q: Emergency heal HP threshold?**
A: 25% HP

**Q: Should opener sequences be supported?**
A: Yes, toggleable per job

**Q: Hook conflict behavior?**
A: Detect on startup, show dismissable banner in UI, do not crash
