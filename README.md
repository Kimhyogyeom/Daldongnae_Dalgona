# 달동네 달고나 프로젝트

Unity 기반 달고나 게임 프로젝트

## Scripts 구조

### Core
- **GameManager.cs** - 게임 전체 흐름 관리 (스텝 진행, 결과 표시, 리셋)

### Effect
- **ConfettiEffect.cs** - 클릭/터치 시 폭죽(꽃가루) 파티클 효과
- **ShakeEffect.cs** - X축 좌우 흔들림 효과
- **TimerRotation.cs** - 시간에 맞춘 시계 바늘 회전 효과

---

## 2025-01-08 업데이트 내역

### ConfettiEffect 개선
1. **파티클 중앙 출현 버그 수정**
   - 파티클 활성화 전에 위치/회전/스케일을 먼저 설정하도록 변경
   - 생성 시 화면 중앙에 잠깐 나타나는 현상 해결

2. **파티클 부모 변경**
   - Canvas가 아닌 자기 자신(transform)에 파티클 생성
   - 좌표 변환도 자기 자신 RectTransform 기준으로 변경

3. **렌더링 순서 문제 해결**
   - Overlay Canvas 추가로 다른 UI 위에 항상 표시
   - `overrideSorting` 활용하여 Sorting Order 100으로 설정
   - Inspector에서 조절 가능 (`_useOverlayCanvas`, `_overlaySortingOrder`)

### ShakeEffect 개선
1. **결과 화면 상태 감지 양방향 처리**
   - 결과 화면 시작 시 멈춤 + 오브젝트 비활성화
   - 결과 화면 종료 시 자동 재개 + 오브젝트 재활성화
   - 두 번째 플레이 시에도 정상 작동

2. **ResumeFromResult() 메서드 추가**
   - 결과 화면 종료 감지 시 자동 호출
   - 숨겼던 오브젝트 다시 활성화
   - 흔들림 효과 재시작

### GameManager 개선
1. **인덱스별 추가 오브젝트 지원**
   - `_extraObject0` ~ `_extraObject3` 필드 추가
   - 성공/실패 달고나 패널 활성화 시 해당 인덱스 오브젝트도 함께 활성화
   - 결과 패널 표시 시 모든 추가 오브젝트 비활성화

---

## 기술 노트

### Sorting Order 성능
- Sorting Order 값의 크기(0,1 vs 100,200)는 성능에 영향 없음
- Draw Call 배칭에 영향을 주는 것은 서로 다른 Sorting Order **값의 개수**
- 같은 Sorting Order를 가진 UI끼리만 배칭 가능
