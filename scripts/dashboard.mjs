import { readFileSync, readdirSync, statSync, writeFileSync } from "fs"
import { join, relative, sep } from "path"
import { fileURLToPath } from "url"
import { execSync } from "child_process"

const ROOT = fileURLToPath(new URL("..", import.meta.url))

// ── Helpers ──────────────────────────────────────────────

function read(p) {
  try { return readFileSync(join(ROOT, p), "utf-8") } catch { return "" }
}

const EXCLUDE = ["node_modules", ".next", ".git", "bin", "obj"]
function glob(dir, pattern) {
  const entries = []
  function walk(d) {
    let items
    try { items = readdirSync(join(ROOT, d)) } catch { return }
    for (const e of items) {
      if (EXCLUDE.includes(e)) continue
      const full = join(d, e)
      try {
        if (statSync(join(ROOT, full)).isDirectory()) walk(full)
        else if (e.match(pattern)) entries.push(full)
      } catch {}
    }
  }
  walk(dir)
  return entries
}

function fileInfo(p) {
  const s = statSync(join(ROOT, p))
  return { path: p, size: s.size, mtime: s.mtime }
}

function escapeHtml(s) { return s.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;") }

function sizeStr(bytes) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1048576) return `${(bytes/1024).toFixed(1)} KB`
  return `${(bytes/1048576).toFixed(1)} MB`
}

function ago(d) {
  const sec = Math.floor((Date.now() - new Date(d)) / 1000)
  if (sec < 60) return `${sec}s ago`
  const min = Math.floor(sec / 60)
  if (min < 60) return `${min}m ago`
  const hr = Math.floor(min / 60)
  if (hr < 24) return `${hr}h ago`
  return `${Math.floor(hr / 24)}d ago`
}

// ── Collect data ─────────────────────────────────────────

// Memory tree
const memDirs = [".memory/active-context.md", ".memory/sessions", ".memory/memories/decisions",
                 ".memory/memories/bugs", ".memory/memories/learnings", ".memory/memories/architecture"]
const memFiles = memDirs.flatMap(d => {
  if (d.endsWith(".md")) return [d]
  return glob(d, /\.md$/)
})

const memoryData = memFiles.map(fileInfo).sort((a,b) => b.mtime - a.mtime)

// Active context
const ctx = read(".memory/active-context.md")

// Parse checklist items (supports both "- [x]" and numbered "1. ✅" formats)
const checklist = []
for (const line of ctx.split("\n")) {
  const m1 = line.match(/^(- \[([ x])\])\s+(.+)$/)
  const m2 = line.match(/^\d+\.\s+([✅⬜])\s+(.+)$/)
  if (m1) checklist.push({ done: m1[2] === "x", text: m1[3] })
  else if (m2) checklist.push({ done: m2[1] === "✅", text: m2[2] })
}

// Parse open tasks (bottom of active-context uses "- [x]" format)
const tasks = []
for (const line of ctx.split("\n")) {
  const m = line.match(/^- \[([ x])\]\s+(.+)$/)
  if (m) tasks.push({ done: m[1] === "x", text: m[2] })
}

// Parse decisions
const decisions = []
let inDecisions = false
for (const line of ctx.split("\n")) {
  if (line.startsWith("## Active Decisions")) { inDecisions = true; continue }
  if (inDecisions && line.startsWith("##")) break
  if (inDecisions && line.startsWith("- ")) decisions.push(line.slice(2))
}

