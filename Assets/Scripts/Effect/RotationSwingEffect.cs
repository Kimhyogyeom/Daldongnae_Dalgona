using UnityEngine;

/// <summary>
/// 이미지(또는 다른 오브젝트)에 부드러운 좌우 흔들림 효과를 적용하는 스크립트
/// - Z축 회전이 지정된 각도 범위 내에서 부드럽게 왔다 갔다 함
/// - Inspector에서 회전 각도, 속도 등을 조절 가능
/// </summary>
public class RotationSwingEffect : MonoBehaviour
{
    [Header("회전 설정")]
    [SerializeField] private float _maxAngle = 15f;
    [SerializeField] private float _minAngle = -15f;

    [Header("속도 설정")]
    [SerializeField] private float _swingSpeed = 2.0f;

    [Header("옵션")]
    [SerializeField] private bool _playOnAwake = true;
    [SerializeField] private bool _useUnscaledTime = false;

    private Quaternion _originalRotation;
    private bool _isPlaying;
    private float _time;

    private void Awake()
    {
        _originalRotation = transform.localRotation;

        if (_playOnAwake)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _time += deltaTime * _swingSpeed;

        // Sin 함수를 사용하여 부드러운 스윙 효과 (-1~1 범위)
        float normalizedValue = Mathf.Sin(_time);
        float currentAngle = Mathf.Lerp(_minAngle, _maxAngle, (normalizedValue + 1f) / 2f);

        transform.localRotation = _originalRotation * Quaternion.Euler(0f, 0f, currentAngle);
    }

    /// <summary>
    /// 스윙 효과 재생 시작
    /// </summary>
    public void Play()
    {
        _isPlaying = true;
        _time = 0f;
    }

    /// <summary>
    /// 스윙 효과 일시정지
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }

    /// <summary>
    /// 스윙 효과 정지 및 원래 회전으로 복귀
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _time = 0f;
        transform.localRotation = _originalRotation;
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
    /// 런타임에서 스윙 속도 변경
    /// </summary>
    public void SetSwingSpeed(float speed)
    {
        _swingSpeed = speed;
    }

    /// <summary>
    /// 런타임에서 회전 각도 범위 변경
    /// </summary>
    public void SetAngleRange(float minAngle, float maxAngle)
    {
        _minAngle = minAngle;
        _maxAngle = maxAngle;
    }
}
