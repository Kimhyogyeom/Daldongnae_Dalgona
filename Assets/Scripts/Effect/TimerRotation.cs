using UnityEngine;

/// <summary>
/// 시간에 맞춰 오브젝트 Z축 회전을 제어하는 스크립트
/// - 설정된 시간이 지나면 정확히 한 바퀴(360도) 회전
/// - 시계 방향(-Z)으로 회전
/// </summary>
public class TimerRotation : MonoBehaviour
{
    [Header("시간 설정")]
    [SerializeField] private float _totalDuration = 180f;

    [Header("옵션")]
    [SerializeField] private bool _playOnAwake = false;
    [SerializeField] private bool _useUnscaledTime = false;

    [Header("긴박감 효과 (남은 시간 기준)")]
    [SerializeField] private bool _enableUrgencyShake = true;
    [SerializeField] private float _urgencyStartTime = 10f;      // 이 시간 이하로 남으면 흔들림 시작
    [SerializeField] private float _shakeAmount = 3f;            // 흔들림 각도
    [SerializeField] private float _shakeSpeed = 15f;            // 흔들림 속도
    [SerializeField] private Transform _shakeTarget;             // 흔들릴 대상 (비워두면 자기 자신)

    private float _elapsed;
    private bool _isPlaying;
    private float _startRotationZ;
    private bool _isUrgencyMode;
    private float _shakeTime;
    private Vector3 _shakeTargetOriginalPosition;

    private void Awake()
    {
        _startRotationZ = transform.localEulerAngles.z;

        // 흔들림 대상의 원본 위치 저장
        if (_shakeTarget != null)
        {
            _shakeTargetOriginalPosition = _shakeTarget.localPosition;
        }

        if (_playOnAwake)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _elapsed += deltaTime;

        // 진행률 계산 (0 ~ 1)
        float progress = Mathf.Clamp01(_elapsed / _totalDuration);

        // 시계 방향으로 회전 (-360도)
        float currentRotation = _startRotationZ - (progress * 360f);

        // 시침 회전 (흔들림 없이)
        transform.localRotation = Quaternion.Euler(0, 0, currentRotation);

        // 긴박감 흔들림 효과 (별도 대상에 X축 좌우 흔들림 적용)
        if (_enableUrgencyShake && _shakeTarget != null)
        {
            float remainingTime = _totalDuration - _elapsed;

            // 남은 시간이 기준 이하면 긴박감 모드
            if (remainingTime <= _urgencyStartTime && remainingTime > 0)
            {
                if (!_isUrgencyMode)
                {
                    _isUrgencyMode = true;
                    _shakeTime = 0f;
                    Debug.Log($"[TimerRotation] 긴박감 모드 시작! 남은 시간: {remainingTime:F1}초");
                }

                _shakeTime += deltaTime * _shakeSpeed;

                // 남은 시간이 적을수록 더 심하게 흔들림
                float urgencyIntensity = 1f - (remainingTime / _urgencyStartTime);
                float shakeOffset = Mathf.Sin(_shakeTime) * _shakeAmount * (0.5f + urgencyIntensity * 0.5f);

                // X축 좌우 흔들림 적용
                Vector3 newPos = _shakeTargetOriginalPosition;
                newPos.x += shakeOffset;
                _shakeTarget.localPosition = newPos;
            }
            else if (_isUrgencyMode)
            {
                // 긴박감 모드 종료 시 원래 위치로 복귀
                _shakeTarget.localPosition = _shakeTargetOriginalPosition;
                _isUrgencyMode = false;
            }
        }

        // 시간 완료
        if (_elapsed >= _totalDuration)
        {
            _isPlaying = false;
            _isUrgencyMode = false;
        }
    }

    /// <summary>
    /// 타이머 시작
    /// </summary>
    public void Play()
    {
        _isPlaying = true;
        _isUrgencyMode = false;
        _shakeTime = 0f;
        _elapsed = 0f;
        transform.localRotation = Quaternion.Euler(0, 0, _startRotationZ);
        Debug.Log($"[TimerRotation] Play() 호출됨 - Duration: {_totalDuration}초");
    }

    /// <summary>
    /// 타이머 일시정지
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }

    /// <summary>
    /// 타이머 재개
    /// </summary>
    public void Resume()
    {
        _isPlaying = true;
    }

    /// <summary>
    /// 타이머 정지 및 초기화
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _isUrgencyMode = false;
        _shakeTime = 0f;
        _elapsed = 0f;
        transform.localRotation = Quaternion.Euler(0, 0, _startRotationZ);

        // 흔들림 대상도 원래 위치로 복귀
        if (_shakeTarget != null)
        {
            _shakeTarget.localPosition = _shakeTargetOriginalPosition;
        }
    }

    /// <summary>
    /// 리셋 시 호출 (ResetController 연동용)
    /// </summary>
    public void ResetCall()
    {
        Stop();
    }

    /// <summary>
    /// 런타임에서 총 시간 설정
    /// </summary>
    public void SetDuration(float duration)
    {
        _totalDuration = duration;
    }

    /// <summary>
    /// 현재 남은 시간 반환
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0, _totalDuration - _elapsed);
    }

    /// <summary>
    /// 현재 경과 시간 반환
    /// </summary>
    public float GetElapsedTime()
    {
        return _elapsed;
    }

    /// <summary>
    /// 타이머 진행 중인지 확인
    /// </summary>
    public bool IsPlaying()
    {
        return _isPlaying;
    }

    /// <summary>
    /// 타이머 완료 여부 확인
    /// </summary>
    public bool IsCompleted()
    {
        return _elapsed >= _totalDuration;
    }
}
