using UnityEngine;

public class LightFuelItem : MonoBehaviour
{
    [Header("회복 설정")]
    public float RestoreDurationAmount = 15f; 
    
    [Header("UI 설정")]
    // 💡 에디터에서 여기에 FloatingTextPrefab을 드래그하여 연결해야 합니다.
    public GameObject floatingTextPrefab; 
    public string fullFuelMessage = "횃불 게이지가 가득 찼습니다!";
    
    // 💡 메시지가 붙을 Canvas 오브젝트를 명시적으로 연결합니다.
    [Header("캔버스 설정")]
    public Canvas targetCanvas; // 이 변수에 씬의 메인 UI Canvas를 연결하세요.

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryRestoreLight();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void TryRestoreLight()
    {
        // 플레이어 루트 트랜스폼 찾기
        Transform playerRoot = FindObjectOfType<PlayerHealth>()?.transform.root; 
        if (playerRoot == null)
        {
            Debug.LogWarning("🚨 PlayerHealth의 루트 오브젝트를 찾을 수 없습니다. 아이템 획득 실패.");
            return;
        }

        LightControl lightControl = playerRoot.GetComponentInChildren<LightControl>();

        if (lightControl != null)
        {
            // 연료가 이미 가득 찼는지 확인
            if (lightControl.IsFuelFull())
            {
                Debug.Log("✅ 게이지가 가득 찼습니다. 아이템 파괴를 건너뜁니다.");
                ShowFloatingMessage(this.transform.position, fullFuelMessage);
                return;
            }
            
            Debug.Log("❌ 게이지가 가득 차지 않았습니다. 아이템을 획득하고 파괴합니다.");

            // 연료 회복
            lightControl.RestoreLight(RestoreDurationAmount);
            
            // 아이템 파괴
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("🚨 LightControl 스크립트를 찾을 수 없습니다. 아이템 획득 및 파괴를 방지합니다.");
        }
    }

    /// <summary>
    /// 지정된 월드 위치에 메시지를 생성하여 표시합니다.
    /// </summary>
    private void ShowFloatingMessage(Vector3 position, string message)
    {
        if (floatingTextPrefab != null && targetCanvas != null && Camera.main != null)
        {
            // 1. 월드 좌표를 캔버스 내부의 로컬 좌표로 변환합니다.
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);
            Vector2 localPoint;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.GetComponent<RectTransform>(), 
                screenPoint,                               
                targetCanvas.worldCamera,                  
                out localPoint                             
            );

            // 2. 프리팹 생성 및 부모 설정
            GameObject messageInstance = Instantiate(floatingTextPrefab, targetCanvas.transform);
            
            // 3. RectTransform의 위치를 계산된 로컬 좌표로 설정합니다.
            RectTransform rectTransform = messageInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 💡 [수정됨] 텍스트 위치를 아래로 내리기 위해 음수 오프셋을 사용합니다.
                // -40f는 40픽셀만큼 아래로 이동시킵니다.
                float heightOffset = -40f; 
                localPoint.y += heightOffset; 
                
                rectTransform.localPosition = localPoint;
                
                // 메시지 크기 보정
                rectTransform.localScale = Vector3.one;
            }
            
            // 4. 메시지 설정 및 파괴 예약 (FloatingMessage.cs 내부에서 처리)
            FloatingMessage floatingScript = messageInstance.GetComponent<FloatingMessage>();
            if (floatingScript != null)
            {
                floatingScript.SetMessage(message);
            }
            else
            {
                 Debug.LogError("🚨 FloatingTextPrefab에 FloatingMessage.cs 스크립트가 없습니다!");
            }
        }
        else
        {
             Debug.LogError("🚨 UI 생성에 필요한 요소가 누락되었습니다. (프리팹/캔버스/메인카메라 연결 확인)");
        }
    }
}