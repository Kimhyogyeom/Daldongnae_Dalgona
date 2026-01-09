using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 드릴 파편 효과 - 홀드 중 연속으로 파편이 튀어나감
/// Step4NeedleTipDetector에서 호출하여 사용
/// </summary>
public class DrillDebrisEffect : MonoBehaviour
{
    [Header("파편 프리팹 (Image가 붙은 UI 오브젝트)")]
    [SerializeField] private GameObject _debrisPrefab;

    [Header("파편 스프라이트 (랜덤 선택, 프리팹 없을 시 사용)")]
    [SerializeField] private Sprite[] _debrisSprites;

    [Header("스폰 설정")]
    [SerializeField] private float _spawnInterval = 0.08f;
    [SerializeField] private int _spawnCountPerInterval = 2;

    [Header("파편 크기")]
    [SerializeField] private float _minScale = 0.3f;
    [SerializeField] private float _maxScale = 0.7f;

    [Header("파편 속도")]
    [SerializeField] private float _minSpeed = 100f;
    [SerializeField] private float _maxSpeed = 250f;

    [Header("파편 방향 (각도 범위)")]
    [SerializeField] private float _minAngle = 0f;
    [SerializeField] private float _maxAngle = 360f;

    [Header("파편 수명")]
    [SerializeField] private float _lifetime = 0.5f;

    [Header("페이드 아웃")]
    [SerializeField] private bool _fadeOut = true;

    [Header("중력 효과")]
    [SerializeField] private float _gravity = 200f;

    [Header("회전 효과")]
    [SerializeField] private float _minRotationSpeed = -360f;
    [SerializeField] private float _maxRotationSpeed = 360f;

    [Header("렌더링 순서")]
    [SerializeField] private bool _useOverlayCanvas = true;
    [SerializeField] private int _overlaySortingOrder = 9999;

    [Header("오브젝트 풀 설정")]
    [SerializeField] private int _poolSize = 30;

    // ─────────────────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────────────────
    private List<Image> _pool = new List<Image>();
    private RectTransform _myRect;
    private Canvas _overlayCanvas;
    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;
    private Vector3 _spawnWorldPosition;

    private void Awake()
    {
        _myRect = GetComponent<RectTransform>();
        if (_myRect == null)
        {
            _myRect = gameObject.AddComponent<RectTransform>();
        }

        SetupOverlayCanvas();
        InitializePool();
    }

    private void SetupOverlayCanvas()
    {
        if (!_useOverlayCanvas) return;

        _overlayCanvas = GetComponent<Canvas>();
        if (_overlayCanvas == null)
        {
            _overlayCanvas = gameObject.AddComponent<Canvas>();
        }
        _overlayCanvas.overrideSorting = true;
        _overlayCanvas.sortingOrder = _overlaySortingOrder;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            Image img = CreatePooledImage();
            img.gameObject.SetActive(false);
            _pool.Add(img);
        }
    }

    private Image CreatePooledImage()
    {
        GameObject go;

        // 프리팹이 있으면 프리팹 사용, 없으면 동적 생성
        if (_debrisPrefab != null)
        {
            go = Instantiate(_debrisPrefab, transform);
            go.name = "Debris";
        }
        else
        {
            go = new GameObject("Debris");
            go.transform.SetParent(transform, false);

            // 각 파편에 Canvas 추가하여 렌더링 순서 강제
            Canvas debrisCanvas = go.AddComponent<Canvas>();
            debrisCanvas.overrideSorting = true;
            debrisCanvas.sortingOrder = _overlaySortingOrder;

            go.AddComponent<Image>();
        }

        Image img = go.GetComponent<Image>();
        if (img == null)
        {
            img = go.AddComponent<Image>();
        }
        img.raycastTarget = false;

        return img;
    }

    private Image GetFromPool()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].gameObject.activeInHierarchy)
            {
                return _pool[i];
            }
        }

        // 풀 부족 시 추가 생성
        Image newImg = CreatePooledImage();
        newImg.gameObject.SetActive(false);
        _pool.Add(newImg);
        return newImg;
    }

    /// <summary>
    /// 스폰 시작 - 월드 좌표 기준
    /// </summary>
    public void StartSpawning(Vector3 worldPosition)
    {
        if (_debrisSprites == null || _debrisSprites.Length == 0)
        {
            Debug.LogWarning("[DrillDebrisEffect] 파편 스프라이트가 없습니다!");
            return;
        }

        _spawnWorldPosition = worldPosition;
        _isSpawning = true;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 스폰 위치 업데이트 (바늘이 움직일 때)
    /// </summary>
    public void UpdateSpawnPosition(Vector3 worldPosition)
    {
        _spawnWorldPosition = worldPosition;
    }

    /// <summary>
    /// 스폰 중지
    /// </summary>
    public void StopSpawning()
    {
        _isSpawning = false;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            for (int i = 0; i < _spawnCountPerInterval; i++)
            {
                SpawnDebris();
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnDebris()
    {
        Image img = GetFromPool();
        if (img == null) return;

        // 프리팹이 없을 때만 스프라이트 배열에서 랜덤 선택
        if (_debrisPrefab == null && _debrisSprites != null && _debrisSprites.Length > 0)
        {
            Sprite sprite = _debrisSprites[Random.Range(0, _debrisSprites.Length)];
            img.sprite = sprite;
            img.SetNativeSize();
        }

        // 좌표 변환: 바늘 위치를 파편 컨테이너의 로컬 좌표로 변환
        Vector2 localPos;

        // 부모 Canvas 찾기
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        Camera cam = null;

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = parentCanvas.worldCamera;
        }

        // 스폰 위치가 UI 요소(RectTransform)의 위치인 경우
        // 해당 위치를 스크린 좌표로 변환 후 다시 로컬 좌표로 변환
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(cam, _spawnWorldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _myRect,
            screenPos,
            cam,
            out localPos
        );

        // 랜덤 파라미터
        float scale = Random.Range(_minScale, _maxScale);
        float angle = Random.Range(_minAngle, _maxAngle);
        float speed = Random.Range(_minSpeed, _maxSpeed);
        float rotSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed);
        float startRot = Random.Range(0f, 360f);

        // 초기 설정 (활성화 전에!)
        RectTransform rt = img.rectTransform;
        rt.anchoredPosition = localPos;
        rt.localScale = Vector3.one * scale;
        rt.localRotation = Quaternion.Euler(0f, 0f, startRot);

        // 색상 완전 불투명 흰색으로 설정
        img.color = Color.white;

        img.gameObject.SetActive(true);

        // 방향 계산
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        StartCoroutine(AnimateDebris(img, rt, direction, speed, rotSpeed));
    }

    private IEnumerator AnimateDebris(Image img, RectTransform rt, Vector2 direction, float speed, float rotSpeed)
    {
        float elapsed = 0f;
        Vector2 velocity = direction * speed;

        // 항상 흰색(불투명)에서 시작
        img.color = Color.white;

        while (elapsed < _lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _lifetime;

            // 중력 적용
            velocity.y -= _gravity * Time.deltaTime;

            // 위치 이동
            rt.anchoredPosition += velocity * Time.deltaTime;

            // 회전
            rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

            // 페이드 아웃
            if (_fadeOut)
            {
                Color c = Color.white;
                c.a = 1f - t;
                img.color = c;
            }

            yield return null;
        }

        img.gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 스폰 중인지 확인
    /// </summary>
    public bool IsSpawning => _isSpawning;
}
