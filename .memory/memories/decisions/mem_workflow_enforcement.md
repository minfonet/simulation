---
id: mem_decision_workflow_enforcement
type: decision
tags:
  - workflow
  - agents
  - lead
  - implementer
  - reviewer
---

# Context

The defined workflow (LEAD → IMPLEMENTER → REVIEWER) was not executed in any MVP phase. The LEAD implemented everything directly. The architecture-review revealed inconsistencies that a reviewer would have detected.

# Decision

Make the workflow mandatory instead of optional. Change all "Prefer delegation" rules to "Delegate ALL implementation / ALL review". The only exception: trivial changes (< 10 lines, 1 file).

# Why

- The detected inconsistencies (ContactMonitor, proxy vs localStorage, DI bypass) are exactly the type of issues a reviewer catches
- The multi-agent workflow only adds value if it is actually executed
- Aspirational rules do not work — they must be binding

# Consequences

- active-context.md has a "Workflow Rules (mandatory)" section with 6 steps
- .opencode/agents/lead.md updated with imperative language
- agents/lead.md updated for consistency
- The next task must follow the pipeline mandatorily
- architecture-review.md must be read before starting new work
