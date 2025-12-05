using UnityEngine;
using UnityEngine.SceneManagement;

public class MansionDoorController : MonoBehaviour
{
    [Header("씬 이동 설정")]
    public string nextSceneName = "NextScene";
    public float loadDelay = 0.3f;

    [Header("퀘스트 설정")]
    public string doorQuestID = "MANSION_KEY";
    public string prerequisiteID = "CANDLE_TOGGLE";

    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange) return;

        // 🔥 QuestManager null-safe 체크
        if (QuestManager.Instance == null)
        {
            Debug.LogError("🚨 QuestManager.Instance가 null입니다! 씬에 QuestManager가 존재하는지 확인하세요.");
            return;
        }

        bool prereq = QuestManager.Instance.IsQuestDone(prerequisiteID);

        // 🔥 FloatingNotificationUI null-safe 체크
        var ui = FloatingNotificationUI.Instance;

        if (!prereq)
        {
            if (ui != null)
                ui.ShowNotification("[잠김] 선행 퀘스트를 완수하세요.", false);

            return;
        }

        if (ui != null)
            ui.ShowNotification("E 키를 눌러 문 열기", false);

        if (Input.GetKeyDown(KeyCode.E))
        {
            QuestManager.Instance.CompleteQuest(doorQuestID);

            if (ui != null)
                ui.HideNotification();

            Invoke(nameof(LoadScene), loadDelay);
        }
    }

    void LoadScene()
    {
        // 🔥 FadeManager null-safe 체크
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠ FadeManager.Instance가 null입니다. 즉시 씬 이동합니다.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // 🔥 FloatingNotificationUI null-safe 체크
            if (FloatingNotificationUI.Instance != null)
                FloatingNotificationUI.Instance.HideNotification();
        }
    }
}
