using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Step4 달고나 게임 - 바늘 끝 감지
/// - 포인트 홀드 감지 (3초)
/// - 제한 시간 타이머
/// - 성공/실패 시 GameManager에 알림
/// </summary>
public class Step4NeedleTipDetector : MonoBehaviour
{
    [Header("GameManager 참조")]
    [SerializeField] private GameManager _gameManager;

    [Header("패널 선택 컨트롤러")]
    [SerializeField] private Step3SelectButtonController _step3SelectButtonController;

    [Header("각 패널별 Circle 포인트 배열들")]
    [SerializeField] private Collider2D[] _circlePointsSet0;
    [SerializeField] private Collider2D[] _circlePointsSet1;
    [SerializeField] private Collider2D[] _circlePointsSet2;
    [SerializeField] private Collider2D[] _circlePointsSet3;

    [Header("홀드(3초) 관련 설정")]
    [SerializeField] private float _holdTime = 3f;
    [SerializeField] private Color _completeColor = Color.green;

    [Header("포인트 홀드 진행도 (슬라이더)")]
    [SerializeField] private Slider _holdSlider;

    [Header("게임 제한 시간 설정")]
    [SerializeField] private float _timeLimit = 60f;
    [SerializeField] private TimerRotation _timerRotation;

    [Header("타임스케일 조건값")]
    [Range(1f, 10f)]
    [SerializeField] private float timer = 1f;

    [Header("게임 패널 (Step3 선택에 따라 열리는 패널)")]
    [SerializeField] private GameObject[] _gamePanels;

    [Header("옵션")]
    [SerializeField] private bool _autoStartTimerOnEnable = true;

    [Header("드릴 파편 효과")]
    [SerializeField] private DrillDebrisEffect _drillDebrisEffect;

    // ─────────────────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────────────────

    private Collider2D _currentCircle;
    private SpriteRenderer _currentRenderer;
    private float _holdTimer = 0f;

    private HashSet<Collider2D> _completedCircles = new HashSet<Collider2D>();

    private float _timeElapsed = 0f;
    private bool _isTimerRunning = false;
    private bool _isTimerFinished = false;

    private Dictionary<Collider2D, Color> _originalColors = new Dictionary<Collider2D, Color>();

    private void OnEnable()
    {
        if (_autoStartTimerOnEnable)
        {
            StartLimitTimer();
        }
    }

    private void Awake()
    {
        Time.timeScale = timer;

        if (_holdSlider != null)
            _holdSlider.value = 0f;

        if (_timerRotation != null)
            _timerRotation.SetDuration(_timeLimit);

        CacheOriginalColors();
    }

