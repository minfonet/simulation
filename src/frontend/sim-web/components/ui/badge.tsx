import { cn } from "@/lib/utils"

const variants = {
  default: "bg-zinc-100 text-zinc-800",
  secondary: "bg-zinc-100 text-zinc-600",
  destructive: "bg-red-100 text-red-800",
  outline: "border border-zinc-300 text-zinc-600",
  success: "bg-green-100 text-green-800",
  warning: "bg-yellow-100 text-yellow-800",
} as const

interface BadgeProps {
  variant?: keyof typeof variants
  className?: string
  children: React.ReactNode
}

export function Badge({ variant = "default", className, children }: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
        variants[variant],
        className,
      )}
    >
      {children}
    </span>
  )
}
