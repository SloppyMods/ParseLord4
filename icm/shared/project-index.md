---
tags:
  - type/index
  - project/parselord4
  - status/active
  - workflow/icm
type: index
project: parselord4
status: active
aliases: []
---
# Project Index

Project: ParseLord4

## Core docs

- `CONTEXT.md` - canonical router, active stage, status snapshot, and current build status.
- `AGENTS_HISTORY.md` - durable implementation log with root-cause notes and verification history.
- `_config/project.md` - identity, SDK, build commands, output path, and shipping constraints.
- `_config/architecture.md` - three-layer hook model and system boundaries.
- `_config/action-ids.md` - action and status IDs before code changes.

## Active code and stages

- `stages/06-polish/CONTEXT.md` - active stage contract.
- `stages/06-polish/output/ParseLord4/` - active implementation tree and deployed plugin source.
- `stages/05-auto-rotation/CONTEXT.md` - tested Stage 05 rotation reference.
- `icm/CONTEXT.md` - ICM router for discovery, planning, implementation, verification, and handoff.

## External references

- `C:\Users\you\Documents\Projects\ActionStacksEX` - external stack behavior and editor UX reference.
- `stages/01-design/references/source-analysis.md` - upstream design/reference notes, including WC and related patterns.

## Build and verify

- Release: `.\scripts\build-active.ps1 -Configuration Release`
- Debug: `.\scripts\build-active.ps1 -Configuration Debug`
- Deploy target: `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`
