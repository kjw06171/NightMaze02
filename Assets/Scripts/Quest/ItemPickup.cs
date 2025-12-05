using UnityEngine;
using System; // Action 사용을 위해 추가
using UnityEngine.SceneManagement;


/// <summary>
/// 상호작용(E 키)으로 아이템 획득 + 대화 실행 + 스토리 UI 표시 + 선행퀘스트 체크
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 정보")]
    public string itemID = "KEY_A";

    [Header("대화 데이터 연결")]
    [SerializeField]
    private DialogueSO dialogueData;

    [Header("스토리 UI (선택)")]
    public GameObject storyUIPanel;          // ← Story UI 패널 (없어도 OK)

    [Header("상호작용 알림 설정")]
    public bool useNotificationUI = true;
    public string interactionMessage = "E키를 눌러 획득";

    [Header("선행 퀘스트 설정")]
    public string requiredQuestID = "";       // 빈 값이면 선행퀘 없음
    public string lockedMessage = "[잠김] 선행 퀘스트를 완료하세요";

    [Header("입력키")]
    public KeyCode interactionKey = KeyCode.E;

    private bool playerInRange = false;
    private bool isInteractable = true;



    // ==========================================================
    // 초기 설정
    // ==========================================================
    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            Debug.LogWarning($"[ItemPickup] 콜라이더가 Trigger가 아닙니다: {gameObject.name}");

        // 이미 CANDLE 먹었으면 제거
        if (itemID == "CANDLE" && GameState.HasCandle)
        {
            isInteractable = false;
            Destroy(gameObject);
            return;
        }
    }



    // ==========================================================
    // Update – 아이템 획득 처리
    // ==========================================================
    void Update()
    {
        if (!playerInRange || !isInteractable)
            return;

        // 선행 퀘스트 미완료 → 획득 불가
        if (!IsPrerequisiteCleared())
            return;

        if (Input.GetKeyDown(interactionKey))
        {
            bool isDialogueActive =
                (DialogueManager.Instance != null && DialogueManager.Instance.IsActive());

            if (!isDialogueActive)
                PickUp();
        }
    }



    // ==========================================================
    // 선행 퀘스트 완료 여부 체크
    // ==========================================================
    private bool IsPrerequisiteCleared()
    {
        if (string.IsNullOrEmpty(requiredQuestID))
            return true;

        if (QuestManager.Instance == null)
            return true;

        return QuestManager.Instance.IsQuestDone(requiredQuestID);
    }



    // ==========================================================
    // 플레이어 트리거 진입
    // ==========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !isInteractable)
            return;

        playerInRange = true;

        if (!useNotificationUI || FloatingNotificationUI.Instance == null)
            return;

        // 🔒 잠김 상태 UI 표시
        if (!IsPrerequisiteCleared())
            FloatingNotificationUI.Instance.ShowNotification(lockedMessage, false);
        else
            FloatingNotificationUI.Instance.ShowNotification(interactionMessage, false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }



    // ==========================================================
    // PickUp – 아이템 획득
    // ==========================================================
    private void PickUp()
    {
        isInteractable = false;

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();

        // 🔥 CANDLE 상태 업데이트
        if (itemID == "CANDLE")
            GameState.HasCandle = true;

        // ======================================================
        // 🔥 스토리 UI 플레이 → 끝난 뒤 대화 시작하기
        // ======================================================
        if (storyUIPanel != null)
        {
            StoryUIFader fader = storyUIPanel.GetComponent<StoryUIFader>();
            if (fader != null)
            {
                // 스토리 UI 끝난 후 대화 시작
                fader.Play(() =>
                {
                    StartItemDialogue();
                });
                return; // 대화는 콜백에서 실행되므로 여기서 종료
            }
            else
            {
                storyUIPanel.SetActive(true);
            }
        }

        // 스토리 UI가 없으면 즉시 대화 실행
        StartItemDialogue();
    }

    private void StartItemDialogue()
    {
        // DialogueManager가 없으면 즉시 종료 처리
        if (DialogueManager.Instance == null)
        {
            OnDialogueEnd();
            return;
        }

        // 대화 실행
        if (dialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, OnDialogueEnd);
        }
        else
        {
            OnDialogueEnd();
        }
    }



    // ==========================================================
    // 대화 종료 후 콜백
    // ==========================================================
    private void OnDialogueEnd()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(itemID);

        Destroy(gameObject);
    }
}
