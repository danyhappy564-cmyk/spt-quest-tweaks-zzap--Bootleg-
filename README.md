### ⚠️ IMPORTANT NOTICE / DISCLAIMER

**Original Author:** sgtlaggy
**Original Repository:** spt-quest-tweaks
**Original Link:** https://github.com/sgtlaggy/spt-quest-tweaks
**License:** MIT (원본 저장소의 `LICENSE` 파일 그대로 유지)

1. **Reflection & Take-Downs:** I deeply reflect on the ECOT incident. As an AI-assisted "vibe coder," I will immediately delete files if the original authors ask.
2. **No Re-Distribution:** These ported builds are unverified, temporary fixes. Please do NOT re-upload or share them anywhere else.
3. **Do Not Pester Original Authors:** Never report bugs or pester original modders regarding issues from my unofficial ports.
4. **Full Credit & Respect:** I will always credit original creators on GitHub and prioritize their decisions above all else.
5. **Support Original Creators:** Instead of using my ports, please visit the original authors' Forge pages to leave kind words or tips.

---

## 변경 이력

- 2026-09-24 20:54 (KST) — **"이미 받은 퀘스트에도 적용" 옵션 추가** (F12 → `4. 편의 기능`, `config.json`의
  `QualityOfLife.applyToExistingProgress`, **기본 꺼짐**). 일부 완화는 값이 프로필에 복사돼 저장되기 때문에
  켜기 전에 받은 것에는 안 먹었다. 켜면 서버가 프로필의 그 복사본을 직접 고친다:
  - 이미 돌고 있는 **퀘스트 대기 시간** → 즉시 해제 (`퀘스트 대기 시간 제거`가 켜져 있을 때)
  - 이미 받아둔 **일일/주간 퀘스트**의 대상/무기/부위/거리/FIR 조건 → 켜진 완화만큼 제거
  - **잠김**으로 저장된 등대지기(Network Provider 1)/컬렉터 → 잠김 기록을 지워 시작 조건을 다시 판정
  - ⚠️ **되돌릴 수 없다.** 고치기 전 `SPT_Runtime\user\mods\sgtlaggyQuestTweaks\backups\`에 프로필 백업을
    남긴다. 게임 재시작 후 반영 (F12 `0. 상태`에 수정 건수 표시).
- 2026-09-24 20:46 (KST) — **완화 표시 위치 선택.** F12 → `1. 표시` → `완화 표시 위치`에서 왼쪽 / 중앙 / 오른쪽
  (줄바꿈이 켜져 있을 때만, 목표 문구 줄은 그대로 두고 완화 표시 줄만 이동). 긴 목표 문구에서 표시가
  왼쪽에 붙어 보기 불편하던 것 대응.
- 2026-09-24 20:38 (KST) — **표시 줄바꿈/크기 + 일일 퀘스트 + 모드 퀘스트 대응.**
  - 완화 표시를 목표 문구 **아래 줄**에 따로 표시 (F12 → `완화 표시 줄바꿈`), 글자 크기 조절
    (F12 → `완화 표시 글자 크기(%)`, 기본 90%).
  - **일일/주간 퀘스트에도 표시.** 반복 퀘스트는 게임이 문구를 직접 조립하는 방식이라, 그 조립 결과에도
    표시를 붙이도록 추가. 서버가 내 프로필의 현재 반복 퀘스트를 보고 **실제로 조건이 없는 것만**
    표시한다 (완화 켜기 전에 받은 헤드샷 일일 퀘스트에는 "부위 무관"이 안 붙음). 30초마다 갱신.
  - **로드 순서 변경: 모든 모드 중 가장 마지막**(`int.MaxValue - 1000`, 이전 `PostLoad + 999`).
    `AddMissingQuestRequirements`(`PostLoad + 50000`, 퀘스트 무기 목록에 모드 무기 추가)보다
    먼저 돌던 탓에, F12로 설정을 바꾸면 원본 복원 과정에서 **그 모드가 추가한 무기가 사라지는 문제**가
    있었다 → 이제 그 모드 작업이 끝난 뒤의 상태를 원본으로 삼는다. 늦게 퀘스트를 추가하는 모드 대비로,
    게임 접속 시 새로 생긴 퀘스트가 있으면 자동으로 다시 적용한다.
- 2026-09-24 20:25 (KST) — **완화 표시를 빨간 글씨로.** `[퀘스트 완화됨: ...]` 부분만 색이 바뀐다
  (기본 `#FF4040` 빨강). F12 → `1. 표시` → `완화 표시 색상`에서 바꿀 수 있고, 비우면 색 없음.
  이 색 설정은 게임 쪽 설정이라 서버 `config.json`에는 저장되지 않는다.
