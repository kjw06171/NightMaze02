using UnityEngine;
using System; // Action 사용을 위해 추가

/// <summary>
/// 상호작용(E 키)을 통해 아이템을 획득하고, 대화를 시작하며,
/// 동시에 근처에 있을 때 상호작용 알림 메시지를 표시하는 스크립트입니다. (NotificationTrigger 기능 통합)
/// </summary>
[RequireComponent(typeof(Collider2D))] 
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 정보")]
    public string itemID = "KEY_A"; 
    
    [Header("대화 데이터 연결")]
    [SerializeField] 
    private DialogueSO dialogueData; 
    
    [Header("상호작용 알림 설정")]
    public bool useNotificationUI = true;
    public string interactionMessage = "E키를 눌러 획득";
    public KeyCode interactionKey = KeyCode.E;

    private bool playerInRange = false;
    private bool isInteractable = true;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[ItemPickup] 콜라이더가 Trigger가 아닙니다: {gameObject.name}");
        }

        // 이미 CANDLE을 먹은 상태면 제거
        if (itemID == "CANDLE" && GameState.HasCandle)
        {
            isInteractable = false;
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (playerInRange && isInteractable && Input.GetKeyDown(interactionKey))
        {
            bool isDialogueActive = (DialogueManager.Instance != null && DialogueManager.Instance.IsActive());
            if (!isDialogueActive)
                PickUp();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isInteractable)
        {
            playerInRange = true;

            if (useNotificationUI && FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.ShowNotification(interactionMessage, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (useNotificationUI && FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.HideNotification();
            }
        }
    }

    private void PickUp()
    {
        isInteractable = false;

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.HideNotification();
        }

        // 🔥 횃불 상태 업데이트
        if (itemID == "CANDLE")
        {
            GameState.HasCandle = true;
            Debug.Log("🔥 횃불 획득! GameState.HasCandle = true");
        }

        // 대화 매니저 체크
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager가 없음 → 대화 없이 퀘스트만 완료");
            OnDialogueEnd();
            return;
        }

        // 대화가 있을 때
        if (dialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, OnDialogueEnd);
        }
        else
        {
            Debug.LogWarning($"{itemID}는 DialogueSO 없음 → 즉시 완료");
            OnDialogueEnd();
        }
    }

    /// <summary>
    /// 대화가 종료된 뒤 호출되는 콜백
    /// </summary>
    private void OnDialogueEnd()
    {
        // 🔥🔥🔥 여기만 수정됨! NotifyItemCollected → CompleteQuest
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(itemID);
            Debug.Log($"퀘스트 완료: {itemID}");
        }

        Destroy(gameObject);
        Debug.Log($"아이템 파괴 완료: {itemID}");
    }
}
