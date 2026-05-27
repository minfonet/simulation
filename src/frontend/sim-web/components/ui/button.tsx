import { forwardRef, type ButtonHTMLAttributes } from "react"
import { cn } from "@/lib/utils"

const variants = {
  default: "bg-zinc-900 text-white hover:bg-zinc-800",
  destructive: "bg-red-600 text-white hover:bg-red-500",
  outline: "border border-zinc-300 bg-white hover:bg-zinc-50",
  secondary: "bg-zinc-100 text-zinc-900 hover:bg-zinc-200",
  ghost: "hover:bg-zinc-100",
  link: "text-zinc-900 underline-offset-4 hover:underline",
} as const

const sizes = {
  default: "h-10 px-4 py-2",
  sm: "h-9 rounded-md px-3",
  lg: "h-11 rounded-md px-8",
  icon: "h-10 w-10",
} as const

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: keyof typeof variants
  size?: keyof typeof sizes
}

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = "default", size = "default", ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={cn(
          "inline-flex items-center justify-center gap-2 rounded-md text-sm font-medium transition-colors disabled:pointer-events-none disabled:opacity-50",
          variants[variant],
          sizes[size],
          className,
        )}
        {...props}
      />
    )
  },
)
Button.displayName = "Button"

export { Button }
