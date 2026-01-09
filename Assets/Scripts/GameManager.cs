using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 전체 흐름을 관리하는 매니저
/// - 패널 전환 (Step1 ~ Step4)
/// - 게임 결과 처리 (성공/실패)
/// - 자동 리셋 타이머
/// - 전체 리셋
///
/// 이 오브젝트는 항상 활성화 상태여야 함 (DontDestroyOnLoad 또는 씬 루트에 배치)
/// </summary>
public class GameManager : MonoBehaviour
{
    // 싱글톤 제거됨 - 각 캔버스에 독립적인 GameManager 인스턴스 사용
    // 다른 스크립트에서는 Inspector에서 직접 연결하여 사용

    [Header("패널들")]
    [SerializeField] private GameObject _step1Panel;
    [SerializeField] private GameObject _step2Panel;
    [SerializeField] private GameObject _step3Panel;
    [SerializeField] private GameObject _step4Panel;

    [Header("Step 1 컨트롤러")]
    [SerializeField] private Step1ButtonController _step1ButtonController;

    [Header("Step 2 컨트롤러")]
    [SerializeField] private Step2ButtonController _step2ButtonController;
    [SerializeField] private Step2VideoController _step2VideoController;

    [Header("Step 3 컨트롤러")]
    [SerializeField] private Step3ButtonController _step3ButtonController;
    [SerializeField] private Step3SelectButtonController _step3SelectButtonController;

    [Header("Step 4 컨트롤러")]
    [SerializeField] private Step4NeedleDrag _step4NeedleDrag;
    [SerializeField] private Step4NeedleTipDetector _step4NeedleTipDetector;
    [SerializeField] private Step4SetSelctPointArray _step4SetSelctPointArray;

    [Header("흔들림 효과 (리셋용)")]
    [SerializeField] private ShakeEffect[] _shakeEffects;

    [Header("결과 관련")]
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private GameObject _resultSuccessObject;
    [SerializeField] private GameObject _resultFailObject;
    [SerializeField] private GameObject[] _nextFlowObjects;        // 결과 후 활성화할 오브젝트들

    [Header("달고나 결과 오브젝트 (인덱스별)")]
    [SerializeField] private GameObject _successPieces0;
    [SerializeField] private GameObject _failObject0;
    [SerializeField] private GameObject _successPieces1;
    [SerializeField] private GameObject _failObject1;
    [SerializeField] private GameObject _successPieces2;
    [SerializeField] private GameObject _failObject2;
    [SerializeField] private GameObject _successPieces3;
    [SerializeField] private GameObject _failObject3;

    [Header("달고나 결과 시 추가 활성화 오브젝트 (인덱스별)")]
    [Tooltip("성공/실패 달고나 패널 활성화 시 같이 켜지고, 결과 패널 뜰 때 꺼짐")]
    [SerializeField] private GameObject _extraObject0;
    [SerializeField] private GameObject _extraObject1;
    [SerializeField] private GameObject _extraObject2;
    [SerializeField] private GameObject _extraObject3;

    [Header("타이머 설정")]
    [SerializeField] private float _step2ToStep3Delay = 5f;        // Step2 비디오 후 Step3까지 대기
    [SerializeField] private float _resultShowDelay = 10f;         // 달고나 결과 후 최종 결과 패널까지 대기
    [SerializeField] private float _autoTransitionDelay = 10f;     // 최종 결과 후 다음 흐름까지 대기
    [SerializeField] private float _autoResetDelay = 10f;          // 다음 흐름 후 Step1 리셋까지 대기

    [Header("자동 리셋 설정")]
    [SerializeField] private bool _enableAutoReset = true;

    [Header("리셋 버튼 (선택)")]
    [SerializeField] private Button _resetButton;

    // 내부 상태
    private bool _isWaitingForStep2ToStep3 = false;
    private bool _isWaitingForResultShow = false;
    private bool _isWaitingForTransition = false;
    private bool _isWaitingForReset = false;
    private bool _lastResultWasSuccess = false;
    private int _lastSelectIndex = 0;

    private Coroutine _currentTimerCoroutine;
    private Coroutine _step2TimerCoroutine;

