import { useState, useCallback } from "react";
import {
  LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, Area, AreaChart,
  PieChart, Pie, Cell,
} from "recharts";
import {
  Home, Search, Bell, ChevronDown, ChevronRight, Star,
  Package, BarChart3, DollarSign, Users, Monitor, Clock,
  Filter, Grid3X3, List, FileText, ArrowUpRight,
  ArrowDownRight, Building2, RefreshCw,
  FileSpreadsheet, ChevronLeft, Eye,
} from "lucide-react";

const C = {
  pri: "#005758", priH: "#006d6e", priL: "#e8f4f4",
  acc: "#00b4b6",
  bg: "#f5f7f8", card: "#ffffff",
  border: "#e2e8ea", borderL: "#eef2f3",
  text: "#1a2e2f", textSec: "#5c7576", textTer: "#94aeb0",
  green: "#0d9668", red: "#dc4a4a",
  shadow: "0 1px 3px rgba(0,87,88,0.06), 0 1px 2px rgba(0,87,88,0.04)",
  shadowMd: "0 4px 12px rgba(0,87,88,0.08), 0 2px 4px rgba(0,87,88,0.04)",
};

const DEPTS = [
  { id: "procurement", label: "採購部", icon: Package, count: 24, subs: ["成本分析", "供應商管理", "訂單追蹤", "績效報告"] },
  { id: "sales", label: "業務部", icon: BarChart3, count: 31, subs: ["客戶分析", "業績排名", "區域統計", "產品銷售"] },
  { id: "finance", label: "財務部", icon: DollarSign, count: 18, subs: ["收支分析", "預算管理", "帳齡分析", "資金流向"] },
  { id: "hr", label: "人資部", icon: Users, count: 12, subs: ["出勤管理", "薪資統計", "人力配置", "招募進度"] },
  { id: "it", label: "資訊部", icon: Monitor, count: 22, subs: ["系統監控", "資安報告", "設備管理", "服務台統計"] },
];

const REVENUE = [
  { m: "10月", cur: 10.2, prev: 9.8, budget: 10.5 },
  { m: "11月", cur: 11.1, prev: 10.3, budget: 10.8 },
  { m: "12月", cur: 12.5, prev: 11.0, budget: 11.2 },
  { m: "1月", cur: 10.8, prev: 9.5, budget: 11.0 },
  { m: "2月", cur: 11.6, prev: 10.1, budget: 11.3 },
  { m: "3月", cur: 12.8, prev: 10.9, budget: 11.5 },
];

const DEPT_CMP = [
  { name: "採購部", actual: 4.2, target: 4.5 },
  { name: "業務部", actual: 5.8, target: 5.2 },
  { name: "財務部", actual: 1.2, target: 1.3 },
  { name: "人資部", actual: 0.8, target: 0.9 },
  { name: "資訊部", actual: 1.6, target: 1.8 },
];

const PIE = [
  { name: "原料", value: 62 }, { name: "包材", value: 18 },
  { name: "設備", value: 12 }, { name: "其他", value: 8 },
];
const PIE_C = [C.pri, C.acc, C.priH, C.textTer];

const COST = [
  { m: "10月", raw: 2.5, pkg: 0.8, equip: 0.6, other: 0.4 },
  { m: "11月", raw: 2.7, pkg: 0.7, equip: 0.5, other: 0.3 },
  { m: "12月", raw: 2.9, pkg: 0.9, equip: 0.7, other: 0.5 },
  { m: "1月", raw: 2.6, pkg: 0.8, equip: 0.4, other: 0.4 },
  { m: "2月", raw: 2.8, pkg: 0.7, equip: 0.6, other: 0.3 },
  { m: "3月", raw: 2.6, pkg: 0.8, equip: 0.5, other: 0.3 },
];

