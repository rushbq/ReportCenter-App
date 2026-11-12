// ─── 報表分類配色 (須與後端 Helpers/CategoryPalette.cs 保持一致) ───
// 依分類名稱決定性地對應 5 種色票，讓同一分類全站同色。
window.catBadge = function (cat) {
    const palette = [
        'bg-sky-50 text-sky-700',
        'bg-amber-50 text-amber-700',
        'bg-rose-50 text-rose-700',
        'bg-violet-50 text-violet-700',
        'bg-slate-100 text-slate-600',
    ];
    if (!cat) return palette[0];
    let sum = 0;
    for (let i = 0; i < cat.length; i++) sum += cat.charCodeAt(i);
    return palette[sum % palette.length];
};

// ─── 簡繁轉換 (字典 wwwroot/js/zh-t2s.js，須在本檔之前載入) ───
// 搜尋：查詢字串與資料都轉成簡體再比對，輸入簡體或繁體都能命中。
// 顯示：cookie rc_lang=zh-CN 時，整頁文字節點逐字轉簡體 (資料本身不變)。
window.zh = (() => {
    let map = null;
    const getMap = () => {
        if (map) return map;
        map = new Map();
        const s = window.ZH_T2S || '';
        for (let i = 0; i + 1 < s.length; i += 2) map.set(s[i], s[i + 1]);
        return map;
    };

    const toCn = (s) => {
        if (!s) return s || '';
        const m = getMap();
        let out = '';
        for (const ch of s) out += m.get(ch) || ch;
        return out;
    };

    // 正規化結果快取：同一批資料每次按鍵都會比對，避免重複轉換
    const normCache = new Map();
    const norm = (s) => {
        const key = String(s ?? '');
        let v = normCache.get(key);
        if (v === undefined) {
            v = toCn(key.toLowerCase());
            if (normCache.size > 20000) normCache.clear();
            normCache.set(key, v);
        }
        return v;
    };

    return {
        toCn,
        norm,
        /** 任一欄位包含查詢字串即命中 (簡繁、大小寫不分)；q 為空時回傳 true */
        match(q, ...fields) {
            const nq = norm(String(q ?? '').trim());
            if (!nq) return true;
            return fields.some(f => norm(f).includes(nq));
        },
        isCn: () => /(?:^|;\s*)rc_lang=zh-CN(?:;|$)/.test(document.cookie),
        setLang(lang) {
            document.cookie = 'rc_lang=' + lang + ';path=/;max-age=31536000';
            location.reload();
        }
    };
})();

// 簡體顯示：初次整頁轉換後，以 MutationObserver 轉換 Alpine / HTMX 之後改寫的文字。
// 略過 script/style/textarea 與標記 data-no-zh 的元素；input 的值不轉換 (使用者輸入)。
(() => {
    if (!window.zh.isCn()) return;
    const SKIP = new Set(['SCRIPT', 'STYLE', 'TEXTAREA', 'NOSCRIPT']);
    const ATTRS = ['placeholder', 'title', 'aria-label'];
    const toCn = window.zh.toCn;

    const skipped = (el) => !el || SKIP.has(el.tagName) || !!el.closest('[data-no-zh]');

    const convertText = (node) => {
        if (skipped(node.parentElement)) return;
        const v = node.nodeValue;
        const c = toCn(v);
        if (c !== v) node.nodeValue = c;   // 值相同不寫回，避免 Observer 無限觸發
    };

    const convertAttrs = (el) => {
        if (skipped(el)) return;
        for (const a of ATTRS) {
            const v = el.getAttribute(a);
            if (!v) continue;
            const c = toCn(v);
            if (c !== v) el.setAttribute(a, c);
        }
    };

    const convertTree = (root) => {
        if (root.nodeType === Node.TEXT_NODE) { convertText(root); return; }
        if (root.nodeType !== Node.ELEMENT_NODE) return;
        if (skipped(root)) return;
        convertAttrs(root);
        root.querySelectorAll('[placeholder],[title],[aria-label]').forEach(convertAttrs);
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        for (let n = walker.nextNode(); n; n = walker.nextNode()) convertText(n);
    };

    document.addEventListener('DOMContentLoaded', () => {
        try {
            document.title = toCn(document.title);
            convertTree(document.body);
            new MutationObserver((records) => {
                for (const r of records) {
                    if (r.type === 'characterData') convertText(r.target);
                    else if (r.type === 'attributes') convertAttrs(r.target);
                    else r.addedNodes.forEach(convertTree);
                }
            }).observe(document.body, {
                childList: true, characterData: true, subtree: true,
                attributes: true, attributeFilter: ATTRS
            });
        } finally {
            document.documentElement.classList.remove('zh-pending');
        }
    });
})();