- 2026-09-23 10:39 (KST) — **완화 표시가 게임 화면에 안 붙던 문제 대응.**
  - 서버 번역 문구에만 의존하지 않고, **F12 플러그인이 게임에서 문구를 보여주는 순간 직접**
    `[퀘스트 완화됨: ...]`을 붙이도록 추가했다 (게임의 모든 번역 조회가 지나가는 한 곳에 연결).
    다른 한글화 모드가 번역을 덮어써도 표시가 붙는다. 이미 붙어 있으면 중복으로 안 붙는다.
  - 서버 모드를 사용자 환경과 같은 SPT **4.1.6** 패키지로 다시 빌드 (이전은 4.1.0 기준).
  - 인게임 미검증 — 확인 방법은 아래 "안 붙을 때 확인할 것" 참고.
- 2026-09-23 10:20 (KST) — **완화 표시 + F12 실시간 설정 추가.**
  - 완화된 퀘스트 목표의 한국어 문구 뒤에 `[퀘스트 완화됨: 부위 무관 · 목표 5→3]`
    처럼 **무엇이 완화됐는지** 붙는다. 설정에서 켜기만 한 게 아니라 **그 목표에
    원래 해당 제한이 있었고 실제로 풀린 경우에만** 붙는다 (예: 원래 헤드샷 조건이
    없던 퀘스트에는 "부위 무관"이 안 붙음).
  - 새 클라 플러그인 `QuestTweaksLive.dll` — 게임 안 F12 메뉴에서 설정을 바꾸면
    서버의 `config.json`에 저장되고 **서버 재시작 없이** 바로 다시 적용된다.
  - 이전에 시도했던 `DynamicLocale` 방식(게임이 문구를 새로 조립하게 하는 방식)은
    인게임에서 안 됐기 때문에 쓰지 않았다. 대신 원래 번역 문구 뒤에 표시를 덧붙이는
    방식이다.
  - 서버 쪽 동작은 실제 SPT 퀘스트/한국어 번역 데이터로 테스트 완료. **게임 화면에서
    실제로 보이는지는 아직 미검증** (아래 "알려진 한계" 참고).

---

# sgtlaggy's Quest Tweaks (zzap 포크)

⚠️ 모드를 빼도 대체로 안전하지만, 진행 중인 퀘스트에는 적용됐던 효과가 남아 있을 수 있다.

## 설치

zip을 SPT 폴더(`E:\SPT 4.1`)에 그대로 풀면 된다.

| 파일 | 위치 | 역할 |
|---|---|---|
| `sgtlaggyQuestTweaks.dll`, `config.json` | `SPT_Runtime\user\mods\sgtlaggyQuestTweaks\` | 서버 모드 (퀘스트 완화 본체) |
| `QuestTweaksLive.dll` | `BepInEx\plugins\` | F12 설정 메뉴 (선택 — 없어도 서버 모드는 `config.json`만으로 동작) |

## 새 기능 1: 완화 표시

`config.json`의 `QualityOfLife.showRelaxedTag` (기본 `true`) 또는 F12 → `1. 표시` →
`완화 표시 붙이기`. 표시 부분은 빨간 글씨(F12 → `완화 표시 색상`에서 변경, 예: `#FFD040` 노랑).

```
삼림(Woods)에서 볼트액션 소총을 사용하여 헤드샷으로 PMC 사살하기 [퀘스트 완화됨: 무기 무관 · 부위 무관 · 목표 5→3]
```

붙는 표시 종류:

