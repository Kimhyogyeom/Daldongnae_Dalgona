using UnityEngine;

/// <summary>
/// 성공 시 피스가 초기 위치에서 목표 위치로 부드럽게 이동하는 효과
/// - 활성화 시 자동으로 애니메이션 시작
/// - ResetCall로 초기 위치로 복귀
/// </summary>
public class PieceMoveEffect : MonoBehaviour
{
    [Header("목표 위치 (로컬 좌표)")]
    [SerializeField] private Vector2 _targetPosition;

    [Header("애니메이션 설정")]
    [SerializeField] private float _duration = 1.5f;
    [SerializeField] private float _delay = 0f;

    [Header("옵션")]
    [SerializeField] private bool _playOnEnable = true;

    private Vector2 _startPosition;
    private Vector2 _originalPosition;
    private bool _isMoving;
    private float _elapsed;
    private float _delayElapsed;
    private bool _positionCached;

    private void Awake()
    {
        CacheOriginalPosition();
    }

    private void OnEnable()
    {
        // 시작 위치를 현재 위치로 설정
        _startPosition = transform.localPosition;
        _elapsed = 0f;
        _delayElapsed = 0f;

        if (_playOnEnable)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isMoving) return;

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

        Vector2 newPos = Vector2.Lerp(_startPosition, _targetPosition, easedT);
        transform.localPosition = new Vector3(newPos.x, newPos.y, transform.localPosition.z);

        if (t >= 1f)
        {
            _isMoving = false;
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
    /// 애니메이션 시작
    /// </summary>
    public void Play()
    {
        _isMoving = true;
        _elapsed = 0f;
        _delayElapsed = 0f;
        _startPosition = transform.localPosition;
    }

    /// <summary>
    /// 초기 위치로 즉시 복귀
    /// </summary>
    public void ResetCall()
    {
        _isMoving = false;
        _elapsed = 0f;
        _delayElapsed = 0f;

        if (_positionCached)
        {
            transform.localPosition = new Vector3(_originalPosition.x, _originalPosition.y, transform.localPosition.z);
        }
    }

    /// <summary>
    /// 런타임에서 목표 위치 설정
    /// </summary>
    public void SetTargetPosition(Vector2 target)
    {
        _targetPosition = target;
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
