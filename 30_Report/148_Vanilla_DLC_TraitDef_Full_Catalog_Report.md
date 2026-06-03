# 림월드 바닐라 + DLC TraitDef 전수 목록

**태그:** TraitDef, Vanilla, Core, Biotech, Anomaly, 특성, RimWorld

## 조사 범위

| DLC | TraitDef 파일 | 신규 특성 |
|-----|---------------|-----------|
| Core | Traits_Singular.xml, Traits_Spectrum.xml | 51개 정의 / 73개 등급(degree) |
| Biotech | Biotech/Defs/TraitDefs/Traits.xml | 2 |
| Anomaly | Anomaly/Defs/TraitDefs/Traits.xml | 6 |
| Royalty | *(없음)* | 무기 특성(WeaponTraitDef)만 존재 |
| Ideology | Patches/Traits.xml | 신규 없음 — Kind 설명문만 패치 |
| Odyssey | *(없음)* | 무기 특성만 존재 |

> **폰 특성(TraitDef)** 과 **무기 특성(WeaponTraitDef)** 은 별개입니다. 본 보고서는 폰 특성만 포함합니다.

**합계:** TraitDef **51**개 · 플레이어에게 표시되는 특성 등급 **73**종

---

## Core — 단일 특성

| defName | degree | 한글명 | 한 줄 요약 |
|---------|--------|--------|------------|
| Nudist | +0 | 나체주의자 | 옷 없이 지낼 때 기분 보너스, 옷은 착용 가능. |
| Bloodlust | +0 | 피의 갈망 | 살해·부상 목격에 긍정, 사교적 다툼 4배, 인육·미매장·수감·노예에 무관심. |
| Kind | +0 | 다정다감 | 다툼·모욕 거의 없음, 위로 대화, 외모 판단 없음(Ideology 시 여론 순응↑). |
| Psychopath | +0 | 사이코패스 | 공감 없음, 인육·시체·수감·노예 무관심, 사교 기분 보너스 없음. |
| Cannibal | +0 | 식인종 | 인육 선호·기분 보너스, 도축·미매장에 덜 민감. |
| Abrasive | +0 | 직설적 | 모욕 확률↑, 사교 관계 악화. |
| TooSmart | +0 | 괴짜 천재 | 전역 학습 +75%, 정신이상 임계 +12%(붕괴 쉬움), 확신 상실 50%(Ideology). |
| Brawler | +0 | 싸움꾼 | 근접 명중↑, 원거리 무기 착용 시 기분↓(사격 특성과 상충). |
| Masochist | +0 | 피학적 | 고통·부상 시 기분 보너스. |
| NightOwl | +0 | 야행성 | 밤(23~6시) 각성 보너스, 낮(11~18시) 각성 디버프, 어둠 무패널티. |
| Greedy | +0 | 탐욕 | 침실 임프레션 미달 시 기분↓. |
| Jealous | +0 | 시샘 | 타인보다 낮은 침실 임프레션 시 기분↓. |
| Ascetic | +0 | 검소 | 호화 침실·요리 싫어함, 생식 무스트레스, 외모 판단 없음. |
| Gay | +0 | 동성애 | 동성에게만 연애 끌림. |
| Bisexual | +0 | 양성애 | 남녀 모두에게 연애 끌림. |
| Asexual | +0 | 무성애 | 성적 끌림 없음. |
| AnnoyingVoice | +0 | 거슬리는 목소리 | 사교 상호작용 시 상대 기분↓. |
| CreepyBreathing | +0 | 거친 숨소리 | 사교 상호작용 시 상대 기분↓. |
| Pyromaniac | +0 | 방화광 | 화재 미진화, 무작위 방화, 불·화염무기 주변 기분↑. |
| Wimp | +0 | 엄살쟁이 | 통증 한계 매우 낮음(소량 피해도 무력화). |
| Nimble | +0 | 날렵함 | 근접 회피 +15. |
| FastLearner | +0 | 빠른 학습가 | 전역 학습 속도 +75%. |
| SlowLearner | +0 | 느린 학습가 | 전역 학습 속도 -75%. |
| Undergrounder | +0 | 실내 선호 | 실내증·어둠 무패널티, 실외 시 기분↓. |
| Transhumanist | +0 | 신체 개조주의자 | 인공신체·제노젠 선호, 없으면 기분↓. |
| BodyPurist | +0 | 신체 순수주의자 | 인공신체·제노젠 혐오, 장착 시 기분↓. |
| DislikesMen | +0 | 남성 혐오 | 남성과 사교 시 기분↓. |
| DislikesWomen | +0 | 여성 혐오 | 여성과 사교 시 기분↓. |
| GreatMemory | +0 | 대단한 기억력 | 미사용 기술 퇴화 속도 50%. |
| Tough | +0 | 강인함 | 받는 피해 50% 감소. |
| TorturedArtist | +0 | 괴로운 예술가 | 상시 기분 디버프, 정신이상 후 50% 영감. |
| Gourmand | +0 | 식탐 | 허기 빠름, 폭식 충동, 좋은 음식에 기분↑. |
| QuickSleeper | +0 | 숙면가 | 수면 시간 약 2/3로 충분 회복. |