| 표시 | 뜻 |
|---|---|
| 대상 무관 / 무기 무관 / 부착물 무관 / 부위 무관 / 거리 무관 / 시간대 무관 | 해당 제한이 풀림 |
| 맵 무관 / 구역 무관 / 구역→맵 전체 | 위치 제한이 풀리거나 넓어짐 |
| 착용 장비 무관 / 적 장비 무관 / 본인 상태 무관 / 적 상태 무관 | 장비·상태 조건이 풀림 |
| FIR 불필요 | 납품 아이템의 인레이드 획득 조건이 풀림 |
| 목표 5→3 / 수량 10→5 | 사살 수 / 납품 수가 바뀜 |
| 자동 완료 | 조건이 다 지워져서 목표가 0으로 바뀜 |
| TRG M10 허용 | 타르코프 슈터에 M10 추가 |

## 새 기능 2: F12 실시간 설정

게임 안에서 **F12** (BepInEx ConfigurationManager) → `Quest Tweaks Live (F12)`.

- 게임을 켜면 서버의 `config.json` 값을 불러와서 F12 메뉴에 표시한다 (**서버가 기준**).
- F12에서 값을 바꾸면 약 1초 뒤 서버에 보내고, 서버가 `config.json`에 저장한 뒤
  퀘스트 완화를 처음부터 다시 적용한다 (**서버 재시작 불필요**).
- `0. 상태` → `서버 연결 상태`에 결과가 표시된다 (예: `적용 완료 (10:21:03, 문구 17개 갱신)`).
- F12에서 못 바꾸는 항목: `exemptQuests`, `onlyQuests`, `questOverrides` (퀘스트 ID
  목록이라 `config.json`에서 직접 수정).

## 안 붙을 때 확인할 것

1. **완화 설정이 하나라도 켜져 있는지** — 기본 `config.json`은 전부 `false`라서 아무것도 완화되지
   않고, 그래서 표시도 없다. 예: F12 → `2. 조건 제거` → `부위 제한 해제` 켜기.
2. **서버 콘솔**에 `[QuestTweaks] tagged N relaxed quest objectives (kr)`가 뜨는지 (N이 0이면 1번 문제).
3. **F12 → `0. 상태`**가 `서버와 동기화됨`인지 (실패면 플러그인이 서버와 통신을 못 하는 것).
4. **`BepInEx\LogOutput.log`**에 `relaxed-quest tags received: N`, `relaxed-tag patch failed`가 있는지.
5. 게임 언어가 **한국어**인지 (표시는 한국어 전용).

## 알려진 한계 (중요)

- **켜기 전에 받은 것에는 기본적으로 안 먹는 완화**: 일일/주간 퀘스트 조건(생성 시점에 프로필에 저장),
  이미 돌고 있는 퀘스트 대기 시간(끝나는 시각이 프로필에 저장), 프로필에 이미 잠김으로 기록된 시작 퀘스트.
  → `이미 받은 퀘스트에도 적용` 옵션으로 해결 가능 (위 변경 이력 참고). 일반 퀘스트 조건·수량은 원래 기존 진행분에도 적용된다.
- 일일/주간 퀘스트에는 원래(원본 모드부터) 사살 수·납품 수·시간·맵·구역·장비·상태 완화가 적용되지 않는다.

- **서버 재시작은 필요 없지만, 게임 쪽 반영 시점은 미검증이다.**
  - 문구(완화 표시): 플러그인이 게임에 이미 불러온 한국어 문구를 바로 덮어쓴다.
    코드상으로는 바로 반영돼야 하지만 **인게임 미검증**이다. 이미 열려 있는 퀘스트
    창은 닫았다 다시 열어야 할 수 있다. 안 바뀌면 게임만 재시작하면 된다 (서버는 그대로).
  - 퀘스트 조건(실제 판정): 서버에는 즉시 반영되지만, 게임은 퀘스트 정보를 받아둔
    것을 쓰기 때문에 **다음 레이드부터, 확실한 건 게임 재시작 후**다.
- 완화 표시는 **한국어(`kr`) 클라이언트 전용**이다. 영어 등 다른 언어는 원래 문구 그대로.
- 일일/주간 반복 퀘스트 표시는 대상/무기/부위/거리/FIR 항목만 (서버가 반복 퀘스트에 적용하는 항목이 이것뿐).
- 로드 순서는 모든 모드 중 마지막이다. `AddMissingQuestRequirements`와 같이 쓸 때는 그 모드가
  먼저 모드 무기를 추가하고, 이 모드가 그 결과를 기준으로 완화한다 (무기 제한 해제를 켜면 확장된
  무기 목록도 같이 비워진다 = 아무 무기나 가능).
