using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LightControl : MonoBehaviour
{
    private const string STARTING_SCENE_NAME = "Lv_00_1";
    private const string SECOND_SCENE_NAME = "Lv_00_2";   // 🔥 추가
    private const string CANDLE_TUTORIAL_ID = "CANDLE_TOGGLE";

    private Light2D playerLight;

    [Header("빛 반경 설정")]
    public float startRadius = 6.6f;
    public float endRadius = 1.5f;
    public float duration = 60f;

    [Header("빛 세기 설정")]
    public float startIntensity = 1f;
    public float endIntensity = 0.5f;

    [Header("튜토리얼 대화 연결")]
    [SerializeField] private DialogueSO toggleTutorialDialogue;

    [Header("UI 연결")]
    [SerializeField] private GameObject lightGaugeUI;

    private float timer = 0f;
    private bool isLightOn = false;
    private bool isLightDepleted = false;

    private bool hasToggleQuestCompleted = false;

    public bool IsLightOn => isLightOn;
    public float LightRatio => 1f - Mathf.Clamp01(timer / duration);


    void Start()
    {
        playerLight = GetComponent<Light2D>();
        playerLight.enabled = false;

        string scene = SceneManager.GetActiveScene().name;
        bool isTutorialScene = scene == STARTING_SCENE_NAME;

        // ----------------------------------------------------------------------
        // 🔥 Lv_00_1 ↔ Lv_00_2 사이에서는 SharedLightTimer 값을 불러옴
        // ----------------------------------------------------------------------
        if (scene == STARTING_SCENE_NAME || scene == SECOND_SCENE_NAME)
        {
            timer = Mathf.Clamp(GameState.SharedLightTimer, 0f, duration);

            float t = Mathf.Clamp01(timer / duration);
            playerLight.pointLightOuterRadius = Mathf.Lerp(startRadius, endRadius, t);
            playerLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
        }
        else
        {
            timer = 0f; // 다른 씬에서는 초기화
        }

        // 튜토리얼에서는 UI 숨기기
        if (isTutorialScene && lightGaugeUI != null)
            lightGaugeUI.SetActive(false);

        // 일반 씬은 토글 제한 없음
        if (!isTutorialScene)
        {
            hasToggleQuestCompleted = true;

            if (lightGaugeUI != null)
                lightGaugeUI.SetActive(true);
        }
    }


    void Update()
    {
        string scene = SceneManager.GetActiveScene().name;
        bool isTutorialScene = scene == STARTING_SCENE_NAME;

        // 🔒 촛불 얻기 전에는 토글 금지
        if (isTutorialScene && !GameState.HasCandle)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
                Debug.Log("촛불을 먼저 획득하세요.");
            return;
        }

        HandleToggle(isTutorialScene);
        HandleConsumption();
    }


    // ---------------------------------------------------------
    // 🔥 횃불 토글 처리
    // ---------------------------------------------------------
    private void HandleToggle(bool isTutorialScene)
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isLightDepleted)
        {
            bool wasLightOn = isLightOn;
            isLightOn = !isLightOn;
            playerLight.enabled = isLightOn;

            if (isTutorialScene && isLightOn && !wasLightOn && !hasToggleQuestCompleted)
            {
                if (lightGaugeUI != null)
                    lightGaugeUI.SetActive(true);

                if (QuestManager.Instance != null)
                    QuestManager.Instance.CompleteQuest(CANDLE_TUTORIAL_ID);

                hasToggleQuestCompleted = true;

                if (DialogueManager.Instance != null && toggleTutorialDialogue != null)
                    DialogueManager.Instance.StartDialogue(toggleTutorialDialogue);
            }
        }
    }


    // ---------------------------------------------------------
    // 🔥 빛 소모 처리 + 공유 저장
    // ---------------------------------------------------------
    private void HandleConsumption()
    {
        if (!isLightOn || isLightDepleted) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        playerLight.pointLightOuterRadius = Mathf.Lerp(startRadius, endRadius, t);
        playerLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);

        // 🔥 값이 변할 때마다 SharedLightTimer 저장
        SaveSharedLight();

        if (t >= 1f)
        {
            isLightOn = false;
            isLightDepleted = true;
            playerLight.enabled = false;
        }
    }


    // ---------------------------------------------------------
    // 🔥 회복 아이템 처리 + 공유 저장
    // ---------------------------------------------------------
    public void RestoreLight(float percentageChange)
    {
        isLightDepleted = false;

        float adj = -percentageChange * duration;
        timer = Mathf.Clamp(timer + adj, 0f, duration);

        float t = Mathf.Clamp01(timer / duration);
        playerLight.pointLightOuterRadius = Mathf.Lerp(startRadius, endRadius, t);
        playerLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);

        // 🔥 회복 후 저장
        SaveSharedLight();
    }


    public bool IsFuelFull()
    {
        return timer <= 0.001f;
    }

    // ---------------------------------------------------------
    // 🔥 공유 저장 함수
    // ---------------------------------------------------------
    private void SaveSharedLight()
    {
        string scene = SceneManager.GetActiveScene().name;

        // 두 씬에서만 공유 저장
        if (scene == STARTING_SCENE_NAME || scene == SECOND_SCENE_NAME)
            GameState.SharedLightTimer = timer;
    }
}