// ─── 全站版面 Alpine 元件 ───
window.appShell = function () {
    const sidebarMedia = window.matchMedia('(max-width: 1023px)');

    return {
        sidebarOpen: false,
        mobileViewport: sidebarMedia.matches,
        sidebarReturnFocus: null,

        init() {
            sidebarMedia.addEventListener('change', (event) => {
                this.mobileViewport = event.matches;
                if (!event.matches) this.sidebarOpen = false;
            });
        },

        toggleSidebar(trigger) {
            if (this.sidebarOpen) {
                this.closeSidebar();
                return;
            }

            this.sidebarReturnFocus = trigger instanceof HTMLElement ? trigger : document.activeElement;
            this.sidebarOpen = true;
            this.$nextTick(() => {
                this.$refs.mobileSidebar
                    ?.querySelector('a[href], button:not([disabled]), input:not([disabled]), select:not([disabled])')
                    ?.focus();
            });
        },

        closeSidebar() {
            if (!this.sidebarOpen) return;
            this.sidebarOpen = false;
            const returnFocus = this.sidebarReturnFocus;
            this.sidebarReturnFocus = null;
            this.$nextTick(() => returnFocus?.focus());
        }
    };
};

// ─── ReportCenter 全站共用 Alpine Store ───
document.addEventListener('alpine:init', () => {

    // ─── 全域 Toast Store ───
    Alpine.store('toast', {
        msg: '',
        type: 'success',
        visible: false,
        timer: null,
        show(msg, type = 'success') {
            if (this.timer) clearTimeout(this.timer);
            this.msg = msg;
            this.type = type;
            this.visible = true;
            this.timer = setTimeout(() => {
                this.visible = false;
                this.timer = null;
            }, type === 'error' ? 5000 : 2500);
        }
    });

    // 全站搜尋 Store
    Alpine.store('search', {
        open: false,
        query: '',
        results: [],
        // 搜尋索引由 Layout 注入
        index: [],
        returnFocusTo: null,

        toggle(trigger) {
            if (this.open) {
                this.close();
                return;
            }

            this.openDialog(trigger);
        },

        openDialog(trigger) {
            const dialog = document.getElementById('globalSearchDialog');
            if (!dialog) return;

            this.returnFocusTo = trigger instanceof HTMLElement ? trigger : document.activeElement;
            this.open = true;
            this.query = '';
            this.results = [];
            if (!dialog.open) dialog.showModal();
            requestAnimationFrame(() => document.getElementById('globalSearchInput')?.focus());
        },

        close() {
            const dialog = document.getElementById('globalSearchDialog');
            if (dialog?.open) {
                dialog.close();
                return;
            }

            this.open = false;
        },

        onDialogClosed() {
            this.open = false;
            const returnFocus = this.returnFocusTo;
            this.returnFocusTo = null;
            requestAnimationFrame(() => returnFocus?.focus());
        },

        search() {
            const q = this.query.trim();
            if (!q) { this.results = []; return; }
            this.results = this.index
                .filter(item => zh.match(q, item.name, item.desc, item.dept, item.cat))
                .slice(0, 8);
        },

        go(url, target, reportId) {
            this.close();
            if (reportId) window.trackReportClick(reportId, 'search');
            if (target === '_blank') {
                // 外部報表：用動態 <a> 開新分頁
                Object.assign(document.createElement('a'), { href: url, target: '_blank', rel: 'noopener' }).click();
            } else {
                window.location.href = url;
            }
        }
    });
});

