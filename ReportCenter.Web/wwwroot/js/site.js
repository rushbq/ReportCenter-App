// ─── ReportCenter 全站共用 Alpine Store ───
document.addEventListener('alpine:init', () => {

    // ─── 全域 Toast Store ───
    Alpine.store('toast', {
        msg: '',
        visible: false,
        show(msg) {
            this.msg = msg;
            this.visible = true;
            setTimeout(() => this.visible = false, 2500);
        }
    });

    // 全站搜尋 Store
    Alpine.store('search', {
        open: false,
        query: '',
        results: [],
        // 搜尋索引由 Layout 注入
        index: [],

        toggle() {
            if (this.open) {
                this.open = false;
            } else {
                this.open = true;
                this.query = '';
                this.results = [];
                setTimeout(() => {
                    const el = document.getElementById('globalSearchInput');
                    if (el) el.focus();
                }, 80);
            }
        },

        search() {
            const q = this.query.trim().toLowerCase();
            if (!q) { this.results = []; return; }
            this.results = this.index
                .filter(item =>
                    item.name.toLowerCase().includes(q) ||
                    item.desc.toLowerCase().includes(q) ||
                    item.dept.toLowerCase().includes(q) ||
                    item.cat.toLowerCase().includes(q)
                )
                .slice(0, 8);
        },

        go(url) {
            this.open = false;
            window.location.href = url;
        }
    });

    // 收藏 Store (localStorage 持久化)
    Alpine.store('favorites', {
        items: JSON.parse(localStorage.getItem('rc_favorites') || '[]'),

        has(deptId, name) {
            return this.items.some(f => f.deptId === deptId && f.name === name);
        },

        toggle(deptId, name, dept) {
            const wasAdded = !this.has(deptId, name);
            if (wasAdded) {
                this.items.push({ deptId, name, dept, time: Date.now() });
            } else {
                this.items = this.items.filter(f => !(f.deptId === deptId && f.name === name));
            }
            localStorage.setItem('rc_favorites', JSON.stringify(this.items));
            Alpine.store('toast').show(wasAdded ? '已加入收藏' : '已取消收藏');
        },

        get count() { return this.items.length; }
    });

    // 最近瀏覽 Store (localStorage 持久化)
    Alpine.store('recent', {
        items: JSON.parse(localStorage.getItem('rc_recent') || '[]'),

        add(deptId, name, dept) {
            // 移除重複
            this.items = this.items.filter(r => !(r.deptId === deptId && r.name === name));
            // 加到最前面
            this.items.unshift({ deptId, name, dept, time: Date.now() });
            // 只保留最近 20 筆
            if (this.items.length > 20) this.items = this.items.slice(0, 20);
            localStorage.setItem('rc_recent', JSON.stringify(this.items));
        },

        get count() { return Math.min(this.items.length, 20); }
    });
});

// ─── 全域鍵盤快捷鍵 ⌘K / ESC ───
document.addEventListener('keydown', (e) => {
    if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        e.stopPropagation();
        const store = Alpine.store('search');
        store.open = true;
        store.query = '';
        store.results = [];
        setTimeout(() => {
            const el = document.getElementById('globalSearchInput');
            if (el) el.focus();
        }, 50);
    }
    if (e.key === 'Escape' && Alpine.store('search').open) {
        e.preventDefault();
        Alpine.store('search').open = false;
    }
});
