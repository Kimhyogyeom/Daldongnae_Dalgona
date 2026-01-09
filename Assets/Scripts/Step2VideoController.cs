using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Step2 영상 재생 컨트롤러
/// - 패널이 켜질 때(OnEnable) 자동 재생(옵션)
/// - 외부에서 ResetCall()로 다시 재생 요청 가능
/// - 동시에 재생되는 VideoPlayer 수를 최대 4개로 제한
/// </summary>
public class Step2VideoController : MonoBehaviour
{
    [Header("GameManager 참조")]
    [SerializeField] private GameManager _gameManager;

    [Header("Video")]
    [SerializeField] private VideoPlayer _videoPlayer;    // 재생할 VideoPlayer

    [Header("On Video End")]
    [SerializeField] private GameObject _objectSuccess;   // 영상 재생이 끝난 후 활성화할 오브젝트

    [Header("On Enable 자동 재생 여부")]
    [SerializeField] private bool _autoPlayOnEnable = true;

    // 현재 이 컨트롤러가 영상 재생 중인지 여부
    private bool _isPlaying = false;

    private void OnEnable()
    {
        // 패널이 활성화될 때마다 초기 상태로 세팅 후 자동 재생 옵션 처리
        if (_autoPlayOnEnable)
        {
            InitializeAndPlay();
        }
        else
        {
            // 자동 재생을 안 쓴다면, 성공 오브젝트만 꺼두고 대기
            if (_objectSuccess != null)
                _objectSuccess.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // 패널이 비활성화될 때 이벤트 등록 해제
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoFinished;
        }

        // 재생 중이었다면 플래그 리셋
        _isPlaying = false;
    }

    /// <summary>
    /// 초기 상태 세팅 후 영상 재생을 시작하는 공통 처리
    /// </summary>
    private void InitializeAndPlay()
    {
        // VideoPlayer가 유효한지 확인
        if (_videoPlayer == null)
            return;

        // VideoPlayer가 비활성화 상태면 재생 시도하지 않음
        if (!_videoPlayer.gameObject.activeInHierarchy || !_videoPlayer.enabled)
        {
            Debug.LogWarning("[Step2VideoController] VideoPlayer가 비활성화 상태입니다. Play()를 호출하지 않습니다.");
            return;
        }

        // 성공 오브젝트 비활성화
        if (_objectSuccess != null)
        {
            _objectSuccess.SetActive(false);
        }

        // 이벤트 중복 등록 방지
        _videoPlayer.loopPointReached -= OnVideoFinished;
        _videoPlayer.loopPointReached += OnVideoFinished;

        _isPlaying = true;

        // 영상 처음부터 다시 재생
        _videoPlayer.Stop();
        _videoPlayer.Play();
    }

    /// <summary>
    /// 영상 재생이 끝났을 때 호출되는 콜백
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        // 영상 종료 후 성공 오브젝트 활성화
        if (_objectSuccess != null)
        {
            _objectSuccess.SetActive(true);
        }

        // 재생 종료 처리
        _isPlaying = false;

        // GameManager에 비디오 종료 알림 (자동 전환 타이머 시작)
        if (_gameManager != null)
        {
            _gameManager.OnStep2VideoFinished();
        }
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 리셋 함수
    /// - 성공 오브젝트를 끄고
    /// - (가능하다면) 영상을 처음부터 다시 재생
    /// </summary>
    public void ResetCall()
    {
        // 이 스크립트가 붙은 오브젝트(패널)가 비활성화 상태면 재생하지 않음
        // (비활성화 상태에서는 OnEnable에서 다시 재생되므로 정상 동작)
        if (!gameObject.activeInHierarchy)
            return;

        InitializeAndPlay();
    }
}