    private void Update()
    {
        // ───── 1) 제한 시간 타이머 업데이트 ─────
        if (_isTimerRunning && !_isTimerFinished)
        {
            if (_timerRotation != null)
            {
                _timeElapsed = _timerRotation.GetElapsedTime();
            }
            else
            {
                _timeElapsed += Time.deltaTime;
            }

            // 제한 시간 초과
            if (_timeElapsed >= _timeLimit || (_timerRotation != null && _timerRotation.IsCompleted()))
            {
                _isTimerRunning = false;
                _isTimerFinished = true;

                if (_timerRotation != null)
                    _timerRotation.Stop();

                ResetCurrentHold();
                FailCall();
                Debug.Log("제한시간 내에 실패!");
            }
        }

        if (_isTimerFinished || !_isTimerRunning)
            return;

        // ───── 2) 포인트 홀드(3초) 로직 ─────
        if (_currentCircle != null)
        {
            _holdTimer += Time.deltaTime;

            if (_holdSlider != null)
            {
                _holdSlider.value = Mathf.Clamp01(_holdTimer / _holdTime);
            }

            // 드릴 파편 위치 업데이트
            if (_drillDebrisEffect != null && _drillDebrisEffect.IsSpawning)
            {
                _drillDebrisEffect.UpdateSpawnPosition(transform.position);
            }

            if (_holdTimer >= _holdTime)
            {
                if (_currentRenderer != null)
                {
                    _currentRenderer.color = _completeColor;
                }

                _completedCircles.Add(_currentCircle);
                CheckAllCompleted();
                ResetCurrentHold();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Step4Tip] OnTriggerEnter2D with {other.name}");

        if (!_isTimerRunning || _isTimerFinished)
            return;

        if (_completedCircles.Contains(other))
            return;

        if (!IsTargetCircle(other))
            return;

        _currentCircle = other;
        _currentRenderer = other.GetComponent<SpriteRenderer>();
        _holdTimer = 0f;

        if (_holdSlider != null)
            _holdSlider.value = 0f;

        // 드릴 파편 효과 시작
        if (_drillDebrisEffect != null)
        {
            _drillDebrisEffect.StartSpawning(transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _currentCircle)
        {
            ResetCurrentHold();
        }
    }

    private void ResetCurrentHold()
    {
        _currentCircle = null;
        _currentRenderer = null;
        _holdTimer = 0f;

        if (_holdSlider != null)
            _holdSlider.value = 0f;

        // 드릴 파편 효과 중지
        if (_drillDebrisEffect != null)
        {
            _drillDebrisEffect.StopSpawning();
        }
    }

    public void StartLimitTimer()
    {
        _timeElapsed = 0f;
        _isTimerRunning = true;
        _isTimerFinished = false;

        if (_timerRotation != null)
        {
            _timerRotation.SetDuration(_timeLimit);
            _timerRotation.Play();
        }

        ResetCurrentHold();
        _completedCircles.Clear();

        Debug.Log("[Step4Tip] 제한 시간 타이머 시작");
    }

    private Collider2D[] GetActiveCircleArray()
    {
        if (_step3SelectButtonController == null)
        {
            return _circlePointsSet0;
        }

        int idx = _step3SelectButtonController._selectIndex;
        int arrayIndex;

        if (idx >= 0 && idx <= 3)
        {
            arrayIndex = idx;
        }
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
            if (_isTimerRunning && !_isTimerFinished && _timeElapsed <= _timeLimit)
            {
                SuccessCall();
                Debug.Log("제한시간 내에 성공!");
            }

            _isTimerRunning = false;
            _isTimerFinished = true;

            if (_timerRotation != null)
                _timerRotation.Stop();
        }
    }

    private int GetCurrentSelectIndex()
    {
        if (_step3SelectButtonController == null)
            return 0;

        int idx = _step3SelectButtonController._selectIndex;

        if (idx >= 0 && idx <= 3)
            return idx;
        else if (idx >= 1 && idx <= 4)
            return idx - 1;
        else
            return 0;
    }

    /// <summary>
    /// 성공 시 처리 - GameManager에 알림
    /// </summary>
    private void SuccessCall()
    {
        int idx = GetCurrentSelectIndex();

        // 게임 패널 닫기
        CloseGamePanel(idx);

        // GameManager에 성공 알림
        if (_gameManager != null)
        {
            _gameManager.OnGameSuccess(idx);
        }
        else
        {
            Debug.LogWarning("[Step4Tip] GameManager가 연결되지 않았습니다!");
        }

        Debug.Log($"[Step4Tip] 성공! 인덱스 {idx}");
    }

    /// <summary>
    /// 실패 시 처리 - GameManager에 알림
    /// </summary>
    private void FailCall()
    {
        int idx = GetCurrentSelectIndex();

        // 게임 패널 닫기
        CloseGamePanel(idx);

        // GameManager에 실패 알림
        if (_gameManager != null)
        {
            _gameManager.OnGameFail(idx);
        }
        else
        {
            Debug.LogWarning("[Step4Tip] GameManager가 연결되지 않았습니다!");
        }

        Debug.Log($"[Step4Tip] 실패! 인덱스 {idx}");
    }

    private void CloseGamePanel(int index)
    {
        if (_gamePanels != null && index < _gamePanels.Length && _gamePanels[index] != null)
        {
            _gamePanels[index].SetActive(false);
            Debug.Log($"[Step4Tip] 게임 패널 닫기: index={index}");
        }
    }

    private void CacheOriginalColors()
    {
        CacheFromArray(_circlePointsSet0);
        CacheFromArray(_circlePointsSet1);
        CacheFromArray(_circlePointsSet2);
        CacheFromArray(_circlePointsSet3);
    }

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
    /// 리셋 (GameManager 또는 외부에서 호출)
    /// </summary>
    public void ResetCall()
    {
        _timeElapsed = 0f;
        _isTimerRunning = false;
        _isTimerFinished = false;

        if (_timerRotation != null)
            _timerRotation.ResetCall();

        ResetCurrentHold();
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
    }
}
