const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat,
  HeadingLevel, BorderStyle, WidthType, ShadingType, PageNumber, PageBreak,
  TableOfContents,
} = require("docx");

// ── Design tokens ──
const C = {
  pri: "005758",
  priLight: "E8F4F4",
  acc: "00B4B6",
  txt: "1A2E2F",
  txtSec: "5C7576",
  border: "D5E0E2",
  bg: "F5F7F8",
  white: "FFFFFF",
  black: "000000",
};

const PAGE_W = 12240; // US Letter
const PAGE_H = 15840;
const MARGIN = 1440;  // 1 inch
const CONTENT_W = PAGE_W - MARGIN * 2; // 9360

// ── Helpers ──
const border = { style: BorderStyle.SINGLE, size: 1, color: C.border };
const borders = { top: border, bottom: border, left: border, right: border };
const noBorder = { style: BorderStyle.NONE, size: 0 };
const noBorders = { top: noBorder, bottom: noBorder, left: noBorder, right: noBorder };
const cellMargins = { top: 80, bottom: 80, left: 120, right: 120 };

function heading(text, level) {
  return new Paragraph({
    heading: level,
    children: [new TextRun({ text })],
  });
}

function para(text, opts = {}) {
  return new Paragraph({
    spacing: { after: 120 },
    ...opts,
    children: [new TextRun({ text, size: 22, font: "Arial", color: C.txt, ...opts.run })],
  });
}

function boldPara(text) {
  return para(text, { run: { bold: true } });
}

function codePara(text) {
  return new Paragraph({
    spacing: { after: 60 },
    shading: { fill: C.bg, type: ShadingType.CLEAR },
    indent: { left: 240, right: 240 },
    children: [new TextRun({ text, font: "Courier New", size: 18, color: C.txtSec })],
  });
}

function codeBlock(lines) {
  return lines.map(line => codePara(line));
}

function tableRow(cells, isHeader = false) {
  return new TableRow({
    children: cells.map((text, i) => {
      const widths = cells._widths || [CONTENT_W / cells.length];
      return new TableCell({
        borders,
        width: { size: widths[i] || widths[0], type: WidthType.DXA },
        shading: { fill: isHeader ? C.priLight : C.white, type: ShadingType.CLEAR },
        margins: cellMargins,
        verticalAlign: "center",
        children: [new Paragraph({
          children: [new TextRun({
            text: String(text),
            bold: isHeader,
            font: "Arial",
            size: 20,
            color: C.txt,
          })],
        })],
      });
    }),
  });
}

function makeTable(headers, rows, widths) {
  headers._widths = widths;
  const allRows = [tableRow(headers, true)];
  for (const r of rows) {
    r._widths = widths;
    allRows.push(tableRow(r));
  }
  return new Table({
    width: { size: CONTENT_W, type: WidthType.DXA },
    columnWidths: widths,
    rows: allRows,
  });
}

function spacer(h = 200) {
  return new Paragraph({ spacing: { after: h }, children: [] });
}