const RPT = {
  procurement: [
    { name: "月度採購成本分析", desc: "各類物料採購金額與趨勢分析", cat: "成本分析", updated: "03/20", fav: true },
    { name: "供應商交貨準時率", desc: "各供應商交期達成率統計", cat: "供應商管理", updated: "03/18", fav: false },
    { name: "採購訂單狀態追蹤", desc: "進行中、已完成、逾期訂單一覽", cat: "訂單追蹤", updated: "03/20", fav: true },
    { name: "物料價格波動分析", desc: "主要原料歷史價格走勢與預測", cat: "成本分析", updated: "03/17", fav: false },
    { name: "供應商績效評比", desc: "品質、交期、配合度綜合評分", cat: "供應商管理", updated: "03/15", fav: false },
    { name: "採購預算執行率", desc: "各部門採購預算使用進度", cat: "績效報告", updated: "03/16", fav: true },
    { name: "進口關稅成本報表", desc: "進口物料之關稅與運費分析", cat: "成本分析", updated: "03/14", fav: false },
    { name: "合約到期提醒清單", desc: "即將到期之供應商合約一覽", cat: "供應商管理", updated: "03/13", fav: false },
    { name: "緊急採購統計表", desc: "非計畫性採購次數與金額統計", cat: "訂單追蹤", updated: "03/12", fav: false },
  ],
  sales: [
    { name: "客戶銷售排名", desc: "依營收排序之客戶清單", cat: "客戶分析", updated: "03/20", fav: true },
    { name: "區域營收分佈", desc: "各區域銷售金額與占比", cat: "區域統計", updated: "03/19", fav: true },
    { name: "產品銷售趨勢", desc: "各產品線月銷售走勢", cat: "產品銷售", updated: "03/18", fav: false },
    { name: "業務員績效報表", desc: "各業務員目標達成率", cat: "業績排名", updated: "03/17", fav: false },
    { name: "新客戶開發統計", desc: "新客戶數量與首單分析", cat: "客戶分析", updated: "03/16", fav: true },
    { name: "報價成交轉換率", desc: "報價單轉成交之比率追蹤", cat: "業績排名", updated: "03/15", fav: false },
  ],
  finance: [
    { name: "應收帳款帳齡表", desc: "客戶應收帳款帳齡分佈", cat: "帳齡分析", updated: "03/20", fav: true },
    { name: "預算執行率追蹤", desc: "年度預算各科目執行進度", cat: "預算管理", updated: "03/19", fav: true },
    { name: "資金日報表", desc: "每日資金餘額與異動明細", cat: "資金流向", updated: "03/20", fav: false },
    { name: "損益月報", desc: "本月收入、成本、費用彙總", cat: "收支分析", updated: "03/18", fav: false },
  ],
  hr: [
    { name: "人員出勤統計", desc: "各部門出勤率與異常統計", cat: "出勤管理", updated: "03/20", fav: true },
    { name: "加班時數分析", desc: "各部門加班時數趨勢", cat: "出勤管理", updated: "03/19", fav: false },
    { name: "人力結構分析", desc: "年齡、學歷、年資分佈", cat: "人力配置", updated: "03/15", fav: false },
  ],
  it: [
    { name: "系統可用性報告", desc: "各系統 SLA 達成率", cat: "系統監控", updated: "03/20", fav: true },
    { name: "資安事件統計", desc: "安全事件次數與處理時效", cat: "資安報告", updated: "03/18", fav: false },
    { name: "IT 服務台統計", desc: "工單數量、處理時效、滿意度", cat: "服務台統計", updated: "03/17", fav: false },
  ],
};

const QUICK = [
  { dept: "採購部", did: "procurement", name: "月度採購成本分析", tag: "成本" },
  { dept: "業務部", did: "sales", name: "客戶銷售排名", tag: "銷售" },
  { dept: "財務部", did: "finance", name: "應收帳款帳齡表", tag: "財務" },
  { dept: "業務部", did: "sales", name: "區域營收分佈", tag: "銷售" },
  { dept: "採購部", did: "procurement", name: "供應商績效評比", tag: "供應商" },
  { dept: "人資部", did: "hr", name: "人員出勤統計", tag: "人資" },
  { dept: "資訊部", did: "it", name: "系統可用性報告", tag: "IT" },
  { dept: "財務部", did: "finance", name: "預算執行率追蹤", tag: "財務" },
];

const ROWS = [
  ["碳鋼板 SUS304", "台灣鋼鐵", "5,200 KG", "$85", "$442,000", -2.3],
  ["PE 包裝膜", "永豐塑膠", "12,000 M", "$12", "$144,000", 1.1],
  ["電子控制模組", "矽達科技", "800 PCS", "$320", "$256,000", -5.2],
  ["潤滑油 ISO VG68", "中油化學", "2,400 L", "$45", "$108,000", 0.8],
  ["銅線 Ø1.2mm", "嘉義銅業", "3,600 KG", "$210", "$756,000", -1.7],
  ["不鏽鋼螺栓 M10", "正達五金", "20,000 PCS", "$3.5", "$70,000", 2.4],
  ["矽膠密封圈", "聯合橡膠", "8,500 PCS", "$8", "$68,000", -0.9],
];

/* ── Shared ── */
const badge = (bg, color) => ({ fontSize: 10, fontWeight: 600, padding: "2px 8px", borderRadius: 10, background: bg, color, display: "inline-block" });

function Chip({ label, active, dashed, icon: Ic, onClick }) {
  return (
    <div onClick={onClick} style={{
      fontSize: 12, padding: "5px 12px", borderRadius: 6, cursor: "pointer",
      border: dashed ? `1.5px dashed ${C.border}` : `1px solid ${active ? C.pri : C.border}`,
      background: active ? C.priL : C.card, color: active ? C.pri : C.textSec,
      fontWeight: active ? 600 : 400, whiteSpace: "nowrap", userSelect: "none",
      display: "inline-flex", alignItems: "center", gap: 4,
    }}>
      {Ic && <Ic size={12} />}{label}
    </div>
  );
}