    /// <summary>
    /// 현재 결과 화면 중인지 (외부에서 읽기 전용)
    /// 결과 화면 중에는 다른 버튼 클릭을 막기 위해 사용
    /// </summary>
    public bool IsShowingResult => _isWaitingForResultShow || _isWaitingForReset;

    private void Awake()
    {
        // 리셋 버튼 연결
        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(FullReset);
        }
    }

    private void Start()
    {
        // 초기 상태: Step1만 활성화
        InitializePanels();
    }

    /// <summary>
    /// 패널 초기화 (Step1만 활성화)
    /// </summary>
    private void InitializePanels()
    {
        if (_step1Panel != null) _step1Panel.SetActive(true);
        if (_step2Panel != null) _step2Panel.SetActive(false);
        if (_step3Panel != null) _step3Panel.SetActive(false);
        if (_step4Panel != null) _step4Panel.SetActive(false);

        HideResultPanel();
        HideAllDalgonaResults();
        HideNextFlowObjects();
    }

    // ─────────────────────────────────────────────────────
    // 패널 전환 메서드 (외부에서 호출)
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Step1 → Step2 전환
    /// </summary>
    public void GoToStep2()
    {
        if (_step1Panel != null) _step1Panel.SetActive(false);
        if (_step2Panel != null) _step2Panel.SetActive(true);
        Debug.Log("[GameManager] Step1 → Step2");
    }

    /// <summary>
    /// Step2 → Step3 전환
    /// </summary>
    public void GoToStep3()
    {
        if (_step2Panel != null) _step2Panel.SetActive(false);
        if (_step3Panel != null) _step3Panel.SetActive(true);
        Debug.Log("[GameManager] Step2 → Step3");
    }

    /// <summary>
    /// Step3 → Step4 전환
    /// </summary>
    public void GoToStep4()
    {
        if (_step3Panel != null) _step3Panel.SetActive(false);
        if (_step4Panel != null) _step4Panel.SetActive(true);
        Debug.Log("[GameManager] Step3 → Step4");
    }

    /// <summary>
    /// Step1로 돌아가기 (모든 패널 닫고 Step1만 열기)
    /// </summary>
    public void GoToStep1()
    {
        Debug.Log($"[GameManager] GoToStep1 - Step1Panel: {(_step1Panel != null ? _step1Panel.name : "NULL")}, Step4Panel: {(_step4Panel != null ? _step4Panel.name : "NULL")}");

        if (_step2Panel != null) _step2Panel.SetActive(false);
        if (_step3Panel != null) _step3Panel.SetActive(false);

        if (_step4Panel != null)
        {
            _step4Panel.SetActive(false);
            Debug.Log("[GameManager] Step4 패널 비활성화됨");
        }
        else
        {
            Debug.LogWarning("[GameManager] Step4 패널이 연결되지 않음!");
        }

        if (_step1Panel != null)
        {
            _step1Panel.SetActive(true);
            Debug.Log("[GameManager] Step1 패널 활성화됨");
        }
        else
        {
            Debug.LogWarning("[GameManager] Step1 패널이 연결되지 않음!");
        }

        Debug.Log("[GameManager] → Step1 완료");
    }

    // ─────────────────────────────────────────────────────
    // Step2 비디오 종료 후 자동 전환
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Step2 비디오 종료 시 호출 (Step2VideoController에서 호출)
    /// </summary>
    public void OnStep2VideoFinished()
    {
        Debug.Log($"[GameManager] Step2 비디오 종료, {_step2ToStep3Delay}초 후 Step3으로 이동");

        // 기존 타이머가 있으면 중지
        if (_step2TimerCoroutine != null)
        {
            StopCoroutine(_step2TimerCoroutine);
        }

        _isWaitingForStep2ToStep3 = true;
        _step2TimerCoroutine = StartCoroutine(Step2ToStep3TimerCoroutine());
    }

    private IEnumerator Step2ToStep3TimerCoroutine()
    {
        yield return new WaitForSeconds(_step2ToStep3Delay);

        if (_isWaitingForStep2ToStep3)
        {
            _isWaitingForStep2ToStep3 = false;
            GoToStep3();
            Debug.Log("[GameManager] Step2 → Step3 자동 전환 완료");
        }
    }

    /// <summary>
    /// Step2에서 Step3으로 전환 대기 중인지 (버튼 클릭 방지용)
    /// </summary>
    public bool IsWaitingForStep2ToStep3 => _isWaitingForStep2ToStep3;

    // ─────────────────────────────────────────────────────
    // 게임 결과 처리 (Step4NeedleTipDetector에서 호출)
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// 게임 성공 시 호출
    /// </summary>
    public void OnGameSuccess(int selectIndex)
    {
        _lastResultWasSuccess = true;
        _lastSelectIndex = selectIndex;

        Debug.Log($"[GameManager] 게임 성공! 인덱스: {selectIndex}");

        // 바늘 드래그 잠금 + 초기 위치로 이동
        if (_step4NeedleDrag != null)
        {
            _step4NeedleDrag.Lock();
        }

        // 달고나 결과 표시 (성공 조각 이동)
        ShowDalgonaResult(selectIndex, true);

        // 결과 패널 표시 타이머 시작
        StartResultShowTimer();
    }

    /// <summary>
    /// 게임 실패 시 호출
    /// </summary>
    public void OnGameFail(int selectIndex)
    {
        _lastResultWasSuccess = false;
        _lastSelectIndex = selectIndex;

        Debug.Log($"[GameManager] 게임 실패! 인덱스: {selectIndex}");

        // 바늘 드래그 잠금 + 초기 위치로 이동
        if (_step4NeedleDrag != null)
        {
            _step4NeedleDrag.Lock();
        }

        // 달고나 결과 표시 (실패 기울기)
        ShowDalgonaResult(selectIndex, false);

        // 결과 패널 표시 타이머 시작
        StartResultShowTimer();
    }

    // ─────────────────────────────────────────────────────
    // 타이머 관리
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// 결과 패널 표시 타이머 시작
    /// </summary>
    private void StartResultShowTimer()
    {
        StopCurrentTimer();
        _isWaitingForResultShow = true;
        _currentTimerCoroutine = StartCoroutine(ResultShowTimerCoroutine());
        Debug.Log($"[GameManager] 결과 표시 타이머 시작: {_resultShowDelay}초 후 결과 패널");
    }

    private IEnumerator ResultShowTimerCoroutine()
    {
        yield return new WaitForSeconds(_resultShowDelay);

        if (_isWaitingForResultShow)
        {
            _isWaitingForResultShow = false;
            ShowResultPanel(_lastResultWasSuccess);

            // 결과 패널 표시 후 바로 리셋 타이머 시작
            if (_enableAutoReset)
            {
                StartAutoResetTimer();
            }
        }
    }

    private IEnumerator AutoTransitionTimerCoroutine()
    {
        yield return new WaitForSeconds(_autoTransitionDelay);

        if (_isWaitingForTransition)
        {
            _isWaitingForTransition = false;
            ExecuteAutoTransition();
        }
    }

    /// <summary>
    /// 자동 리셋 타이머 시작
    /// </summary>
    private void StartAutoResetTimer()
    {
        StopCurrentTimer();
        _isWaitingForReset = true;
        _currentTimerCoroutine = StartCoroutine(AutoResetTimerCoroutine());
        Debug.Log($"[GameManager] 자동 리셋 타이머 시작: {_autoResetDelay}초 후 Step1로 리셋");
    }

    private IEnumerator AutoResetTimerCoroutine()
    {
        Debug.Log("[GameManager] AutoResetTimerCoroutine 시작됨");
        yield return new WaitForSeconds(_autoResetDelay);
        Debug.Log("[GameManager] AutoResetTimerCoroutine 대기 완료");

        if (_isWaitingForReset)
        {
            _isWaitingForReset = false;
            Debug.Log("[GameManager] ExecuteAutoReset 호출 직전");
            ExecuteAutoReset();
        }
        else
        {
            Debug.Log("[GameManager] _isWaitingForReset가 false라 리셋 안함");
        }
    }

    /// <summary>
    /// 현재 실행 중인 타이머 중지
    /// </summary>
    private void StopCurrentTimer()
    {
        if (_currentTimerCoroutine != null)
        {
            StopCoroutine(_currentTimerCoroutine);
            _currentTimerCoroutine = null;
        }
    }

    // ─────────────────────────────────────────────────────
    // 전환/리셋 실행
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// 자동 전환 실행 (결과 패널 후)
    /// </summary>
    private void ExecuteAutoTransition()
    {
        Debug.Log("[GameManager] 자동 전환 실행");

        // 달고나 결과 숨기기
        HideAllDalgonaResults();

        // 결과 패널 숨기기
        HideResultPanel();

        // 피스 효과 리셋
        ResetAllPieceEffects();

        // 다음 흐름 오브젝트 활성화
        ShowNextFlowObjects();

        // 자동 리셋 활성화 시 리셋 타이머 시작
        if (_enableAutoReset)
        {
            StartAutoResetTimer();
        }
    }

    /// <summary>
    /// 자동 리셋 실행 (Step1로 돌아가기)
    /// - 결과 패널 숨기기
    /// - 달고나 결과 숨기기
    /// - Step4 비활성화
    /// - Step1 활성화
    /// - 전체 리셋
    /// </summary>
    private void ExecuteAutoReset()
    {
        Debug.Log("[GameManager] 자동 리셋 실행: Step1로 돌아갑니다");

        // 결과 패널 숨기기
        HideResultPanel();

        // 달고나 결과 숨기기
        HideAllDalgonaResults();

        // 다음 흐름 오브젝트 숨기기
        HideNextFlowObjects();

        // Step4 비활성화, Step1 활성화
        if (_step4Panel != null)
        {
            _step4Panel.SetActive(false);
            Debug.Log("[GameManager] Step4 비활성화");
        }

        if (_step1Panel != null)
        {
            _step1Panel.SetActive(true);
            Debug.Log("[GameManager] Step1 활성화");
        }

        // 전체 리셋
        FullReset();

        Debug.Log("[GameManager] 리셋 완료!");
    }

    // ─────────────────────────────────────────────────────
    // 달고나 결과 관련
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// 달고나 결과 표시
    /// </summary>
    private void ShowDalgonaResult(int index, bool isSuccess)
    {
        HideAllDalgonaResults();

        GameObject target = isSuccess ? GetSuccessPieces(index) : GetFailObject(index);
        if (target != null)
        {
            target.SetActive(true);
        }

        // 인덱스에 맞는 추가 오브젝트 활성화
        GameObject extra = GetExtraObject(index);
        if (extra != null)
        {
            extra.SetActive(true);
        }
    }

    private GameObject GetSuccessPieces(int index)
    {
        switch (index)
        {
            case 0: return _successPieces0;
            case 1: return _successPieces1;
            case 2: return _successPieces2;
            case 3: return _successPieces3;
            default: return null;
        }
    }

    private GameObject GetFailObject(int index)
    {
        switch (index)
        {
            case 0: return _failObject0;
            case 1: return _failObject1;
            case 2: return _failObject2;
            case 3: return _failObject3;
            default: return null;
        }
    }

    private GameObject GetExtraObject(int index)
    {
        switch (index)
        {
            case 0: return _extraObject0;
            case 1: return _extraObject1;
            case 2: return _extraObject2;
            case 3: return _extraObject3;
            default: return null;
        }
    }

    private void HideAllDalgonaResults()
    {
        if (_successPieces0 != null) _successPieces0.SetActive(false);
        if (_successPieces1 != null) _successPieces1.SetActive(false);
        if (_successPieces2 != null) _successPieces2.SetActive(false);
        if (_successPieces3 != null) _successPieces3.SetActive(false);

        if (_failObject0 != null) _failObject0.SetActive(false);
        if (_failObject1 != null) _failObject1.SetActive(false);
        if (_failObject2 != null) _failObject2.SetActive(false);
        if (_failObject3 != null) _failObject3.SetActive(false);

        // 추가 오브젝트도 비활성화
        if (_extraObject0 != null) _extraObject0.SetActive(false);
        if (_extraObject1 != null) _extraObject1.SetActive(false);
        if (_extraObject2 != null) _extraObject2.SetActive(false);
        if (_extraObject3 != null) _extraObject3.SetActive(false);
    }

    /// <summary>
    /// 피스 효과 리셋 (PieceMoveEffect, PieceTiltEffect)
    /// </summary>
    private void ResetAllPieceEffects()
    {
        // 성공 조각 - PieceMoveEffect 리셋
        ResetPieceMoveInObject(_successPieces0);
        ResetPieceMoveInObject(_successPieces1);
        ResetPieceMoveInObject(_successPieces2);
        ResetPieceMoveInObject(_successPieces3);

        // 실패 조각 - PieceTiltEffect 리셋
        ResetPieceTiltInObject(_failObject0);
        ResetPieceTiltInObject(_failObject1);
        ResetPieceTiltInObject(_failObject2);
        ResetPieceTiltInObject(_failObject3);
    }

    private void ResetPieceMoveInObject(GameObject obj)
    {
        if (obj == null) return;
        PieceMoveEffect[] effects = obj.GetComponentsInChildren<PieceMoveEffect>(true);
        foreach (var effect in effects)
        {
            effect.ResetCall();
        }
    }

    private void ResetPieceTiltInObject(GameObject obj)
    {
        if (obj == null) return;
        PieceTiltEffect[] effects = obj.GetComponentsInChildren<PieceTiltEffect>(true);
        foreach (var effect in effects)
        {
            effect.ResetCall();
        }
    }

    // ─────────────────────────────────────────────────────
    // 결과 패널 관련
    // ─────────────────────────────────────────────────────

    private void ShowResultPanel(bool isSuccess)
    {
        if (_resultPanel != null)
            _resultPanel.SetActive(true);

        if (isSuccess)
        {
            if (_resultSuccessObject != null) _resultSuccessObject.SetActive(true);
            if (_resultFailObject != null) _resultFailObject.SetActive(false);
            Debug.Log("[GameManager] 성공 결과 패널 표시");
        }
        else
        {
            if (_resultFailObject != null) _resultFailObject.SetActive(true);
            if (_resultSuccessObject != null) _resultSuccessObject.SetActive(false);
            Debug.Log("[GameManager] 실패 결과 패널 표시");
        }
    }

    private void HideResultPanel()
    {
        if (_resultPanel != null) _resultPanel.SetActive(false);
        if (_resultSuccessObject != null) _resultSuccessObject.SetActive(false);
        if (_resultFailObject != null) _resultFailObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────
    // 다음 흐름 오브젝트 관련
    // ─────────────────────────────────────────────────────

    private void ShowNextFlowObjects()
    {
        if (_nextFlowObjects == null) return;
        foreach (var obj in _nextFlowObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        Debug.Log($"[GameManager] 다음 흐름 오브젝트 {_nextFlowObjects.Length}개 활성화");
    }

    private void HideNextFlowObjects()
    {
        if (_nextFlowObjects == null) return;
        foreach (var obj in _nextFlowObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────────────
    // 전체 리셋
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// 전체 게임 리셋 (모든 컨트롤러 리셋)
    /// </summary>
    public void FullReset()
    {
        Debug.Log("[GameManager] 전체 리셋 실행");

        // 타이머 중지
        StopCurrentTimer();
        if (_step2TimerCoroutine != null)
        {
            StopCoroutine(_step2TimerCoroutine);
            _step2TimerCoroutine = null;
        }
        _isWaitingForStep2ToStep3 = false;
        _isWaitingForResultShow = false;
        _isWaitingForTransition = false;
        _isWaitingForReset = false;

        // Step 1 리셋
        if (_step1ButtonController != null)
            _step1ButtonController.ResetCall();

        // Step 2 리셋
        if (_step2ButtonController != null)
            _step2ButtonController.ResetCall();
        if (_step2VideoController != null)
            _step2VideoController.ResetCall();

        // Step 3 리셋
        if (_step3ButtonController != null)
            _step3ButtonController.ResetCall();
        if (_step3SelectButtonController != null)
            _step3SelectButtonController.ResetCall();

        // Step 4 리셋
        if (_step4NeedleDrag != null)
            _step4NeedleDrag.ResetCall();
        if (_step4NeedleTipDetector != null)
            _step4NeedleTipDetector.ResetCall();
        if (_step4SetSelctPointArray != null)
            _step4SetSelctPointArray.ResetCall();

        // UI 리셋
        HideResultPanel();
        HideAllDalgonaResults();
        HideNextFlowObjects();
        ResetAllPieceEffects();
        ResetAllShakeEffects();
    }

    /// <summary>
    /// 모든 ShakeEffect 리셋
    /// </summary>
    private void ResetAllShakeEffects()
    {
        if (_shakeEffects == null) return;
        foreach (var effect in _shakeEffects)
        {
            if (effect != null)
            {
                effect.ResetCall();
            }
        }
    }
}
