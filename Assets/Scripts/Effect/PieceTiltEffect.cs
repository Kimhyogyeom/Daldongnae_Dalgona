using UnityEngine;

/// <summary>
/// 실패 시 피스가 X 위치에 따라 기울어지는 효과
/// - X가 양수면: Z 로테이션 -5도 (오른쪽으로 기울어짐)
/// - X가 음수면: Z 로테이션 +5도 (왼쪽으로 기울어짐)
/// - 활성화 시 자동으로 애니메이션 시작
/// - ResetCall로 원래 회전으로 복귀
/// </summary>
public class PieceTiltEffect : MonoBehaviour
{
    [Header("기울기 설정")]
    [SerializeField] private float _tiltAngle = 5f;          // 기울어질 각도

    [Header("애니메이션 설정")]
    [SerializeField] private float _duration = 0.5f;         // 기울어지는 시간
    [SerializeField] private float _delay = 0f;              // 시작 전 딜레이

    [Header("옵션")]
    [SerializeField] private bool _playOnEnable = true;

    private Quaternion _originalRotation;
    private Quaternion _targetRotation;
    private bool _isTilting;
    private float _elapsed;
    private float _delayElapsed;
    private bool _rotationCached;

    private void Awake()
    {
        CacheOriginalRotation();
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _delayElapsed = 0f;

        if (_playOnEnable)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isTilting) return;

        // 딜레이 처리
        if (_delayElapsed < _delay)
        {
            _delayElapsed += Time.deltaTime;
            return;
        }

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        // EaseOutQuad - 처음 빠르고 끝에 느려짐
        float easedT = 1f - Mathf.Pow(1f - t, 2f);

        transform.localRotation = Quaternion.Lerp(_originalRotation, _targetRotation, easedT);

        if (t >= 1f)
        {
            _isTilting = false;
        }
    }

    /// <summary>
    /// 원본 회전 캐싱
    /// </summary>
    private void CacheOriginalRotation()
    {
        if (!_rotationCached)
        {
            _originalRotation = transform.localRotation;
            _rotationCached = true;
        }
    }

    /// <summary>
    /// 애니메이션 시작
    /// </summary>
    public void Play()
    {
        CacheOriginalRotation();

        // X 위치에 따라 기울기 방향 결정
        float xPos = transform.localPosition.x;
        float tiltDirection = 0f;

        if (xPos > 0f)
        {
            // X가 양수 → Z 로테이션 감소 (오른쪽으로 기울어짐)
            tiltDirection = -_tiltAngle;
        }
        else if (xPos < 0f)
        {
            // X가 음수 → Z 로테이션 증가 (왼쪽으로 기울어짐)
            tiltDirection = _tiltAngle;
        }

        // 목표 회전 계산
        Vector3 currentEuler = _originalRotation.eulerAngles;
        _targetRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, currentEuler.z + tiltDirection);

        _isTilting = true;
        _elapsed = 0f;
        _delayElapsed = 0f;
    }

    /// <summary>
    /// 초기 회전으로 즉시 복귀
    /// </summary>
    public void ResetCall()
    {
        _isTilting = false;
        _elapsed = 0f;
        _delayElapsed = 0f;

        if (_rotationCached)
        {
            transform.localRotation = _originalRotation;
        }
    }

    /// <summary>
    /// 런타임에서 기울기 각도 설정
    /// </summary>
    public void SetTiltAngle(float angle)
    {
        _tiltAngle = angle;
    }

    /// <summary>
    /// 런타임에서 지속 시간 설정
    /// </summary>
    public void SetDuration(float duration)
    {
        _duration = duration;
    }

    /// <summary>
    /// 런타임에서 딜레이 설정
    /// </summary>
    public void SetDelay(float delay)
    {
        _delay = delay;
    }
}
