using UnityEngine;

public class ExitDoorController : MonoBehaviour
{
    // 💡 문이 열렸을 때 시각적으로 표시할 메시지
    private string lockedMessage = "E를 눌러 상호작용 (모든 열쇠 필요)";
    private string unlockedMessage = "E를 눌러 탈출!";

    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;
    
    // 💡 문 오브젝트의 SpriteRenderer와 Collider2D를 참조합니다.
    private SpriteRenderer doorRenderer;
    private Collider2D doorCollider;

    void Awake()
    {
        // 스크립트가 붙은 오브젝트에서 SpriteRenderer와 Collider2D를 가져옵니다.
        doorRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null || doorRenderer == null)
        {
            // Debug.LogWarning 대신 Debug.LogError를 사용하여 문제를 강조합니다.
            Debug.LogError("🚨 ExitDoorController: SpriteRenderer 또는 Collider2D를 찾을 수 없습니다. 문 오브젝트에 컴포넌트가 있는지 확인하세요. 이 스크립트는 이 컴포넌트들이 필요합니다.");
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryExit();
        }
    }
    
    // 💡 퀘스트 완료 여부에 따라 문을 열거나 메시지를 표시합니다.
    private void TryExit()
    {
        if (isDoorOpen) return;
        
        // UI를 사용하는 방식이므로, FloatingNotificationUI가 없다면 안전하게 종료합니다.
        if (FloatingNotificationUI.Instance == null)
        {
             Debug.Log("🚨 FloatingNotificationUI가 씬에 없습니다. 문 상호작용 UI를 표시할 수 없습니다.");
             return;
        }

        // 퀘스트 관리자의 완료 상태를 확인합니다.
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted)
        {
            // 퀘스트 완료: 문을 엽니다.
            OpenDoor();
            
            // 💡 [예시] 여기에서 다음 씬으로 이동하는 코드를 추가합니다.
            // SceneManager.LoadScene("NextSceneName"); 
        }
        else
        {
            // 퀘스트 미완료: 사용자에게 알립니다.
            Debug.Log($"[ExitDoor - TryExit] 🔐 아직 모든 열쇠를 모으지 못했습니다.");
            // 💡 [수정] FloatingNotificationUI를 사용하여 잠긴 메시지를 다시 표시합니다. (사용자에게 피드백)
            // 잠긴 메시지는 한번 누르고 나면 사라지도록 (기본값 true)로 둡니다.
            FloatingNotificationUI.Instance.ShowNotification($"잠김: {lockedMessage}");
        }
    }
    
    private void OpenDoor()
    {
        isDoorOpen = true;
        
        // 💡 문이 열리면 시각적 요소와 충돌체를 비활성화하여 문이 사라진 것처럼 보이게 합니다.
        if (doorRenderer != null) doorRenderer.enabled = false;
        if (doorCollider != null) doorCollider.enabled = false;
        
        // 💡 [수정] 문이 열리면 상호작용 UI는 숨깁니다.
        if (FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.HideNotification();
        }
        
        Debug.Log("🎉 문이 열렸습니다! 탈출 성공!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDoorOpen)
        {
            isPlayerNearby = true;
            
            string messageToShow;
            
            // 퀘스트 완료 여부를 확인하여 적절한 메시지를 설정
            if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted)
            {
                messageToShow = unlockedMessage;
            }
            else
            {
                messageToShow = lockedMessage;
            }
            
            // 💡 [핵심] FloatingNotificationUI를 사용하여 화면에 고정된 상호작용 문구를 표시합니다.
            // 💡 두 번째 인수로 'false'를 전달하여 자동 숨김을 비활성화합니다.
            if (FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.ShowNotification(messageToShow, false);
            }
            Debug.Log($"[ExitDoor - Enter] 상호작용 문구 표시: {messageToShow}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDoorOpen)
        {
            isPlayerNearby = false;
            
            // 💡 [수정] 플레이어가 벗어날 때 FloatingNotificationUI를 수동으로 숨깁니다.
            if (FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.HideNotification();
            }
            Debug.Log("[ExitDoor - Exit] 근처에서 벗어남. 상호작용 종료.");
        }
    }
}