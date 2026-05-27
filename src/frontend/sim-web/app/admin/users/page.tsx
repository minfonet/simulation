"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select } from "@/components/ui/select"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"

interface OrgOption {
  id: string
  name: string
}

interface UserInfo {
  id: string
  email: string
  name: string
  role: string
  createdAt: string
}

export default function UsersPage() {
  const [orgs, setOrgs] = useState<OrgOption[]>([])
  const [selectedOrg, setSelectedOrg] = useState("")
  const [users, setUsers] = useState<UserInfo[]>([])

  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [name, setName] = useState("")
  const [role, setRole] = useState("Trainee")

  useEffect(() => {
    api.get<OrgOption[]>("/api/admin/organizations").then((data) => {
      setOrgs(data)
      if (data.length > 0) setSelectedOrg(data[0].id)
    })
  }, [])

  useEffect(() => {
    if (selectedOrg) {
      api
        .get<UserInfo[]>(`/api/admin/organizations/${selectedOrg}/users`)
        .then(setUsers)
    }
  }, [selectedOrg])

  async function inviteUser() {
    if (!selectedOrg || !email || !password || !name) return
    await api.post(`/api/admin/organizations/${selectedOrg}/users`, {
      email,
      password,
      name,
      role,
      organizationId: selectedOrg,
    })
    setEmail("")
    setPassword("")
    setName("")
    const updated = await api.get<UserInfo[]>(`/api/admin/organizations/${selectedOrg}/users`)
    setUsers(updated)
  }

  return (
    <div>
      <Navbar title="Users" />
      <div className="p-6 space-y-6">
        <div className="flex gap-3 items-end">
          <div className="space-y-1">
            <label className="text-sm font-medium">Organization</label>
            <Select value={selectedOrg} onChange={(e) => setSelectedOrg(e.target.value)}>
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </Select>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardContent className="pt-6 space-y-3">
              <Input placeholder="Email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
              <Input placeholder="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
              <Input placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} />
              <Select value={role} onChange={(e) => setRole(e.target.value)}>
                <option value="Admin">Admin</option>
                <option value="Instructor">Instructor</option>
                <option value="Trainee">Trainee</option>
              </Select>
              <Button onClick={inviteUser} className="w-full">
                Invite User
              </Button>
            </CardContent>
          </Card>

          <div className="space-y-3">
            {users.map((u) => (
              <Card key={u.id}>
                <CardContent className="flex items-center justify-between py-3">
                  <div>
                    <p className="font-medium">{u.name}</p>
                    <p className="text-sm text-zinc-500">{u.email}</p>
                  </div>
                  <Badge>{u.role}</Badge>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
