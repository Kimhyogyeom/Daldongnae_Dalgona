using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 클릭/터치 시 폭죽(꽃가루) 효과
/// - World Space Canvas용
/// - 파편 풀링으로 GC 최소화
/// - 포물선 이동 + 회전 + 스케일 축소 + 페이드 아웃
/// </summary>
public class ConfettiEffect : MonoBehaviour
{
    [Header("Canvas 설정")]
    [SerializeField] private Canvas _canvas;

    [Header("파편 스타일 프리팹들")]
    [Tooltip("각기 다른 스프라이트/색/사이즈를 가진 Image 프리팹들")]
    [SerializeField] private Image[] _sparkPrefabs;

    [Header("풀 설정")]
    [SerializeField] private int _poolSize = 100;
    [SerializeField] private bool _allowPoolExpand = true;

    [Header("파편 설정")]
    [SerializeField] private int _sparksPerClick = 12;
    [SerializeField] private float _minSpeed = 200f;
    [SerializeField] private float _maxSpeed = 400f;
    [SerializeField] private float _gravity = -800f;
    [SerializeField] private float _lifetime = 0.8f;
    [SerializeField] private float _spawnRadius = 10f;
    [SerializeField] private float _maxSpawnDelay = 0.05f;

    [Header("회전 설정")]
    [SerializeField] private float _startRotationRange = 180f;
    [SerializeField] private float _minAngularSpeed = 90f;
    [SerializeField] private float _maxAngularSpeed = 360f;

    [Header("스케일 설정")]
    [SerializeField] private float _minStartScale = 0.3f;
    [SerializeField] private float _maxStartScale = 0.6f;

    [Header("옵션")]
    [SerializeField] private bool _detectTouchAutomatically = true;

    [Header("렌더링 순서")]
    [Tooltip("파티클이 다른 UI 위에 보이도록 별도 Canvas 사용")]
    [SerializeField] private bool _useOverlayCanvas = true;
    [SerializeField] private int _overlaySortingOrder = 100;

    private RectTransform _myRect;
    private Canvas _overlayCanvas;
    private Camera _worldCamera;
    private readonly List<Image> _pool = new List<Image>();
    private bool _isValid;

    private void Awake()
    {
        Debug.Log("[ConfettiEffect] Awake 시작");
        ValidateCanvas();
        SetupOverlayCanvas();
        InitPool();
        Debug.Log($"[ConfettiEffect] Awake 완료 - isValid: {_isValid}, poolCount: {_pool.Count}");
    }

    private void SetupOverlayCanvas()
    {
        if (!_useOverlayCanvas) return;

        // 이 오브젝트에 Canvas 컴포넌트 추가 (없으면)
        _overlayCanvas = GetComponent<Canvas>();
        if (_overlayCanvas == null)
        {
            _overlayCanvas = gameObject.AddComponent<Canvas>();
        }

        // 오버레이 설정
        _overlayCanvas.overrideSorting = true;
        _overlayCanvas.sortingOrder = _overlaySortingOrder;

        // GraphicRaycaster 추가 (없으면) - 필요 시
        if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        Debug.Log($"[ConfettiEffect] 오버레이 Canvas 설정 완료 - sortingOrder: {_overlaySortingOrder}");
    }

    private void ValidateCanvas()
    {
        _isValid = false;

        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        if (_canvas == null)
        {
            Debug.LogError("[ConfettiEffect] Canvas를 찾을 수 없습니다.");
            return;
        }

        _myRect = GetComponent<RectTransform>();
        _worldCamera = _canvas.worldCamera;

        if (_worldCamera == null)
        {
            _worldCamera = Camera.main;
        }

        _isValid = true;
    }

    private void InitPool()
    {
        if (!_isValid) return;
        if (_sparkPrefabs == null || _sparkPrefabs.Length == 0)
        {
            Debug.LogError("[ConfettiEffect] sparkPrefabs가 비어있습니다.");
            return;
        }

        _pool.Clear();

        var basePrefab = _sparkPrefabs[0];

        for (int i = 0; i < _poolSize; i++)
        {
            var img = CreatePooledImage(basePrefab, i);
            _pool.Add(img);
        }
    }

    private Image CreatePooledImage(Image basePrefab, int index)
    {
        var img = Instantiate(basePrefab, transform);
        img.name = $"Spark_{index}";
        img.raycastTarget = false;
        img.gameObject.SetActive(false);
        return img;
    }

    private void Update()
    {
        if (!_isValid)
        {
            Debug.LogWarning("[ConfettiEffect] isValid가 false입니다.");
            return;
        }
        if (!_detectTouchAutomatically) return;

        // 마우스 클릭
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            Debug.Log($"[ConfettiEffect] 마우스 클릭 감지: {pos}");
            SpawnConfettiAtScreenPosition(pos);
        }

