using UnityEngine;
using UnityEngine.UI;

public class ResetController : MonoBehaviour
{
    [Header("Step 1")]
    [SerializeField] private Step1ButtonController _step1ButtonController;

    [Header("Step 2")]
    [SerializeField] private Step2ButtonController _step2ButtonController;
    [SerializeField] private Step2VideoController _step2VideoController;

    [Header("Step 3")]
    [SerializeField] private Step3ButtonController _step3ButtonController;
    [SerializeField] private Step3SelectButtonController _step3SelectButtonController;

    [Header("Step 4")]
    [SerializeField] private Step4NeedleDrag _step4NeedleDrag;
    [SerializeField] private Step4NeedleTipDetector _step4NeedleTipDetector;
    [SerializeField] private Step4SetSelctPointArray _step4SetSelctPointArray;

    [SerializeField] private Button _resetButton;

    private void Awake()
    {
        _resetButton.onClick.AddListener(OnRessetCall);
    }

    private void OnRessetCall()
    {
        // Step 1 리셋
        _step1ButtonController.ResetCall();

        // Step 2 리셋
        _step2ButtonController.ResetCall();
        _step2VideoController.ResetCall();

        // Step 3 리셋
        _step3ButtonController.ResetCall();
        _step3SelectButtonController.ResetCall();

        // Step 4 리셋
        _step4NeedleDrag.ResetCall();
        _step4NeedleTipDetector.ResetCall();
        _step4SetSelctPointArray.ResetCall();
    }
}
