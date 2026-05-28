# Session: Flow gap docs and agent/skill readiness

**Date:** 2026-05-28

## What changed

- Documented the requested target flows in `docs/`: Admin creates organization, Evaluator/Instructor defines an evaluation with a base scenario preset, Evaluated/Trainee launches Godot and runs a test with no mandatory time limit, and Evaluator grades using a basic report with critical events.
- Documented the minimum gap plan to complete those flows, with P0/P1/P2 priorities, acceptance criteria, and architecture boundaries.
- Analyzed whether the current agent architecture (`lead`, `implementer`, `reviewer`, `qa`) is ready to execute the pending work.
- Documented when project-specific skills should be created to keep base agents more agnostic.
- Fixed the `mdvp.md` inconsistency that mentioned a fixed 5-minute duration; the target now states that there is no mandatory time limit.
- Passed through REVIEWER and QA.

## Files changed

- `docs/01-product/user-flows.md`
- `docs/01-product/flow-gap-plan.md`
- `docs/03-ai-system/agent-skill-readiness.md`
- `docs/01-product/mdvp.md`
- `.memory/active-context.md`
- `.memory/sessions/session_2026_05_28_flow_gap_docs_agent_readiness.md`

## Bugs found

- REVIEWER found that `user-flows.md` overstated E2E coverage as if the smoke test had already run against live services. It was corrected to state that the script includes the path but live execution is still pending.
- REVIEWER found an inconsistency between the new no-time-limit target and `mdvp.md` (“John drives for 5 minutes”). It was corrected.
- QA found no new bugs or blocking documentation gaps.

## Decisions made

- Current agents are sufficient to execute P0 if Lead delegates with explicit criteria from `docs/01-product/flow-gap-plan.md`.
- Before P1 (critical events/report), project skills should be created, especially `backend-telemetry-reporting` and `godot-telemetry-hal`, because those tasks cross sensitive boundaries.
- Recommended skills should provide domain knowledge/checklists and keep base agents agnostic: Lead coordinates, Implementer implements, Reviewer reviews, and QA tests.
- `.opencode/agents/*` was not modified and no skills were created in this session; only the proposal was documented.

## Verification

- REVIEWER: PASS.
- Architecture Gate: PASS.
- QA: PASS.
- Architecture Boundary Coverage: PASS.
- QA ran `git diff --check` on the 4 docs modified by the task; no whitespace errors, only non-blocking LF→CRLF normalization warnings.

## Next steps

1. Implement P0 base scenario preset: contract/listing, backend validation, UI selector, and mapping to Godot base scene/config.
2. Implement P0 Godot launch handoff from Trainee with `sessionId`, `apiUrl`, and authorization strategy.
3. Fix/define the auth strategy for Godot (`StartSession`, telemetry, and `FinishSession`) while avoiding embedded credentials.
4. Before P1, create project skills if repeated/sensitive work is confirmed: `backend-telemetry-reporting`, `godot-telemetry-hal`, `nextjs-role-ui`, `qa-e2e-simulation`.
5. Keep new telemetry/reporting implementations behind ingestion/storage/query boundaries and versionable contracts with producer/consumer tests.
