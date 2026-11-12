/** Tailwind 3.4.17 設定：由 standalone CLI 在 dotnet build 時編譯為 wwwroot/css/site.css（見 csproj 的 BuildTailwindCss） */
module.exports = {
    content: [
        './Pages/**/*.cshtml',
        './wwwroot/js/**/*.js',
    ],
    // 以字串組合、掃描不到的 class 須列在這裡：
    // Helpers/CategoryPalette.cs 的分類色票 (徽章 + 小圓點) 與 wwwroot/js/site.js 的 window.catBadge
    safelist: [
        { pattern: /^bg-(sky|amber|rose|violet)-50$/ },
        { pattern: /^text-(sky|amber|rose|violet)-700$/ },
        'bg-slate-100',
        'text-slate-600',
        { pattern: /^bg-(sky|amber|rose|violet|slate)-400$/ },
    ],
    theme: {
        extend: {
            colors: {
                pri: { DEFAULT: '#005758', hover: '#006d6e', light: '#e8f4f4' },
                acc: '#00b4b6',
                surface: { DEFAULT: '#f5f7f8', hover: '#fafcfc' },
                star: '#f5a623',
                txt: { DEFAULT: '#1a2e2f', sec: '#4a6465', ter: '#5c7576' },
                ok: '#087a55',
                bad: '#b93838',
                bdr: { DEFAULT: '#e2e8ea', light: '#eef2f3' },
            }
        }
    }
};
