using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RandomBoxItem : MonoBehaviour
{
    private enum EffectType { Health, Light }

    private struct RandomEffect
    {
        public EffectType type;
        public string message;
        public float value;
        public Color color;
    }

    [Header("UI 설정")]
    public GameObject floatingTextPrefab;

    [Header("캔버스 설정")]
    public Canvas targetCanvas;

    [Header("아이템 설명 대사 (Lv_00_2 전용)")]
    public DialogueSO itemDialogue;   // ← Inspector에서 연결

    [Header("상호작용 메시지 옵션")]
    public bool showInteractMessage = true;


    private bool playerInRange = false;
    private List<RandomEffect> possibleEffects;

    void Awake()
    {
        possibleEffects = new List<RandomEffect>
        {
            new RandomEffect { type = EffectType.Health, message = "+1 HP 회복", value = 1f, color = Color.green },
            new RandomEffect { type = EffectType.Health, message = "-1 HP 피해", value = -1f, color = Color.red },

            new RandomEffect { type = EffectType.Light, message = "빛 15% 감소", value = -0.15f, color = new Color(0.8f, 0.5f, 0f) },
            new RandomEffect { type = EffectType.Light, message = "빛 50% 감소!", value = -0.50f, color = Color.red },
            new RandomEffect { type = EffectType.Light, message = "빛 모두 소멸!", value = -1.00f, color = Color.magenta },

            new RandomEffect { type = EffectType.Light, message = "빛 15% 증가", value = 0.15f, color = Color.yellow },
            new RandomEffect { type = EffectType.Light, message = "빛 30% 증가!", value = 0.30f, color = Color.yellow },
            new RandomEffect { type = EffectType.Light, message = "빛 완충!", value = 1.00f, color = Color.cyan }
        };
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OpenRandomBox();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (showInteractMessage && FloatingNotificationUI.Instance != null)
                FloatingNotificationUI.Instance.ShowNotification("E키를 눌러 획득", false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (showInteractMessage && FloatingNotificationUI.Instance != null)
                FloatingNotificationUI.Instance.HideNotification();
        }
    }


    private void OpenRandomBox()
    {
        Transform playerRoot = FindObjectOfType<PlayerHealth>()?.transform.root;
        if (playerRoot == null)
        {
            ShowFloatingMessage(transform.position, "플레이어 없음!", Color.red);
            return;
        }

        PlayerHealth healthControl = playerRoot.GetComponentInChildren<PlayerHealth>();
        LightControl lightControl = playerRoot.GetComponentInChildren<LightControl>();

        // 🔥 무작위 효과 선택 (Lv_00_2일 때는 빛 감소 효과만)
        RandomEffect selectedEffect;

        if (SceneManager.GetActiveScene().name == "Lv_00_2")
        {
            var lightNegativeEffects = possibleEffects.FindAll(e =>
                e.type == EffectType.Light && e.value < 0f
            );

            selectedEffect = lightNegativeEffects[Random.Range(0, lightNegativeEffects.Count)];
        }
        else
        {
            selectedEffect = possibleEffects[Random.Range(0, possibleEffects.Count)];
        }

        Debug.Log($"📦 랜덤 상자 → {selectedEffect.message}");

        // 🔥 효과 적용
        if (selectedEffect.type == EffectType.Health && healthControl != null)
        {
            healthControl.Heal((int)selectedEffect.value);
        }
        else if (selectedEffect.type == EffectType.Light && lightControl != null)
        {
            lightControl.RestoreLight(selectedEffect.value);
        }

        // 🔥 메시지 표시
        ShowFloatingMessage(transform.position, selectedEffect.message, selectedEffect.color);

        // 🔥 진행형 퀘스트 증가
        QuestManager.Instance.AddProgress("COLLECT_ITEMS", 1);

        // 🔥 아이템 설명 대사 (Lv_00_2 전용)
        if (SceneManager.GetActiveScene().name == "Lv_00_2")
        {
            if (itemDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(itemDialogue);
            }
        }

        // 🔥 상자 소멸
        Destroy(gameObject);
    }


    // ---------------------------------------------------------
    // 🔥 UI 메시지 표시 함수
    // ---------------------------------------------------------
    private void ShowFloatingMessage(Vector3 position, string message, Color color)
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (floatingTextPrefab == null || targetCanvas == null || Camera.main == null)
        {
            Debug.LogError("🚨 FloatingText 생성 실패: 프리팹 / Canvas / Camera 연결 필요!");
            return;
        }

        Camera cam = Camera.main;
        Vector2 screenPoint = cam.WorldToScreenPoint(position);
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPoint,
            targetCanvas.worldCamera,
            out localPoint
        );

        GameObject messageObj = Instantiate(floatingTextPrefab, targetCanvas.transform);
        RectTransform rect = messageObj.GetComponent<RectTransform>();

        if (rect != null)
        {
            localPoint.y -= 40f;
            rect.localPosition = localPoint;
            rect.localScale = Vector3.one;
        }

        FloatingMessage fm = messageObj.GetComponent<FloatingMessage>();
        if (fm != null)
        {
            fm.SetMessage(message);
            fm.SetColor(color);
        }
    }
}
