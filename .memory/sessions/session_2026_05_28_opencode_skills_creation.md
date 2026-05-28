# Session: 2026-05-28 — Opencode Project Skills Creation

## What was done

Created five opencode project skills under `.opencode/skills/` as recommended in `docs/03-ai-system/agent-skill-readiness.md`:

1. **simulation-domain** — domain flows, roles, session states, presets, evaluation rules
2. **godot-telemetry-hal** — Godot adapter code, HAL, telemetry collection, launch handoff
3. **backend-telemetry-reporting** — ingestor/store, reports, ownership, versioning
4. **nextjs-role-ui** — Next.js role pages, auth, route map, UI states, component tests
5. **qa-e2e-simulation** — E2E smoke test, Docker Compose, seeding, Godot launch, full-flow triage

Each skill was written in English per `docs/06-engineering/language-policy.md`, frontmatter-validated, and architecture-guardrail-enforcing.

## Files changed

### Created
- `.opencode/skills/simulation-domain/SKILL.md`
- `.opencode/skills/godot-telemetry-hal/SKILL.md`
- `.opencode/skills/backend-telemetry-reporting/SKILL.md`
- `.opencode/skills/nextjs-role-ui/SKILL.md`
- `.opencode/skills/qa-e2e-simulation/SKILL.md`
- `.memory/memories/learnings/mem_opencode_skill_restart_required.md`

### Modified
- `.memory/active-context.md` — updated project state, MVP reference, flow gap docs, review/QA status, memory state, risks, current focus, open tasks

## Review/QA results

- **REVIEWER**: PASS, Architecture Gate PASS. Confirmed correct paths, frontmatter, English content, guardrail enforcement. Noted unrelated workspace changes but skills themselves are clean.
- **QA**: 30/30 static checks PASS. Architecture Boundary Coverage PASS. Zero Spanish accents or terms found. Created learning: `mem_opencode_skill_restart_required.md`.

## Decisions made

- Skills follow the pattern `.opencode/skills/<name>/SKILL.md` — no `opencode.json` change needed for project skills auto-discovery.
- No `.opencode/agents/*` modifications were made (per constraint).
- Skills reinforce the architectural guardrails from `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`.

## Restart requirement

Skills are only loaded on opencode startup. The user must quit and restart opencode for the new skills to appear in the available skills list.

## Next steps

1. Restart opencode to load the new skills.
2. Begin P0 implementation: base scenario preset contract/list/endpoint, Godot launch handoff, and Godot auth/token strategy.
3. Run E2E smoke test against live Docker Compose services.
4. Add critical event model and basic evaluator report.
5. Write frontend page component tests (admin, instructor, trainee).
