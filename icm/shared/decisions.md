---
tags:
  - type/doc
  - project/parselord4
  - status/active
  - workflow/icm
type: doc
project: parselord4
status: active
aliases: []
---
# Decisions

Record durable decisions here when they should survive the current task.

| Date | Decision | Reason | Source |
| --- | --- | --- | --- |
| 2026-05-11 | Use the ICM router and stage contracts as the canonical workflow for project work. | Keeps work scoped to staged context instead of legacy planning folders. | `CONTEXT.md`, `AGENTS_HISTORY.md` |
| 2026-05-11 | Treat `stages/06-polish/output/ParseLord4/` as the active implementation tree. | Stage 06 owns the deployed plugin code and current build/deploy flow. | `CONTEXT.md` |
| 2026-05-11 | Build and verify through `scripts/build-active.ps1`, not arbitrary stage project files. | All stage projects deploy into the same `devPlugins\ParseLord4` target, so active-stage builds avoid clobbering snapshots. | `_config/project.md`, `CONTEXT.md` |
| 2026-05-13 | Keep WC-style button combines as standalone presets rather than full WC simple/advanced scaffolding, and leave them disabled by default until enabled. | Preserves existing Assist/FullAuto behavior while exposing optional WC-style replacements. | `AGENTS_HISTORY.md` |
| 2026-05-14 | Use `C:\Users\you\Documents\Projects\ActionStacksEX` as the external reference for ParseLord4 stack behavior and editor UX. | Stack work should stay aligned with known ActionStacksEX behavior unless the user broadens scope. | `CONTEXT.md`, Codex ad-hoc memory notes |
| 2026-05-16 | For future job WC combo passes, implement every applicable non-simple/non-advanced combo option, not only main/area WC buttons. | User explicitly wants full WC-style coverage while excluding WC simple and advanced mode scaffolding. | User request during VPR pass |
