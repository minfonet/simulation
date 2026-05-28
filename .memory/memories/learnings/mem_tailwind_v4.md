---
id: mem_learning_tailwind_v4
type: learning
tags:
  - frontend
  - tailwind
  - css
---

# Topic

Tailwind CSS v4 changes configuration compared with v3.

# Details

- There is no `tailwind.config.js` — configuration lives in `globals.css` with the `@theme` directive
- The `@tailwind base/components/utilities` directives are not used — only `@import "tailwindcss"`
- The PostCSS plugin is `@tailwindcss/postcss` (not `tailwindcss`)
- Dark mode variants are configured via `@variant` or direct media query in CSS
- There is no `darkMode: "class"` in config — use `@custom-variant dark (&:is(.dark *))` if class-based dark mode is needed

# Impact

If someone tries to create a `tailwind.config.js` with v3 syntax, it will have no effect. All theme configuration must live in CSS.