// ── Build document ──
const doc = new Document({
  styles: {
    default: {
      document: { run: { font: "Arial", size: 22, color: C.txt } },
    },
    paragraphStyles: [
      {
        id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 36, bold: true, font: "Arial", color: C.pri },
        paragraph: { spacing: { before: 360, after: 200 }, outlineLevel: 0 },
      },
      {
        id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 28, bold: true, font: "Arial", color: C.pri },
        paragraph: { spacing: { before: 280, after: 160 }, outlineLevel: 1 },
      },
      {
        id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 24, bold: true, font: "Arial", color: C.txtSec },
        paragraph: { spacing: { before: 200, after: 120 }, outlineLevel: 2 },
      },
    ],
  },
  numbering: {
    config: [
      {
        reference: "bullets",
        levels: [{
          level: 0, format: LevelFormat.BULLET, text: "\u2022", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } },
        }],
      },
      {
        reference: "bullets2",
        levels: [{
          level: 0, format: LevelFormat.BULLET, text: "\u2013", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 1080, hanging: 360 } } },
        }],
      },
      {
        reference: "numbers",
        levels: [{
          level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } },
        }],
      },
    ],
  },
  sections: [
    // ═══════════════════════════════════════
    // COVER PAGE
    // ═══════════════════════════════════════
    {
      properties: {
        page: {
          size: { width: PAGE_W, height: PAGE_H },
          margin: { top: MARGIN, right: MARGIN, bottom: MARGIN, left: MARGIN },
        },
      },
      children: [
        spacer(2400),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ text: "ReportCenter", font: "Arial", size: 56, bold: true, color: C.pri })],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 400 },
          children: [new TextRun({ text: "\u524D\u7AEF\u6280\u8853\u898F\u683C\u6587\u4EF6", font: "Arial", size: 36, color: C.txtSec })],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 100 },
          children: [new TextRun({ text: "Frontend Technical Specification", font: "Arial", size: 24, color: C.txtSec, italics: true })],
        }),
        spacer(800),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ text: "\u53F0\u7063\u5BF6\u5DE5\u5BE6\u696D\u80A1\u4EFD\u6709\u9650\u516C\u53F8", font: "Arial", size: 24, color: C.txt })],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 200 },
          children: [new TextRun({ text: "\u8CC7\u8A0A\u90E8", font: "Arial", size: 22, color: C.txtSec })],
        }),
        spacer(600),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ text: "\u7248\u672C: 1.0", font: "Arial", size: 20, color: C.txtSec })],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ text: "\u65E5\u671F: 2026-04-01", font: "Arial", size: 20, color: C.txtSec })],
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ text: "\u6A94\u6848\u7D1A\u5225: \u5167\u90E8\u4F7F\u7528", font: "Arial", size: 20, color: C.txtSec })],
        }),
      ],
    },
    // ═══════════════════════════════════════
    // TOC
    // ═══════════════════════════════════════
    {
      properties: {
        page: {
          size: { width: PAGE_W, height: PAGE_H },
          margin: { top: MARGIN, right: MARGIN, bottom: MARGIN, left: MARGIN },
        },
      },
      headers: {
        default: new Header({
          children: [new Paragraph({
            alignment: AlignmentType.RIGHT,
            children: [new TextRun({ text: "ReportCenter \u524D\u7AEF\u6280\u8853\u898F\u683C", font: "Arial", size: 16, color: C.txtSec })],
          })],
        }),
      },
      footers: {
        default: new Footer({
          children: [new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
              new TextRun({ text: "Page ", font: "Arial", size: 16, color: C.txtSec }),
              new TextRun({ children: [PageNumber.CURRENT], font: "Arial", size: 16, color: C.txtSec }),
            ],
          })],
        }),
      },
      children: [
        heading("\u76EE\u9304", HeadingLevel.HEADING_1),
        new TableOfContents("Table of Contents", { hyperlink: true, headingStyleRange: "1-3" }),
        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 1. 架構總覽
        // ═══════════════════════════════════════
        heading("1. \u67B6\u69CB\u7E3D\u89BD", HeadingLevel.HEADING_1),
        para("\u672C\u6587\u4EF6\u5B9A\u7FA9 ReportCenter \u524D\u7AEF\u6280\u8853\u898F\u683C\uFF0C\u4F5C\u70BA\u65B0\u5C08\u6848\u958B\u767C\u6642\u7684\u6A19\u6E96\u7BC4\u672C\u3002\u67B6\u69CB\u57FA\u65BC .NET Razor Pages \u4F3A\u670D\u5668\u7AEF\u6E32\u67D3\uFF0C\u642D\u914D\u8F15\u91CF\u7D1A\u524D\u7AEF\u5DE5\u5177\u93C8\uFF0C\u9054\u6210\u300C\u4F3A\u670D\u5668\u512A\u5148\u3001\u524D\u7AEF\u589E\u5F37\u300D\u7684\u8A2D\u8A08\u54F2\u5B78\u3002"),

        heading("1.1 \u6280\u8853\u7D44\u5408", HeadingLevel.HEADING_2),
        makeTable(
          ["\u6280\u8853", "\u7248\u672C", "\u7528\u9014", "CDN / \u5F15\u7528"],
          [
            [".NET Razor Pages", "10.0", "\u9801\u9762\u6846\u67B6\u8207\u4F3A\u670D\u5668\u7AEF\u6E32\u67D3", "NuGet"],
            ["Tailwind CSS", "CDN latest", "Utility-first CSS \u6A23\u5F0F\u7CFB\u7D71", "cdn.tailwindcss.com"],
            ["Alpine.js", "3.x", "\u8F15\u91CF\u7D1A\u524D\u7AEF\u4E92\u52D5", "cdn.jsdelivr.net/npm/alpinejs@3.x.x"],
            ["Chart.js", "4.x", "\u5716\u8868\u6E32\u67D3", "cdn.jsdelivr.net/npm/chart.js@4"],
            ["HTMX", "2.0.4", "\u5C40\u90E8\u66F4\u65B0\u8207\u4F3A\u670D\u5668\u4E92\u52D5", "unpkg.com/htmx.org@2.0.4"],
            ["Lucide Icons", "latest", "\u5716\u793A\u7CFB\u7D71", "unpkg.com/lucide@latest"],
            ["Noto Sans TC", "400-800", "\u4E2D\u6587\u5B57\u578B", "Google Fonts API v2"],
          ],
          [1800, 1200, 2800, 3560],
        ),

        heading("1.2 \u5C08\u6848\u7D50\u69CB", HeadingLevel.HEADING_2),
        ...codeBlock([
          "ReportCenter.Web/",
          "\u251C\u2500\u2500 Pages/",
          "\u2502   \u251C\u2500\u2500 Shared/",
          "\u2502   \u2502   \u251C\u2500\u2500 _Layout.cshtml        \u2190 \u4E3B\u7248\u9762\uFF08CDN \u5F15\u7528\u3001Tailwind \u8A2D\u5B9A\uFF09",
          "\u2502   \u2502   \u251C\u2500\u2500 _TopNav.cshtml        \u2190 \u9802\u90E8\u5C0E\u822A\u5217",
          "\u2502   \u2502   \u251C\u2500\u2500 _Sidebar.cshtml       \u2190 \u5074\u6B04\u5C0E\u822A",
          "\u2502   \u2502   \u2514\u2500\u2500 _KpiCard.cshtml       \u2190 KPI \u5361\u7247\u5171\u7528\u5143\u4EF6",
          "\u2502   \u251C\u2500\u2500 Index.cshtml / .cs       \u2190 \u9996\u9801\u5100\u8868\u677F",
          "\u2502   \u251C\u2500\u2500 Department.cshtml / .cs  \u2190 \u90E8\u9580\u5831\u8868\u5217\u8868",
          "\u2502   \u2514\u2500\u2500 Report.cshtml / .cs      \u2190 \u5831\u8868\u660E\u7D30\u9801",
          "\u251C\u2500\u2500 Models/",
          "\u2502   \u2514\u2500\u2500 ReportModels.cs          \u2190 \u8CC7\u6599\u6A21\u578B",
          "\u251C\u2500\u2500 wwwroot/                     \u2190 \u975C\u614B\u8CC7\u6E90",
          "\u2514\u2500\u2500 Program.cs                   \u2190 \u61C9\u7528\u7A0B\u5F0F\u9032\u5165\u9EDE",
        ]),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 2. 設計系統
        // ═══════════════════════════════════════
        heading("2. \u8A2D\u8A08\u7CFB\u7D71 (Design System)", HeadingLevel.HEADING_1),

        heading("2.1 \u8272\u5F69\u8A2D\u8A08\u78BC (Design Tokens)", HeadingLevel.HEADING_2),
        para("\u6240\u6709\u8272\u5F69\u900F\u904E Tailwind CSS \u81EA\u8A02 config \u5B9A\u7FA9\uFF0C\u78BA\u4FDD\u5168\u5C08\u6848\u4E00\u81F4\u6027\u3002"),
        makeTable(
          ["Token \u540D\u7A31", "Tailwind Class", "HEX", "\u7528\u9014"],
          [
            ["Primary", "pri", "#005758", "\u4E3B\u8272\u3001\u91CD\u9EDE\u5143\u7D20\u3001\u6D3B\u52D5\u72C0\u614B"],
            ["Primary Hover", "pri-hover", "#006D6E", "\u4E3B\u8272 Hover \u72C0\u614B"],
            ["Primary Light", "pri-light", "#E8F4F4", "\u6D3B\u52D5\u80CC\u666F\u3001\u6A19\u7C64\u5E95\u8272"],
            ["Accent", "acc", "#00B4B6", "\u8F14\u52A9\u8272\u3001\u7B2C\u4E8C\u5716\u8868\u7DDA"],
            ["Surface", "surface", "#F5F7F8", "\u9801\u9762\u80CC\u666F\u8272"],
            ["Text", "txt", "#1A2E2F", "\u4E3B\u8981\u6587\u5B57"],
            ["Text Secondary", "txt-sec", "#5C7576", "\u6B21\u8981\u6587\u5B57"],
            ["Text Tertiary", "txt-ter", "#94AEB0", "\u63D0\u793A\u6587\u5B57\u3001\u8EF8\u6A19\u7C64"],
            ["OK (Positive)", "ok", "#0D9668", "\u6B63\u5411\u8DA8\u52E2\u3001\u4E0A\u6F32"],
            ["Bad (Negative)", "bad", "#DC4A4A", "\u8CA0\u5411\u8DA8\u52E2\u3001\u4E0B\u8DCC"],
            ["Border", "bdr", "#E2E8EA", "\u5143\u4EF6\u908A\u6846"],
            ["Border Light", "bdr-light", "#EEF2F3", "\u8868\u683C\u5206\u9694\u7DDA\u3001\u6DE1\u908A\u6846"],
          ],
          [1800, 1800, 1400, 4360],
        ),

        heading("2.2 \u5B57\u578B\u898F\u7BC4", HeadingLevel.HEADING_2),
        makeTable(
          ["\u5143\u7D20", "\u5B57\u578B", "\u5927\u5C0F", "\u7C97\u7D30", "\u8272\u5F69"],
          [
            ["\u9801\u9762\u6A19\u984C", "Noto Sans TC", "22px", "700 (Bold)", "txt (#1A2E2F)"],
            ["KPI \u6578\u5B57", "Noto Sans TC", "26px", "700 (Bold)", "pri (#005758)"],
            ["\u5361\u7247\u6A19\u984C", "Noto Sans TC", "14px", "600 (Semibold)", "txt"],
            ["\u6B63\u6587", "Noto Sans TC", "13px", "400 (Regular)", "txt"],
            ["\u6B21\u8981\u6587\u5B57", "Noto Sans TC", "12px", "400 (Regular)", "txt-sec"],
            ["\u63D0\u793A / \u6A19\u7C64", "Noto Sans TC", "10-11px", "600 (Semibold)", "txt-ter"],
            ["\u5716\u8868\u8EF8\u6A19\u7C64", "Noto Sans TC", "11px", "400", "txt-ter (#94AEB0)"],
          ],
          [1800, 1800, 1200, 2000, 2560],
        ),

        heading("2.3 \u9670\u5F71\u8207\u5713\u89D2", HeadingLevel.HEADING_2),
        makeTable(
          ["\u5143\u7D20", "\u9670\u5F71", "\u5713\u89D2"],
          [
            ["\u5361\u7247\u5143\u4EF6", "shadow-sm", "rounded-xl (12px)"],
            ["\u5361\u7247 Hover", "shadow-md", "rounded-xl"],
            ["\u5C0E\u822A\u5217", "shadow-[0_2px_8px_rgba(0,87,88,0.15)]", "\u7121"],
            ["Chip \u6A19\u7C64", "\u7121", "rounded-md (6px)"],
            ["Badge \u5F98\u7AE0", "\u7121", "rounded-full"],
            ["\u982D\u50CF / \u5716\u793A\u5E95", "\u7121", "rounded-full / rounded-lg"],
          ],
          [3120, 4200, 2040],
        ),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 3. 共用元件
        // ═══════════════════════════════════════
        heading("3. \u5171\u7528\u5143\u4EF6 (Shared Components)", HeadingLevel.HEADING_1),

        heading("3.1 Chip \u6A19\u7C64", HeadingLevel.HEADING_2),
        para("\u7528\u65BC\u7BE9\u9078\u5668\u3001\u5207\u63DB\u6309\u9215\u3001\u5C0E\u51FA\u64CD\u4F5C\u7B49\u3002\u4E09\u7A2E\u8B8A\u9AD4\uFF1A"),
        makeTable(
          ["\u8B8A\u9AD4", "CSS Class", "\u8AAA\u660E"],
          [
            ["\u9810\u8A2D", ".chip", "\u767D\u8272\u80CC\u666F + \u7070\u8272\u908A\u6846"],
            ["\u6D3B\u52D5", ".chip-active", "\u6DFA\u7DA0\u80CC\u666F + \u4E3B\u8272\u908A\u6846\u8207\u6587\u5B57"],
            ["\u865B\u7DDA", ".chip-dashed", "\u865B\u7DDA\u908A\u6846\uFF0C\u7528\u65BC\u300C+ \u66F4\u591A\u300D\u7B49\u52D5\u4F5C"],
          ],
          [1800, 3000, 4560],
        ),
        spacer(100),
        ...codeBlock([
          ".chip {",
          "  font-size: 12px; padding: 5px 12px; border-radius: 6px;",
          "  border: 1px solid #e2e8ea; background: white; color: #5c7576;",
          "  display: inline-flex; align-items: center; gap: 4px;",
          "}",
          ".chip-active { border-color: #005758; background: #e8f4f4; color: #005758; font-weight: 600; }",
          ".chip-dashed { border: 1.5px dashed #e2e8ea; }",
        ]),

        heading("3.2 KPI \u5361\u7247 (_KpiCard.cshtml)", HeadingLevel.HEADING_2),
        para("\u900F\u904E ViewData \u50B3\u905E\u53C3\u6578\u7684\u5171\u7528 Partial View\u3002"),
        makeTable(
          ["\u53C3\u6578", "\u985E\u578B", "\u8AAA\u660E"],
          [
            ["title", "string", "\u6307\u6A19\u540D\u7A31\uFF08\u4F8B\uFF1A\u672C\u6708\u71DF\u6536\uFF09"],
            ["value", "string", "\u986F\u793A\u503C\uFF08\u4F8B\uFF1A$12.8M\uFF09"],
            ["trend", "string", "\u8DA8\u52E2\u767E\u5206\u6BD4\uFF08\u4F8B\uFF1A8.3 \u6216 -3.1\uFF09"],
            ["note", "string", "\u8A3B\u89E3\uFF08\u53EF\u7701\u7565\uFF0C\u4F8B\uFF1Avs \u4E0A\u6708\uFF09"],
          ],
          [1800, 1400, 6160],
        ),
        spacer(100),
        para("\u547C\u53EB\u7BC4\u4F8B\uFF1A"),
        ...codeBlock([
          '<partial name="_KpiCard"',
          '  view-data=\'new ViewDataDictionary(ViewData) {',
          '    { "title", "\u672C\u6708\u71DF\u6536" }, { "value", "$12.8M" },',
          '    { "trend", "8.3" }, { "note", "vs \u4E0A\u6708" }',
          '  }\' />',
        ]),

        heading("3.3 \u5074\u6B04\u5C0E\u822A (_Sidebar.cshtml)", HeadingLevel.HEADING_2),
        para("\u56FA\u5B9A\u5BEC\u5EA6 248px\uFF0C\u900F\u904E Alpine.js \u63A7\u5236\u5C55\u958B/\u6536\u5408\u72C0\u614B\u3002"),
        makeTable(
          ["\u7279\u6027", "\u5BE6\u4F5C\u65B9\u5F0F"],
          [
            ["\u5BEC\u5EA6", "w-[248px] min-w-[248px]"],
            ["\u5B9A\u4F4D", "sticky top-14\uFF0Cheight: calc(100vh - 56px)"],
            ["\u6EFE\u52D5", "overflow-y-auto"],
            ["\u5C55\u958B\u72C0\u614B", "x-data=\"{ expanded: '@currentDept' }\""],
            ["\u5B50\u5206\u985E", "x-show=\"expanded === '@d.Id'\" + x-cloak"],
            ["\u6D3B\u52D5\u6A23\u5F0F", "font-semibold text-pri bg-pri-light"],
            ["Hover \u6A23\u5F0F", "hover:bg-[#f8fafa]"],
          ],
          [3000, 6360],
        ),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 4. RWD
        // ═══════════════════════════════════════
        heading("4. \u97FF\u61C9\u5F0F\u8A2D\u8A08 (RWD)", HeadingLevel.HEADING_1),

        heading("4.1 \u65B7\u9EDE\u5B9A\u7FA9", HeadingLevel.HEADING_2),
        para("\u63A1\u7528 Tailwind CSS \u9810\u8A2D\u65B7\u9EDE\uFF0CMobile-First \u8A2D\u8A08\u7B56\u7565\u3002"),
        makeTable(
          ["\u65B7\u9EDE", "\u6700\u5C0F\u5BEC\u5EA6", "\u5C0D\u61C9\u88DD\u7F6E", "\u4E3B\u8981\u7528\u9014"],
          [
            ["\u9810\u8A2D", "0px", "\u624B\u6A5F", "\u57FA\u790E\u6A23\u5F0F"],
            ["sm", "640px", "\u5927\u624B\u6A5F / \u5C0F\u5E73\u677F", "\u986F\u793A\u641C\u5C0B\u6B04\u3001Logo \u6587\u5B57"],
            ["md", "768px", "\u5E73\u677F", "\u986F\u793A\u516C\u53F8\u9078\u64C7\u5668\u3001\u8ABF\u6574 padding"],
            ["lg", "1024px", "\u684C\u9762", "\u986F\u793A\u5074\u6B04\u3001\u56DB\u6B04 Grid"],
          ],
          [1400, 1600, 2400, 3960],
        ),

        heading("4.2 \u5143\u4EF6\u97FF\u61C9\u5F0F\u884C\u70BA", HeadingLevel.HEADING_2),
        makeTable(
          ["\u5143\u4EF6", "\u624B\u6A5F (\u9810\u8A2D)", "\u5E73\u677F (sm/md)", "\u684C\u9762 (lg+)"],
          [
            ["\u5074\u6B04", "\u96B1\u85CF\uFF0C\u6F22\u5821\u9078\u55AE\u958B\u555F", "\u96B1\u85CF\uFF0C\u6F22\u5821\u9078\u55AE\u958B\u555F", "\u56FA\u5B9A\u986F\u793A"],
            ["Logo \u6587\u5B57", "\u96B1\u85CF\uFF08\u53EA\u986F\u793A\u5716\u793A\uFF09", "\u986F\u793A", "\u986F\u793A"],
            ["\u641C\u5C0B\u6B04", "\u53F3\u4E0A\u89D2\u5716\u793A\uFF0C\u5C55\u958B\u8986\u84CB\u5C64", "\u5C0E\u822A\u5217\u5167\u5D4C", "\u5C0E\u822A\u5217\u5167\u5D4C"],
            ["KPI Grid", "2 \u6B04", "2 \u6B04", "4 \u6B04"],
            ["\u5716\u8868\u5340", "\u5806\u758A\u6392\u5217", "\u5806\u758A\u6392\u5217", "\u4E26\u6392 1.6:1"],
            ["\u5FEB\u901F\u5B58\u53D6", "2 \u6B04", "2 \u6B04", "4 \u6B04"],
            ["\u5831\u8868\u5361\u7247", "1 \u6B04", "2 \u6B04", "3 \u6B04"],
            ["\u8CC7\u6599\u8868\u683C", "\u6C34\u5E73\u6372\u52D5 (min-w-700px)", "\u6C34\u5E73\u6372\u52D5", "\u5B8C\u6574\u986F\u793A"],
            ["\u90E8\u9580 Tab", "Pill \u81A0\u56CA\u63DB\u884C", "\u5E95\u7DDA\u5F0F\u6A6B\u5411", "\u5E95\u7DDA\u5F0F\u6A6B\u5411"],
          ],
          [1800, 2400, 2400, 2760],
        ),

        heading("4.3 \u5074\u6B04\u624B\u6A5F\u7248\u5BE6\u4F5C", HeadingLevel.HEADING_2),
        para("Layout \u5C64\u7D1A\u900F\u904E Alpine.js \u63A7\u5236 sidebarOpen \u72C0\u614B\uFF1A"),
        makeTable(
          ["\u5143\u4EF6", "\u529F\u80FD"],
          [
            ["\u906E\u7F69\u5C64", "fixed inset-0 bg-black/40 z-30 lg:hidden\uFF0C\u9EDE\u64CA\u95DC\u9589\u5074\u6B04"],
            ["\u5074\u6B04\u5BB9\u5668", "fixed top-14 left-0 z-40\uFF0C\u900F\u904E translate-x \u63A7\u5236\u6ED1\u5165\u6ED1\u51FA"],
            ["\u6F22\u5821\u6309\u9215", "lg:hidden\uFF0C\u5207\u63DB sidebarOpen \u72C0\u614B"],
            ["\u52D5\u756B", "transition-transform duration-200 ease-in-out"],
          ],
          [3000, 6360],
        ),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 5. Alpine.js 模式
        // ═══════════════════════════════════════
        heading("5. Alpine.js \u4E92\u52D5\u6A21\u5F0F", HeadingLevel.HEADING_1),

        para("Alpine.js \u7528\u65BC\u8655\u7406\u4E0D\u9700\u8981\u4F3A\u670D\u5668\u53C3\u8207\u7684\u524D\u7AEF\u4E92\u52D5\uFF0C\u5305\u62EC\u5207\u63DB\u3001\u5C55\u958B\u6536\u5408\u3001\u8A02\u96B1\u8A02\u986F\u7B49\u3002"),

        heading("5.1 \u6A19\u6E96\u6A21\u5F0F\u5C0D\u7167", HeadingLevel.HEADING_2),
        makeTable(
          ["\u6A21\u5F0F", "\u6307\u4EE4", "\u7BC4\u4F8B"],
          [
            ["\u72C0\u614B\u5B9A\u7FA9", "x-data", "x-data=\"{ view: 'card', tab: 'all' }\""],
            ["\u689D\u4EF6\u986F\u793A", "x-show + x-cloak", "x-show=\"tab==='all'\" x-cloak"],
            ["\u52D5\u614B Class", ":class", ":class=\"active ? 'bg-pri' : 'bg-white'\""],
            ["\u9EDE\u64CA\u4E8B\u4EF6", "@@click", "@@click=\"view='table'\""],
            ["\u5916\u90E8\u9EDE\u64CA", "@@click.outside", "@@click.outside=\"isOpen = false\""],
            ["\u904E\u6E21\u52D5\u756B", "x-transition", "x-transition:enter=\"transition ease-out duration-150\""],
            ["DOM \u53C3\u7167", "x-ref", "x-ref=\"searchInput\""],
          ],
          [2000, 2800, 4560],
        ),

        heading("5.2 \u4F7F\u7528\u539F\u5247", HeadingLevel.HEADING_2),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "Alpine.js \u72C0\u614B\u50C5\u7528\u65BC\u7D14 UI \u4E92\u52D5\uFF08Tab \u5207\u63DB\u3001\u5074\u6B04\u5C55\u958B\uFF09\uFF0C\u4E0D\u7528\u65BC\u696D\u52D9\u908F\u8F2F", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u8CC7\u6599\u7BE9\u9078\u8207\u9801\u9762\u5C0E\u822A\u4EA4\u7531\u4F3A\u670D\u5668\u7AEF\u8655\u7406\uFF08Query String + PageModel\uFF09", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "x-cloak \u5FC5\u9808\u642D\u914D [x-cloak] { display: none !important } \u907F\u514D\u9583\u723D", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u5728 Razor Pages \u4E2D\u4F7F\u7528 @@click\uFF08\u96D9 @\uFF09\u4EE5\u907F\u514D\u8207 Razor \u8A9E\u6CD5\u885D\u7A81", size: 22, font: "Arial" })],
        }),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 6. Chart.js
        // ═══════════════════════════════════════
        heading("6. Chart.js \u5716\u8868\u898F\u7BC4", HeadingLevel.HEADING_1),

        heading("6.1 \u5716\u8868\u985E\u578B\u5C0D\u7167", HeadingLevel.HEADING_2),
        makeTable(
          ["\u9801\u9762", "\u5716\u8868", "Chart.js \u985E\u578B", "\u8CC7\u6599\u96C6\u6578"],
          [
            ["\u9996\u9801", "\u71DF\u6536\u8DA8\u52E2", "line (filled area)", "3 (\u672C\u5E74/\u53BB\u5E74/\u9810\u7B97)"],
            ["\u9996\u9801", "\u90E8\u9580\u6BD4\u8F03", "bar", "2 (\u5BE6\u969B/\u76EE\u6A19)"],
            ["\u660E\u7D30\u9801", "\u7269\u6599\u63A1\u8CFC\u8DA8\u52E2", "line (mixed area+line)", "4 (\u539F\u6599/\u5305\u6750/\u8A2D\u5099/\u5176\u4ED6)"],
            ["\u660E\u7D30\u9801", "\u63A1\u8CFC\u4F54\u6BD4", "doughnut", "4 (\u539F\u6599/\u5305\u6750/\u8A2D\u5099/\u5176\u4ED6)"],
          ],
          [1600, 2400, 2800, 2560],
        ),

        heading("6.2 \u5716\u8868\u5BB9\u5668\u898F\u7BC4", HeadingLevel.HEADING_2),
        para("\u91CD\u8981\uFF1AChart.js \u7684 canvas \u5FC5\u9808\u7528\u56FA\u5B9A\u9AD8\u5EA6\u7684 wrapper div \u5305\u88F9\uFF0C\u5426\u5247\u6703\u7121\u9650\u62C9\u9577\u3002"),
        ...codeBlock([
          "<!-- \u6B63\u78BA\u4F5C\u6CD5\uFF1A\u7528 relative + \u56FA\u5B9A\u9AD8\u5EA6\u5305\u88F9 -->",
          '<div class="relative h-[220px]">',
          '  <canvas id="chartId"></canvas>',
          "</div>",
          "",
          "<!-- \u932F\u8AA4\u4F5C\u6CD5\uFF1A\u76F4\u63A5\u7D66 canvas height -->",
          '<canvas id="chartId" height="220"></canvas>',
        ]),

        heading("6.3 \u5171\u7528\u914D\u7F6E", HeadingLevel.HEADING_2),
        para("\u6240\u6709\u5716\u8868\u7D71\u4E00\u4F7F\u7528\u4EE5\u4E0B\u57FA\u790E\u914D\u7F6E\uFF1A"),
        ...codeBlock([
          "const C = { pri: '#005758', acc: '#00b4b6', priH: '#006d6e',",
          "            ter: '#94aeb0', borderL: '#eef2f3' };",
          "",
          "options: {",
          "  responsive: true,",
          "  maintainAspectRatio: false,",
          "  plugins: { legend: { display: false } },",
          "  scales: {",
          "    x: { grid: { color: C.borderL },",
          "         ticks: { font: { size: 11 }, color: C.ter } },",
          "    y: { grid: { color: C.borderL },",
          "         ticks: { font: { size: 11 }, color: C.ter } }",
          "  }",
          "}",
        ]),

        heading("6.4 \u7DDA\u689D\u6A23\u5F0F\u5C0D\u7167", HeadingLevel.HEADING_2),
        makeTable(
          ["\u7528\u9014", "\u908A\u6846\u5BEC", "\u586B\u5145", "\u865B\u7DDA", "\u5713\u9EDE", "tension"],
          [
            ["\u4E3B\u8981\u6578\u64DA (\u672C\u5E74)", "2.5", "\u586B\u5145 (alpha 20%)", "\u7121", "0 (\u7121)", "0.4"],
            ["\u5C0D\u6BD4\u6578\u64DA (\u53BB\u5E74)", "1.5", "\u7121", "[4,3]", "0", "0.4"],
            ["\u57FA\u6E96\u7DDA (\u9810\u7B97)", "1", "\u7121", "[2,2]", "0", "0.4"],
            ["\u6B21\u8981\u6578\u64DA", "2", "\u586B\u5145 (alpha 30%)", "\u7121", "0", "0.4"],
            ["\u8F14\u52A9\u7DDA", "1", "\u7121", "[3,3]", "0", "0.4"],
          ],
          [2200, 1200, 2000, 1200, 1200, 1560],
        ),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 7. 資料流
        // ═══════════════════════════════════════
        heading("7. \u8CC7\u6599\u6D41\u8207\u9801\u9762\u5C0E\u822A", HeadingLevel.HEADING_1),

        heading("7.1 \u9801\u9762\u8DEF\u7531", HeadingLevel.HEADING_2),
        makeTable(
          ["\u9801\u9762", "\u8DEF\u5F91", "\u53C3\u6578", "PageModel"],
          [
            ["\u9996\u9801\u5100\u8868\u677F", "/", "\u7121", "IndexModel"],
            ["\u90E8\u9580\u5831\u8868", "/Department", "?dept=procurement", "DepartmentModel"],
            ["\u5831\u8868\u660E\u7D30", "/Report", "?dept=procurement&name=...", "ReportModel"],
          ],
          [2000, 2200, 3000, 2160],
        ),

        heading("7.2 \u8CC7\u6599\u50B3\u905E\u6A21\u5F0F", HeadingLevel.HEADING_2),
        para("\u76EE\u524D\u63A1\u7528\u975C\u614B\u8CC7\u6599\u985E\u5225 ReportData\uFF0C\u672A\u4F86\u53EF\u66FF\u63DB\u70BA\u8CC7\u6599\u5EAB\u67E5\u8A62\u3002"),
        new Paragraph({
          numbering: { reference: "bullets", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "PageModel.OnGet() \u63A5\u6536 Query String \u53C3\u6578\uFF0C\u67E5\u8A62\u8CC7\u6599\u5F8C\u50B3\u905E\u7D66 View", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "bullets", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "View \u900F\u904E Model.Property \u5B58\u53D6\u8CC7\u6599\uFF0C\u6216\u76F4\u63A5\u53C3\u7167 ReportData \u975C\u614B\u6210\u54E1", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "bullets", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "Partial View \u900F\u904E ViewDataDictionary \u50B3\u905E\u53C3\u6578", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "bullets", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u9801\u9762\u5C0E\u822A\u4F7F\u7528\u6A19\u6E96 <a href> + URL \u7DE8\u78BC\uFF08System.Net.WebUtility.UrlEncode\uFF09", size: 22, font: "Arial" })],
        }),

        heading("7.3 \u8CC7\u6599\u6A21\u578B", HeadingLevel.HEADING_2),
        makeTable(
          ["Model", "\u5C6C\u6027", "\u7528\u9014"],
          [
            ["Department", "Id, Label, Icon, Count, Subs", "\u90E8\u9580\u5C0E\u822A\u8207\u5206\u985E"],
            ["Report", "Name, Desc, Cat, Updated, Fav", "\u5831\u8868\u5361\u7247\u8207\u5217\u8868"],
            ["QuickAccess", "Dept, DeptId, Name, Tag", "\u9996\u9801\u5FEB\u901F\u5B58\u53D6"],
            ["MaterialRow", "Material, Supplier, Qty, UnitPrice, Amount, Change", "\u660E\u7D30\u8CC7\u6599\u8868\u683C"],
          ],
          [2000, 4200, 3160],
        ),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 8. 頁面範本
        // ═══════════════════════════════════════
        heading("8. \u9801\u9762\u7BC4\u672C\u8207 Grid \u4F48\u5C40", HeadingLevel.HEADING_1),

        heading("8.1 \u5100\u8868\u677F\u9801\u9762\u7D50\u69CB", HeadingLevel.HEADING_2),
        para("\u9069\u7528\u65BC\u9996\u9801\u3001\u7E3D\u89BD\u985E\u578B\u9801\u9762\u3002"),
        ...codeBlock([
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Header: \u6A19\u984C + \u7BE9\u9078 Chip \u5217",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " KPI Grid: grid-cols-2 lg:grid-cols-4",
          " [ KPI ] [ KPI ] [ KPI ] [ KPI ]",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Charts: grid-cols-1 lg:grid-cols-[1.6fr_1fr]",
          " [ \u4E3B\u5716\u8868 (1.6fr)    ] [ \u526F\u5716\u8868 (1fr) ]",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " \u5361\u7247\u5340: grid-cols-2 lg:grid-cols-4",
          " [ Card ] [ Card ] [ Card ] [ Card ]",
        ]),

        heading("8.2 \u5217\u8868\u9801\u9762\u7D50\u69CB", HeadingLevel.HEADING_2),
        para("\u9069\u7528\u65BC\u90E8\u9580\u5831\u8868\u3001\u8CC7\u6599\u5217\u8868\u985E\u578B\u9801\u9762\u3002"),
        ...codeBlock([
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Breadcrumb: \u9996\u9801 / \u90E8\u9580 / \u7576\u524D\u9801",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Header: Icon + \u6A19\u984C + \u641C\u5C0B\u6846 + \u6AA2\u8996\u5207\u63DB",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Tabs: [\u5168\u90E8] [\u5206\u985E1] [\u5206\u985E2] [\u5206\u985E3]",
          "        Desktop=\u5E95\u7DDA  Mobile=Pill\u81A0\u56CA",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " \u5361\u7247: grid-cols-1 sm:grid-cols-2 lg:grid-cols-3",
          " \u6216 \u8868\u683C: grid-cols-[...] + overflow-x-auto",
        ]),

        heading("8.3 \u660E\u7D30\u9801\u9762\u7D50\u69CB", HeadingLevel.HEADING_2),
        para("\u9069\u7528\u65BC\u5831\u8868\u660E\u7D30\u3001\u8CC7\u6599\u5206\u6790\u985E\u578B\u9801\u9762\u3002"),
        ...codeBlock([
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Breadcrumb + \u8FD4\u56DE\u6309\u9215",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Header + \u64CD\u4F5C\u6309\u9215 (\u6536\u85CF/Excel/PDF)",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Filter Bar: sticky top-14 z-40 (\u7BE9\u9078\u5217)",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Charts: grid-cols-1 lg:grid-cols-[1.6fr_1fr]",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " KPI Grid: grid-cols-2 lg:grid-cols-4",
          "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
          " Data Table: grid + \u5206\u9801 + overflow-x-auto",
        ]),

        new Paragraph({ children: [new PageBreak()] }),

        // ═══════════════════════════════════════
        // 9. z-index
        // ═══════════════════════════════════════
        heading("9. Z-Index \u8207 Sticky \u898F\u7BC4", HeadingLevel.HEADING_1),
        makeTable(
          ["\u5C64\u7D1A", "z-index", "\u5143\u4EF6"],
          [
            ["z-50", "50", "\u9802\u90E8\u5C0E\u822A\u5217 (TopNav)\u3001\u624B\u6A5F\u7248\u641C\u5C0B\u8986\u84CB\u5C64"],
            ["z-40", "40", "\u624B\u6A5F\u7248\u5074\u6B04\u3001\u660E\u7D30\u9801\u7BE9\u9078\u5217 (sticky)"],
            ["z-30", "30", "\u624B\u6A5F\u7248\u5074\u6B04\u906E\u7F69\u5C64 (overlay)"],
            ["\u9810\u8A2D", "auto", "\u4E00\u822C\u5167\u5BB9"],
          ],
          [2000, 1400, 5960],
        ),
        spacer(200),
        para("Sticky \u5143\u7D20\u5747\u4F7F\u7528 top-14 (56px) \u5C0D\u9F4A\u5C0E\u822A\u5217\u9AD8\u5EA6\u3002"),

        // ═══════════════════════════════════════
        // 10. 開發指南
        // ═══════════════════════════════════════
        heading("10. \u958B\u767C\u6307\u5357", HeadingLevel.HEADING_1),

        heading("10.1 \u958B\u767C\u74B0\u5883\u8A2D\u5B9A", HeadingLevel.HEADING_2),
        makeTable(
          ["\u9805\u76EE", "\u503C"],
          [
            ["\u57F7\u884C\u74B0\u5883", ".NET 10.0 SDK"],
            ["\u555F\u52D5\u6307\u4EE4", "dotnet run --project ReportCenter.Web"],
            ["\u9810\u8A2D Port", "5276 (http)"],
            ["\u5EFA\u7F6E\u6307\u4EE4", "dotnet build"],
          ],
          [3000, 6360],
        ),

        heading("10.2 \u547D\u540D\u6163\u4F8B", HeadingLevel.HEADING_2),
        makeTable(
          ["\u985E\u578B", "\u6163\u4F8B", "\u7BC4\u4F8B"],
          [
            ["Razor Page", "PascalCase", "Department.cshtml, Report.cshtml"],
            ["Partial View", "\u5E95\u7DDA\u958B\u982D + PascalCase", "_TopNav.cshtml, _KpiCard.cshtml"],
            ["PageModel", "Page\u540D\u7A31 + Model", "DepartmentModel, ReportModel"],
            ["CSS Class (Tailwind)", "utility-first", "grid-cols-4, text-pri, bg-surface"],
            ["CSS Class (\u81EA\u8A02)", "kebab-case", ".chip, .chip-active, .chip-dashed"],
            ["Alpine.js \u72C0\u614B", "camelCase", "sidebarOpen, mobileSearch, view"],
            ["Lucide Icon", "kebab-case", "bar-chart-3, chevron-down, arrow-up-right"],
          ],
          [2600, 3000, 3760],
        ),

        heading("10.3 \u65B0\u589E\u9801\u9762\u6AA2\u67E5\u6E05\u55AE", HeadingLevel.HEADING_2),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u5EFA\u7ACB .cshtml \u8207 .cshtml.cs \u6A94\u6848\uFF0C\u7E7C\u627F PageModel", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u4F7F\u7528 @section Styles \u548C @section Scripts \u653E\u7F6E\u9801\u9762\u5C08\u5C6C CSS \u548C JS", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u5957\u7528\u6A19\u6E96\u5BB9\u5668\uFF1Ap-4 md:p-6 pb-10 max-w-[1200px]", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u52A0\u5165 Breadcrumb \u5C0E\u822A", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u78BA\u8A8D RWD \u65B7\u9EDE\u884C\u70BA\uFF08\u624B\u6A5F / \u5E73\u677F / \u684C\u9762\uFF09", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u5716\u8868\u4F7F\u7528\u56FA\u5B9A\u9AD8\u5EA6 wrapper div", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "Alpine.js \u4E8B\u4EF6\u4F7F\u7528 @@click\uFF08\u96D9 @ \u907F\u514D Razor \u885D\u7A81\uFF09", size: 22, font: "Arial" })],
        }),
        new Paragraph({
          numbering: { reference: "numbers", level: 0 },
          spacing: { after: 80 },
          children: [new TextRun({ text: "\u66F4\u65B0\u5074\u6B04 _Sidebar.cshtml \u52A0\u5165\u65B0\u9801\u9762\u9023\u7D50\uFF08\u5982\u9700\u8981\uFF09", size: 22, font: "Arial" })],
        }),

        spacer(400),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          border: { top: { style: BorderStyle.SINGLE, size: 6, color: C.border, space: 12 } },
          spacing: { before: 200 },
          children: [new TextRun({ text: "\u2500 \u6587\u4EF6\u7D50\u675F \u2500", font: "Arial", size: 20, color: C.txtSec, italics: true })],
        }),
      ],
    },
  ],
});

// ── Generate ──
Packer.toBuffer(doc).then(buffer => {
  const outPath = "/Users/clyde/Desktop/CLIProject/ReportCenter-App/ReportCenter.Web/docs/ReportCenter-Frontend-Spec.docx";
  fs.writeFileSync(outPath, buffer);
  console.log("Generated: " + outPath);
});
