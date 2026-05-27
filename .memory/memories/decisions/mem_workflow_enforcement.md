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

El workflow definido (LEAD → IMPLEMENTER → REVIEWER) no se ejecutó en ninguna fase del MVP. El LEAD implementó todo directamente. La architecture-review reveló inconsistencias que un reviewer habría detectado.

# Decision

Hacer el workflow obligatorio en lugar de optativo. Cambiar todas las reglas de "Prefer delegation" a "Delegate ALL implementation / ALL review". La única excepción: cambios triviales (< 10 líneas, 1 archivo).

# Why

- Las inconsistencias detectadas (ContactMonitor, proxy vs localStorage, DI bypass) son exactamente el tipo de issues que un reviewer atrapa
- El workflow multi-agente solo aporta valor si se ejecuta realmente
- Las reglas aspiracionales no funcionan — deben ser vinculantes

# Consequences

- active-context.md tiene sección "Workflow Rules (mandatory)" con 6 pasos
- .opencode/agents/lead.md actualizado con lenguaje imperativo
- agents/lead.md actualizado para consistencia
- La próxima tarea debe seguir el pipeline obligatoriamente
- architecture-review.md debe leerse antes de empezar trabajo nuevo
