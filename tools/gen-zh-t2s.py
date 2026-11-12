"""
產生 ReportCenter.Web/wwwroot/js/zh-t2s.js（繁→簡逐字對照表）。

來源：OpenCC 官方字典 TSCharacters.txt（Apache-2.0）
  https://github.com/BYVoid/OpenCC/blob/master/data/dictionary/TSCharacters.txt

用法：
  python tools/gen-zh-t2s.py <TSCharacters.txt 路徑>

規則：
- 一對多的字取第一個候選（OpenCC 的預設候選）
- 繁簡相同的字略過
- 只收 BMP 字元（前端以「每兩個 code unit 一組」解析，避免代理對錯位）
"""
import sys
from pathlib import Path

OUT = Path(__file__).resolve().parent.parent / "ReportCenter.Web" / "wwwroot" / "js" / "zh-t2s.js"


def is_bmp_char(s: str) -> bool:
    return len(s) == 1 and ord(s) <= 0xFFFF


def main() -> None:
    if len(sys.argv) != 2:
        sys.exit("用法：python tools/gen-zh-t2s.py <TSCharacters.txt 路徑>")

    pairs = []
    for line in Path(sys.argv[1]).read_text(encoding="utf-8").splitlines():
        if not line or line.startswith("#"):
            continue
        key, _, values = line.partition("\t")
        first = values.split(" ")[0].strip()
        if is_bmp_char(key) and is_bmp_char(first) and key != first:
            pairs.append(key + first)

    body = "".join(pairs)
    OUT.write_text(
        "// 繁→簡逐字對照表（每兩個字一組：繁、簡）。由 tools/gen-zh-t2s.py 產生，請勿手改。\n"
        "// 資料來源：OpenCC TSCharacters.txt，Apache-2.0 授權 https://github.com/BYVoid/OpenCC\n"
        f"window.ZH_T2S = \"{body}\";\n",
        encoding="utf-8",
    )
    print(f"已輸出 {OUT}（{len(pairs)} 組）")


if __name__ == "__main__":
    main()
