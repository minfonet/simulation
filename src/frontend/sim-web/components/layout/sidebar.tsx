"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"
import { useAuth } from "@/lib/auth-context"
import {
  LayoutDashboard,
  Building2,
  Users,
  Gamepad2,
  ClipboardCheck,
  FileText,
  LogOut,
} from "lucide-react"

const roleMenus: Record<string, { href: string; label: string; icon: React.ReactNode }[]> = {
  Admin: [
    { href: "/admin", label: "Dashboard", icon: <LayoutDashboard className="h-4 w-4" /> },
    { href: "/admin/organizations", label: "Organizations", icon: <Building2 className="h-4 w-4" /> },
    { href: "/admin/users", label: "Users", icon: <Users className="h-4 w-4" /> },
  ],
  Instructor: [
    { href: "/instructor", label: "Dashboard", icon: <LayoutDashboard className="h-4 w-4" /> },
    { href: "/instructor/sessions", label: "Sessions", icon: <Gamepad2 className="h-4 w-4" /> },
    { href: "/instructor/evaluations", label: "Evaluations", icon: <ClipboardCheck className="h-4 w-4" /> },
  ],
  Trainee: [
    { href: "/trainee", label: "Dashboard", icon: <LayoutDashboard className="h-4 w-4" /> },
    { href: "/trainee/sessions", label: "Sessions", icon: <Gamepad2 className="h-4 w-4" /> },
    { href: "/trainee/evaluations", label: "Evaluations", icon: <FileText className="h-4 w-4" /> },
  ],
}

export function Sidebar() {
  const pathname = usePathname()
  const { user, logout } = useAuth()
  const menu = user ? roleMenus[user.role] ?? [] : []

  return (
    <aside className="flex h-screen w-60 flex-col border-r border-zinc-200 bg-white">
      <div className="flex h-14 items-center border-b border-zinc-200 px-6 font-semibold text-zinc-900">
        Sim Platform
      </div>

      <nav className="flex-1 space-y-1 p-3">
        {menu.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
              pathname === item.href || (item.href !== `/${user?.role?.toLowerCase()}` && pathname.startsWith(item.href))
                ? "bg-zinc-100 text-zinc-900"
                : "text-zinc-500 hover:bg-zinc-50 hover:text-zinc-900",
            )}
          >
            {item.icon}
            {item.label}
          </Link>
        ))}
      </nav>

      <div className="border-t border-zinc-200 p-3">
        <div className="mb-2 px-3 text-xs text-zinc-400">
          {user?.name} ({user?.role})
        </div>
        <button
          onClick={logout}
          className="flex w-full items-center gap-3 rounded-md px-3 py-2 text-sm font-medium text-zinc-500 transition-colors hover:bg-zinc-50 hover:text-zinc-900"
        >
          <LogOut className="h-4 w-4" />
          Sign out
        </button>
      </div>
    </aside>
  )
}
