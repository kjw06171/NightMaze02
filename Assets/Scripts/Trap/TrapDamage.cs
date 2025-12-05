using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapDamage : MonoBehaviour
{
    [Header("데미지 설정")]
    public int damageAmount = 1;
    public float damageCooldown = 1f;

    [Header("튜토리얼 대화 연결")]
    public DialogueSO trapTutorialDialogue; // Inspector에서 드래그

    [Header("튜토리얼 퀘스트 ID")]
    public string trapQuestID = "TRAP_TUTORIAL";

    private float lastDamageTime = -999f;

    private bool tutorialTriggered = false; 
    // 퀘스트 + 대화 1회 제한

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet")) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        // 쿨타임
        if (Time.time - lastDamageTime < damageCooldown) return;

        // 데미지     
        playerHealth.TakeDamage(damageAmount);
        lastDamageTime = Time.time;

        Debug.Log("함정 데미지 적용됨 (입장 시)");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet")) return;

        // 이미 실행했다면 무시
        if (tutorialTriggered) return;

        // 🎯 Lv_00_2에서만 튜토리얼 대화 실행
        if (SceneManager.GetActiveScene().name != "Lv_00_2") return;

        tutorialTriggered = true;

        // 1) 퀘스트 완료
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(trapQuestID);
            Debug.Log("🎉 TRAP_TUTORIAL 퀘스트 완료!");
        }

        // 2) 대화 실행
        if (DialogueManager.Instance != null && trapTutorialDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(trapTutorialDialogue);
            Debug.Log("💬 함정 튜토리얼 대화 시작 (트랩 벗어났을 때)");
        }
        else
        {
            Debug.LogWarning("⚠ trapTutorialDialogue 또는 DialogueManager가 설정되지 않음");
        }
    }
}