// Parse risks
const risks = []
let inRisks = false
for (const line of ctx.split("\n")) {
  if (line.startsWith("## Current Risks")) { inRisks = true; continue }
  if (inRisks && line.startsWith("##")) break
  if (inRisks && line.startsWith("- ") && !line.match(/^-\s+\[/)) risks.push(line.slice(2))
}

// Architecture review — parse resolution table
const archReview = read("docs/99-reference/architecture-review.md")
const resolutions = []
for (const line of archReview.split("\n")) {
  const m = line.match(/^\| (\d+) \| (.+?) \| (.+) \|$/)
  if (m && m[1] !== "#" && m[1] !== "---" && !m[1].includes("Issue")) {
    const status = m[3].includes("✅") ? "done" : m[3].includes("⬜") ? "pending" : "info"
    resolutions.push({ num: m[1], issue: m[2], detail: m[3], status })
  }
}

// Git log
let gitLog = ""
try {
  gitLog = execSync("git log --oneline --stat -15", { cwd: ROOT, encoding: "utf-8", maxBuffer: 1024*1024 })
} catch { gitLog = "(not a git repository)" }

// ── OpenCode Telemetry ────────────────────────────────────
const telemetryRaw = read(".opencode/telemetry.jsonl")
const telemetry = telemetryRaw ? telemetryRaw.trim().split("\n").filter(Boolean).map(l => {
  try { return JSON.parse(l) } catch { return null }
}).filter(Boolean) : []

const telSessions = telemetry.filter(e => e.t === "session_start")
const chatReqs = telemetry.filter(e => e.t === "chat_request")
const toolExecs = telemetry.filter(e => e.t === "tool_exec")

// Group tool execs by tool name with stats
const toolStats = {}
for (const te of toolExecs) {
  const name = te.data.tool
  if (!toolStats[name]) toolStats[name] = { calls: 0, errors: 0, totalDuration: 0 }
  toolStats[name].calls++
  if (te.data.hasError) toolStats[name].errors++
  if (te.data.duration) toolStats[name].totalDuration += te.data.duration
}

// Model usage stats
const modelStats = {}
for (const cr of chatReqs) {
  const m = cr.data.model || "unknown"
  if (!modelStats[m]) modelStats[m] = { requests: 0, totalChars: 0 }
  modelStats[m].requests++
  modelStats[m].totalChars += cr.data.inputChars || 0
}

// Recent activity timeline (last 50 events)
const recentActivity = [...telemetry].sort((a, b) => b.ts - a.ts).slice(0, 50)

// File counts by layer
const layers = {
  backend: glob("src/backend", /\.cs$/),
  frontend: glob("src/frontend/sim-web", /\.(ts|tsx)$/),
  godot: glob("simulation/driving-sim", /\.(cs|tscn|gd)$/),
  docs: glob("docs", /\.md$/),
}
const layerStats = Object.entries(layers).map(([name, files]) => ({
  name, count: files.length,
  size: files.reduce((s, f) => s + statSync(join(ROOT, f)).size, 0)
}))

// Session files
const sessions = memFiles.filter(f => f.startsWith(".memory/sessions") && !f.endsWith("README.md")).map(fileInfo)

// Agent definitions
const agents = {}
const agentFiles = glob(".opencode/agents", /\.md$/)
for (const af of agentFiles) {
  const content = read(af)
  const name = af.replace(".opencode/agents/", "").replace(".md", "")
  agents[name] = content.split("\n").slice(0, 3).map(l => l.replace(/^#+\s*/, "")).filter(Boolean)[0] || name
}

// ── Generate HTML ────────────────────────────────────────

const totalMemSize = memoryData.reduce((s, f) => s + f.size, 0)
const doneCount = checklist.filter(c => c.done).length
const totalCount = checklist.length

const html = `<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Project Dashboard</title>
<style>
  :root {
    --bg: #0f172a; --surface: #1e293b; --border: #334155;
    --text: #e2e8f0; --text-dim: #94a3b8; --accent: #38bdf8;
    --green: #22c55e; --yellow: #eab308; --red: #ef4444;
    --orange: #f97316;
  }
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: system-ui, -apple-system, sans-serif; background: var(--bg); color: var(--text); padding: 24px; }
  h1 { font-size: 24px; font-weight: 700; margin-bottom: 4px; }
  h2 { font-size: 16px; font-weight: 600; color: var(--accent); margin-bottom: 12px;
       padding-bottom: 6px; border-bottom: 1px solid var(--border); }
  h3 { font-size: 14px; font-weight: 600; margin-bottom: 8px; }
  .subtitle { color: var(--text-dim); font-size: 13px; margin-bottom: 24px; }
  .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(380px, 1fr)); gap: 16px; }
  .card { background: var(--surface); border: 1px solid var(--border); border-radius: 10px;
          padding: 16px; }
  .stat-row { display: flex; gap: 12px; flex-wrap: wrap; margin-bottom: 16px; }
  .stat { background: var(--surface); border: 1px solid var(--border); border-radius: 8px;
          padding: 12px 16px; flex: 1; min-width: 120px; }
  .stat .val { font-size: 22px; font-weight: 700; }
  .stat .lbl { font-size: 11px; color: var(--text-dim); text-transform: uppercase; letter-spacing: .5px; }
  .badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 11px; font-weight: 600; }
  .badge.done { background: #166534; color: var(--green); }
  .badge.pending { background: #713f12; color: var(--yellow); }
  .badge.info { background: #1e3a5f; color: var(--accent); }
  .badge.framework { background: #4a1942; color: #d946ef; }
  .badge.infra { background: #1e3a5f; color: var(--accent); }
  .badge.be { background: #166534; color: var(--green); }
  .badge.fe { background: #4a1942; color: #d946ef; }
  table { width: 100%; border-collapse: collapse; font-size: 13px; }
  th, td { padding: 6px 8px; text-align: left; border-bottom: 1px solid var(--border); }
  th { font-size: 11px; text-transform: uppercase; letter-spacing: .5px; color: var(--text-dim); }
  .file-path { font-family: ui-monospace, monospace; font-size: 12px; color: var(--accent); }
  .file-size { font-family: ui-monospace, monospace; font-size: 12px; color: var(--text-dim); }
  .progress-bar { height: 8px; background: var(--border); border-radius: 999px; overflow: hidden; margin: 8px 0 4px; }
  .progress-fill { height: 100%; background: var(--accent); border-radius: 999px; }
  .progress-fill.green { background: var(--green); }
  .progress-label { display: flex; justify-content: space-between; font-size: 12px; }
  .flex { display: flex; align-items: center; gap: 8px; }
  .mt-8 { margin-top: 8px; }
  .mt-16 { margin-top: 16px; }
  .mb-16 { margin-bottom: 16px; }
  .gap-8 { gap: 8px; }
  .pre-wrap { white-space: pre-wrap; word-break: break-all; font-family: ui-monospace, monospace; font-size: 12px; line-height: 1.5; }
  .commit { font-family: ui-monospace, monospace; font-size: 12px; padding: 6px 0; border-bottom: 1px solid var(--border); }
  .commit:last-child { border: none; }
  .commit-hash { color: var(--accent); cursor: default; }
  .commit-msg { font-weight: 500; }
  .commit-stats { color: var(--text-dim); font-size: 11px; }
  .checkbox { display: flex; align-items: flex-start; gap: 8px; font-size: 13px; padding: 3px 0; }
  .checkbox.done { color: var(--text-dim); text-decoration: line-through; }
  .checkbox input { margin-top: 3px; accent-color: var(--accent); }
  .risk { font-size: 13px; padding: 8px; background: rgba(239,68,68,.08); border-left: 3px solid var(--red); border-radius: 4px; margin-bottom: 6px; }
  .layer-dot { display: inline-block; width: 10px; height: 10px; border-radius: 50%; margin-right: 6px; }
  .agent-tag { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; }
  .agent-lead { background: #1e3a5f; color: var(--accent); }
  .agent-imp { background: #166534; color: var(--green); }
  .agent-rev { background: #4a1942; color: #d946ef; }
</style>
</head>
<body>

<h1>🚗 Simulation Project Dashboard</h1>
<div class="subtitle">Last generated: ${new Date().toLocaleString()} &middot; Root: ${ROOT}</div>

<div class="stat-row">
  <div class="stat"><div class="val">${layerStats.reduce((s, l) => s + l.count, 0)}</div><div class="lbl">Source Files</div></div>
  <div class="stat"><div class="val">${layerStats.reduce((s, l) => s + l.size, 0) / 1024 | 0} KB</div><div class="lbl">Code Size</div></div>
  <div class="stat"><div class="val">${memoryData.length}</div><div class="lbl">Memory Files</div></div>
  <div class="stat"><div class="val">${sizeStr(totalMemSize)}</div><div class="lbl">Memory Size</div></div>
  <div class="stat"><div class="val">${doneCount}/${totalCount}</div><div class="lbl">MVP Checklist</div></div>
  <div class="stat"><div class="val">${resolutions.filter(r => r.status === "done").length}/${resolutions.length}</div><div class="lbl">Arch Review Items</div></div>
  <div class="stat"><div class="val">${telemetry.length}</div><div class="lbl">Telemetry Events</div></div>
</div>

<div class="grid">

<!-- ─── AGENTS ─── -->
<div class="card">
  <h2>🤖 Agents</h2>
  ${Object.entries(agents).map(([k,v]) =>
    `<div class="flex mt-8"><span class="agent-tag agent-${k === "lead" ? "lead" : k === "implementer" ? "imp" : "rev"}">${k}</span><span>${escapeHtml(v)}</span></div>`
  ).join("")}
  <div class="file-path mt-16">Files: ${agentFiles.join(", ") || "none"}</div>
</div>

<!-- ─── LAYERS ─── -->
<div class="card">
  <h2>📁 Code Layers</h2>
  ${layerStats.map(l =>
    `<div class="flex"><span class="layer-dot" style="background:${l.name==='backend'?'var(--green)':l.name==='frontend'?'#d946ef':l.name==='godot'?'var(--orange)':'var(--accent)'}"></span><b>${l.name}</b> <span class="file-size">${l.count} files · ${sizeStr(l.size)}</span></div>`
  ).join("")}
</div>

<!-- ─── MVP CHECKLIST ─── -->
<div class="card">
  <h2>📋 MVP Progress</h2>
  <div class="progress-label"><span>${doneCount}/${totalCount} done</span><span>${(doneCount/totalCount*100)|0}%</span></div>
  <div class="progress-bar"><div class="progress-fill green" style="width:${(doneCount/totalCount*100)|0}%"></div></div>
  ${checklist.map(c =>
    `<div class="checkbox${c.done ? " done" : ""}"><input type="checkbox"${c.done ? " checked" : ""} disabled>${escapeHtml(c.text)}</div>`
  ).join("")}
</div>

<!-- ─── ACTIVE DECISIONS ─── -->
<div class="card">
  <h2>📐 Active Decisions</h2>
  ${decisions.map(d => `<div class="flex gap-8 mt-8"><span style="color:var(--accent)">→</span>${escapeHtml(d)}</div>`).join("")}
</div>

<!-- ─── RISKS ─── -->
<div class="card">
  <h2>⚠️ Risks</h2>
  ${risks.map(r => `<div class="risk">${escapeHtml(r)}</div>`).join("")}
</div>

<!-- ─── MEMORY STATUS ─── -->
<div class="card" style="grid-column: span 2;">
  <h2>🧠 Memory System</h2>
  <table>
    <tr><th>File</th><th>Section</th><th>Size</th><th>Modified</th></tr>
    ${memoryData.map(f => {
      const section = f.path.includes("/decisions/") ? "decisions"
        : f.path.includes("/bugs/") ? "bugs"
        : f.path.includes("/learnings/") ? "learnings"
        : f.path.includes("/architecture/") ? "architecture"
        : f.path.includes("/sessions/") ? "sessions"
        : "context"
      const color = section === "decisions" ? "var(--accent)" : section === "bugs" ? "var(--red)" : section === "learnings" ? "#d946ef" : section === "architecture" ? "var(--green)" : section === "sessions" ? "var(--orange)" : "var(--text)"
      return `<tr><td class="file-path">${f.path.replace(/^.memory\//, "")}</td><td><span class="badge" style="background:${color}22;color:${color}">${section}</span></td><td class="file-size">${sizeStr(f.size)}</td><td class="file-size">${ago(f.mtime)}</td></tr>`
    }).join("")}
  </table>
  <div class="file-size mt-8">${memoryData.length} files · ${sizeStr(totalMemSize)} total</div>
</div>

<!-- ─── ARCHITECTURE REVIEW RESOLUTION ─── -->
<div class="card" style="grid-column: span 2;">
  <h2>🔍 Architecture Review — Resolution</h2>
  <table>
    <tr><th>#</th><th>Issue</th><th>Status</th></tr>
    ${resolutions.map(r =>
      `<tr><td>${r.num}</td><td>${escapeHtml(r.issue)}</td><td><span class="badge ${r.status}">${r.status === "done" ? "✅ Resolved" : r.status === "pending" ? "⬜ Pending" : escapeHtml(r.detail)}</span></td></tr>`
    ).join("")}
  </table>
</div>

<!-- ─── RECENT SESSIONS ─── -->
<div class="card">
  <h2>📝 Recent Sessions</h2>
  ${sessions.length ? sessions.slice(0, 5).map(s =>
    `<div class="flex mt-8"><span class="badge" style="background:var(--orange);color:#111">session</span><span class="file-path">${s.path.replace(/^.memory\/sessions\//, "")}</span><span class="file-size">${sizeStr(s.size)} · ${ago(s.mtime)}</span></div>`
  ).join("") : "<div class='text-dim'>No sessions yet</div>"}
</div>

<!-- ─── GIT LOG ─── -->
<div class="card" style="grid-column: span 2;">
  <h2>📜 Git History</h2>
  <div class="pre-wrap">${escapeHtml(gitLog)}</div>
</div>

<!-- ─── OPENTELEMETRY ─── -->
<div class="card" style="grid-column: span 2;">
  <h2>📡 OpenCode Telemetry</h2>
  ${telemetry.length === 0 ? '<div class="risk">No telemetry data yet. Restart opencode to start collecting.</div>' : `
  <div class="stat-row" style="margin-bottom:12px">
    <div class="stat"><div class="val">${telSessions.length}</div><div class="lbl">Sessions</div></div>
    <div class="stat"><div class="val">${chatReqs.length}</div><div class="lbl">Chat Requests</div></div>
    <div class="stat"><div class="val">${toolExecs.length}</div><div class="lbl">Tool Executions</div></div>
    <div class="stat"><div class="val">${(toolExecs.filter(t => t.data.hasError).length / (toolExecs.length || 1) * 100).toFixed(0)}%</div><div class="lbl">Error Rate</div></div>
  </div>
  <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
    <div>
      <h3>Models Used</h3>
      <table>
        <tr><th>Model</th><th>Requests</th><th>Est. Input</th></tr>
        ${Object.entries(modelStats).sort((a,b) => b[1].requests - a[1].requests).map(([model, st]) =>
          `<tr><td class="file-path">${escapeHtml(model)}</td><td>${st.requests}</td><td class="file-size">${sizeStr(st.totalChars)}</td></tr>`
        ).join("")}
      </table>
    </div>
    <div>
      <h3>Top Tools</h3>
      <table>
        <tr><th>Tool</th><th>Calls</th><th>Avg (ms)</th><th>Errors</th></tr>
        ${Object.entries(toolStats).sort((a,b) => b[1].calls - a[1].calls).slice(0, 10).map(([tool, st]) =>
          `<tr><td class="file-path">${escapeHtml(tool)}</td><td>${st.calls}</td><td class="file-size">${st.calls ? (st.totalDuration / st.calls).toFixed(0) : "-"}</td><td>${st.errors}</td></tr>`
        ).join("")}
      </table>
    </div>
  </div>
  <div class="mt-16">
    <h3>Recent Activity</h3>
    <div style="max-height:300px;overflow-y:auto">
      ${recentActivity.slice(0, 30).map(e => {
        const icon = e.t === "session_start" ? "🟢" : e.t === "chat_request" ? "💬" : e.t === "tool_exec" ? (e.data.hasError ? "❌" : "🔧") : "❓"
        const label = e.t === "session_start" ? "Session start"
          : e.t === "chat_request" ? `${e.data.model || "?"} · ${e.data.messageCount || "?"} msgs`
          : e.t === "tool_exec" ? `${e.data.tool} ${e.data.duration ? "· "+e.data.duration+"ms" : ""}${e.data.hasError ? " · FAIL" : ""}`
          : e.t
        return `<div style="display:flex;gap:8px;font-size:12px;padding:2px 0;border-bottom:1px solid var(--border)"><span>${icon}</span><span class="file-size">${ago(e.ts)}</span><span>${escapeHtml(label)}</span></div>`
      }).join("")}
    </div>
  </div>
  `}
  <div class="file-size mt-8">Source: .opencode/telemetry.jsonl · ${telemetry.length} events</div>
</div>

</div>
</body>
</html>`

// ── Write output ─────────────────────────────────────────

writeFileSync(join(ROOT, "dashboard.html"), html)
console.log("✅ Dashboard written to dashboard.html")
console.log(`   ${memoryData.length} memory files, ${layerStats.reduce((s,l)=>s+l.count,0)} source files, ${doneCount}/${totalCount} MVP items done`)
