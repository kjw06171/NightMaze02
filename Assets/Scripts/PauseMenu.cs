using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요
// using System.IO; // 저장 기능을 위해 필요 (예시 코드는 생략)

/// <summary>
/// 게임 일시정지 메뉴를 관리하고 ESC 키 입력을 처리하는 스크립트입니다.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // 유니티 인스펙터에서 연결할 일시정지 UI 패널
    public GameObject pauseMenuUI; 

    // 현재 게임이 일시정지 상태인지 확인하는 변수
    public static bool isGamePaused = false; 

    void Start()
    {
        // 💡 [수정] DialogueManager나 다른 스크립트의 Time.timeScale 설정을 방해하지 않도록
        // 💡 Time.timeScale = 1f; 코드를 제거합니다. (Awake/Start 순서 문제 방지)
        isGamePaused = false;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // (PlayerHealth 연결 로직은 생략)
    }

    void Update()
    {
        // 1) 스토리 UI 재생 중이면 ESC 완전 차단
        if (StoryUIFader.IsStoryPlaying)
            return;

        // 2) 대화창 활성 상태면 ESC 완전 차단
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive())
            return;

        // 3) ESC 입력 처리 (오직 여기에서 단 한 번만)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
                Resume();
            else
                Pause();
        }
    }


    /// <summary>
    /// 게임을 재개하고 UI를 숨깁니다.
    /// </summary>
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // UI 비활성화 (숨기기)
        Time.timeScale = 1f;          // 시간 흐름을 정상(1배속)으로 복구
        isGamePaused = false;
        Debug.Log("게임 재개");
    }

    /// <summary>
    /// 게임을 일시정지하고 UI를 보여줍니다.
    /// </summary>
    void Pause()
    {
        pauseMenuUI.SetActive(true);  // UI 활성화 (보이기)
        Time.timeScale = 0f;          // 시간 흐름을 멈춤 (0배속)
        isGamePaused = true;
        Debug.Log("게임 일시정지");
    }

    // ----------------------------------------------------
    // 버튼 이벤트에 연결할 공개 함수들
    // ----------------------------------------------------

    /// <summary>
    /// 저장 버튼 클릭 시 호출됩니다. (실제 저장 로직 필요)
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("게임 저장 기능을 실행합니다.");
        // **여기에 실제 저장 로직(예: PlayerPrefs 또는 파일 I/O)을 구현해야 합니다.**
        // 저장 후 자동으로 재개하려면 Resume() 호출
        // Resume(); 
    }

    /// <summary>
    /// 종료 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        
        // 💡 주의: 에디터에서는 작동하지 않고 빌드된 게임에서만 작동합니다.
        Application.Quit(); 
        
        // 💡 에디터 테스트용 (게임 멈춤)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 빌드 환경을 위해 제거
        #endif
    }
}