---
id: mem_learning_tailwind_v4
type: learning
tags:
  - frontend
  - tailwind
  - css
---

# Topic

Tailwind CSS v4 cambia la configuración respecto a v3.

# Details

- No hay `tailwind.config.js` — la configuración va en `globals.css` con la directiva `@theme`
- No se usan las directivas `@tailwind base/components/utilities` — solo `@import "tailwindcss"`
- El PostCSS plugin es `@tailwindcss/postcss` (no `tailwindcss`)
- Las variantes dark mode se configuran via `@variant` o media query directa en CSS
- No hay `darkMode: "class"` en config — se usa `@custom-variant dark (&:is(.dark *))` si se necesita modo oscuro por clase

# Impact

Si alguien intenta crear un `tailwind.config.js` con la sintaxis v3, no tendrá efecto. Toda la configuración de tema debe ir en CSS.
