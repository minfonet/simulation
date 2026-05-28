---
id: mem_learning_opencode_skill_restart_required
type: learning
tags:
  - opencode
  - skills
  - qa
---

# opencode Skill Changes Require Restart

Project skill files under `.opencode/skills/<name>/SKILL.md` are configuration-time artifacts. opencode loads them when the application starts and does not hot-reload changes in an already running session.

QA validation for skill creation or edits should therefore confirm the files statically and tell the user to quit and restart opencode before expecting new or updated skills to be available.
