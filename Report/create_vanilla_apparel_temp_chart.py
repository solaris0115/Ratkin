# -*- coding: utf-8 -*-
"""
림월드 코어 + DLC(바이오텍/로열티/이데올로기/어노말리/오디세이) 의상 소재별 온도 차트 엑셀 생성
- 쾌적 온도(Cold/Heat) 입력 시 defName별 차트 자동 갱신
"""
from pathlib import Path

try:
    from openpyxl import Workbook
    from openpyxl.chart import BarChart, Reference
    from openpyxl.styles import Font, Alignment, Border, Side
    from openpyxl.utils import get_column_letter
except ImportError:
    print("openpyxl이 필요합니다. 설치: pip install openpyxl")
    exit(1)

# 기본값 (Human: 16~26°C)
DEFAULT_COLD, DEFAULT_HEAT = 16, 26
LEATHER_COLD, LEATHER_HEAT = 16, 16
BLUEFUR_COLD, BLUEFUR_HEAT = 20, 16

# Core + Biotech + Royalty + Ideology + Anomaly 통합 (Odyssey Vacsuit는 costList 고정이라 제외)
ALL_VANILLA_STUFF_APPAREL = [
    # Core
    ("Apparel_TribalA", 0.55, 0.55),
    ("Apparel_Parka", 2.00, 0.00),
    ("Apparel_Pants", 0.20, 0.08),
    ("Apparel_BasicShirt", 0.22, 0.10),
    ("Apparel_CollarShirt", 0.26, 0.10),
    ("Apparel_Duster", 0.60, 0.85),
    ("Apparel_Jacket", 0.80, 0.30),
    ("Apparel_PlateArmor", 1.0, 0),
    ("Apparel_CowboyHat", 0.10, 0.50),
    ("Apparel_BowlerHat", 0.10, 0.40),
    ("Apparel_TribalHeaddress", 0.10, 0.15),
    ("Apparel_Tuque", 0.50, 0),
    ("Apparel_WarMask", 0.05, 0.05),
    ("Apparel_WarVeil", 0.05, 0.05),
    ("Apparel_SimpleHelmet", 0.15, 0),
    ("Apparel_AdvancedHelmet", 0.15, 0),
    ("Apparel_Cape", 0.60, 0.85),
    ("Apparel_Robe", 0.80, 0.25),
    ("Apparel_HatHood", 0.10, 0.25),
    ("Apparel_ClothMask", 0.02, 0.02),
    # Biotech
    ("Apparel_KidRomper", 0.22, 0.10),
    ("Apparel_KidShirt", 0.22, 0.10),
    ("Apparel_KidPants", 0.20, 0.08),
    ("Apparel_KidParka", 1.5, 0),
    ("Apparel_KidTribal", 0.5, 0.5),
    ("Apparel_KidHelmet", 0.15, 0),
    ("Apparel_Bandolier", 0.1, 0.1),
    ("Apparel_Sash", 0.1, 0.1),
    # Royalty
    ("Apparel_ShirtRuffle", 0.22, 0.10),
    ("Apparel_Corset", 0.4, 0.10),
    ("Apparel_VestRoyal", 0.4, 0.10),
    ("Apparel_RobeRoyal", 0.80, 0.25),
    ("Apparel_HatLadies", 0.10, 0.25),
    ("Apparel_HatTop", 0.10, 0.25),
    ("Apparel_Beret", 0.07, 0.15),
    # Ideology
    ("Apparel_BodyStrap", 0.10, 0.10),
    ("Apparel_Burka", 0.1, 0.2),
    ("Apparel_Blindfold", 0.35, 0),
    ("Apparel_Headwrap", 0.1, 0.1),
    ("Apparel_Broadwrap", 0.2, 0.2),
    ("Apparel_VisageMask", 0.05, 0.05),
    ("Apparel_Slicecap", 0, 0.1),
    ("Apparel_AuthorityCap", 0.1, 0.1),
    ("Apparel_Tailcap", 0.5, 0),
    ("Apparel_Shadecone", 0.10, 0.50),
    ("Apparel_Flophat", 0.50, 0),
    # Anomaly
    ("Apparel_LabCoat", 0.50, 0.60),
]


