이름: 파이어웍스 플레임넛

유년기 배경: 불장난 소년
000은 불장난을 좋아하는 아이입니다. 병충해로 밭을 불태우는 것을 보고 불에 완전히 매료 되었습니다.
어른들은 주의를 주었지만 000은 한귀로 흘려 듣곤 했습니다.

연구 +1
사교 -3
(방화광 제외)

성년기 배경: 전문 방화꾼
000은 불에 대한 전문가이며 이 때문에 000은 악명과 명성 모두 자자한 용병입니다.
마을에 병충해가 퍼지면 불을 질러 필요한 만큼만 불태우고, 적진에 침입했을 땐 유류고 같은 가장 취약하고 치명적인 곳에 불을 지릅니다.
아주 가끔은 이런 지식을 이용해 효과적으로 소방활동을 하거나 마구잡이로 불을 지르는 방화광들을 사냥하기도 합니다.

연구 +2
사교 -2
사격 +4
격투 +2
(방화광 제외)

무기: 도끼(빨갛게 염질된)
소지품: 화염병 ~ 소이탄 발사기

고정 트레잇 - "소방화광(Antipyr Maniac)"

{PAWN_pronoun}(은)는 불은 불로 제압해야 한다는 철학을 지녔습니다. 불을 끄기 보단 확실하게 태워서 더 탈것이 없게 합니다. 
{PAWN_nameDef}의 불은 안전합니다(방화광 치고). {PAWN_nameDef}(이)가 만든 불은 주변으로 더 번지지 않습니다.
{PAWN_pronoun}는 불 주위에서 행복해하고, 불과 관련된 무기를 사용할 때 더 행복할 것입니다.


-------
참고: 코어 트레잇 **Pyromaniac** 한국어 번역  
`RimworldData\Core\Languages\Korean (한국어)\DefInjected\TraitDef\Traits_Singular.xml`

- **label** (`Pyromaniac.degreeDatas.pyromaniac.label`): 방화광
- **description** (`Pyromaniac.degreeDatas.pyromaniac.description`): {PAWN_nameDef}(은)는 불을 좋아합니다. {PAWN_pronoun}는 절대 불을 끄지 않으며 때때로 아무 데나 불을 피웁니다. {PAWN_pronoun}는 불 주위에서 행복해하고, 소이 무기를 사용할 때 더 행복할 것입니다.

------------------------
### 방화광(Pyromaniac) — 게임 메커니즘 메모 (코어)

**왜 불을 안 끄나**  
- 트레잇 Def에 `disabledWorkTags`로 **Firefighting(소방)** 작업 태그가 꺼져 있다. (`RimworldData\Core\Defs\TraitDefs\Traits_Singular.xml` — `Pyromaniac`)  
- `WorkGiver_FightFires`가 맵 위 일반 불(`Fire`)에 소방 일을 줄 때 `pawn.WorkTagIsDisabled(WorkTags.Firefighting)`이면 **작업 불가**로 처리한다. (`RimworldSource\RimWorld\WorkGiver_FightFires.cs`)  
- 서술상 “불을 좋아한다 / 끄지 않는다”와 맞물리게, **소방 능력 자체가 막힌 셈**이다. (동료 몸에 붙은 불 등 일부 예외 분기는 별도 조건이라, 지면·건물 화재 소화는 원칙적으로 불가에 가깝다.)

**왜 불을 지르나**  
1. **트레잇 전용 랜덤 정신상태**  
   - `randomMentalState`가 `FireStartingSpree`(방화)로 지정되어 있고, `TraitMentalStateGiver`가 주기적으로 굴린다. 현재 기분(`CurMood`)으로 MTB(평균 간격 일수)를 `randomMentalStateMtbDaysMoodCurve`에서 읽는데, 점이 `(0, 50)` 한 개뿐이라 곡선 평가 시 **대략 50일 MTB**로 고정에 가깝다. (`TraitMentalStateGiver.cs`, `SimpleCurve.Evaluate`, TraitDef XML)  
2. **정신 붕괴(극단)**  
   - `theOnlyAllowedMentalBreaks`에 `FireStartingSpree`만 있어, 같은 강도(Extreme)에서 **허용된 붕괴가 방화 쪽으로 제한**되는 효과가 있다. (`MentalBreakWorker.BreakCanOccur`, `TraitSet.TheOnlyAllowedMentalBreaks`)  
   - `MentalBreakDef` `FireStartingSpree`는 `requiredTrait`가 `Pyromaniac`이라 방화광 전용 극단 붕괴로 묶인다. (`MentalStates_Mood.xml`)  
3. **방화 상태 중 행동**  
   - 정신상태 `FireStartingSpree`일 때 `JobGiver_FireStartingSpree`가 **약 75% 확률**로 주변에서 타기 쉬운 건물·물건·식물에 `Ignite`(방화) 작업을 준다. 나머지는 배회 대기. (`JobGiver_FireStartingSpree.cs`)

**기분 보너스(참고)**  
- 같은 방·반경 8칸 안 불 개수에 비례하는 `ThoughtWorker_PyromaniacNearFlames`, 소이 무기 관련 생각 등이 있다. (`ThoughtWorker_PyromaniacNearFlames.cs`, `Thoughts_Situation_Traits.xml`)

**참고 (멘탈 스테이트·붕괴 계층 조사)**  
- [146 — MentalState / MentalBreak / Trait 계층 보고서](../Report/146_MentalState_MentalBreak_Trait_Layer_Report.md)