using UnityEngine;
using UnityEngine.UI;

public class Step3SelectButtonController : MonoBehaviour
{
    [Header("Step4 포인트 배열 설정 컨트롤러")]
    [SerializeField] private Step4SetSelctPointArray _step4SetSelctPointArray;
    // Step4에서 사용할 포인트 배열의 부모를 설정해주는 스크립트

    [Header("선택 버튼들")]
    [SerializeField] private Button[] _selectButton;
    // 선택용 버튼 4개 (예: 모양 선택 버튼)

    [Header("선택 표시 오브젝트들")]
    [SerializeField] private GameObject[] _selectImageObject;
    // 각 선택 상태를 표시해줄 이미지 오브젝트들 (예: 체크 표시, 하이라이트 등)

    [Header("현재 선택 인덱스 (0~3)")]
    public int _selectIndex = 0;
    // 현재 선택된 버튼/모양의 인덱스

    private void Awake()
    {
        // 버튼에 클릭 이벤트 등록
        // 각 버튼이 눌렸을 때, 해당 인덱스를 선택하도록 연결
        _selectButton[0].onClick.AddListener(OnStep3SelectButton0);
        _selectButton[1].onClick.AddListener(OnStep3SelectButton1);
        _selectButton[2].onClick.AddListener(OnStep3SelectButton2);
        _selectButton[3].onClick.AddListener(OnStep3SelectButton3);
    }

    /// <summary>
    /// 결과 화면 중인지 확인
    /// </summary>
    private bool IsResultShowing()
    {
        return GameManager.Instance != null && GameManager.Instance.IsShowingResult;
    }

    /// <summary>
    /// 0번 선택 버튼 클릭 시
    /// </summary>
    private void OnStep3SelectButton0()
    {
        if (IsResultShowing()) return;
        _selectIndex = 0;
        SelectObjectActiveCtrl(_selectImageObject[0]);
    }

    /// <summary>
    /// 1번 선택 버튼 클릭 시
    /// </summary>
    private void OnStep3SelectButton1()
    {
        if (IsResultShowing()) return;
        _selectIndex = 1;
        SelectObjectActiveCtrl(_selectImageObject[1]);
    }

    /// <summary>
    /// 2번 선택 버튼 클릭 시
    /// </summary>
    private void OnStep3SelectButton2()
    {
        if (IsResultShowing()) return;
        _selectIndex = 2;
        SelectObjectActiveCtrl(_selectImageObject[2]);
    }

    /// <summary>
    /// 3번 선택 버튼 클릭 시
    /// </summary>
    private void OnStep3SelectButton3()
    {
        if (IsResultShowing()) return;
        _selectIndex = 3;
        SelectObjectActiveCtrl(_selectImageObject[3]);
    }

    /// <summary>
    /// 선택된 인덱스에 해당하는 선택 표시 오브젝트만 활성화하고
    /// 나머지는 비활성화한 뒤, Step4 쪽에 현재 선택 인덱스를 전달
    /// </summary>
    /// <param name="selectImageObject">활성화할 선택 표시 오브젝트</param>
    private void SelectObjectActiveCtrl(GameObject selectImageObject)
    {
        // 선택 표시 오브젝트들 중에서 파라미터로 들어온 오브젝트만 활성화
        for (int i = 0; i < _selectImageObject.Length; i++)
        {
            if (_selectImageObject[i] == selectImageObject)
            {
                _selectImageObject[i].SetActive(true);
            }
            else
            {
                _selectImageObject[i].SetActive(false);
            }
        }

        // Step4에서 사용할 포인트(원형 콜라이더들)의 부모를
        // 현재 선택 인덱스에 맞게 설정하도록 전달
        if (_step4SetSelctPointArray != null)
        {
            _step4SetSelctPointArray.SetDalgonaPointParent(_selectIndex);
        }
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 리셋 함수
    /// 0번 선택 버튼 클릭 시
    /// </summary>
    public void ResetCall()
    {
        OnStep3SelectButton0();
    }
}
