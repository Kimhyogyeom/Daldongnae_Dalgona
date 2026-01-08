using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 버튼에 호버/클릭 시 스케일 커지는 효과를 적용하는 스크립트
/// - 마우스 오버 또는 터치 시 스케일 증가
/// - 마우스 벗어나거나 터치 끝나면 원래 크기로 복귀
/// - 비활성화 시에도 원래 크기로 복귀
/// </summary>
public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("스케일 설정")]
    [SerializeField] private float _hoverScale = 1.1f;
    [SerializeField] private float _pressedScale = 1.15f;

    [Header("속도 설정")]
    [SerializeField] private float _scaleUpDuration = 0.1f;
    [SerializeField] private float _scaleDownDuration = 0.08f;

    private Vector3 _originalScale;
    private float _targetScale;
    private float _currentScale;
    private bool _isPointerInside;
    private bool _isPressed;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _targetScale = 1f;
        _currentScale = 1f;
    }

    private void OnEnable()
    {
        // 활성화 시 원래 스케일로 초기화
        _targetScale = 1f;
        _currentScale = 1f;
        _isPointerInside = false;
        _isPressed = false;
        transform.localScale = _originalScale;
    }

    private void OnDisable()
    {
        // 비활성화 시 원래 스케일로 복귀
        _currentScale = 1f;
        _targetScale = 1f;
        transform.localScale = _originalScale;
    }

    private void Update()
    {
        if (Mathf.Approximately(_currentScale, _targetScale)) return;

        float duration = _targetScale > _currentScale ? _scaleUpDuration : _scaleDownDuration;
        float speed = duration > 0f ? 1f / duration : 100f;

        _currentScale = Mathf.MoveTowards(_currentScale, _targetScale, speed * Time.unscaledDeltaTime);
        transform.localScale = _originalScale * _currentScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerInside = true;
        UpdateTargetScale();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerInside = false;
        _isPressed = false;
        UpdateTargetScale();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        UpdateTargetScale();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        UpdateTargetScale();
    }

    private void UpdateTargetScale()
    {
        if (_isPressed && _isPointerInside)
        {
            _targetScale = _pressedScale;
        }
        else if (_isPointerInside)
        {
            _targetScale = _hoverScale;
        }
        else
        {
            _targetScale = 1f;
        }
    }

    /// <summary>
    /// 리셋 시 호출 (ResetController 연동용)
    /// </summary>
    public void ResetCall()
    {
        _isPointerInside = false;
        _isPressed = false;
        _targetScale = 1f;
        _currentScale = 1f;
        transform.localScale = _originalScale;
    }
}