## Core — 스펙트럼 특성

| defName | degree | 한글명 | 한 줄 요약 |
|---------|--------|--------|------------|
| SpeedOffset | -1 | 느림보 | 이동속도 -0.2. |
| SpeedOffset | +1 | 가벼운 발 | 이동속도 +0.2. |
| SpeedOffset | +2 | 신속 | 이동속도 +0.4. |
| DrugDesire | +2 | 약물광 | 약물 강매력, 금지 정책 무시, 과다 복용. |
| DrugDesire | +1 | 약물선호 | 약물 선호, 금지 정책 무시. |
| DrugDesire | -1 | 금주 | 알코올·오락용 약물 엄격 회피. |
| NaturalMood | +2 | 낙천적 | 상시 기분 +12. |
| NaturalMood | +1 | 긍정적 | 상시 기분 +6. |
| NaturalMood | -1 | 부정적 | 상시 기분 -6. |
| NaturalMood | -2 | 우울증 | 상시 기분 -12. |
| Nerves | +2 | 철의 의지 | 정신이상 임계 -18%(낮은 기분까지 버팀), 확신 상실 25%(Ideology). |
| Nerves | +1 | 확고한 의지 | 정신이상 임계 -9%, 확신 상실 50%(Ideology). |
| Nerves | -1 | 신경과민 | 정신이상 임계 +8%(쉽게 붕괴), 확신 상실 ×2(Ideology). |
| Nerves | -2 | 유리정신 | 정신이상 임계 +15%, 확신 상실 ×3(Ideology). |
| Neurotic | +1 | 강박증 | 작업속도 +20%, 정신이상 임계 +8%(붕괴 쉬움). |
| Neurotic | +2 | 심각한 강박증 | 작업속도 +40%, 정신이상 임계 +14%. |
| Industriousness | +2 | 일벌레 | 작업속도 +35%. |
| Industriousness | +1 | 근면성실 | 작업속도 +20%. |
| Industriousness | -1 | 게으름 | 작업속도 -20%. |
| Industriousness | -2 | 나태 | 작업속도 -35%. |
| PsychicSensitivity | +2 | 정신적 초감각 | 초능력 민감도 +80%. |
| PsychicSensitivity | +1 | 민감한 정신 | 초능력 민감도 +40%. |
| PsychicSensitivity | -1 | 둔감한 정신 | 초능력 민감도 -50%. |
| PsychicSensitivity | -2 | 정신적 무감각 | 초능력 민감도 -100%(사실상 무감). |
| ShootingAccuracy | +1 | 신중한 사수 | 조준 지연 +25%, 사격 명중 +5. |
| ShootingAccuracy | -1 | 난사광 | 조준 지연 -50%, 사격 명중 -5. |
| Beauty | +2 | 아름다움 | 외모 +2(사교 유리). |
| Beauty | +1 | 잘생김 | 외모 +1. |
| Beauty | -1 | 못생김 | 외모 -1. |
| Beauty | -2 | 충격적인 외모 | 외모 -2(사교 불리). |
| Immunity | +1 | 면역체질 | 항체 형성 +30%. |
| Immunity | -1 | 병약 체질 | 면역 약함, 희귀 질병 랜덤 발생↑. |

## Biotech

| defName | degree | 한글명 | 한 줄 요약 |
|---------|--------|--------|------------|
| Delicate | +0 | 연약함 | 받는 피해 15% 증가(Tough 반대). |
| Recluse | +0 | 은둔자 | 세력 인원 수 적을수록 기분↑. |

## Anomaly

| defName | degree | 한글명 | 한 줄 요약 |
|---------|--------|--------|------------|
| PerfectMemory | +0 | 완벽한 기억력 | 기술 퇴화 없음(GreatMemory 상위). |
| Occultist | +0 | 신비학자 | 어노말리 연구 가속, 주기적으로 지식 전수. |
| Joyous | +0 | 환희의 전령 | 주변 폰 기분·영감 부여(능동 효과). |
| BodyMastery | +0 | 신체 통달 | 식사·수면·쾌적 불필요, 흰 눈(특수 NPC용). |
| Disturbing | +0 | 음침함 | 대화 상대 기분↓. |
| VoidFascination | +0 | 공허 매료 | 부자연 실체 매료·연구 가속, 관련 기분 변동. |

---

## 참고

- **데이터 출처:** `RimworldData/{Core,Biotech,Anomaly}/Defs/TraitDefs/` (2025-05 기준 워크스페이스)
- **Anomaly 6종:** `commonality=0` — 랜덤 생성 풀에 없고 이벤트·특수 폰 전용.
- **상충 예:** `Brawler` ↔ `ShootingAccuracy`, `TooSmart` ↔ `Nerves`/`SlowLearner`, `FastLearner` ↔ `SlowLearner`.
- **무기 특성:** Royalty·Odyssey `WeaponTraitDefs.xml` 은 본 목록 제외.
