# 림월드 총기류 사운드 분석 - 2025-01-XX

## 작업 개요
- **요청 내용**: 림월드 데이터에서 일반 총기류 사운드 체크
- **목표**: 림월드 코어의 총기류 사운드 정의 구조 파악 및 프로젝트와 비교

## 계획 (AI가 결정한 계획)
1. 림월드 코어 SoundDefs 폴더 구조 확인
2. 총기류 사운드 정의 파일 분석 (World_Oneshots_Weapons.xml)
3. Interact 사운드와 Shot 사운드 분류
4. 프로젝트 내 사운드 사용 현황 확인
5. 비교 분석 보고서 작성

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 림월드 코어 SoundDefs 폴더 구조 확인 [v]
2. 총기류 사운드 정의 파일 분석 [v]
3. Interact 사운드와 Shot 사운드 분류 [v]
4. 프로젝트 내 사운드 사용 현황 확인 [v]
5. 비교 분석 보고서 작성 [v]

## 작업 세부 진행
1. [림월드 코어 SoundDefs 구조 확인] [v]
2. [World_Oneshots_Weapons.xml 분석] [v]
3. [무기 정의에서 사운드 사용 패턴 확인] [v]
4. [프로젝트 사운드 정의 확인] [v]
5. [보고서 작성] [v]

## 진행 상황

### 1. 림월드 코어 SoundDefs 구조 확인
- **위치**: `RimworldData/Core/Defs/SoundDefs/`
- **총기 관련 파일**: `World_Oneshots_Weapons.xml`
- **기타 관련**: `World_Oneshots_Tails.xml` (GunTail 사운드)

### 2. 총기류 사운드 정의 분석

#### Interact 사운드 (무기 조작 시)
- `Interact_Revolver`: 리볼버 조작
- `Interact_Autopistol`: 자동권총 조작
- `Interact_Shotgun`: 샷건 조작
- `Interact_Rifle`: 소총 조작
- `Interact_AssaultRifle`: 돌격소총 조작
- `Interact_SMG`: 기관단총 조작
- `Interact_ChargeRifle`: 충전 소총 조작
- `Interact_ChargeLance`: 충전 랜스 조작

#### Shot 사운드 (발사 시)
- `Shot_Revolver`: 리볼버 발사
- `Shot_Autopistol`: 자동권총 발사
- `Shot_MachinePistol`: 기관권총 발사
- `Shot_HeavySMG`: 중기관단총 발사
- `Shot_AssaultRifle`: 돌격소총 발사
- `Shot_Shotgun`: 샷건 발사
- `Shot_BoltActionRifle`: 볼트액션 소총 발사
- `Shot_SniperRifle`: 저격소총 발사
- `Shot_Minigun`: 미니건 발사
- `Shot_ChargeRifle`: 충전 소총 발사
- `Shot_NeedleGun`: 니들건 발사
- `Shot_IncendiaryLauncher`: 소이 발사기 발사
- `Shot_ChargeBlaster`: 충전 블래스터 발사
- `Shot_MiniSlug`: 미니 슬러그 발사

#### GunTail 사운드 (발사 후 꼬리 사운드)
- `GunTail_Light`: 경량 총기 꼬리
- `GunTail_Medium`: 중량 총기 꼬리
- `GunTail_Heavy`: 중무기 꼬리

### 3. 무기 정의에서 사운드 사용 패턴

#### soundInteract (무기 조작 시)
- 리볼버: `Interact_Revolver`
- 자동권총: `Interact_Autopistol`
- 기관단총: `Interact_SMG`
- 소총류: `Interact_Rifle`
- 샷건: `Interact_Shotgun`
- 충전 무기: `Interact_ChargeRifle` / `Interact_ChargeLance`

#### soundCast (발사 시, verb에서 사용)
- 리볼버: `Shot_Revolver` + `GunTail_Light`
- 자동권총: `Shot_Autopistol` + `GunTail_Light`
- 기관권총: `Shot_MachinePistol` + `GunTail_Light`
- 중기관단총: `Shot_HeavySMG` + `GunTail_Heavy`
- 돌격소총: `Shot_AssaultRifle` + `GunTail_Medium`
- 샷건: `Shot_Shotgun` + `GunTail_Heavy`
- 볼트액션: `Shot_BoltActionRifle` + `GunTail_Heavy`
- 저격소총: `Shot_SniperRifle` + `GunTail_Heavy`
- 미니건: `Shot_Minigun` + `GunTail_Medium`
- 충전 소총: `Shot_ChargeRifle` + `GunTail_Medium`
- 충전 랜스: `ChargeLance_Fire` + `GunTail_Heavy`

### 4. 프로젝트 사운드 정의 확인

#### 현재 프로젝트 SoundDef.xml
- `Rifle`: FlechetteRifle 클립 사용 (순환 참조 문제 가능성)
- `PrototypePulse`: PrototypePulse 클립 사용
- `FlechetteRifle`: Rifle 클립 사용 (순환 참조 문제 가능성)

**문제점 발견:**
- `Rifle` SoundDef가 `FlechetteRifle` 클립을 참조
- `FlechetteRifle` SoundDef가 `Rifle` 클립을 참조
- 순환 참조 구조로 인한 문제 가능성

#### 프로젝트 무기에서 사용 중인 사운드
- `soundInteract`: `Interact_Rifle` (다수 무기)
- `soundCast`: `FlechetteRifle` (플레셰트 라이플)

## 최종 작업 결과
✅ 완료: 림월드 총기류 사운드 분석 보고서 작성
- 림월드 코어 사운드 정의 구조 파악
- Interact/Shot/GunTail 사운드 분류
- 무기별 사운드 사용 패턴 정리
- 프로젝트 사운드 정의 문제점 발견

## 관련 파일 목록
- `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Weapons.xml`
- `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Tails.xml`
- `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml`
- `Project/1.6/Defs/SoundDefs/SoundDef.xml`
- `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml`

## 참고사항
- 림월드 코어는 `Shot_` 접두사로 발사 사운드 구분
- `Interact_` 접두사로 무기 조작 사운드 구분
- `GunTail_` 접두사로 발사 후 꼬리 사운드 구분
- 프로젝트의 Rifle/FlechetteRifle 순환 참조 문제 확인 필요

