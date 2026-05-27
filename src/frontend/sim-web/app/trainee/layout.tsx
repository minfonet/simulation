import { Sidebar } from "@/components/layout/sidebar"

export default function TraineeLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex">
      <Sidebar />
      <main className="flex-1 bg-zinc-50">{children}</main>
    </div>
  )
}
