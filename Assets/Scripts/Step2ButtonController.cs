using UnityEngine;
using UnityEngine.UI;

public class Step2ButtonController : MonoBehaviour
{
    [Header("GameManager 참조")]
    [SerializeField] private GameManager _gameManager;

    [Header("패널 오브젝트들")]
    [SerializeField] private GameObject _step2Panel;   // STEP2 패널 (게임 설명 or 준비 화면)
    [SerializeField] private GameObject _step3Panel;   // STEP3 패널 (실제 게임 화면)

    [Header("버튼")]
    [SerializeField] private Button _gameStartButton;  // 게임 시작 버튼 (STEP2 → STEP3 전환 트리거)

    private void Awake()
    {
        // 게임 시작 버튼 클릭 시 OnStep1StartButton 함수 실행되도록 리스너 등록
        // (함수명은 OnGameStartButton 같이 바꿔도 됨)
        _gameStartButton.onClick.AddListener(OnStep1StartButton);
    }

    /// <summary>
    /// STEP2 화면에서 "게임 시작" 버튼 눌렀을 때 호출
    /// - STEP2 패널 비활성화
    /// - STEP3 패널 활성화
    /// </summary>
    private void OnStep1StartButton()
    {
        // 결과 화면 표시 중이면 클릭 무시
        if (_gameManager != null && _gameManager.IsShowingResult)
        {
            Debug.Log("[Step2] 결과 화면 중이라 버튼 클릭 무시");
            return;
        }

        // Step2 → Step3 자동 전환 대기 중이면 클릭 무시 (타이머로만 전환)
        if (_gameManager != null && _gameManager.IsWaitingForStep2ToStep3)
        {
            Debug.Log("[Step2] 자동 전환 대기 중이라 버튼 클릭 무시");
            return;
        }

        if (_step2Panel != null && _step3Panel != null)
        {
            _step2Panel.SetActive(false);
            _step3Panel.SetActive(true);
        }
    }

    /// <summary>
    /// STEP2/STEP3 상태를 초기 상태로 되돌리는 함수
    /// - STEP2 패널 활성화
    /// - STEP3 패널 비활성화
    /// </summary>
    public void ResetCall()
    {
        if (_step2Panel != null && _step3Panel != null)
        {
            _step2Panel.SetActive(false);
            _step3Panel.SetActive(false);
        }
    }
}
