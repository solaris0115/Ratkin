# -*- coding: utf-8 -*-
"""
119 보고서 기반 Ratkin 의상 소재별 온도 차트 엑셀 생성 스크립트
- 랫킨 쾌적 온도(Cold/Heat) 입력 시 defName별 차트 자동 갱신
"""
import os
from pathlib import Path

try:
    from openpyxl import Workbook
    from openpyxl.chart import BarChart, Reference
    from openpyxl.styles import Font, Alignment, Border, Side
    from openpyxl.utils import get_column_letter
except ImportError:
    print("openpyxl이 필요합니다. 설치: pip install openpyxl")
    exit(1)

# 상수
LEATHER_COLD, LEATHER_HEAT = 16, 16
BLUEFUR_COLD, BLUEFUR_HEAT = 20, 16
DEFAULT_COLD, DEFAULT_HEAT = 21, 26

# 재료 의존 단열 의상 (Cold배수, Heat배수) - Heat 0이면 열기 단열 없음
STUFF_APPAREL = [
    ("RK_ApronSkirt", 0.20, 0.10),
    ("RK_ApronSkirtChildren", 0.20, 0.10),
    ("RK_SummerDress", 0.10, 0.70),
    ("RK_Muffler", 0.65, 0.25),
    ("RK_WoolenHat", 0.50, 0),
    ("RK_WorkerWear", 0.30, 0.20),
    ("RK_Coif", 0.15, 0.10),
    ("RK_ResearchGown", 0.30, 0.15),
    ("RK_ExplorerWear", 0.20, 0.40),
    ("RK_ExplorerHat", 0.15, 0.25),
    ("RK_ChefSuit", 0.25, 0.25),
    ("RK_ChefHat", 0.15, 0.15),
    ("RK_GaurdenUniform", 0.40, 0.25),
    ("RK_OrderUniform", 0.40, 0.40),
    ("RK_BulletProofHelmet", 0.25, 0.15),
    ("RK_FlatColorCoat", 0.40, 0.35),
    ("RK_FrillOnepiece", 0.35, 0.40),
    ("RK_SistersDerss", 0.35, 0.45),
    ("RK_SistersVeil", 0.25, 0.15),
    ("RK_BattleSuit", 0.55, 0.45),
    ("RK_Apparel_GasMask", 0.55, 0.45),
    ("RK_HeadBand", 0.15, 0.15),
    ("RK_SantaHat", 0.65, 0),
]

# 고정 + 재료 의존 혼합 (고정Cold, 고정Heat, Cold배수, Heat배수)
HYBRID_APPAREL = [
    ("RK_Cardigan", 8, 0, 0.95, 0.30),
    ("RK_WinterRobe", 8, 0, 1.20, 0),
    ("RK_RoyalRobe", 12, 0, 1.40, 0),
    ("RK_SantaRobe", 13, 0, 1.30, 0),
]