function KPI({ title, value, trend, note }) {
  const up = trend > 0;
  return (
    <div style={{ flex: 1, minWidth: 155, background: C.card, border: `1px solid ${C.border}`, borderRadius: 12, padding: "18px 20px", boxShadow: C.shadow }}>
      <div style={{ fontSize: 12, color: C.textSec, fontWeight: 500, marginBottom: 6 }}>{title}</div>
      <div style={{ fontSize: 26, fontWeight: 700, color: C.pri, letterSpacing: "-0.02em", lineHeight: 1 }}>{value}</div>
      <div style={{ display: "flex", alignItems: "center", gap: 4, marginTop: 8, fontSize: 12, fontWeight: 600, color: up ? C.green : C.red }}>
        {up ? <ArrowUpRight size={13} /> : <ArrowDownRight size={13} />}
        {up ? "+" : ""}{trend}%
        {note && <span style={{ color: C.textTer, fontWeight: 400, marginLeft: 2 }}>{note}</span>}
      </div>
    </div>
  );
}

function HoverCard({ children, onClick, style: sx = {} }) {
  return (
    <div onClick={onClick} style={{
      background: C.card, border: `1px solid ${C.border}`, borderRadius: 10,
      boxShadow: C.shadow, cursor: "pointer", transition: "all 0.15s", ...sx,
    }}
      onMouseEnter={e => { e.currentTarget.style.boxShadow = C.shadowMd; e.currentTarget.style.borderColor = C.pri; }}
      onMouseLeave={e => { e.currentTarget.style.boxShadow = C.shadow; e.currentTarget.style.borderColor = C.border; }}
    >{children}</div>
  );
}

