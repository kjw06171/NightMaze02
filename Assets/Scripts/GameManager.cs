using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject titleScreenPanel;  // 시작 화면 패널
    
    void Start()
    {
        // 게임 시작 시 시작 화면 표시
        ShowTitleScreen();
    }
    
    // 시작 화면 보이기
    void ShowTitleScreen()
    {
        titleScreenPanel.SetActive(true);
        Time.timeScale = 0f;  // 게임 일시정지
    }

    // 게임 시작 함수 (버튼에서 호출)
    public void StartGame()
    {
        titleScreenPanel.SetActive(false);  // 시작 화면 숨기기
        Time.timeScale = 1f;  // 게임 재개
    }
    
     public void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();  // 빌드된 게임 종료
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 종료
        #endif
    }
}