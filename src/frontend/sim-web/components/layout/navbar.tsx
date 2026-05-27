"use client"

import { useAuth } from "@/lib/auth-context"

export function Navbar({ title }: { title: string }) {
  const { user } = useAuth()

  return (
    <header className="flex h-14 items-center justify-between border-b border-zinc-200 bg-white px-6">
      <h1 className="text-lg font-semibold text-zinc-900">{title}</h1>
      <div className="text-sm text-zinc-500">
        {user?.name} <span className="text-zinc-300">·</span> {user?.email}
      </div>
    </header>
  )
}
