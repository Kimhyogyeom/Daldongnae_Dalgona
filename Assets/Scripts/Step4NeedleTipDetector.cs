using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Step4NeedleTipDetector : MonoBehaviour
{
    [Header("패널 선택 컨트롤러")]
    [SerializeField] private Step3SelectButtonController _step3SelectButtonController;
    // Step3에서 선택된 인덱스를 가져오기 위한 컨트롤러

    [Header("각 패널별 Circle 포인트 배열들")]
    [SerializeField] private Collider2D[] _circlePointsSet0; // selectIndex 0
    [SerializeField] private Collider2D[] _circlePointsSet1; // selectIndex 1
    [SerializeField] private Collider2D[] _circlePointsSet2; // selectIndex 2
    [SerializeField] private Collider2D[] _circlePointsSet3; // selectIndex 3

    [Header("홀드(3초) 관련 설정")]
    [SerializeField] private float _holdTime = 3f;           // 한 포인트를 유지해야 하는 시간
    [SerializeField] private Color _completeColor = Color.green; // 완료 시 변경할 색상

    [Header("포인트 홀드 진행도 (버튼 Filled Image)")]
    [SerializeField] private Image _fillImage;               // 현재 홀드 진행도 표시용 이미지 (Type: Filled)

    [Header("게임 제한 시간 설정")]
    [SerializeField] private float _timeLimit = 60f;         // 전체 제한 시간 (초)
    [SerializeField] private Image _timeFillImage;           // 제한 시간 게이지용 이미지 (Type: Filled)

    [Header("타임스케일 조건값")]
    [Range(1f, 10f)]
    [SerializeField] private float timer = 1f;               // Time.timeScale 값

    [Header("엔딩 관련 오브젝트")]
    [SerializeField] private GameObject _endingObject;       // 엔딩 전체 패널
    [SerializeField] private GameObject[] _endingMessageObjects; // [0] 성공 메시지, [1] 실패 메시지 등

    // ─────────────────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────────────────

    private Collider2D _currentCircle;                       // 현재 홀드 중인 Circle
    private SpriteRenderer _currentRenderer;                 // 현재 Circle의 SpriteRenderer
    private float _holdTimer = 0f;                           // 현재 포인트를 유지한 시간

    // 이미 완료된 Circle들
    private HashSet<Collider2D> _completedCircles = new HashSet<Collider2D>();

    // 제한 시간 타이머
    private float _timeElapsed = 0f;                         // 경과 시간
    private bool _isTimerRunning = false;                    // 타이머 동작 여부
    private bool _isTimerFinished = false;                   // 성공/실패로 게임이 끝났는지 여부

    // 자동 시작을 쓸 거면 true, 수동으로 시작할 거면 false
    [SerializeField] private bool _autoStartTimerOnEnable = true;

    // 리셋 시 색을 되돌리기 위한 원본 색상 저장용
    private Dictionary<Collider2D, Color> _originalColors = new Dictionary<Collider2D, Color>();

    private void OnEnable()
    {
        if (_autoStartTimerOnEnable)
        {
            StartLimitTimer();
        }

        // 엔딩 패널 및 메시지는 비활성화 상태로 시작
        if (_endingObject != null)
            _endingObject.SetActive(false);

        if (_endingMessageObjects != null)
        {
            for (int i = 0; i < _endingMessageObjects.Length; i++)
            {
                if (_endingMessageObjects[i] != null)
                    _endingMessageObjects[i].SetActive(false);
            }
        }
    }

    private void Awake()
    {
        // 전체 게임 타임스케일 설정
        Time.timeScale = timer;

        // 진행도 이미지 초기화
        if (_fillImage != null)
            _fillImage.fillAmount = 0f;

        if (_timeFillImage != null)
            _timeFillImage.fillAmount = 0f;

        // 모든 Circle의 초기 색상 캐싱
        CacheOriginalColors();
    }

    private void Update()
    {
        // ───── 1) 제한 시간 타이머 업데이트 ─────
        if (_isTimerRunning && !_isTimerFinished)
        {
            _timeElapsed += Time.deltaTime;

            if (_timeFillImage != null)
            {
                _timeFillImage.fillAmount = Mathf.Clamp01(_timeElapsed / _timeLimit);
            }

            // 제한 시간 초과
            if (_timeElapsed >= _timeLimit)
            {
                _isTimerRunning = false;
                _isTimerFinished = true;

                // 현재 홀드 중인 것도 리셋
                ResetCurrentHold();

                FailCall();
                Debug.Log("제한시간 내에 실패!");
            }
        }

        // 타이머가 끝났으면 더 이상 포인트 채우기 로직은 수행하지 않음
        if (_isTimerFinished || !_isTimerRunning)
            return;

        // ───── 2) 포인트 홀드(3초) 로직 ─────
        if (_currentCircle != null)
        {
            _holdTimer += Time.deltaTime;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = Mathf.Clamp01(_holdTimer / _holdTime);
            }

            // 홀드 시간이 기준 이상이면 완료 처리
            if (_holdTimer >= _holdTime)
            {
                // 3초 홀드 완료 → 색 변경 + 완료 목록 추가
                if (_currentRenderer != null)
                {
                    _currentRenderer.color = _completeColor;
                }

                _completedCircles.Add(_currentCircle);

                // 현재 선택된 배열 기준으로 모두 완료됐는지 체크
                CheckAllCompleted();

                // 현재 홀드 상태 리셋
                ResetCurrentHold();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Step4Tip] OnTriggerEnter2D with {other.name}");

        // 타이머가 안 돌거나 이미 끝났으면 무시
        if (!_isTimerRunning || _isTimerFinished)
            return;

        // 이미 완료된 포인트면 무시
        if (_completedCircles.Contains(other))
            return;

        // 현재 선택된 배열에 속한 포인트인지 확인
        if (!IsTargetCircle(other))
            return;

        _currentCircle = other;
        _currentRenderer = other.GetComponent<SpriteRenderer>();
        _holdTimer = 0f;

        if (_fillImage != null)
            _fillImage.fillAmount = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _currentCircle)
        {
            ResetCurrentHold();
        }
    }

    /// <summary>
    /// 현재 홀드(3초) 상태 리셋
    /// </summary>
    private void ResetCurrentHold()
    {
        _currentCircle = null;
        _currentRenderer = null;
        _holdTimer = 0f;

        if (_fillImage != null)
            _fillImage.fillAmount = 0f;
    }

    /// <summary>
    /// 외부에서 "게임 시작" 시 호출해주면 됨
    /// - 타이머 및 내부 상태 초기화
    /// - 완료 포인트 목록 초기화
    /// </summary>
    public void StartLimitTimer()
    {
        // 전체 상태 초기화
        _timeElapsed = 0f;
        _isTimerRunning = true;
        _isTimerFinished = false;

        if (_timeFillImage != null)
            _timeFillImage.fillAmount = 0f;

        ResetCurrentHold();

        // 게임 시작 시 완료 정보 초기화
        _completedCircles.Clear();

        Debug.Log("[Step4Tip] 제한 시간 타이머 시작");
    }

    /// <summary>
    /// 현재 선택된 패널에 해당하는 Collider 배열 하나 가져오기
    /// </summary>
    private Collider2D[] GetActiveCircleArray()
    {
        if (_step3SelectButtonController == null)
        {
            return _circlePointsSet0;
        }

        int idx = _step3SelectButtonController._selectIndex;

        int arrayIndex;
        // 0~3을 직접 쓰는 경우
        if (idx >= 0 && idx <= 3)
        {
            arrayIndex = idx;
        }
        // 1~4를 쓰는 경우 (1→0, 2→1, 3→2, 4→3) 같은 상황도 방어
        else if (idx >= 1 && idx <= 4)
        {
            arrayIndex = idx - 1;
        }
        else
        {
            arrayIndex = 0;
        }

        switch (arrayIndex)
        {
            default:
            case 0: return _circlePointsSet0;
            case 1: return _circlePointsSet1;
            case 2: return _circlePointsSet2;
            case 3: return _circlePointsSet3;
        }
    }

    /// <summary>
    /// 현재 선택된 배열에 속한 Circle인지 확인
    /// + 이미 완료된 포인트는 false
    /// </summary>
    private bool IsTargetCircle(Collider2D col)
    {
        if (_completedCircles.Contains(col))
            return false;

        Collider2D[] activeArray = GetActiveCircleArray();
        if (activeArray == null || activeArray.Length == 0)
            return false;

        for (int i = 0; i < activeArray.Length; i++)
        {
            if (activeArray[i] == col)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 현재 선택된 배열의 모든 포인트가 완료됐는지 확인
    /// </summary>
    private void CheckAllCompleted()
    {
        Collider2D[] activeArray = GetActiveCircleArray();
        if (activeArray == null || activeArray.Length == 0)
            return;

        int total = activeArray.Length;
        int completed = 0;

        for (int i = 0; i < total; i++)
        {
            if (_completedCircles.Contains(activeArray[i]))
                completed++;
        }

        Debug.Log($"[Step4Tip] selectIndex={_step3SelectButtonController._selectIndex}, 완료: {completed} / {total}");

        if (completed == total)
        {
            // 제한 시간 안에 모두 완료
            if (_isTimerRunning && !_isTimerFinished && _timeElapsed <= _timeLimit)
            {
                SuccessCall();
                Debug.Log("제한시간 내에 성공!");
            }

            // 타이머 종료
            _isTimerRunning = false;
            _isTimerFinished = true;
        }
    }

    /// <summary>
    /// 성공 시 호출되는 처리 (엔딩 패널 + 성공 메시지 활성화)
    /// </summary>
    private void SuccessCall()
    {
        if (_endingObject != null)
            _endingObject.SetActive(true);

        if (_endingMessageObjects != null && _endingMessageObjects.Length > 0 && _endingMessageObjects[0] != null)
            _endingMessageObjects[0].SetActive(true);
    }

    /// <summary>
    /// 실패 시 호출되는 처리 (엔딩 패널 + 실패 메시지 활성화)
    /// </summary>
    private void FailCall()
    {
        if (_endingObject != null)
            _endingObject.SetActive(true);

        if (_endingMessageObjects != null && _endingMessageObjects.Length > 1 && _endingMessageObjects[1] != null)
            _endingMessageObjects[1].SetActive(true);
    }

    /// <summary>
    /// 모든 Circle들의 초기 색상을 캐싱
    /// - 리셋 시 색상을 원래대로 돌리기 위함
    /// </summary>
    private void CacheOriginalColors()
    {
        CacheFromArray(_circlePointsSet0);
        CacheFromArray(_circlePointsSet1);
        CacheFromArray(_circlePointsSet2);
        CacheFromArray(_circlePointsSet3);
    }

    /// <summary>
    /// 주어진 배열에 포함된 Collider2D에서 SpriteRenderer 색상 저장
    /// </summary>
    private void CacheFromArray(Collider2D[] array)
    {
        if (array == null)
            return;

        for (int i = 0; i < array.Length; i++)
        {
            Collider2D col = array[i];
            if (col == null)
                continue;

            if (_originalColors.ContainsKey(col))
                continue;

            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _originalColors.Add(col, sr.color);
            }
        }
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 전체 리셋 함수
    /// - 타이머, 진행도, 완료 상태, 색상, 엔딩 UI 등을 초기 상태로 되돌림
    /// </summary>
    public void ResetCall()
    {
        // 타이머 관련 초기화
        _timeElapsed = 0f;
        _isTimerRunning = false;
        _isTimerFinished = false;

        if (_timeFillImage != null)
            _timeFillImage.fillAmount = 0f;

        // 현재 홀드 상태 초기화
        ResetCurrentHold();

        // 완료된 포인트 목록 초기화
        _completedCircles.Clear();

        // Circle 색상 원래대로 복구
        foreach (var pair in _originalColors)
        {
            Collider2D col = pair.Key;
            if (col == null)
                continue;

            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = pair.Value;
            }
        }

        // 엔딩 패널 및 메시지 숨기기
        if (_endingObject != null)
            _endingObject.SetActive(false);

        if (_endingMessageObjects != null)
        {
            for (int i = 0; i < _endingMessageObjects.Length; i++)
            {
                if (_endingMessageObjects[i] != null)
                    _endingMessageObjects[i].SetActive(false);
            }
        }
    }
}
