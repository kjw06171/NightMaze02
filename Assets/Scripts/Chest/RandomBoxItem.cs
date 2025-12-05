using UnityEngine;
using UnityEngine.UI; // UI 관련 함수 (RectTransformUtility) 사용을 위해 필수적으로 추가합니다.
using System.Collections.Generic; // List<T> 사용을 위해 추가

/// <summary>
/// E 키 상호작용으로 무작위 효과를 플레이어에게 적용하는 아이템입니다.
/// (체력 회복/피해, 빛 증가/감소)
/// </summary>
public class RandomBoxItem : MonoBehaviour
{
    // 효과의 종류를 정의합니다.
    private enum EffectType { Health, Light }

    // 무작위 효과를 정의하기 위한 구조체입니다.
    private struct RandomEffect
    {
        public EffectType type; // 효과의 종류 (체력 또는 빛)
        public string message; // UI에 표시될 메시지
        public float value;    // 양수: 회복/증가, 음수: 피해/감소 (Light는 비율로 사용)
        public Color color;    // 메시지의 색상
    }

    [Header("UI 설정")]
    public GameObject floatingTextPrefab; // 에디터에서 FloatingTextPrefab을 연결
    [Header("캔버스 설정")]
    public Canvas targetCanvas; // 씬의 메인 UI Canvas를 연결

    private bool playerInRange = false;
    private List<RandomEffect> possibleEffects;

    void Awake()
    {
        // 💡 8가지 효과 목록 초기화 (각각 1/8 확률)
        possibleEffects = new List<RandomEffect>
        {
            // 1. 체력 1 회복
            new RandomEffect { type = EffectType.Health, message = "+1 HP 회복", value = 1f, color = Color.green },
            // 2. 체력 1 피해
            new RandomEffect { type = EffectType.Health, message = "-1 HP 피해", value = -1f, color = Color.red },
            
            // 3. 빛 15% 감소
            new RandomEffect { type = EffectType.Light, message = "빛 15% 감소", value = -0.15f, color = new Color(0.8f, 0.5f, 0f) }, // 주황색
            // 4. 빛 50% 감소
            new RandomEffect { type = EffectType.Light, message = "빛 50% 감소!", value = -0.50f, color = Color.red },
            // 5. 빛 100% 감소 (전부 소멸)
            new RandomEffect { type = EffectType.Light, message = "빛 모두 소멸!", value = -1.00f, color = Color.magenta },
            
            // 6. 빛 15% 증가
            new RandomEffect { type = EffectType.Light, message = "빛 15% 증가", value = 0.15f, color = Color.yellow },
            // 7. 빛 30% 증가
            new RandomEffect { type = EffectType.Light, message = "빛 30% 증가!", value = 0.30f, color = Color.yellow },
            // 8. 빛 100% 증가 (완충)
            new RandomEffect { type = EffectType.Light, message = "빛 완충!", value = 1.00f, color = Color.cyan }
        };
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // 상자 열기 함수 호출
            OpenRandomBox();
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

    /// <summary>
    /// 무작위 효과를 적용하고 상자를 파괴하는 로직입니다.
    /// </summary>
    private void OpenRandomBox()
    {
        // 1. 플레이어 루트 트랜스폼 찾기
        Transform playerRoot = FindObjectOfType<PlayerHealth>()?.transform.root; 
        if (playerRoot == null)
        {
            Debug.LogWarning("🚨 PlayerHealth의 루트 오브젝트를 찾을 수 없습니다. 상자 열기 실패.");
            ShowFloatingMessage(this.transform.position, "플레이어를 찾을 수 없습니다!", Color.red);
            return;
        }

        // 2. PlayerHealth 및 LightControl 스크립트 찾기
        PlayerHealth healthControl = playerRoot.GetComponentInChildren<PlayerHealth>();
        LightControl lightControl = playerRoot.GetComponentInChildren<LightControl>();

        if (healthControl == null && lightControl == null)
        {
            Debug.LogError("🚨 PlayerHealth 및 LightControl 스크립트를 찾을 수 없습니다. 상자 열기 실패.");
            ShowFloatingMessage(this.transform.position, "플레이어 컴포넌트 오류!", Color.red);
            return;
        }

        // 3. 무작위 효과 선택 (0부터 possibleEffects.Count - 1까지)
        RandomEffect selectedEffect = possibleEffects[Random.Range(0, possibleEffects.Count)];
        
        Debug.Log($"📦 랜덤 상자 오픈! 효과: {selectedEffect.message} (값: {selectedEffect.value})");

        // 4. 효과 적용 (Heal/Damage 또는 Light Restore/Drain)
        switch (selectedEffect.type)
        {
            case EffectType.Health:
                if (healthControl != null && selectedEffect.value != 0f)
                {
                    // PlayerHealth.Heal() 호출 (양수: 회복, 음수: 피해)
                    healthControl.Heal((int)selectedEffect.value);
                }
                break;
                
            case EffectType.Light:
                if (lightControl != null && selectedEffect.value != 0f)
                {
                    // LightControl.RestoreLight() 함수를 호출하여 빛을 증가/감소시킵니다.
                    lightControl.RestoreLight(selectedEffect.value); 
                }
                break;
        }
        
        // 5. UI 메시지 표시
        ShowFloatingMessage(this.transform.position, selectedEffect.message, selectedEffect.color);
        
        // 6. 상자 파괴
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 지정된 월드 위치에 메시지를 생성하여 표시합니다.
    /// </summary>
    private void ShowFloatingMessage(Vector3 position, string message, Color color)
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
                floatingScript.SetColor(color);  // 색상 적용
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