        // 터치 입력
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    Vector2 pos = touch.position.ReadValue();
                    Debug.Log($"[ConfettiEffect] 터치 감지: {pos}");
                    SpawnConfettiAtScreenPosition(pos);
                }
            }
        }
    }

    /// <summary>
    /// 스크린 좌표에서 폭죽 생성
    /// </summary>
    public void SpawnConfettiAtScreenPosition(Vector2 screenPosition)
    {
        if (!_isValid) return;

        Vector2 localPos;

        // RectTransformUtility를 사용해서 스크린 좌표를 Canvas 로컬 좌표로 변환
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _myRect,
            screenPosition,
            _worldCamera,
            out localPos
        );

        if (!success)
        {
            Debug.LogWarning($"[ConfettiEffect] 좌표 변환 실패: {screenPosition}");
            return;
        }

        Debug.Log($"[ConfettiEffect] 스크린: {screenPosition} → 로컬: {localPos}");

        SpawnConfetti(new Vector3(localPos.x, localPos.y, 0f));
    }

    /// <summary>
    /// 로컬 좌표에서 폭죽 생성
    /// </summary>
    public void SpawnConfetti(Vector3 centerLocalPos)
    {
        if (!_isValid) return;
        if (_sparkPrefabs == null || _sparkPrefabs.Length == 0) return;

        for (int i = 0; i < _sparksPerClick; i++)
        {
            var img = GetPooledImage();
            if (img == null) continue;

            // 랜덤 스타일 적용
            ApplyRandomStyle(img);

            // 시작 위치 (클릭 지점 주변 랜덤)
            Vector2 startPos = new Vector2(centerLocalPos.x, centerLocalPos.y);
            startPos += Random.insideUnitCircle * _spawnRadius;

            // 360도 원형으로 퍼지게
            float angleDeg = Random.Range(0f, 360f);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // 초기 속도
            float speed = Random.Range(_minSpeed, _maxSpeed);
            Vector2 velocity = new Vector2(
                Mathf.Cos(angleRad) * speed,
                Mathf.Sin(angleRad) * speed
            );

            // 생성 딜레이
            float delay = Random.Range(0f, _maxSpawnDelay);

            // 시작 회전
            float startRot = Random.Range(-_startRotationRange, _startRotationRange);

            // 회전 속도
            float angularSpeed = Random.Range(_minAngularSpeed, _maxAngularSpeed);
            angularSpeed *= (Random.value < 0.5f) ? -1f : 1f;

            // 시작 스케일
            float startScale = Random.Range(_minStartScale, _maxStartScale);

            // 활성화 전에 위치/회전/스케일 먼저 설정 (화면 가운데에 잠깐 보이는 문제 방지)
            RectTransform rt = img.rectTransform;
            rt.anchoredPosition = startPos;
            rt.localRotation = Quaternion.Euler(0f, 0f, startRot);
            rt.localScale = Vector3.one * startScale;

            img.gameObject.SetActive(true);

            StartCoroutine(SparkRoutine(img, startPos, velocity, delay, startRot, angularSpeed, startScale));
        }
    }

    private Image GetPooledImage()
    {
        // 비활성화된 이미지 찾기
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].gameObject.activeSelf)
                return _pool[i];
        }

        // 풀 확장 허용 시 새로 생성
        if (_allowPoolExpand && _sparkPrefabs != null && _sparkPrefabs.Length > 0)
        {
            var img = CreatePooledImage(_sparkPrefabs[0], _pool.Count);
            _pool.Add(img);
            return img;
        }

        return null;
    }

    private void ApplyRandomStyle(Image img)
    {
        if (_sparkPrefabs == null || _sparkPrefabs.Length == 0) return;

        var template = _sparkPrefabs[Random.Range(0, _sparkPrefabs.Length)];
        if (template == null) return;

        img.sprite = template.sprite;
        img.color = template.color;

        var rt = img.rectTransform;
        var trt = template.rectTransform;
        rt.sizeDelta = trt.sizeDelta;
        rt.pivot = trt.pivot;
    }

    /// <summary>
    /// 개별 파편 애니메이션 코루틴
    /// </summary>
    private IEnumerator SparkRoutine(
        Image img,
        Vector2 startPos,
        Vector2 initialVelocity,
        float delay,
        float startRotation,
        float angularSpeed,
        float startScale)
    {
        RectTransform rt = img.rectTransform;

        // 딜레이
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float t = 0f;
        Vector2 pos = startPos;
        Vector2 velocity = initialVelocity;
        Color startColor = img.color;
        float currentRot = startRotation;

        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one * startScale;
        rt.localRotation = Quaternion.Euler(0f, 0f, currentRot);

        while (t < _lifetime)
        {
            float dt = Time.deltaTime;
            t += dt;

            // 중력 적용
            velocity.y += _gravity * dt;

            // 위치 업데이트
            pos += velocity * dt;
            rt.anchoredPosition = pos;

            // 회전 업데이트
            currentRot += angularSpeed * dt;
            rt.localRotation = Quaternion.Euler(0f, 0f, currentRot);

            // 진행률 (0 ~ 1)
            float progress = t / _lifetime;

            // 스케일 점점 줄어들기
            float scale = Mathf.Lerp(startScale, 0f, progress);
            rt.localScale = Vector3.one * scale;

            // 알파 점점 감소 (후반 40%에서 페이드)
            float fadeStart = 0.6f;
            if (progress > fadeStart)
            {
                float fadeProgress = (progress - fadeStart) / (1f - fadeStart);
                var c = startColor;
                c.a = Mathf.Lerp(1f, 0f, fadeProgress);
                img.color = c;
            }

            yield return null;
        }

        // 풀로 반환
        img.gameObject.SetActive(false);
    }

    /// <summary>
    /// 리셋 (모든 파편 비활성화)
    /// </summary>
    public void ResetCall()
    {
        StopAllCoroutines();

        foreach (var img in _pool)
        {
            if (img != null)
            {
                img.gameObject.SetActive(false);
            }
        }
    }
}
