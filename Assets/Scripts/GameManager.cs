using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject titleScreenPanel;  // 시작 화면 패널

    public static bool IsTitleScreenActive = false; // 다른 시스템에서도 체크 가능

    void Start()
    {
        // 게임 시작 시 타이틀 화면 표시
        ShowTitleScreen();
    }

    // ---------------------------------------------------------
    // 🔥 타이틀 UI 표시 (게임 멈춤)
    // ---------------------------------------------------------
    void ShowTitleScreen()
    {
        if (titleScreenPanel != null)
            titleScreenPanel.SetActive(true);

        Time.timeScale = 0f;
        IsTitleScreenActive = true;
    }

    // ---------------------------------------------------------
    // 🔥 게임 시작 버튼에서 호출 → storyUI / pauseMenu와 충돌 없게 안전 처리
    // ---------------------------------------------------------
    public void StartGame()
    {
        if (titleScreenPanel != null)
            titleScreenPanel.SetActive(false);

        // 게임을 재개하되, Story UI나 Dialogue가 있다면 TimeScale은 그쪽에서 제어함
        if (!StoryUIFader.IsStoryPlaying &&
            !(DialogueManager.Instance != null && DialogueManager.Instance.IsActive()))
        {
            Time.timeScale = 1f;
        }

        IsTitleScreenActive = false;
    }

    // ---------------------------------------------------------
    // 🔥 게임 종료
    // ---------------------------------------------------------
    public void QuitGame()
    {
        Debug.Log("게임 종료 요청됨");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
