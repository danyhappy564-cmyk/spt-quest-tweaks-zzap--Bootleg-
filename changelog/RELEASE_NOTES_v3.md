# sgtlaggy's Quest Tweaks (zzap 포크) v3 — 서버 4.11.0 / F12 플러그인 1.6.0

> 원작: [sgtlaggy/spt-quest-tweaks](https://github.com/sgtlaggy/spt-quest-tweaks) (MIT) · SPT 4.1.x
> 이 포크는 비공식 빌드입니다. 문제가 생겨도 원작자에게 문의하지 마시고, 재배포하지 말아 주세요.

v2(4.10.0) 이후 바뀐 점입니다. 원작 최신판 **4.9.0**(2026-10-06)의 변경을 모두 합쳤고,
포크 기능(완화 표시, F12 실시간 설정, 이미 받은 퀘스트에도 적용)은 그대로 유지됩니다.

## ⚠️ 업데이트 전에

- zip을 덮어쓰면 `SPT_Runtime\user\mods\sgtlaggyQuestTweaks\config.json`이 **기본값으로 바뀝니다.**
  쓰던 설정을 유지하려면 덮어쓰기 전에 `config.json`을 백업해 두었다가 다시 넣어 주세요.
  예전 `config.json`도 그대로 읽히고, 새 옵션은 꺼진 상태로 시작합니다.
- 서버 모드(`sgtlaggyQuestTweaks.dll`)와 F12 플러그인(`QuestTweaksLive.dll`)을 **둘 다** 교체해 주세요.
  새 옵션은 F12 플러그인 1.6.0에서만 보입니다.

## 새 옵션 (원작 4.9.0)

### F12 → `2. 조건 제거`

| 옵션 | config 키 | 내용 |
|---|---|---|
| 한 판 제한 해제 | `removeInOneRaid` | "한 번의 레이드에서" 조건을 없앱니다. 여러 판에 나눠서 채워도 됩니다. |
| 환승(트랜짓) 목표 제거 | `removeTransit` | 트랜짓으로 맵을 이동해야 하는 목표를 지웁니다. 그 퀘스트는 한 판 제한도 같이 풀립니다. |
| 마지막 생존 탈출 목표 제거 | `removeSurviveAfter` | 다른 목표를 다 한 뒤 붙는 "생존 후 탈출" 목표를 지웁니다 (다른 목표가 있는 퀘스트만). |

### F12 → `3. 수량`

| 옵션 | config 키 | 내용 |
|---|---|---|
| 설치 수 비율(%) / 고정값 | `plantItemPercent` / `plantItemCount` | 아이템을 특정 위치에 두고 오는 목표의 개수 |
| 설치 시간(초) | `plantItemTime` | 아이템을 두는 데 걸리는 시간 |
| 마커/카메라 설치 수 비율(%) / 고정값 | `placeProtectPercent` / `placeProtectCount` | MS2000 마커·WI-FI 카메라 등 설치 목표의 개수 |
| 마커/카메라 설치 시간(초) | `placeProtectTime` | 마커·카메라 설치에 걸리는 시간 |

모두 `-1` = 변경 안 함.

### 동작 변경

- **고정값을 0으로 하면 그 목표가 삭제됩니다** (사살 수·납품 수·설치 수 공통). 예전에는 개수만 0이 됐습니다.
- 조건을 다 지워서 할 일이 없어진 목표도 "0개로 자동 완료" 대신 삭제됩니다.
- `config.json`이 없으면 기본값으로 새로 만들고, 내용이 잘못돼도 서버가 멈추지 않고 콘솔에 오류만 남깁니다.
- `"debug": true`로 두면 모드 폴더에 수정 전/후 퀘스트 목록(`quests_before.json` / `quests_after.json`)을 저장합니다.

## 포크 쪽 변경

- 새 옵션에도 완화 표시가 붙습니다: `[퀘스트 완화됨: 한 판 제한 없음]`, `설치 20→5초`, `수량 4→2`.
  (삭제된 목표는 화면에서 사라지므로 표시가 없습니다.)
- F12에서 설정을 바꿔도, F12 메뉴에 없는 항목(예: 구버전 플러그인을 쓸 때의 새 옵션)은 `config.json`의 값이 유지됩니다.
- 다른 모드가 추가한 퀘스트에 일부 데이터가 비어 있어도 새 기능이 오류로 멈추지 않습니다.
- 직접 빌드할 때 서버 프로젝트만 빌드해도 F12 플러그인이 같이 빌드돼 zip에 들어갑니다
  (`E:\SPT 4.1`, 다른 위치면 `-p:SptRoot="게임 폴더"`).
- zip의 모드 폴더에 원작 영문 설명서 `README.md`와 한국어 설명서 `README_KR.md`가 함께 들어갑니다.

## 설치

zip을 SPT 폴더에 그대로 풀면 됩니다.

| 파일 | 위치 |
|---|---|
| `sgtlaggyQuestTweaks.dll`, `config.json`, `README.md`, `README_KR.md` | `SPT_Runtime\user\mods\sgtlaggyQuestTweaks\` |
| `QuestTweaksLive.dll` | `BepInEx\plugins\` |

## 알려진 한계

- 일일/주간 퀘스트에는 새 옵션이 적용되지 않습니다 (원작과 같음 — 반복 퀘스트는 대상/무기/부위/거리/FIR만).
- 퀘스트 조건 변경은 서버에 즉시 반영되지만, 게임 쪽은 다음 레이드부터, 확실한 건 게임 재시작 후입니다.
- 완화 표시는 한국어 클라이언트 전용입니다.

## 호환

- AddMissingQuestRequirements — 이 모드가 모든 모드 중 마지막에 실행되므로
  그 모드가 추가한 무기 목록을 기준으로 완화합니다.
