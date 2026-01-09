using UnityEngine;

/// <summary>
/// X축으로 왔다갔다 흔들리는 효과
/// - 현재 위치에서 -1 ~ +1 범위로 왔다갔다
/// - 활성화 시 자동 시작
/// - 결과 화면 시 자동으로 멈추고, 지정된 오브젝트 비활성화
/// - ResetCall로 원래 위치로 복귀 및 오브젝트 다시 활성화
/// </summary>
public class ShakeEffect : MonoBehaviour
{
    [Header("GameManager 참조")]
    [SerializeField] private GameManager _gameManager;

    [Header("흔들림 설정")]
    [SerializeField] private float _shakeAmount = 1f;        // 흔들림 범위 (-1 ~ +1)
    [SerializeField] private float _speed = 5f;              // 흔들림 속도

    [Header("옵션")]
    [SerializeField] private bool _playOnEnable = true;
    [SerializeField] private bool _useUnscaledTime = false;  // TimeScale 무시 여부
    [SerializeField] private bool _stopOnResult = true;      // 결과 화면 시 자동 멈춤

    [Header("결과 시 비활성화할 오브젝트")]
    [SerializeField] private GameObject _hideOnResult;       // 결과 화면 시 숨길 오브젝트

    private Vector3 _originalPosition;
    private bool _isShaking;
    private bool _positionCached;
    private float _time;
    private bool _isPausedByResult;                          // 결과 화면으로 인해 멈춘 상태

    private void Awake()
    {
        CacheOriginalPosition();
    }

    private void OnEnable()
    {
        _time = 0f;

        if (_playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        // 비활성화 시 원래 위치로
        if (_positionCached)
        {
            transform.localPosition = _originalPosition;
        }
    }

    private void Update()
    {
        // 결과 화면 감지 및 자동 멈춤/복귀
        if (_stopOnResult)
        {
            bool isResultShowing = _gameManager != null && _gameManager.IsShowingResult;

            // 결과 화면 시작 → 멈춤
            if (isResultShowing && !_isPausedByResult)
            {
                PauseByResult();
            }
            // 결과 화면 종료 → 복귀
            else if (!isResultShowing && _isPausedByResult)
            {
                ResumeFromResult();
            }
        }

        if (!_isShaking) return;

        float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _time += deltaTime * _speed;

        // Sin 함수로 -1 ~ +1 왔다갔다
        float offset = Mathf.Sin(_time) * _shakeAmount;

        Vector3 newPos = _originalPosition;
        newPos.x += offset;
        transform.localPosition = newPos;
    }

    /// <summary>
    /// 결과 화면으로 인해 멈춤
    /// </summary>
    private void PauseByResult()
    {
        _isPausedByResult = true;
        _isShaking = false;

        // 원래 위치로 복귀
        if (_positionCached)
        {
            transform.localPosition = _originalPosition;
        }

        // 지정된 오브젝트 비활성화
        if (_hideOnResult != null)
        {
            _hideOnResult.SetActive(false);
        }
    }

    /// <summary>
    /// 결과 화면 종료 시 자동 복귀
    /// </summary>
    private void ResumeFromResult()
    {
        _isPausedByResult = false;

        // 숨겼던 오브젝트 다시 활성화
        if (_hideOnResult != null)
        {
            _hideOnResult.SetActive(true);
        }

        // 다시 흔들림 시작
        if (_playOnEnable)
        {
            Play();
        }
    }

    /// <summary>
    /// 원본 위치 캐싱
    /// </summary>
    private void CacheOriginalPosition()
    {
        if (!_positionCached)
        {
            _originalPosition = transform.localPosition;
            _positionCached = true;
        }
    }

    /// <summary>
    /// 흔들림 시작
    /// </summary>
    public void Play()
    {
        CacheOriginalPosition();
        _isShaking = true;
        _time = 0f;
    }

    /// <summary>
    /// 흔들림 중지 (현재 위치 유지)
    /// </summary>
    public void Stop()
    {
        _isShaking = false;
    }

    /// <summary>
    /// 초기 위치로 즉시 복귀 및 중지, 숨긴 오브젝트 다시 활성화
    /// </summary>
    public void ResetCall()
    {
        _isShaking = false;
        _isPausedByResult = false;
        _time = 0f;

        if (_positionCached)
        {
            transform.localPosition = _originalPosition;
        }

        // 숨겼던 오브젝트 다시 활성화
        if (_hideOnResult != null)
        {
            _hideOnResult.SetActive(true);
        }

        // 다시 흔들림 시작
        if (_playOnEnable)
        {
            Play();
        }
    }

    /// <summary>
    /// 런타임에서 흔들림 범위 설정
    /// </summary>
    public void SetShakeAmount(float amount)
    {
        _shakeAmount = amount;
    }

    /// <summary>
    /// 런타임에서 속도 설정
    /// </summary>
    public void SetSpeed(float speed)
    {
        _speed = speed;
    }
}
