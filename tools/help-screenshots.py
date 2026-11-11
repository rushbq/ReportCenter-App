"""重拍 /Admin/Help 管理者使用手冊的截圖 (wwwroot/images/help/*.webp)。

截圖規格：1600x956 (視窗 1440x860、1.111 倍)，橘色標註框與編號依元素實際位置自動繪製，
編號順序須與 Help.cshtml 中 Shot(...) 的說明條數一致。

前置：
  - 本機已啟動網站 (cd ReportCenter.Web && dotnet run)，且目前模擬身分為管理員
  - pip install playwright pillow；使用本機已安裝的 Google Chrome (不需 playwright install)

用法：
  python tools/help-screenshots.py                    # 全部重拍
  python tools/help-screenshots.py 03-catalog 04-edit-basic
  python tools/help-screenshots.py --base https://localhost:7276 --out <資料夾>

注意：只做勾選、展開、切換頁籤等操作，不會按儲存或確認指派，不改動任何資料。
畫面中的報表 (ID 7)、人員 (員編 10255) 取自開發 DB，換環境時請調整下方常數。
"""
import argparse
import io
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont
from playwright.sync_api import sync_playwright

DEFAULT_OUT = Path(__file__).resolve().parent.parent / "ReportCenter.Web" / "wwwroot" / "images" / "help"
VW, VH, OUT_W = 1440, 860, 1600
SCALE = OUT_W / VW
ORANGE = (232, 89, 12, 255)
FONT = "C:/Windows/Fonts/arialbd.ttf"

EDIT_REPORT_ID = 7                                            # 編輯視窗截圖用的報表
PERM_REPORTS = ["庫存統計表(SH)(00-003)", "集團年度銷售查詢表( 01-018)"]  # 06 勾選的報表
PERM_CATEGORY = "目標達成"                                     # 06 展開的分類
PERM_USER, PERM_AREA, PERM_DEPT = "10255", "台灣", "資訊部"     # 06 / 07 使用的人員


def annotate(png, marks, dst):
    """marks: [(元素 bounding box (CSS px), 編號位置 left/right/above/below)]，編號依順序 1..N"""
    S = 3  # 超取樣，讓線條與圓形邊緣平滑
    base = Image.open(io.BytesIO(png)).convert("RGBA")
    W, H = base.size
    ov = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    d = ImageDraw.Draw(ov)
    font = ImageFont.truetype(FONT, 20 * S)
    badges = []
    for n, (b, side) in enumerate(marks, 1):
        pad = 4
        x0, y0 = (b["x"] - pad) * SCALE, (b["y"] - pad) * SCALE
        x1, y1 = (b["x"] + b["width"] + pad) * SCALE, (b["y"] + b["height"] + pad) * SCALE
        d.rounded_rectangle([x0 * S, y0 * S, x1 * S, y1 * S], radius=8 * S, outline=ORANGE, width=3 * S)
        cx = {"left": x0 - 27, "right": x1 + 27, "above": (x0 + x1) / 2, "below": (x0 + x1) / 2}[side]
        cy = {"above": y0 - 24, "below": y1 + 24}.get(side, (y0 + y1) / 2)
        badges.append((cx, cy, n))
    for cx, cy, n in badges:  # 編號最後畫，避免被其他框線蓋住
        r = 17
        d.ellipse([(cx - r - 2) * S, (cy - r - 2) * S, (cx + r + 2) * S, (cy + r + 2) * S], fill=(255, 255, 255, 255))
        d.ellipse([(cx - r) * S, (cy - r) * S, (cx + r) * S, (cy + r) * S], fill=ORANGE)
        d.text((cx * S, cy * S), str(n), font=font, fill=(255, 255, 255, 255), anchor="mm")
    ov = ov.resize((W, H), Image.LANCZOS)
    Image.alpha_composite(base, ov).convert("RGB").save(dst, "WEBP", quality=82, method=6)


