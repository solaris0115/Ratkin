# Ratkin 모드 Research 의존성 관계 시각화

## 개요

Ratkin 모드 프로젝트 내 ResearchProjectDef의 prerequisite(선행 연구) 관계를 시각화한 자료입니다.

**데이터 소스**: `Project/1.6/Defs/ResearchDefs/ResearchProjects.xml`

---

## 전체 연구 흐름도

```
원본 연구 (RimWorld)
│
├─ LongBlades ────────────────┐
│                              │
├─ Machining ──────────────────┼─┐
│                              │ │
├─ Greatbow ───────────────────┼─┼─┐
│                              │ │ │
├─ Stonecutting ───────────────┼─┼─┼─┐
│                              │ │ │ │
├─ MicroelectronicsBasics ────┼─┼─┼─┼─┐
│                              │ │ │ │ │
└─ ComplexClothing ────────────┼─┼─┼─┼─┼─┐
                               │ │ │ │ │ │
[Ratkin 연구]                  │ │ │ │ │ │
                               │ │ │ │ │ │
├─ PiercingWeapon ─────────────┘ │ │ │ │ │
│   (핵심 연구: 3개가 의존)      │ │ │ │ │
│                                │ │ │ │ │
├─ MechanicalWeapon ─────────────┘ │ │ │ │
│                                  │ │ │ │
├─ Ballista ───────────────────────┘ │ │ │
│   (2개가 의존)                     │ │ │
│                                    │ │ │
├─ Advanced_BallistaBolt ────────────┘ │ │
│                                      │ │
├─ Bigmouse (Cannon) ──────────────────┘ │
│                                        │
├─ MicroOptical ─────────────────────────┘
│   (1개가 의존)
│
├─ FlechetteBullet ────────────────────┐
│                                      │
├─ RatkinClothing ─────────────────────┼─┐
│                                      │ │
└─ HighClassClothing ───────────────────┘ │
                                          │
[연결 관계]                               │
PiercingWeapon ───────────────────────────┘
```

---

## 연구 그룹별 흐름

### 1. 무기 연구 계열
```
LongBlades → PiercingWeapon
PiercingWeapon → MechanicalWeapon (Machining)
PiercingWeapon → Advanced_BallistaBolt
PiercingWeapon → FlechetteBullet
```

### 2. 발리스타/대포 연구 계열
```
Greatbow + Stonecutting → Ballista
Ballista → Advanced_BallistaBolt (PiercingWeapon)
Ballista → Bigmouse
```

### 3. 산업 시대 연구 계열
```
MicroelectronicsBasics → MicroOptical
MicroOptical → FlechetteBullet (PiercingWeapon)
```

### 4. 의류 연구 계열
```
ComplexClothing → RatkinClothing
RatkinClothing → HighClassClothing
```

---

## 핵심 연구

**PiercingWeapon** (가장 중요)
- 3개의 연구가 의존: MechanicalWeapon, Advanced_BallistaBolt, FlechetteBullet

**Ballista**
- 2개의 연구가 의존: Advanced_BallistaBolt, Bigmouse

**MicroOptical**
- 1개의 연구가 의존: FlechetteBullet

**RatkinClothing**
- 1개의 연구가 의존: HighClassClothing

---

## 통계 요약

- **총 Ratkin 연구 수**: 9개
- **원본 연구 의존**: 6개
- **최대 연구 깊이**: 3단계
- **Medieval 시대**: 7개
- **Industrial 시대**: 2개

---

**마지막 업데이트**: 2025-01-XX  
**데이터 소스**: `Project/1.6/Defs/ResearchDefs/ResearchProjects.xml`

