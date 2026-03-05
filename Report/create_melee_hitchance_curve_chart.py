# -*- coding: utf-8 -*-
"""
MeleeHitChance postProcessCurve 시뮬레이션 (raw 0 ~ 20, 1단위)
- RimWorld Core Stats_Pawns_Combat.xml 기반
- 선형 보간 적용
"""
from pathlib import Path
import numpy as np

# MeleeHitChance postProcessCurve 포인트 (Stats_Pawns_Combat.xml)
CURVE_POINTS = [
    (-20, 0.05),
    (-10, 0.10),
    (0.0, 0.50),
    (10, 0.80),
    (20, 0.90),
    (40, 0.96),
    (60, 0.98),
]


def evaluate_curve(x: float) -> float:
    """SimpleCurve.Evaluate: 선형 보간"""
    if x <= CURVE_POINTS[0][0]:
        return CURVE_POINTS[0][1]
    if x >= CURVE_POINTS[-1][0]:
        return CURVE_POINTS[-1][1]
    for i in range(len(CURVE_POINTS) - 1):
        x1, y1 = CURVE_POINTS[i]
        x2, y2 = CURVE_POINTS[i + 1]
        if x1 <= x <= x2:
            t = (x - x1) / (x2 - x1) if x2 != x1 else 0
            return y1 + (y2 - y1) * t
    return CURVE_POINTS[-1][1]


def main():
    try:
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt
        plt.rcParams["font.family"] = ["Malgun Gothic", "DejaVu Sans"]
        plt.rcParams["axes.unicode_minus"] = False
    except ImportError:
        print("matplotlib이 필요합니다. 설치: pip install matplotlib")
        return

    # 시뮬레이션: raw 0 ~ 20 (0.1 간격, 1단위 눈금)
    raw_values = np.arange(0, 20.1, 0.1)
    hit_chance = [evaluate_curve(x) * 100 for x in raw_values]

    fig, ax = plt.subplots(figsize=(12, 6))
    ax.plot(raw_values, hit_chance, color="#2e86ab", linewidth=2.5, label="MeleeHitChance (명중 확률)")
    ax.set_xlabel("Raw 입력값 (postProcessCurve 입력)", fontsize=11)
    ax.set_ylabel("명중 확률 (%)", fontsize=11)
    ax.set_title("MeleeHitChance postProcessCurve 시뮬레이션 (raw 0 ~ 20, 1단위)", fontsize=13)
    ax.set_xlim(0, 20)
    ax.set_xticks(range(0, 21))
    ax.set_ylim(45, 100)
    ax.grid(True, alpha=0.3, linestyle="--")
    ax.legend(loc="lower right", fontsize=10)

    # 주요 포인트 표시
    for x, y in [(0, 50), (10, 80), (20, 90)]:
        ax.axvline(x=x, color="gray", linestyle=":", alpha=0.5)
        ax.axhline(y=y, color="gray", linestyle=":", alpha=0.5)
        ax.scatter([x], [y], color="#e94f37", s=60, zorder=5)
        ax.annotate(f"({x}, {y}%)", (x, y), xytext=(5, 5), textcoords="offset points", fontsize=9)

    plt.tight_layout()
    out_path = Path(__file__).parent / "MeleeHitChance_postProcessCurve_Simulation.png"
    plt.savefig(out_path, dpi=150, bbox_inches="tight")
    plt.close()
    print(f"생성 완료: {out_path}")


if __name__ == "__main__":
    main()