class Shooter:
    def __init__(self, page, base, out, only):
        self.page, self.base, self.out, self.only = page, base, out, only

    def box(self, selector):
        loc = self.page.locator(selector).first
        loc.wait_for(state="visible")
        return loc.bounding_box()

    def goto(self, path, ms=900):
        self.page.goto(self.base + path)
        self.page.wait_for_load_state("networkidle")
        self.page.wait_for_timeout(ms)

    def wanted(self, name):
        return not self.only or name in self.only

    def shot(self, name, marks):
        annotate(self.page.screenshot(), marks, self.out / f"{name}.webp")
        print("已輸出", name)

    # ─── 管理總覽 ───
    def overview(self):
        if not (self.wanted("01-overview") or self.wanted("02-overview-alerts")):
            return
        p = self.page
        self.goto("/Admin")
        if self.wanted("01-overview"):
            side = p.locator("aside").first.bounding_box()
            label = self.box("aside >> text=系統管理")
            sys_box = {"x": side["x"] + 10, "y": label["y"] - 2, "width": side["width"] - 20, "height": label["height"] + 4}
            self.shot("01-overview", [
                (sys_box, "right"),
                (self.box("span:text-is('報表總數')"), "left"),
                (self.box("h2:text-is('部門使用矩陣')"), "left"),
                (self.box("a:has-text('管理報表目錄')"), "above"),
            ])
        if self.wanted("02-overview-alerts"):
            # 內容在內層容器捲動 (非 window)：先捲到標題，再讓標題停在畫面上方約 175px 處
            p.locator("h2:text-is('未指派部門的報表')").first.evaluate("""e => {
                e.scrollIntoView({ block: 'start' });
                let el = e.parentElement;
                while (el && !(el.scrollHeight > el.clientHeight && /(auto|scroll)/.test(getComputedStyle(el).overflowY))) el = el.parentElement;
                (el || window).scrollBy(0, -175);
            }""")
            p.wait_for_timeout(400)
            self.shot("02-overview-alerts", [
                (self.box("h2:text-is('未指派部門的報表')"), "left"),
                (self.box("h2:text-is('冷門報表')"), "left"),
            ])

    # ─── 報表目錄管理 ───
    def catalog(self):
        if self.wanted("03-catalog"):
            self.goto("/Admin/Catalog")
            self.shot("03-catalog", [
                (self.box("button:has-text('新增報表')"), "above"),
                (self.box("input[placeholder*='搜尋報表名稱']"), "left"),
                (self.box("th:has-text('近 30 日')"), "above"),
                (self.box("th:text-is('狀態')"), "above"),
                (self.box("table button[aria-label^='編輯「']"), "below"),
            ])
        if self.wanted("04-edit-basic"):
            self.goto(f"/Admin/Catalog?edit={EDIT_REPORT_ID}")
            self.shot("04-edit-basic", [
                (self.box("input[x-model='form.reportName']"), "left"),
                (self.box("label:text-is('工具類型')"), "left"),
                # label 是 block，只框文字部分 (第一個 span)
                (self.box("label:has-text('SmartQuery 路徑') >> span >> nth=0"), "left"),
                (self.box("label:text-is('帶公司別參數')"), "left"),
                (self.box("button:has-text('部門選單設定')"), "right"),
                (self.box("label[for=catalog-sort-order] >> xpath=.."), "right"),
            ])
        if self.wanted("05-edit-dept"):
            self.goto(f"/Admin/Catalog?edit={EDIT_REPORT_ID}")
            self.page.locator("button:has-text('部門選單設定')").first.click()
            self.page.wait_for_timeout(500)
            self.shot("05-edit-dept", [
                (self.box("span:has-text('此為「左側部門選單」歸屬設定')"), "left"),
                (self.box("button:has-text('儲存變更')"), "right"),
            ])

    # ─── 權限管理 (只勾選、不送出) ───
    def permission(self):
        if not (self.wanted("06-perm-batch") or self.wanted("07-perm-single")):
            return
        p = self.page
        user_search = "input[placeholder='搜尋員工編號或姓名...']:visible"
        self.goto("/Admin/Permission")
        if self.wanted("06-perm-batch"):
            p.locator("button:has-text('功能 → 多人')").first.click(); p.wait_for_timeout(300)
            p.locator(f"text={PERM_CATEGORY}").first.click(); p.wait_for_timeout(300)
            for name in PERM_REPORTS:
                p.locator(f"label:has-text('{name}') input[type=checkbox]").first.check()
            p.locator(user_search).first.fill(PERM_USER); p.wait_for_timeout(800)
            # 搜尋後人員樹不會自動展開：依序展開地區與部門再勾選
            p.locator(f"span:text-is('{PERM_AREA}'):visible").first.click(); p.wait_for_timeout(300)
            p.locator(f"span:text-is('{PERM_DEPT}'):visible").first.click(); p.wait_for_timeout(300)
            p.locator(f"label:has-text('{PERM_USER}') input[type=checkbox]").first.check(); p.wait_for_timeout(300)
            self.shot("06-perm-batch", [
                (self.box("button:has-text('功能 → 多人')"), "left"),
                (self.box("xpath=//div[contains(@class,'font-semibold') and normalize-space(.)='報表清單']"), "left"),
                (self.box("xpath=//div[contains(@class,'font-semibold') and normalize-space(.)='人員清單']"), "left"),
                (self.box("button:has-text('確認指派')"), "above"),
            ])
        if self.wanted("07-perm-single"):
            p.locator("button:has-text('單一人 → 功能')").first.click(); p.wait_for_timeout(300)
            p.locator(user_search).first.fill(PERM_USER); p.wait_for_timeout(1200)
            p.locator(f"button:has-text('{PERM_USER}')").first.click()
            p.wait_for_load_state("networkidle"); p.wait_for_timeout(1000)
            self.shot("07-perm-single", [
                (self.box("button:has-text('單一人 → 功能')"), "left"),
                (self.box(user_search), "left"),
                (self.box("button:has-text('新增權限')"), "above"),
                (self.box("button[title='移除權限']"), "right"),
            ])

    # ─── 使用分析 ───
    def usage(self):
        if self.wanted("08-usage"):
            self.goto("/Admin/Usage", ms=1500)  # 等趨勢圖動畫結束
            self.shot("08-usage", [
                (self.box("span:text-is('近 30 日總點擊')"), "left"),
                (self.box("h2:has-text('每日點擊趨勢')"), "left"),
                (self.box("h2:has-text('熱門報表 Top 10')"), "left"),
            ])
        if self.wanted("09-usage-logs"):
            self.goto("/Admin/Usage?tab=logs")
            self.shot("09-usage-logs", [
                (self.box("#filterReport"), "left"),
                (self.box("#filterUser"), "above"),
                (self.box("label[for=filterFrom]"), "above"),
                (self.box("button[type=submit]:has-text('查詢')"), "above"),
            ])


def main():
    ap = argparse.ArgumentParser(description="重拍 /Admin/Help 手冊截圖")
    ap.add_argument("names", nargs="*", help="只重拍指定檔名 (不含 .webp)，省略則全部")
    ap.add_argument("--base", default="https://localhost:7276", help="網站網址")
    ap.add_argument("--out", type=Path, default=DEFAULT_OUT, help="輸出資料夾")
    args = ap.parse_args()
    args.out.mkdir(parents=True, exist_ok=True)

    with sync_playwright() as pw:
        browser = pw.chromium.launch(channel="chrome")
        ctx = browser.new_context(viewport={"width": VW, "height": VH}, device_scale_factor=SCALE,
                                  ignore_https_errors=True)
        s = Shooter(ctx.new_page(), args.base.rstrip("/"), args.out, set(args.names))
        try:
            s.overview()
            s.catalog()
            s.permission()
            s.usage()
        finally:
            browser.close()


if __name__ == "__main__":
    main()
