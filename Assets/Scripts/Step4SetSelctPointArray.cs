using UnityEngine;

public class Step4SetSelctPointArray : MonoBehaviour
{
    [Header("달고나 포인트 부모 오브젝트 배열")]
    [SerializeField] private GameObject[] _dalgonaPointParnet;
    // 예: 0 = 별, 1 = 동그라미, 2 = 세모, 3 = 네모 등
    // 각 부모 안에 해당 모양의 포인트(Circle)들이 자식으로 들어있다고 가정

    /// <summary>
    /// 선택된 인덱스에 해당하는 달고나 포인트 부모만 활성화
    /// </summary>
    /// <param name="selectIndex">활성화할 인덱스 (0 ~ 배열 길이-1)</param>
    public void SetDalgonaPointParent(int selectIndex)
    {
        if (_dalgonaPointParnet == null || _dalgonaPointParnet.Length == 0)
            return;

        // 인덱스 범위 방어 코드
        if (selectIndex < 0 || selectIndex >= _dalgonaPointParnet.Length)
        {
            Debug.LogWarning($"[Step4SetSelctPointArray] 잘못된 인덱스: {selectIndex}");
            selectIndex = 0;
        }

        for (int i = 0; i < _dalgonaPointParnet.Length; i++)
        {
            if (_dalgonaPointParnet[i] == null)
                continue;

            // 선택된 인덱스만 활성화, 나머지는 비활성화
            _dalgonaPointParnet[i].SetActive(i == selectIndex);
        }
    }

    /// <summary>
    /// 리셋 함수
    /// - 0번 인덱스만 활성화, 나머지는 모두 비활성화
    /// </summary>
    public void ResetCall()
    {
        if (_dalgonaPointParnet == null || _dalgonaPointParnet.Length == 0)
            return;

        for (int i = 0; i < _dalgonaPointParnet.Length; i++)
        {
            if (_dalgonaPointParnet[i] == null)
                continue;

            _dalgonaPointParnet[i].SetActive(i == 0);
        }
    }
}
