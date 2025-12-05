using UnityEngine;
using System.Collections;
using System; // Action 콜백을 위해 필요

/// <summary>
/// 게임이나 씬이 시작될 때 지정된 DialogueSO를 사용하여 자동으로 대화창을 시작합니다.
/// 대화가 활성화된 동안 DialogueManager에서 게임이 일시정지(Time.timeScale=0f)됩니다.
/// </summary>
public class GameStartDialogue : MonoBehaviour
{
    [Header("시작 대화 데이터")]
    [Tooltip("게임 시작 시 자동으로 표시할 DialogueSO 파일을 연결하세요.")]
    public DialogueSO initialDialogue;

    [Header("UI 관리 설정")]
    [Tooltip("대화 시작 시 숨길 Game UI Canvas 오브젝트를 연결하세요.")]
    public GameObject gameUICanvas;

    // 💡 Awake()에서 코루틴을 시작하여 DialogueManager가 준비될 때까지 기다립니다.
    void Awake()
    {
        // DialogueManager가 준비될 때까지 기다리고 안전하게 대화를 시작합니다.
        StartCoroutine(StartDialogueWhenReady());
    }

    /// <summary>
    /// DialogueManager가 준비될 때까지 대기하고, 준비가 되면 대화를 시작하는 코루틴입니다.
    /// </summary>
    IEnumerator StartDialogueWhenReady()
    {
        // 1. DialogueManager 인스턴스가 준비될 때까지 기다립니다 (최대 10프레임까지)
        int waitFrames = 0;
        while (DialogueManager.Instance == null && waitFrames < 10)
        {
            waitFrames++;
            yield return null; // 다음 프레임까지 기다립니다.
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("🚨 GameStartDialogue: DialogueManager를 찾을 수 없거나 초기화되지 않았습니다. 초기 대화를 표시할 수 없습니다.");
            yield break;
        }

        // 2. DialogueSO 데이터가 연결되었는지 확인합니다.
        if (initialDialogue == null)
        {
            Debug.LogWarning("⚠️ GameStartDialogue: 시작 대화(initialDialogue)가 연결되지 않았습니다. 대화 없이 게임이 시작됩니다.");
            yield break;
        }

        // 3. 게임 UI 숨기기
        if (gameUICanvas != null)
        {
            gameUICanvas.SetActive(false);
            Debug.Log("게임 UI Canvas 비활성화.");
        }
        else
        {
            Debug.LogWarning("⚠️ GameStartDialogue: 게임 UI Canvas 오브젝트가 연결되지 않았습니다. 수동으로 연결해 주세요.");
        }

        // 4. 대화 시작 (DialogueManager가 내부적으로 Time.timeScale=0f 설정)
        // 💡 대화 시작 전에 강제로 게임 일시정지
        Time.timeScale = 0f; // 게임을 일시 정지시킵니다.
        DialogueManager.Instance.StartDialogue(initialDialogue, OnDialogueEnd); // 대화 시작
        
        Debug.Log("✅ 초기 게임 대화가 성공적으로 시작되었습니다. 게임이 일시정지되었습니다.");
    }
    
    /// <summary>
    /// DialogueManager에서 대화가 종료되었을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnDialogueEnd()
    {
        // 1. 숨겼던 게임 UI를 다시 활성화합니다.
        if (gameUICanvas != null)
        {
            gameUICanvas.SetActive(true);
            Debug.Log("게임 UI Canvas 다시 활성화.");
        }

        // 2. 이 객체는 이제 역할을 마쳤으므로 삭제합니다.
        Destroy(gameObject); 
        Debug.Log("초기 게임 대화가 완료되었습니다. 게임이 재개되었습니다.");
        
        // 💡 대화가 끝나면 게임 시간을 다시 진행시킵니다.
        Time.timeScale = 1f; // 게임 시간 재개
    }
}
