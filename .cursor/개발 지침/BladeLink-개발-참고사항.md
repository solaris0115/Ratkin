# BladeLink 개발시 참고 사항

BladeLink 무기 생성 시 원본 무기 대비 스탯 조정 가이드라인.

## 핵심 원칙

- **DPS 기준**: power와 cooldown을 조정하여 목표 DPS 달성
- **관통 유지**: `armorPenetration`은 **절대 변경하지 않음**
- **참고 수치**: 원본 DPS 20 → BladeLink DPS 27 수준 (약 35% 상승)

## DPS 계산

```
DPS = power / cooldownTime
```

목표 DPS 비율: `27 / 20 ≈ 1.35` (원본 대비 35% 상승)

## 조정 방법

power와 cooldown을 **분배**하여 목표 DPS에 맞춘다.

| 방식 | 예시 (원본 power 25, cooldown 2, DPS 12.5) |
|------|--------------------------------------------|
| power만 상승 | power 34, cooldown 2 → DPS 17 |
| cooldown만 감소 | power 25, cooldown 1.48 → DPS 16.9 |
| 둘 다 조정 | power 27, cooldown 1.6 → DPS 16.9 (MonoSword BladeLink 참고) |

### 원본 vs BladeLink 참고 (MonoSword)

| 항목 | 원본 | BladeLink |
|------|------|-----------|
| power | 25 | 27 |
| cooldownTime | 2.0 | 1.6 |
| armorPenetration | 0.9 | 0.9 (동일) |
| DPS | 20.62 | 27.84 |

## 금지 사항

- **armorPenetration 수정 금지**: 관통력은 원본과 동일하게 유지
