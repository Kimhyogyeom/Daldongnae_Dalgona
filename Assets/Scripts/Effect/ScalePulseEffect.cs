using UnityEngine;

/// <summary>
/// 이미지(또는 다른 오브젝트)에 역동적인 스케일 펄스 효과를 적용하는 스크립트
/// - 천천히 커졌다가 확 돌아오는 역동적인 애니메이션
/// - Inspector에서 최대 스케일, 속도, 이징 등을 조절 가능
/// </summary>
public class ScalePulseEffect : MonoBehaviour
{
    [Header("스케일 설정")]
    [SerializeField] private float _maxScale = 1.2f;
    [SerializeField] private float _minScale = 1.0f;

    [Header("속도 설정")]
    [SerializeField] private float _growDuration = 0.8f;
    [SerializeField] private float _shrinkDuration = 0.15f;

    [Header("옵션")]
    [SerializeField] private bool _playOnAwake = true;
    [SerializeField] private bool _useUnscaledTime = false;

    private Vector3 _originalScale;
    private bool _isPlaying;
    private float _time;
    private bool _isGrowing;

    private void Awake()
    {
        _originalScale = transform.localScale;

        if (_playOnAwake)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _time += deltaTime;

        float currentScale;

        if (_isGrowing)
        {
            // 천천히 커지는 구간 (EaseOut - 처음 빠르고 끝에 느려짐)
            float t = Mathf.Clamp01(_time / _growDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 2f); // EaseOutQuad
            currentScale = Mathf.Lerp(_minScale, _maxScale, easedT);

            if (t >= 1f)
            {
                _isGrowing = false;
                _time = 0f;
            }
        }
        else
        {
            // 확 돌아오는 구간 (EaseIn - 처음 느리고 끝에 빨라짐)
            float t = Mathf.Clamp01(_time / _shrinkDuration);
            float easedT = t * t * t; // EaseInCubic - 더 급격하게
            currentScale = Mathf.Lerp(_maxScale, _minScale, easedT);

            if (t >= 1f)
            {
                _isGrowing = true;
                _time = 0f;
            }
        }

        transform.localScale = _originalScale * currentScale;
    }

    /// <summary>
    /// 펄스 효과 재생 시작
    /// </summary>
    public void Play()
    {
        _isPlaying = true;
        _isGrowing = true;
        _time = 0f;
    }

    /// <summary>
    /// 펄스 효과 일시정지
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }

    /// <summary>
    /// 펄스 효과 정지 및 원래 스케일로 복귀
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _time = 0f;
        transform.localScale = _originalScale;
    }

    /// <summary>
    /// 리셋 시 호출 (ResetController 연동용)
    /// </summary>
    public void ResetCall()
    {
        Stop();

        if (_playOnAwake)
        {
            Play();
        }
    }

    /// <summary>
    /// 런타임에서 커지는 시간 변경
    /// </summary>
    public void SetGrowDuration(float duration)
    {
        _growDuration = duration;
    }

    /// <summary>
    /// 런타임에서 줄어드는 시간 변경
    /// </summary>
    public void SetShrinkDuration(float duration)
    {
        _shrinkDuration = duration;
    }

    /// <summary>
    /// 런타임에서 최대 스케일 변경
    /// </summary>
    public void SetMaxScale(float maxScale)
    {
        _maxScale = maxScale;
    }
}