- F12에서 값을 바꾸면 `config.json`이 다시 저장되면서 파일 안의 주석(`//`)이 사라진다.

## 기존 기능 (원본)

### 편의 기능 (`QualityOfLife`)

- **숨겨진 목표 전부 공개** (`revealAllQuestObjectives`) — 다른 목표를 끝내야 나타나는
  목표를 처음부터 보여준다. ⚠️ 목표를 순서와 다르게 완료할 수 있고, 설정을 끄거나 모드를
  빼도 진행 중인 퀘스트에는 공개 상태가 남는다.
- **알 수 없는 보상 공개** (`revealUnknownRewards`) — "알 수 없는 보상"을 실제 아이템으로 표시.
- **대기 시간 제거** (`removeTimeGates`) — 건스미스 등 퀘스트 사이의 대기 시간 제거.

### 조건 제거 (`GlobalConditions`)

🔃 표시는 기본적으로 반복 퀘스트에도 적용된다 (`affectRepeatables`).

- 🔃 사살 대상 (PMC, 스캐브, 보스 등) — `removeTarget`
- 🔃 무기와 부착물 — `removeWeapon`, `removeWeaponMods`
- 장비 — `removeSelfGear`, `removeEnemyGear`
- 상태 효과 (뇌진탕, 탈수 등) — `removeSelfHealthEffect`, `removeEnemyHealthEffect`
- 🔃 부위 — `removeBodyPart`
- 🔃 거리 — `removeDistance`
- 시간 — `removeTime`
- 맵 — `removeMap`
- 구역 — `removeZone` (맵은 유지하고 구역만 지우면 구역이 맵 전체로 넓어진다)
- 🔃 FIR(인레이드 획득) — `removeFindInRaid`

### 사살 수 / 납품 수 (`GlobalConditions`)

- `eliminationCount` / `eliminationPercent`, `handoverItemCount` / `handoverItemPercent`
- 음수(`-1`)면 변경 안 함. 고정값(`Count`)이 비율(`Percent`)보다 우선. 비율 적용 결과가 0이면 1.
- 퀘스트 아이템(골동 회중시계 등)과 열쇠/키카드에는 적용 안 됨.

### 퀘스트별 설정

- `questOverrides` — 퀘스트마다 따로 설정. 형식: `{"퀘스트ID": {"removeZone": true}}`.
  `true` = 제거, `false` = 원래대로, 생략/`null` = 전역 설정 따름. `onlyQuests`/`exemptQuests`와
  상관없이 항상 적용된다.
- `onlyQuests` — 이 목록의 퀘스트만 수정.
- `exemptQuests` — 이 목록의 퀘스트는 건너뜀.
- 퀘스트 ID는 [Tarkynator](https://tarkynator.com/quests?scope=global)에서 찾을 수 있다.
  반복 퀘스트 ID: PMC 일일 `615ffc701c97c768137e719b`, 주간 `618035d38012292db3081bf0`,
  스캐브 일일 `62825ef60e88d037dc1eb426`.

### 특수 케이스 (`SpecialCases`)

- **등대지기 레벨만 요구** (`lightkeeperOnlyRequireLevel`) — `Network Provider - Part 1`
  시작 조건을 이 레벨 하나로 바꾼다. `0`이면 끔.
- **타르코프 슈터 1~6에 SAKO TRG M10 추가** (`tarkovShooterM10`) — BSG는 7, 8에만 넣었다.
- **컬렉터 선행 조건 백포트** (`collectorPrerequisiteBackport`, EFT 1.1.0.0 기준) — 선행 조건을
  완전히 교체한다: 기본 상인 7명 LL4, 펜스 평판 3+, PMC 40레벨, A Shooter Born in Heaven,
  The Tarkov Shooter - Part 4, Sew It Good - Part 4, Chemical - Part 4 또는 Big Customer
  또는 Out of Curiosity. Start Collector Early 같은 모드와는 호환되지 않는다.

## 직접 빌드

```
cd client
dotnet build -c Release -p:SptRoot="E:\SPT 4.1"
cd ..
dotnet build -c Release
```

클라 플러그인을 먼저 빌드하면 서버 빌드가 만드는 `dist\sgtlaggyQuestTweaks-<버전>.zip`에
`BepInEx\plugins\QuestTweaksLive.dll`이 같이 들어간다.