/* ── TopNav ── */
function TopNav({ company }) {
  return (
    <div style={{
      height: 56, background: C.pri, display: "flex", alignItems: "center",
      padding: "0 20px", gap: 12, color: "#fff", position: "sticky", top: 0, zIndex: 200,
      boxShadow: "0 2px 8px rgba(0,87,88,0.15)",
    }}>
      <div style={{ fontWeight: 800, fontSize: 17, display: "flex", alignItems: "center", gap: 10, minWidth: 185 }}>
        <div style={{ width: 30, height: 30, borderRadius: 8, background: "rgba(255,255,255,0.15)", display: "flex", alignItems: "center", justifyContent: "center", border: "1px solid rgba(255,255,255,0.1)" }}>
          <BarChart3 size={16} strokeWidth={2.5} />
        </div>
        ReportCenter
      </div>
      <div style={{ background: "rgba(255,255,255,0.1)", borderRadius: 6, padding: "6px 14px", fontSize: 13, display: "flex", alignItems: "center", gap: 8, cursor: "pointer", border: "1px solid rgba(255,255,255,0.12)" }}>
        <Building2 size={14} /><span style={{ fontWeight: 600 }}>{company}</span><ChevronDown size={12} style={{ opacity: 0.5 }} />
      </div>
      <div style={{ flex: 1, maxWidth: 400, background: "rgba(255,255,255,0.08)", borderRadius: 8, padding: "7px 14px", display: "flex", alignItems: "center", gap: 10, border: "1px solid rgba(255,255,255,0.1)", marginLeft: 8 }}>
        <Search size={14} style={{ opacity: 0.5 }} />
        <span style={{ opacity: 0.4, fontSize: 13 }}>搜尋報表、儀表板…</span>
        <span style={{ marginLeft: "auto", fontSize: 10, opacity: 0.35, border: "1px solid rgba(255,255,255,0.2)", borderRadius: 4, padding: "2px 6px" }}>⌘K</span>
      </div>
      <div style={{ flex: 1 }} />
      <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
        <div style={{ position: "relative", cursor: "pointer", padding: 4 }}>
          <Bell size={18} style={{ opacity: 0.8 }} />
          <div style={{ position: "absolute", top: 2, right: 2, width: 8, height: 8, borderRadius: "50%", background: "#ff6b6b", border: `2px solid ${C.pri}` }} />
        </div>
        <div style={{ width: 34, height: 34, borderRadius: "50%", background: "rgba(255,255,255,0.15)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 13, fontWeight: 700, cursor: "pointer", border: "1px solid rgba(255,255,255,0.1)" }}>CY</div>
      </div>
    </div>
  );
}

/* ── Sidebar ── */
function SideItem({ icon: Ic, label, active, onClick, count, arrow: Ar, bdg }) {
  return (
    <div onClick={onClick} style={{
      display: "flex", alignItems: "center", gap: 10, padding: "8px 10px", borderRadius: 8, cursor: "pointer",
      fontSize: 13, marginBottom: 1, fontWeight: active ? 600 : 400, color: active ? C.pri : C.text,
      background: active ? C.priL : "transparent", transition: "all 0.1s",
    }}
      onMouseEnter={e => { if (!active) e.currentTarget.style.background = "#f8fafa"; }}
      onMouseLeave={e => { if (!active) e.currentTarget.style.background = active ? C.priL : "transparent"; }}
    >
      <Ic size={16} strokeWidth={active ? 2.2 : 1.8} />
      <span style={{ flex: 1 }}>{label}</span>
      {count != null && <span style={badge(active ? "rgba(0,87,88,0.1)" : C.borderL, active ? C.pri : C.textTer)}>{count}</span>}
      {bdg && <span style={{ fontSize: 11, color: C.pri, fontWeight: 600 }}>{bdg}</span>}
      {Ar && <Ar size={12} style={{ color: C.textTer }} />}
    </div>
  );
}

function Sidebar({ page, dept, onNav, expanded, onExpand }) {
  return (
    <div style={{ width: 248, minWidth: 248, background: C.card, borderRight: `1px solid ${C.border}`, display: "flex", flexDirection: "column", height: "calc(100vh - 56px)", position: "sticky", top: 56, overflowY: "auto" }}>
      <div style={{ padding: "14px 14px 6px" }}>
        <SideItem icon={Home} label="首頁總覽" active={page === "home"} onClick={() => onNav("home")} />
      </div>
      <div style={{ padding: "10px 14px 4px" }}>
        <div style={{ fontSize: 10, fontWeight: 700, color: C.textTer, textTransform: "uppercase", letterSpacing: "0.08em", paddingLeft: 10 }}>部門報表</div>
      </div>
      <div style={{ padding: "0 14px" }}>
        {DEPTS.map(d => {
          const Ic = d.icon; const isA = dept === d.id && page !== "home"; const isE = expanded === d.id;
          return (
            <div key={d.id}>
              <SideItem icon={Ic} label={d.label} active={isA} count={d.count}
                arrow={isE ? ChevronDown : ChevronRight}
                onClick={() => { onExpand(isE ? null : d.id); onNav("dept", d.id); }} />
              {isE && (
                <div style={{ paddingLeft: 36, paddingBottom: 4 }}>
                  {d.subs.map(sub => (
                    <div key={sub} style={{ fontSize: 12, color: C.textSec, padding: "5px 10px", borderRadius: 5, cursor: "pointer", transition: "all 0.1s" }}
                      onMouseEnter={e => { e.currentTarget.style.background = C.priL; e.currentTarget.style.color = C.pri; }}
                      onMouseLeave={e => { e.currentTarget.style.background = "transparent"; e.currentTarget.style.color = C.textSec; }}
                    >{sub}</div>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </div>
      <div style={{ height: 1, background: C.border, margin: "10px 14px" }} />
      <div style={{ padding: "0 14px" }}>
        <div style={{ fontSize: 10, fontWeight: 700, color: C.textTer, textTransform: "uppercase", letterSpacing: "0.08em", paddingLeft: 10, marginBottom: 4 }}>快捷存取</div>
        <SideItem icon={Star} label="我的收藏" bdg="8" onClick={() => {}} />
        <SideItem icon={Clock} label="最近瀏覽" bdg="5" onClick={() => {}} />
      </div>
      <div style={{ flex: 1 }} />
      <div style={{ padding: "12px 24px", borderTop: `1px solid ${C.border}`, fontSize: 11, color: C.textTer, display: "flex", alignItems: "center", gap: 6 }}>
        <div style={{ width: 6, height: 6, borderRadius: "50%", background: C.green }} />報表總數: 107 份
      </div>
    </div>
  );
}

/* ══════ Page 1: Home ══════ */
function PageHome({ nav }) {
  return (
    <div style={{ padding: "24px 28px 40px", maxWidth: 1200 }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 22 }}>
        <div>
          <h1 style={{ fontSize: 22, fontWeight: 700, color: C.text, margin: 0 }}>營運總覽</h1>
          <div style={{ fontSize: 12, color: C.textTer, marginTop: 5 }}>台灣大宇集團 ・ 最後更新 2026/03/20 09:30</div>
        </div>
        <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
          <Chip label="本月" active icon={Filter} /><Chip label="區域: 全部" /><Chip label="部門: 全部" /><Chip icon={RefreshCw} label="更新" />
        </div>
      </div>

      <div style={{ display: "flex", gap: 14, marginBottom: 22, flexWrap: "wrap" }}>
        <KPI title="本月營收" value="$12.8M" trend={8.3} note="vs 上月" />
        <KPI title="採購成本" value="$4.2M" trend={-3.1} note="vs 上月" />
        <KPI title="毛利率" value="67.2%" trend={2.4} />
        <KPI title="訂單數" value="1,847" trend={12.6} note="vs 上月" />
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.6fr 1fr", gap: 16, marginBottom: 24 }}>
        <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 12, padding: "20px 24px", boxShadow: C.shadow }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 4 }}>
            <span style={{ fontWeight: 600, fontSize: 14, color: C.text }}>營收趨勢</span>
            <div style={{ display: "flex", gap: 4 }}><Chip label="月" active /><Chip label="季" /><Chip label="年" /></div>
          </div>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={REVENUE} margin={{ top: 10, right: 10, bottom: 0, left: -10 }}>
              <defs><linearGradient id="gP" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor={C.pri} stopOpacity={0.15} /><stop offset="100%" stopColor={C.pri} stopOpacity={0} /></linearGradient></defs>
              <CartesianGrid strokeDasharray="3 3" stroke={C.borderL} />
              <XAxis dataKey="m" tick={{ fontSize: 11, fill: C.textTer }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: C.textTer }} axisLine={false} tickLine={false} unit="M" />
              <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8, border: `1px solid ${C.border}` }} />
              <Area type="monotone" dataKey="cur" stroke={C.pri} strokeWidth={2.5} fill="url(#gP)" name="本年" />
              <Line type="monotone" dataKey="prev" stroke={C.acc} strokeWidth={1.5} strokeDasharray="4 3" dot={false} name="去年" />
              <Line type="monotone" dataKey="budget" stroke={C.textTer} strokeWidth={1} strokeDasharray="2 2" dot={false} name="預算" />
            </AreaChart>
          </ResponsiveContainer>
        </div>
        <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 12, padding: "20px 24px", boxShadow: C.shadow }}>
          <span style={{ fontWeight: 600, fontSize: 14, color: C.text }}>部門比較（百萬）</span>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={DEPT_CMP} margin={{ top: 16, right: 0, bottom: 0, left: -10 }} barGap={4}>
              <CartesianGrid strokeDasharray="3 3" stroke={C.borderL} />
              <XAxis dataKey="name" tick={{ fontSize: 10, fill: C.textTer }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: C.textTer }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8, border: `1px solid ${C.border}` }} />
              <Bar dataKey="actual" fill={C.pri} radius={[4, 4, 0, 0]} name="實際" barSize={20} />
              <Bar dataKey="target" fill={C.borderL} radius={[4, 4, 0, 0]} name="目標" barSize={20} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 14 }}>
        <span style={{ fontWeight: 600, fontSize: 15, color: C.text }}>快速存取</span>
        <span style={{ fontSize: 12, color: C.pri, fontWeight: 500, cursor: "pointer" }}>管理釘選 →</span>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 12 }}>
        {QUICK.map((r, i) => (
          <HoverCard key={i} onClick={() => nav("detail", r.did, r.name)} style={{ padding: "16px 18px", display: "flex", flexDirection: "column", gap: 6 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <span style={badge(C.priL, C.pri)}>{r.dept}</span>
              <Star size={13} style={{ color: "#f5a623" }} fill="#f5a623" />
            </div>
            <span style={{ fontSize: 13, fontWeight: 600, color: C.text, lineHeight: 1.3 }}>{r.name}</span>
            <span style={{ fontSize: 11, color: C.textTer }}>#{r.tag}</span>
          </HoverCard>
        ))}
      </div>
    </div>
  );
}