def create_excel(output_path: str, stuff_apparel: list, title: str = "쾌적 온도"):
    wb = Workbook()
    ws = wb.active
    ws.title = "소재별 온도"

    header_font = Font(bold=True)
    thin_border = Border(
        left=Side(style="thin"),
        right=Side(style="thin"),
        top=Side(style="thin"),
        bottom=Side(style="thin"),
    )

    # === 1. 상단 입력 영역 ===
    ws["A1"] = title
    ws["B1"] = "Cold"
    ws["C1"] = "Heat"
    ws["F1"] = "소재A"
    ws["G1"] = "Cold"
    ws["H1"] = "Heat"
    ws["I1"] = "소재B"
    ws["J1"] = "Cold"
    ws["K1"] = "Heat"
    ws["A1"].font = Font(bold=True, size=12)
    for c in ["B", "C", "G", "H", "J", "K"]:
        ws[f"{c}1"].font = header_font

    ws["A2"] = "(입력값 변경 시 차트 자동 갱신)"
    ws["B2"] = DEFAULT_COLD
    ws["C2"] = DEFAULT_HEAT
    ws["F2"] = "가죽"
    ws["G2"] = LEATHER_COLD
    ws["H2"] = LEATHER_HEAT
    ws["I2"] = "머플로"
    ws["J2"] = BLUEFUR_COLD
    ws["K2"] = BLUEFUR_HEAT

    ws.column_dimensions["A"].width = 28
    for c in "BCDEFGHIJK":
        ws.column_dimensions[c].width = 10

    cold_cell = "$B$2"
    heat_cell = "$C$2"
    mat_a_cold = "$G$2"
    mat_a_heat = "$H$2"
    mat_b_cold = "$J$2"
    mat_b_heat = "$K$2"

    # === 2. 데이터 테이블 ===
    start_row = 6
    headers = [
        "defName", "유형", "Cold배수", "Heat배수", "고정Cold", "고정Heat",
        "소재A Ins_Cold", "소재A Ins_Heat", "소재B Ins_Cold", "소재B Ins_Heat",
        "소재A Min°C", "소재A Max°C", "소재B Min°C", "소재B Max°C",
    ]
    for col, h in enumerate(headers, 1):
        cell = ws.cell(row=start_row, column=col, value=h)
        cell.font = header_font
        cell.border = thin_border
        cell.alignment = Alignment(horizontal="center", wrap_text=True)

    data_row = start_row + 1
    for def_name, cold_mult, heat_mult in stuff_apparel:
        ws.cell(row=data_row, column=1, value=def_name)
        ws.cell(row=data_row, column=2, value="재료의존")
        ws.cell(row=data_row, column=3, value=cold_mult)
        ws.cell(row=data_row, column=4, value=heat_mult)
        ws.cell(row=data_row, column=5, value=0)
        ws.cell(row=data_row, column=6, value=0)
        ws.cell(row=data_row, column=7, value=f"=C{data_row}*{mat_a_cold}")
        ws.cell(row=data_row, column=8, value=f"=D{data_row}*{mat_a_heat}")
        ws.cell(row=data_row, column=9, value=f"=C{data_row}*{mat_b_cold}")
        ws.cell(row=data_row, column=10, value=f"=D{data_row}*{mat_b_heat}")
        ws.cell(row=data_row, column=11, value=f"={cold_cell}-G{data_row}")
        ws.cell(row=data_row, column=12, value=f"={heat_cell}+H{data_row}")
        ws.cell(row=data_row, column=13, value=f"={cold_cell}-I{data_row}")
        ws.cell(row=data_row, column=14, value=f"={heat_cell}+J{data_row}")
        for c in range(1, 15):
            ws.cell(row=data_row, column=c).border = thin_border
        data_row += 1

    last_data_row = data_row - 1

    # === 3. 차트 ===
    chart_row = last_data_row + 3
    ws.cell(row=chart_row, column=1, value="차트 (defName별 착용 온도 범위)")
    ws.cell(row=chart_row, column=1).font = Font(bold=True, size=11)
    chart_row += 1

    chart = BarChart()
    chart.type = "bar"
    chart.style = 10
    chart.title = f"의상별 소재 착용 온도 (°C) - {title}·소재A/B StuffPower 입력 시 자동 갱신"
    chart.x_axis.title = "온도 (°C)"
    chart.y_axis.title = "defName"

    data = Reference(ws, min_col=11, min_row=start_row, max_col=14, max_row=last_data_row)
    cats = Reference(ws, min_col=1, min_row=start_row + 1, max_row=last_data_row)
    chart.add_data(data, titles_from_data=True)
    chart.set_categories(cats)
    chart.width = 22
    chart.height = 18
    ws.add_chart(chart, f"A{chart_row}")

    col_widths = {1: 22, 2: 10, 3: 8, 4: 8, 5: 8, 6: 8, 7: 12, 8: 12, 9: 12, 10: 12, 11: 10, 12: 10, 13: 10, 14: 10}
    for c, w in col_widths.items():
        ws.column_dimensions[get_column_letter(c)].width = w

    wb.save(output_path)
    print(f"생성 완료: {output_path}")


if __name__ == "__main__":
    script_dir = Path(__file__).parent
    create_excel(
        str(script_dir / "Vanilla_AllDLC_Apparel_Leather_vs_Bluefur_Temperature.xlsx"),
        ALL_VANILLA_STUFF_APPAREL,
        title="쾌적 온도 (Human 16~26°C)",
    )
