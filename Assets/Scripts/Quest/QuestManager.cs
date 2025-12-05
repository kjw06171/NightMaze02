using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 퀘스트 데이터 관리 + UI만 담당하는 순수 매니저
/// (게임 진행 제어 로직 제거됨)
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI questText;

    [Header("퀘스트 표시 방식")]
    public QuestDisplayMode displayMode = QuestDisplayMode.AllAtOnce;

    [Header("퀘스트 목록")]
    public List<QuestItemData> initialQuestItems = new List<QuestItemData>();

    private Dictionary<string, bool> keyQuests = new Dictionary<string, bool>();

    private int requiredKeyCount = 0;
    private bool isQuestCompleted = false;
    public bool IsQuestCompleted => isQuestCompleted;

    private const string MOVE_TUTORIAL_ID = "TUTORIAL_MOVE";
    private const string CANDLE_PICKUP_ID = "CANDLE";
    private const string CANDLE_TOGGLE_ID = "CANDLE_TOGGLE";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeQuests();
        UpdateQuestUI();   // 🔥🔥🔥 이게 없어서 처음에 아무것도 안 뜬 거임
    }


    private void InitializeQuests()
    {
        keyQuests.Clear();
        requiredKeyCount = 0;

        foreach (var item in initialQuestItems)
        {
            keyQuests[item.questID] = false;

            if (item.questID != MOVE_TUTORIAL_ID &&
                item.questID != CANDLE_PICKUP_ID &&
                item.questID != CANDLE_TOGGLE_ID)
            {
                requiredKeyCount++;
            }
        }
    }

    // 🔥 외부에서 퀘스트 완료 여부 확인
    public bool IsQuestDone(string questID)
    {
        return keyQuests.ContainsKey(questID) && keyQuests[questID];
    }

    // 🔥 외부에서 퀘스트 완료 요청
    public void CompleteQuest(string questID)
    {
        if (!keyQuests.ContainsKey(questID))
        {
            Debug.LogError($"[QuestManager] 존재하지 않는 퀘스트 ID: {questID}");
            return;
        }

        if (!keyQuests[questID])
        {
            keyQuests[questID] = true;
            UpdateQuestUI();
            CheckMainQuestProgress();
        }
    }

    private void CheckMainQuestProgress()
    {
        int count = 0;

        foreach (var item in initialQuestItems)
        {
            if (item.questID == MOVE_TUTORIAL_ID ||
                item.questID == CANDLE_PICKUP_ID ||
                item.questID == CANDLE_TOGGLE_ID)
                continue;

            if (keyQuests[item.questID])
                count++;
        }

        isQuestCompleted = (count == requiredKeyCount);
    }

    private void UpdateQuestUI()
    {
        if (questText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("🔑 메인 퀘스트");

        int index = 1;

        // ---------------------
        // 🔥 Sequential 모드 처리
        // ---------------------
        if (displayMode == QuestDisplayMode.Sequential)
        {
            foreach (var item in initialQuestItems)
            {
                bool done = keyQuests.GetValueOrDefault(item.questID, false);

                if (!done)
                {
                    // 번호는 실제 리스트 순서 그대로 표시
                    sb.AppendLine($"{index}. {item.displayName}");
                    questText.text = sb.ToString();
                    return;
                }

                index++;
            }

            // 모든 퀘스트 완료
            sb.AppendLine("✨ 모든 퀘스트를 완료했습니다!");
            questText.text = sb.ToString();
            return;
        }

        // ---------------------
        // 일반 AllAtOnce 모드
        // ---------------------
        foreach (var item in initialQuestItems)
        {
            bool done = keyQuests.GetValueOrDefault(item.questID, false);

            string text = done ?
                $"<color=#62B76B><b>{index}. {item.displayName} 완료</b></color>" :
                $"{index}. {item.displayName}";

            sb.AppendLine(text);
            index++;
        }

        questText.text = sb.ToString();
    }

}