/* ══════ Page 2: Department ══════ */
function PageDept({ deptId, onDetail }) {
  const [view, setView] = useState("card");
  const [tab, setTab] = useState("all");
  const d = DEPTS.find(x => x.id === deptId) || DEPTS[0];
  const Ic = d.icon;
  const rpts = RPT[deptId] || RPT.procurement;
  const tabs = [{ id: "all", label: "全部", count: rpts.length }, ...d.subs.map((s, i) => ({ id: `s${i}`, label: s, count: rpts.filter(r => r.cat === s).length }))];
  const list = tab === "all" ? rpts : rpts.filter(r => { const t = tabs.find(x => x.id === tab); return t && r.cat === t.label; });

  return (
    <div style={{ padding: "24px 28px 40px", maxWidth: 1200 }}>
      <div style={{ fontSize: 12, color: C.textTer, marginBottom: 14 }}>首頁 / 部門報表 / <span style={{ color: C.pri, fontWeight: 600 }}>{d.label}</span></div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 20 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
          <div style={{ width: 40, height: 40, borderRadius: 10, background: C.priL, display: "flex", alignItems: "center", justifyContent: "center" }}><Ic size={20} style={{ color: C.pri }} /></div>
          <div><h1 style={{ fontSize: 20, fontWeight: 700, color: C.text, margin: 0 }}>{d.label}報表</h1><div style={{ fontSize: 12, color: C.textTer, marginTop: 2 }}>共 {d.count} 份報表</div></div>
        </div>
        <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
          <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 8, padding: "7px 12px", display: "flex", alignItems: "center", gap: 6, width: 220 }}>
            <Search size={14} style={{ color: C.textTer }} /><span style={{ fontSize: 12, color: C.textTer }}>搜尋部門報表…</span>
          </div>
          <div style={{ display: "flex", border: `1px solid ${C.border}`, borderRadius: 8, overflow: "hidden" }}>
            <div onClick={() => setView("card")} style={{ padding: "6px 10px", cursor: "pointer", background: view === "card" ? C.pri : C.card, color: view === "card" ? "#fff" : C.textTer }}><Grid3X3 size={14} /></div>
            <div onClick={() => setView("table")} style={{ padding: "6px 10px", cursor: "pointer", background: view === "table" ? C.pri : C.card, color: view === "table" ? "#fff" : C.textTer }}><List size={14} /></div>
          </div>
        </div>
      </div>

      <div style={{ display: "flex", borderBottom: `2px solid ${C.border}`, marginBottom: 16 }}>
        {tabs.map(t => (
          <div key={t.id} onClick={() => setTab(t.id)} style={{
            padding: "10px 16px", fontSize: 13, cursor: "pointer", marginBottom: -2,
            fontWeight: tab === t.id ? 600 : 400, color: tab === t.id ? C.pri : C.textSec,
            borderBottom: tab === t.id ? `2px solid ${C.pri}` : "2px solid transparent",
            display: "flex", alignItems: "center", gap: 6,
          }}>{t.label}{t.count > 0 && <span style={badge(tab === t.id ? C.priL : C.borderL, tab === t.id ? C.pri : C.textTer)}>{t.count}</span>}</div>
        ))}
      </div>

      <div style={{ display: "flex", gap: 8, marginBottom: 16 }}><Chip label="日期: 全部" icon={Filter} /><Chip label="排序: 最近更新" /></div>

      {list.length === 0 ? (
        <div style={{ textAlign: "center", padding: "60px 0", color: C.textTer }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>📭</div>
          <div style={{ fontSize: 14 }}>此分類目前沒有報表</div>
        </div>
      ) : view === "card" ? (
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 14 }}>
          {list.map((r, i) => (
            <HoverCard key={i} onClick={() => onDetail(r.name)} style={{ padding: "18px 20px", display: "flex", flexDirection: "column", gap: 8 }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <span style={badge(C.priL, C.pri)}>{r.cat}</span>
                <Star size={14} style={{ color: r.fav ? "#f5a623" : "#ddd" }} fill={r.fav ? "#f5a623" : "none"} />
              </div>
              <span style={{ fontSize: 14, fontWeight: 600, color: C.text }}>{r.name}</span>
              <span style={{ fontSize: 12, color: C.textSec, lineHeight: 1.4 }}>{r.desc}</span>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 4 }}>
                <span style={{ fontSize: 11, color: C.textTer }}>更新 {r.updated}</span>
                <span style={{ fontSize: 12, color: C.pri, fontWeight: 600, display: "flex", alignItems: "center", gap: 3 }}><Eye size={12} /> 檢視</span>
              </div>
            </HoverCard>
          ))}
        </div>
      ) : (
        <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 10, overflow: "hidden", boxShadow: C.shadow }}>
          <div style={{ display: "grid", gridTemplateColumns: "2.2fr 3fr 1fr 0.8fr 80px", padding: "10px 20px", background: C.bg, fontSize: 11, fontWeight: 700, color: C.textTer, textTransform: "uppercase", letterSpacing: "0.06em" }}>
            <span>報表名稱</span><span>說明</span><span>分類</span><span>更新</span><span>操作</span>
          </div>
          {list.map((r, i) => (
            <div key={i} onClick={() => onDetail(r.name)} style={{ display: "grid", gridTemplateColumns: "2.2fr 3fr 1fr 0.8fr 80px", padding: "12px 20px", borderTop: `1px solid ${C.borderL}`, fontSize: 13, alignItems: "center", cursor: "pointer", transition: "background 0.1s" }}
              onMouseEnter={e => e.currentTarget.style.background = "#fafcfc"}
              onMouseLeave={e => e.currentTarget.style.background = "transparent"}
            >
              <span style={{ fontWeight: 500, color: C.text, display: "flex", alignItems: "center", gap: 6 }}>
                {r.fav && <Star size={11} style={{ color: "#f5a623" }} fill="#f5a623" />}{r.name}
              </span>
              <span style={{ color: C.textSec, fontSize: 12 }}>{r.desc}</span>
              <span style={badge(C.priL, C.pri)}>{r.cat}</span>
              <span style={{ fontSize: 12, color: C.textTer }}>{r.updated}</span>
              <span style={{ fontSize: 12, color: C.pri, fontWeight: 600, display: "flex", alignItems: "center", gap: 3 }}><Eye size={12} /> 檢視</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

/* ══════ Page 3: Detail ══════ */
function PageDetail({ deptId, reportName, onBack }) {
  const d = DEPTS.find(x => x.id === deptId) || DEPTS[0];
  return (
    <div style={{ padding: "24px 28px 40px", maxWidth: 1200 }}>
      <div style={{ fontSize: 12, color: C.textTer, marginBottom: 14, display: "flex", alignItems: "center", gap: 4 }}>
        <span style={{ cursor: "pointer" }} onClick={() => onBack("home")}>首頁</span><span>/</span>
        <span style={{ cursor: "pointer" }} onClick={() => onBack("dept")}>{d.label}</span><span>/</span>
        <span style={{ color: C.pri, fontWeight: 600 }}>{reportName}</span>
      </div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 20 }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 4 }}>
            <span onClick={() => onBack("dept")} style={{ cursor: "pointer", color: C.textTer, display: "flex" }}><ChevronLeft size={18} /></span>
            <h1 style={{ fontSize: 20, fontWeight: 700, color: C.text, margin: 0 }}>{reportName}</h1>
          </div>
          <div style={{ fontSize: 12, color: C.textTer, paddingLeft: 26 }}>{d.label} ・ 成本分析 ・ 最後更新 2026/03/20 09:30</div>
        </div>
        <div style={{ display: "flex", gap: 8 }}><Chip label="收藏" icon={Star} /><Chip label="Excel" icon={FileSpreadsheet} /><Chip label="PDF" icon={FileText} /></div>
      </div>

      <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 10, padding: "12px 18px", marginBottom: 22, display: "flex", gap: 8, alignItems: "center", boxShadow: C.shadow, position: "sticky", top: 56, zIndex: 50 }}>
        <Chip label="2026/03" active /><Chip label="物料類別: 全部" /><Chip label="供應商: 全部" /><Chip label="+ 更多" dashed />
        <div style={{ flex: 1 }} /><span style={{ fontSize: 12, color: C.pri, fontWeight: 500, cursor: "pointer" }}>重設篩選</span>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.6fr 1fr", gap: 16, marginBottom: 20 }}>
        <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 12, padding: "20px 24px", boxShadow: C.shadow }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 8 }}>
            <span style={{ fontWeight: 600, fontSize: 14, color: C.text }}>各類物料採購金額趨勢</span>
            <div style={{ display: "flex", gap: 4 }}><Chip label="面積" active /><Chip label="長條" /><Chip label="堆疊" /></div>
          </div>
          <ResponsiveContainer width="100%" height={240}>
            <AreaChart data={COST} margin={{ top: 10, right: 10, bottom: 0, left: -10 }}>
              <defs>
                <linearGradient id="gR" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor={C.pri} stopOpacity={0.2}/><stop offset="100%" stopColor={C.pri} stopOpacity={0}/></linearGradient>
                <linearGradient id="gK" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor={C.acc} stopOpacity={0.2}/><stop offset="100%" stopColor={C.acc} stopOpacity={0}/></linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke={C.borderL} />
              <XAxis dataKey="m" tick={{ fontSize: 11, fill: C.textTer }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: C.textTer }} axisLine={false} tickLine={false} unit="M" />
              <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8, border: `1px solid ${C.border}` }} />
              <Area type="monotone" dataKey="raw" stroke={C.pri} strokeWidth={2} fill="url(#gR)" name="原料" />
              <Area type="monotone" dataKey="pkg" stroke={C.acc} strokeWidth={2} fill="url(#gK)" name="包材" />
              <Line type="monotone" dataKey="equip" stroke={C.priH} strokeWidth={1.5} dot={{ r: 3 }} name="設備" />
              <Line type="monotone" dataKey="other" stroke={C.textTer} strokeWidth={1} strokeDasharray="3 3" dot={false} name="其他" />
            </AreaChart>
          </ResponsiveContainer>
        </div>
        <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 12, padding: "20px 24px", boxShadow: C.shadow }}>
          <span style={{ fontWeight: 600, fontSize: 14, color: C.text }}>採購佔比</span>
          <ResponsiveContainer width="100%" height={200}>
            <PieChart><Pie data={PIE} cx="50%" cy="50%" innerRadius={50} outerRadius={80} paddingAngle={3} dataKey="value">
              {PIE.map((_, i) => <Cell key={i} fill={PIE_C[i]} />)}
            </Pie><Tooltip contentStyle={{ fontSize: 12, borderRadius: 8 }} /></PieChart>
          </ResponsiveContainer>
          <div style={{ display: "flex", gap: 12, justifyContent: "center", flexWrap: "wrap" }}>
            {PIE.map((p, i) => (<div key={i} style={{ display: "flex", alignItems: "center", gap: 5, fontSize: 11, color: C.textSec }}><div style={{ width: 8, height: 8, borderRadius: "50%", background: PIE_C[i] }} />{p.name} {p.value}%</div>))}
          </div>
        </div>
      </div>

      <div style={{ display: "flex", gap: 14, marginBottom: 22 }}>
        <KPI title="本月總採購" value="$4.2M" trend={-3.1} note="vs 上月" />
        <KPI title="最大項目" value="原料 62%" trend={1.2} />
        <KPI title="供應商數" value="47 家" trend={6.8} note="+3 家" />
        <KPI title="平均單價" value="-2.1%" trend={-2.1} />
      </div>

      <div style={{ background: C.card, border: `1px solid ${C.border}`, borderRadius: 12, overflow: "hidden", boxShadow: C.shadow }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "14px 20px", borderBottom: `1px solid ${C.border}` }}>
          <span style={{ fontWeight: 600, fontSize: 14, color: C.text }}>明細資料</span>
          <span style={{ fontSize: 11, color: C.textTer }}>顯示 1-20 / 共 156 筆</span>
        </div>
        <div style={{ display: "grid", gridTemplateColumns: "2fr 1.5fr 1fr 0.8fr 1fr 0.8fr", padding: "10px 20px", background: C.bg, fontSize: 11, fontWeight: 700, color: C.textTer, textTransform: "uppercase", letterSpacing: "0.06em" }}>
          <span>物料名稱</span><span>供應商</span><span>數量</span><span>單價</span><span>金額</span><span>變化</span>
        </div>
        {ROWS.map((r, i) => (
          <div key={i} style={{ display: "grid", gridTemplateColumns: "2fr 1.5fr 1fr 0.8fr 1fr 0.8fr", padding: "12px 20px", borderTop: `1px solid ${C.borderL}`, fontSize: 13, alignItems: "center" }}>
            <span style={{ fontWeight: 500, color: C.text }}>{r[0]}</span>
            <span style={{ color: C.textSec }}>{r[1]}</span>
            <span>{r[2]}</span><span>{r[3]}</span>
            <span style={{ fontWeight: 600, color: C.pri }}>{r[4]}</span>
            <span style={{ display: "inline-flex", alignItems: "center", gap: 3, fontWeight: 600, fontSize: 12, color: r[5] < 0 ? C.green : C.red }}>
              {r[5] < 0 ? <ArrowDownRight size={12} /> : <ArrowUpRight size={12} />}{r[5] > 0 ? "+" : ""}{r[5]}%
            </span>
          </div>
        ))}
        <div style={{ padding: "12px 20px", borderTop: `1px solid ${C.border}`, display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontSize: 11, color: C.textTer }}>每頁 20 筆</span>
          <div style={{ display: "flex", gap: 2 }}>
            {["◀", "1", "2", "3", "…", "8", "▶"].map((p, i) => (
              <span key={i} style={{ padding: "4px 10px", fontSize: 12, borderRadius: 6, cursor: "pointer", background: p === "1" ? C.pri : "transparent", color: p === "1" ? "#fff" : C.textTer, fontWeight: p === "1" ? 600 : 400 }}>{p}</span>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

/* ══════ App ══════ */
export default function App() {
  const [page, setPage] = useState("home");
  const [dept, setDept] = useState("procurement");
  const [report, setReport] = useState("");
  const [expanded, setExpanded] = useState(null);

  const nav = useCallback((t, d, r) => {
    setPage(t); if (d) setDept(d); if (r) setReport(r);
  }, []);

  return (
    <div style={{ fontFamily: "'Noto Sans TC', -apple-system, BlinkMacSystemFont, sans-serif", background: C.bg, minHeight: "100vh", display: "flex", flexDirection: "column" }}>
      <TopNav company="台灣大宇集團" />
      <div style={{ display: "flex", flex: 1 }}>
        <Sidebar page={page} dept={dept}
          onNav={(t, d) => nav(t === "home" ? "home" : "dept", d)}
          expanded={expanded} onExpand={setExpanded} />
        <div style={{ flex: 1, overflowY: "auto", height: "calc(100vh - 56px)" }}>
          {page === "home" && <PageHome nav={nav} />}
          {page === "dept" && <PageDept deptId={dept} onDetail={n => nav("detail", dept, n)} />}
          {page === "detail" && <PageDetail deptId={dept} reportName={report || "月度採購成本分析"} onBack={t => nav(t === "home" ? "home" : "dept", dept)} />}
        </div>
      </div>
    </div>
  );
}