// ─── 報表使用記錄埋點 (sendBeacon 非同步送出，不阻塞開新視窗) ───
// 30 秒內同一報表重複點擊不再送出；後端 UsageService 另有相同時間窗的權威防護
(() => {
    const sentAt = {};
    window.trackReportClick = function (reportId, source) {
        const id = parseInt(reportId, 10);
        if (!id) return;
        const now = Date.now();
        if (sentAt[id] && now - sentAt[id] < 30000) return;
        sentAt[id] = now;

        const fd = new FormData();
        fd.append('reportId', id);
        fd.append('source', source || '');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) fd.append('__RequestVerificationToken', token);

        if (navigator.sendBeacon) {
            navigator.sendBeacon('/Api/Usage?handler=Track', fd);
        } else {
            fetch('/Api/Usage?handler=Track', { method: 'POST', body: fd, keepalive: true }).catch(() => {});
        }
    };

    // 委派監聽：任何帶 data-report-id 的元素被點擊 (含中鍵開新分頁) 即記錄
    const onReportClick = (e) => {
        if (e.type === 'auxclick' && e.button !== 1) return;
        const el = e.target.closest('[data-report-id]');
        if (el) window.trackReportClick(el.dataset.reportId, el.dataset.reportSource);
    };
    document.addEventListener('click', onReportClick);
    document.addEventListener('auxclick', onReportClick);
})();

// ─── 全域鍵盤快捷鍵 ⌘K / ESC ───
document.addEventListener('keydown', (e) => {
    if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        e.stopPropagation();
        const store = Alpine.store('search');
        store.openDialog(document.activeElement);
    }
});

// ─── 登入工作階段 ───
// 本站 Cookie 過期時，fetch 會收到 401 (伺服器只對一般換頁轉址到登入頁)。
// 重新整理頁面即可走「登入頁 → DataHubApi Refresh 自動登入 → 導回原頁」，各頁的 AJAX 不必個別處理。
(() => {
    const nativeFetch = window.fetch.bind(window);
    let reloading = false;
    window.fetch = async (input, init) => {
        const res = await nativeFetch(input, init);
        if (res.status === 401 && !reloading && new URL(res.url, location.href).origin === location.origin) {
            reloading = true;
            location.reload();
        }
        return res;
    };
})();

// 共用登出偵測：在 PKPortal 或其他站台登出（或換人登入）後，本站也要跟著登出。
// 向 DataHubApi /Auth/Status 確認共用登入仍有效；同一瀏覽器所有分頁共用 5 分鐘節流，
// 所以登出最多晚 5 分鐘生效。DataHubApi 連不到時不登出，避免服務中斷就把所有人踢掉。
(() => {
    const auth = window.rcAuth;
    if (!auth?.apiBaseUrl || !auth.account) return;   // 未登入或開發模擬身分
    const KEY = 'rc_session_checked_at';
    const INTERVAL = 5 * 60 * 1000;

    async function signOut() {
        const fd = new FormData();
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) fd.append('__RequestVerificationToken', token);
        try { await fetch('/Account/Logout', { method: 'POST', body: fd }); } catch { /* 仍導向登入頁 */ }
        // 不帶 loggedOut：換人登入的情況，登入頁會以新帳號自動登入
        location.href = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname + location.search);
    }

    async function check() {
        let last = 0;
        try { last = Number(localStorage.getItem(KEY)) || 0; } catch { /* 無法存取 storage 時每次都檢查 */ }
        if (Date.now() - last < INTERVAL) return;
        try { localStorage.setItem(KEY, String(Date.now())); } catch { }

        let res;
        try {
            res = await fetch(auth.apiBaseUrl + '/Auth/Status', { method: 'POST', credentials: 'include' });
        } catch {
            return;
        }
        if (res.status === 401) return signOut();
        if (res.ok) {
            const body = await res.json().catch(() => null);
            const user = body?.data?.username;
            if (user && user.toLowerCase() !== auth.account.toLowerCase()) return signOut();
        }
    }

    check();
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'visible') check();
    });
})();

// 登出：清本站 Cookie → 撤銷 DataHubApi 共用的 Refresh Token (PKPortal 等站台一起登出) → 回登入頁。
// 帶 loggedOut=true 時登入頁不做自動登入，避免 DataHubApi 登出失敗時又馬上登入回來。
window.rcLogout = async function () {
    const fd = new FormData();
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) fd.append('__RequestVerificationToken', token);
    try {
        await fetch('/Account/Logout', { method: 'POST', body: fd });
    } catch { /* 仍繼續撤銷 DataHubApi 登入 */ }

    const apiBaseUrl = window.rcAuth?.apiBaseUrl;
    if (apiBaseUrl) {
        try {
            await fetch(apiBaseUrl + '/Auth/Logout', { method: 'POST', credentials: 'include' });
        } catch { /* 網路錯誤時仍導向登入頁 */ }
    }
    location.href = '/Account/Login?loggedOut=true';
};
