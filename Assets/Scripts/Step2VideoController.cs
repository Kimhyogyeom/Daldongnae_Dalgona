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
    [Header("Video")]
    [SerializeField] private VideoPlayer _videoPlayer;    // 재생할 VideoPlayer

    [Header("On Video End")]
    [SerializeField] private GameObject _objectSuccess;   // 영상 재생이 끝난 후 활성화할 오브젝트

    [Header("On Enable 자동 재생 여부")]
    [SerializeField] private bool _autoPlayOnEnable = true;

    [Header("동시 재생 허용 최대 개수")]
    [SerializeField] private int _maxConcurrentPlayers = 4;

    // 현재 이 컨트롤러가 영상 재생 중인지 여부
    private bool _isPlaying = false;

    // 전체 씬에서 동시에 재생 중인 VideoPlayer 개수 (Step2VideoController 기준)
    private static int _currentPlayingCount = 0;

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

        // 재생 중이었다면, 전역 카운터 감소
        if (_isPlaying)
        {
            _isPlaying = false;
            _currentPlayingCount = Mathf.Max(0, _currentPlayingCount - 1);
        }
    }

    /// <summary>
    /// 초기 상태 세팅 후 영상 재생을 시작하는 공통 처리
    /// </summary>
    private void InitializeAndPlay()
    {
        print("111");
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

        // 동시 재생 개수 체크
        if (!_isPlaying)
        {
            if (_currentPlayingCount >= _maxConcurrentPlayers)
            {
                Debug.LogWarning($"[Step2VideoController] 동시에 재생 가능한 최대 개수({_maxConcurrentPlayers})를 초과하여 재생하지 않습니다.");
                return;
            }

            _currentPlayingCount++;
            _isPlaying = true;
        }

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
        if (_isPlaying)
        {
            _isPlaying = false;
            _currentPlayingCount = Mathf.Max(0, _currentPlayingCount - 1);
        }

        // GameManager에 비디오 종료 알림 (자동 전환 타이머 시작)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStep2VideoFinished();
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
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[Step2VideoController] GameObject가 비활성화 상태에서 ResetCall이 호출되었습니다. 재생하지 않습니다.");
            return;
        }

        InitializeAndPlay();
    }
}
