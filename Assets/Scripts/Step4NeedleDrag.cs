using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Step4NeedleDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("드롭 가능 영역 (World Space Canvas의 UI Image)")]
    [SerializeField] private Image _dropAreaImage;
    // 드롭 가능한 영역을 나타내는 UI Image (World Space Canvas 상의 RectTransform 사용)

    private Camera _worldCamera;      // 펜을 보는 카메라 (일반적으로 Main Camera)
    private bool _isDragging = false; // 현재 드래그 중인지 여부
    private int _pointerId;           // 이 오브젝트를 잡고 있는 터치/포인터 ID
    private Vector3 _offset;          // 드래그 시작 시 오브젝트 위치 - 포인터 위치

    private Vector3 _initialPosition; // 초기 위치 (리셋용)

    private void Awake()
    {
        _worldCamera = Camera.main;
    }

    private void Start()
    {
        // 시작 시 위치를 초기 위치로 저장
        _initialPosition = transform.position;
    }

    /// <summary>
    /// 포인터(터치/마우스)가 오브젝트를 눌렀을 때 호출
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragging) return;

        _isDragging = true;
        _pointerId = eventData.pointerId;

        Vector3 worldPos = ScreenToWorld(eventData.position);
        _offset = transform.position - worldPos;
    }

    /// <summary>
    /// 드래그 중일 때 매 프레임마다 호출
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        if (eventData.pointerId != _pointerId) return;

        Vector3 worldPos = ScreenToWorld(eventData.position);
        transform.position = worldPos + _offset;
    }

    /// <summary>
    /// 포인터를 뗐을 때 호출
    /// - 오브젝트의 최종 위치가 드롭 영역 밖이면 초기 위치로 되돌림
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging) return;
        if (eventData.pointerId != _pointerId) return;

        _isDragging = false;

        // 오브젝트의 현재 월드 위치 기준으로, UI Image 영역 안/밖 판정
        if (!IsInsideDropAreaWorld(transform.position))
        {
            // UI 영역 밖에서 놓았을 때만 원래 자리로 복귀
            ResetToInitialPosition();
        }
        // UI 영역 안이면 그대로 그 자리에 둠
    }

    /// <summary>
    /// 스크린 좌표를 월드 좌표로 변환 (2D 오브젝트 이동용)
    /// </summary>
    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        if (_worldCamera == null)
            _worldCamera = Camera.main;

        var pos = new Vector3(screenPos.x, screenPos.y, -_worldCamera.transform.position.z);
        Vector3 world = _worldCamera.ScreenToWorldPoint(pos);
        world.z = transform.position.z; // 현재 Z 값 유지
        return world;
    }

    /// <summary>
    /// 월드 좌표 기준으로, 주어진 위치가 드롭 가능 UI Image 영역 안에 있는지 확인
    /// (World Space Canvas의 RectTransform.GetWorldCorners 사용)
    /// </summary>
    private bool IsInsideDropAreaWorld(Vector3 worldPos)
    {
        if (_dropAreaImage == null)
        {
            // 드롭 영역을 지정하지 않았다면 항상 허용
            return true;
        }

        RectTransform rect = _dropAreaImage.rectTransform;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // 회전 상관없이 4개 코너에서 min/max를 계산
        float minX = corners[0].x;
        float maxX = corners[0].x;
        float minY = corners[0].y;
        float maxY = corners[0].y;

        for (int i = 1; i < 4; i++)
        {
            Vector3 c = corners[i];
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }

        bool insideX = worldPos.x >= minX && worldPos.x <= maxX;
        bool insideY = worldPos.y >= minY && worldPos.y <= maxY;

        return insideX && insideY;
    }

    /// <summary>
    /// 초기 위치로 되돌리는 내부용 함수
    /// </summary>
    private void ResetToInitialPosition()
    {
        transform.position = _initialPosition;
        _isDragging = false;
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 리셋 함수
    /// - 오브젝트를 초기 위치로 되돌림
    /// </summary>
    public void ResetCall()
    {
        ResetToInitialPosition();
    }
}
