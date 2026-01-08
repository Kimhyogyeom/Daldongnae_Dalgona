using UnityEngine;
using UnityEngine.UI;

public class Step1ButtonController : MonoBehaviour
{
    [Header("패널 오브젝트들")]
    [SerializeField] private GameObject _step1Panel;   // STEP1 패널 (처음 화면)
    [SerializeField] private GameObject _step2Panel;   // STEP2 패널 (다음 화면)

    [Header("버튼")]
    [SerializeField] private Button _step1StartButton; // STEP1에서 다음으로 넘어가는 시작 버튼

    private void Awake()
    {
        // 시작 버튼 클릭 시 OnStep1StartButton 함수 실행되도록 리스너 등록
        _step1StartButton.onClick.AddListener(OnStep1StartButton);
    }

    /// <summary>
    /// STEP1 시작 버튼 눌렀을 때 호출
    /// - STEP1 패널 비활성화
    /// - STEP2 패널 활성화
    /// </summary>
    private void OnStep1StartButton()
    {
        // 결과 화면 표시 중이면 클릭 무시
        if (GameManager.Instance != null && GameManager.Instance.IsShowingResult)
        {
            Debug.Log("[Step1] 결과 화면 중이라 버튼 클릭 무시");
            return;
        }

        if (_step1Panel != null && _step2Panel != null)
        {
            _step1Panel.SetActive(false);
            _step2Panel.SetActive(true);
        }
    }

    /// <summary>
    /// 상태를 초기 화면으로 되돌리는 함수
    /// - STEP1 패널 활성화
    /// - STEP2 패널 비활성화
    /// </summary>
    public void ResetCall()
    {
        if (_step1Panel != null && _step2Panel != null)
        {
            _step1Panel.SetActive(true);
            _step2Panel.SetActive(false);
        }
    }
}