def create_excel(output_path: str):
    wb = Workbook()
    ws = wb.active
    ws.title = "소재별 온도"

    # 스타일
    header_font = Font(bold=True)
    thin_border = Border(
        left=Side(style="thin"),
        right=Side(style="thin"),
        top=Side(style="thin"),
        bottom=Side(style="thin"),
    )

    # === 1. 상단 입력 영역 ===
    # 랫킨 쾌적 온도 | Cold | Heat | | | 소재A | Cold | Heat | 소재B | Cold | Heat
    ws["A1"] = "랫킨 쾌적 온도"
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

    # === 2. 데이터 테이블 헤더 ===
    start_row = 6
    headers = [
        "defName",
        "유형",
        "Cold배수",
        "Heat배수",
        "고정Cold",
        "고정Heat",
        "소재A Ins_Cold",
        "소재A Ins_Heat",
        "소재B Ins_Cold",
        "소재B Ins_Heat",
        "소재A Min°C",
        "소재A Max°C",
        "소재B Min°C",
        "소재B Max°C",
    ]
    for col, h in enumerate(headers, 1):
        cell = ws.cell(row=start_row, column=col, value=h)
        cell.font = header_font
        cell.border = thin_border
        cell.alignment = Alignment(horizontal="center", wrap_text=True)

    data_row = start_row + 1

    # === 3. 재료 의존 데이터 + 수식 ===
    for def_name, cold_mult, heat_mult in STUFF_APPAREL:
        ws.cell(row=data_row, column=1, value=def_name)
        ws.cell(row=data_row, column=2, value="재료의존")
        ws.cell(row=data_row, column=3, value=cold_mult)
        ws.cell(row=data_row, column=4, value=heat_mult)
        ws.cell(row=data_row, column=5, value=0)
        ws.cell(row=data_row, column=6, value=0)

        # 소재A Insulation = Cold배수*소재A StuffPower
        ws.cell(row=data_row, column=7, value=f"=C{data_row}*{mat_a_cold}")
        ws.cell(row=data_row, column=8, value=f"=D{data_row}*{mat_a_heat}")

        # 소재B Insulation
        ws.cell(row=data_row, column=9, value=f"=C{data_row}*{mat_b_cold}")
        ws.cell(row=data_row, column=10, value=f"=D{data_row}*{mat_b_heat}")

        # Min/Max 수식 (Cold - Ins_Cold, Heat + Ins_Heat)
        ws.cell(row=data_row, column=11, value=f"={cold_cell}-G{data_row}")
        ws.cell(row=data_row, column=12, value=f"={heat_cell}+H{data_row}")
        ws.cell(row=data_row, column=13, value=f"={cold_cell}-I{data_row}")
        ws.cell(row=data_row, column=14, value=f"={heat_cell}+J{data_row}")

        for c in range(1, 15):
            ws.cell(row=data_row, column=c).border = thin_border
        data_row += 1

    # === 4. 혼합 의상 데이터 ===
    for def_name, fix_cold, fix_heat, cold_mult, heat_mult in HYBRID_APPAREL:
        ws.cell(row=data_row, column=1, value=def_name)
        ws.cell(row=data_row, column=2, value="혼합")
        ws.cell(row=data_row, column=3, value=cold_mult)
        ws.cell(row=data_row, column=4, value=heat_mult)
        ws.cell(row=data_row, column=5, value=fix_cold)
        ws.cell(row=data_row, column=6, value=fix_heat)

        ws.cell(row=data_row, column=7, value=f"=E{data_row}+C{data_row}*{mat_a_cold}")
        ws.cell(row=data_row, column=8, value=f"=F{data_row}+D{data_row}*{mat_a_heat}")
        ws.cell(row=data_row, column=9, value=f"=E{data_row}+C{data_row}*{mat_b_cold}")
        ws.cell(row=data_row, column=10, value=f"=F{data_row}+D{data_row}*{mat_b_heat}")

        ws.cell(row=data_row, column=11, value=f"={cold_cell}-G{data_row}")
        ws.cell(row=data_row, column=12, value=f"={heat_cell}+H{data_row}")
        ws.cell(row=data_row, column=13, value=f"={cold_cell}-I{data_row}")
        ws.cell(row=data_row, column=14, value=f"={heat_cell}+J{data_row}")

        for c in range(1, 15):
            ws.cell(row=data_row, column=c).border = thin_border
        data_row += 1

    last_data_row = data_row - 1

    # === 5. 차트용 데이터 (가로 막대: defName vs Min~Max 범위) ===
    # 차트는 가죽 Min, 가죽 Max, 머플로 Min, 머플로 Max를 보여주는 게 좋음
    # 또는 각 의상별로 가죽/머플로 착용온도 범위를 막대로 표시

    chart_row = last_data_row + 3
    ws.cell(row=chart_row, column=1, value="차트 데이터 (defName별 착용 온도 범위)")
    ws.cell(row=chart_row, column=1).font = Font(bold=True, size=11)
    chart_row += 1

    # 가로 막대 차트: 카테고리=defName, 시리즈=가죽Min,가죽Max,머플로Min,머플로Max
    # 또는 더 직관적으로: 가죽 범위(Min~Max), 머플로 범위(Min~Max) 2개 시리즈
    # 수평 막대에서 각 의상마다 2개 막대(가죽, 머플로)가 있고, 막대 길이가 Min~Max

    # 차트 생성: X축=defName, Y축=온도. 가죽 Min~Max, 머플로 Min~Max 영역 표시
    # Excel에서 범위 차트는 보통 XY 또는 가로 막대. 여기서는 "가죽 Min", "가죽 Max", "머플로 Min", "머플로 Max" 4개 시리즈
    # 또는 "가죽 범위폭", "머플로 범위폭" + "가죽 Min", "머플로 Min" 스택드 바

    # 간단히: defName | 가죽 Min | 가죽 Max | 머플로 Min | 머플로 Max
    # 100% 스택 수평 막대보다는, 그룹 막대가 나음. 가죽(Min, Max), 머플로(Min, Max)
    # 실제로 "착용 온도 범위"를 보여주려면 Min과 Max를 같은 막대에 표시하는 게 좋음
    # 수평 막대: 카테고리=defName, 값=Min(막대시작), Max(막대끝) -> 이건 Excel 기본으로 어려움
    # 대안: Min과 Max-Min(범위폭) 스택 바. 또는 그냥 Min, Max 2개 막대

    # 실용적으로: defName | 가죽 Min | 가죽 Max | 머플로 Min | 머플로 Max
    # 가로 막대 차트 (defName이 Y축에 표시되어 가독성 좋음)
    chart = BarChart()
    chart.type = "bar"
    chart.style = 10
    chart.title = "의상별 소재 착용 온도 (°C) - 랫킨 쾌적온도·소재A/B StuffPower 입력 시 자동 갱신"
    chart.x_axis.title = "온도 (°C)"
    chart.y_axis.title = "defName"

    # 데이터: K~N열 (가죽 Min, 가죽 Max, 머플로 Min, 머플로 Max)
    data = Reference(ws, min_col=11, min_row=start_row, max_col=14, max_row=last_data_row)
    cats = Reference(ws, min_col=1, min_row=start_row + 1, max_row=last_data_row)
    chart.add_data(data, titles_from_data=True)
    chart.set_categories(cats)

    # 차트 크기 및 위치
    chart.width = 22
    chart.height = 18
    ws.add_chart(chart, f"A{chart_row}")

    # 열 너비
    col_widths = {1: 22, 2: 10, 3: 8, 4: 8, 5: 8, 6: 8, 7: 12, 8: 12, 9: 12, 10: 12, 11: 10, 12: 10, 13: 10, 14: 10}
    for c, w in col_widths.items():
        ws.column_dimensions[get_column_letter(c)].width = w

    wb.save(output_path)
    print(f"생성 완료: {output_path}")


if __name__ == "__main__":
    script_dir = Path(__file__).parent
    output = script_dir / "119_Ratkin_Apparel_Leather_vs_Bluefur_Temperature.xlsx"
    create_excel(str(output))
