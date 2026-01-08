using UnityEngine;
using UnityEngine.UI;

public class Step3ButtonController : MonoBehaviour
{
    [Header("패널 오브젝트들")]
    [SerializeField] private GameObject _step3Panel;   // STEP3 패널 (게임 준비 / 설명 화면)
    [SerializeField] private GameObject _step4Panel;   // STEP4 패널 (실제 게임 화면)

    [Header("버튼")]
    [SerializeField] private Button _gameStartButton;  // 게임 시작 버튼 (STEP3 -> STEP4 전환)

    private void Awake()
    {
        // 게임 시작 버튼 클릭 시 OnGameStartButton 함수 실행되도록 리스너 등록
        _gameStartButton.onClick.AddListener(OnGameStartButton);
    }

    /// <summary>
    /// STEP3 화면에서 "게임 시작" 버튼 눌렀을 때 호출
    /// - STEP3 패널 비활성화
    /// - STEP4 패널 활성화
    /// </summary>
    private void OnGameStartButton()
    {
        // 결과 화면 표시 중이면 클릭 무시
        if (GameManager.Instance != null && GameManager.Instance.IsShowingResult)
        {
            Debug.Log("[Step3] 결과 화면 중이라 버튼 클릭 무시");
            return;
        }

        if (_step3Panel != null && _step4Panel != null)
        {
            _step3Panel.SetActive(false);
            _step4Panel.SetActive(true);
        }
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 리셋 함수
    /// - STEP3 패널 활성화
    /// - STEP4 패널 비활성화
    /// </summary>
    public void ResetCall()
    {
        if (_step3Panel != null && _step4Panel != null)
        {
            _step3Panel.SetActive(false);
            _step4Panel.SetActive(false);
        }
    }
}
