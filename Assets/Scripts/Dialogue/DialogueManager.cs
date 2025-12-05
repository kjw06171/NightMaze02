using UnityEngine;
using TMPro; // TextMeshPro 사용
using UnityEngine.UI; // Image 컴포넌트 사용
using System.Collections;
using System.Collections.Generic;
using System; // 💡 System.Action을 사용하기 위해 추가

/// <summary>
/// 단일 화자 DialogueSO 구조에 맞춰 게임 내 대화창 UI를 관리하는 싱글톤 클래스입니다.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    // 💡 실제 인스턴스를 저장할 private 필드
    private static DialogueManager _instance; 

    // 💡 대화 상태 변화를 외부에 알리는 이벤트 (이벤트 시스템 유지)
    public Action OnDialogueStart;
    public Action OnDialogueEnd; 

    /// <summary>
    /// 싱글톤 인스턴스에 접근하는 프로퍼티. (안정적인 접근 방식)
    /// </summary>
    public static DialogueManager Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                // 씬에서 인스턴스를 찾고 할당합니다.
                _instance = FindObjectOfType<DialogueManager>();
                
                if (_instance == null)
                {
                    // [안전 장치] 런타임에 호출되었는데 인스턴스를 찾을 수 없으면 에러 로그 출력
                    Debug.LogError("🚨 DialogueManager.Instance가 호출되었으나 씬에서 인스턴스를 찾을 수 없습니다! 오브젝트가 씬에 있고 활성화되었는지 확인하세요.");
                }
            }
            return _instance;
        } 
    }

    [Header("UI 요소 연결")]
    [Tooltip("대화창 배경 패널")]
    public GameObject dialoguePanel; 
    
    [Tooltip("실제 대화 내용 텍스트 (TextMeshProUGUI)")]
    public TextMeshProUGUI dialogueText; 
    
    [Tooltip("화자 이름 텍스트 (TextMeshProUGUI)")]
    public TextMeshProUGUI speakerNameText; 
    
    [Tooltip("캐릭터 초상화 이미지")]
    public Image characterPortrait; 
    
    [Tooltip("일시정지 메뉴 캔버스 (대화 중 숨김)")]
    public GameObject pauseMenuCanvas;
    
    // 💡 [새 필드] 플레이어 움직임 스크립트 직접 참조
    [Header("플레이어 제어 통합 (필수 연결)")]
    [Tooltip("대화 중 움직임을 멈출 플레이어 움직임 스크립트 컴포넌트를 연결하세요.")]
    public MonoBehaviour playerMovementComponent; 
    
    [Header("설정")]
    [Tooltip("텍스트가 한 글자씩 나타나는 속도 (글자당 시간)")]
    public float typingSpeed = 0.05f; 

    // 💡 현재 대화 상태 및 데이터
    private DialogueSO currentDialogueData;
    private int currentSentenceIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine; 
    
    // 💡 대화 종료 후 실행할 콜백을 저장하는 변수
    private Action onDialogueEndCallback; 
    
    // 💡 일시정지 메뉴 상태 저장 변수
    private bool wasPauseMenuVisibleBeforeDialogue = false; 

    void Awake()
    {
        // 1. 싱글톤 초기화
        if (_instance == null)
        {
            _instance = this; // 현재 인스턴스를 할당
            Debug.Log("✅ DialogueManager 인스턴스 초기화 성공!");
            
            // 💡 씬이 바뀌어도 파괴되지 않도록 설정 (씬 재로드 문제 해결)
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this) // 이미 다른 인스턴스가 존재하면 스스로 파괴
        {
            Debug.LogWarning("⚠️ 중복된 DialogueManager 인스턴스가 발견되어 파괴합니다.");
            Destroy(gameObject);
            return;
        }
        
        // 2. 필수 UI 요소 연결 확인 및 안전 장치
        if (dialoguePanel == null || dialogueText == null || speakerNameText == null)
        {
            Debug.LogError("🚨 DialogueManager 초기화 오류: 필수 UI 요소(Panel, DialogueText, SpeakerNameText) 연결을 확인하세요. 이 컴포넌트를 비활성화합니다.");
            gameObject.SetActive(false); // 문제가 있으면 이 Manager를 비활성화
            return;
        }

        // 3. 초기 상태 설정
        dialoguePanel.SetActive(false);
        if (characterPortrait != null) characterPortrait.gameObject.SetActive(false);
        
        // 4. 게임 시간 초기화
        Time.timeScale = 1f; 
    }

    /// <summary>
    /// 매 프레임 입력 확인 및 대화 진행을 처리합니다.
    /// </summary>
    void Update()
    {
        // 대화가 활성화된 상태에서만 입력 확인
        if (isDialogueActive)
        {
            // E 키나 마우스 왼쪽 버튼을 누르면
            // Time.timeScale=0f 상태에서도 Input은 감지되므로 Realtime 코루틴과 함께 이 Update는 작동해야 합니다.
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                HandleAdvanceDialogue();
            }
        }
    }
    
    /// <summary>
    /// 다음 문장으로 진행하거나 타이핑을 즉시 완료합니다.
    /// </summary>
    private void HandleAdvanceDialogue()
    {
        // 1. 현재 타이핑 중인 경우: 타이핑을 즉시 완료
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                isTyping = false;
            }
            // 현재 문장의 전체 텍스트를 즉시 표시
            if (currentDialogueData != null && currentSentenceIndex < currentDialogueData.SentenceCount)
            {
                 dialogueText.text = currentDialogueData.sentences[currentSentenceIndex]; 
            }
            return;
        }

        // 2. 타이핑이 완료된 경우: 다음 문장으로 이동
        currentSentenceIndex++;

        // 3. 다음 문장이 남아있는 경우
        if (currentDialogueData != null && currentSentenceIndex < currentDialogueData.SentenceCount)
        {
            DisplayCurrentSentence();
        }
        // 4. 모든 대화가 끝난 경우
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 새로운 대화를 DialogueSO 데이터 기반으로 시작합니다.
    /// </summary>
    public void StartDialogue(DialogueSO dialogueData, Action onEnd = null) 
    {
        if (isDialogueActive) return;
        
        // [새로운 안전 장치] UI Panel이 연결되지 않았을 경우 강제 종료
        if (dialoguePanel == null)
        {
            Debug.LogError("🚨 StartDialogue 호출 실패! Dialogue Panel이 연결되지 않았습니다.");
            return;
        }

        // 💡 [안전 확인] DialogueSO 데이터가 유효한지 확인
        if (dialogueData == null || dialogueData.SentenceCount == 0)
        {
            Debug.LogError("[DialogueManager] 🚨 전달된 DialogueSO 데이터가 Null이거나 빈 목록입니다.");
            return;
        }
        
        // 💡 콜백 저장
        this.onDialogueEndCallback = onEnd;

        currentDialogueData = dialogueData;
        currentSentenceIndex = 0;
        isDialogueActive = true;
        
        // Time.timeScale=0f로 게임 시간을 멈춥니다.
        // 💡 (주의) 플레이어 움직임 로직이 Time.unscaledDeltaTime을 사용하면 멈추지 않을 수 있습니다.
        Time.timeScale = 0f; 

        // ----------------------------------------------------
        // 💡 플레이어 움직임 비활성화 로직 (통합된 솔루션)
        // Time.timeScale=0f만으로는 부족할 수 있는 상황(Input 감지, Unscaled Time 사용 등)을 대비하여
        // 플레이어 움직임 스크립트 실행 자체를 명시적으로 중단합니다.
        if (playerMovementComponent != null)
        {
            playerMovementComponent.enabled = false;
            
            // Rigidbody를 사용한다면 잔여 움직임을 막기 위해 속도를 리셋합니다.
            // playerMovementComponent가 부착된 GameObject에서 Rigidbody를 찾습니다.
            Rigidbody rb = playerMovementComponent.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log($"플레이어 움직임 스크립트 ({playerMovementComponent.GetType().Name}) 비활성화됨.");
        }
        // ----------------------------------------------------

        // 1. UI 활성화
        dialoguePanel.SetActive(true);

        // 💡 일시정지 메뉴 비활성화: 메뉴가 이미 활성화되어 있었다면 상태를 저장하고 숨깁니다.
        if (pauseMenuCanvas != null)
        {
            if (pauseMenuCanvas.activeSelf)
            {
                 wasPauseMenuVisibleBeforeDialogue = true; // 이전 상태 저장
                 pauseMenuCanvas.SetActive(false);
                 Debug.Log("Pause Menu Canvas를 비활성화했습니다. (대화 중 오버라이드)");
            }
            else
            {
                 wasPauseMenuVisibleBeforeDialogue = false;
            }
        }
        
        // 2. 캐릭터 이름 설정
        if (speakerNameText != null)
        {
            speakerNameText.text = currentDialogueData.characterName;
        }

        // 3. 초상화 설정
        if (characterPortrait != null)
        {
            Sprite portraitSprite = dialogueData.portrait;
            characterPortrait.sprite = portraitSprite;
            // 초상화가 있으면 활성화, 없으면 비활성화
            characterPortrait.gameObject.SetActive(portraitSprite != null);
        }

        // 4. 첫 번째 문장 표시 시작
        DisplayCurrentSentence();

        // 5. 이벤트 호출
        OnDialogueStart?.Invoke();
    }

    /// <summary>
    /// 현재 인덱스에 해당하는 대화 문장을 가져와 표시합니다.
    /// </summary>
    private void DisplayCurrentSentence()
    {
        // 💡 null 체크를 추가하여 안전성 확보
        if (currentDialogueData == null || currentSentenceIndex >= currentDialogueData.SentenceCount)
        {
            EndDialogue();
            return;
        }
        
        string sentence = currentDialogueData.sentences[currentSentenceIndex];
        
        // 텍스트 타이핑 코루틴 시작
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    /// <summary>
    /// 텍스트를 한 글자씩 출력하는 코루틴입니다.
    /// </summary>
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = ""; 
        
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            // Time.timeScale=0f 상태에서도 작동하도록 WaitForSecondsRealtime 사용
            yield return new WaitForSecondsRealtime(typingSpeed); 
        }

        isTyping = false;
        typingCoroutine = null;
    }

    /// <summary>
    /// 대화를 종료하고 UI를 숨깁니다.
    /// </summary>
    public void EndDialogue()
    {
        isDialogueActive = false;
        
        // UI 정리
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (characterPortrait != null) characterPortrait.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
        if (speakerNameText != null) speakerNameText.text = "";
        
        // 💡 콜백 함수 실행
        if (onDialogueEndCallback != null)
        {
            onDialogueEndCallback.Invoke();
            onDialogueEndCallback = null; // 실행 후 초기화
        }
        
        // 💡 이벤트 호출
        OnDialogueEnd?.Invoke();
        
        // 💡 일시정지 메뉴 복원: 대화 시작 전에 메뉴가 활성화 상태였다면 다시 활성화합니다.
        if (pauseMenuCanvas != null && wasPauseMenuVisibleBeforeDialogue)
        {
             pauseMenuCanvas.SetActive(true);
             wasPauseMenuVisibleBeforeDialogue = false; // 상태 초기화
             Debug.Log("Pause Menu Canvas를 다시 활성화했습니다. (이전 상태 복원)");
        }
        
        Time.timeScale = 1f; // 💡 게임 시간 다시 진행
        currentDialogueData = null;
        
        // ----------------------------------------------------
        // 💡 플레이어 움직임 활성화 로직 (통합된 솔루션)
        // 비활성화했던 움직임 스크립트를 다시 활성화하여 플레이어 움직임을 복원합니다.
        if (playerMovementComponent != null)
        {
            playerMovementComponent.enabled = true;
            Debug.Log($"플레이어 움직임 스크립트 ({playerMovementComponent.GetType().Name}) 활성화됨.");
        }
        // ----------------------------------------------------
        
        Debug.Log("대화 종료 및 게임 재개.");
    }

    /// <summary>
    /// 대화가 진행 중인지 여부를 반환합니다.
    /// </summary>
    public bool IsActive()
    {
        return isDialogueActive;
    }
}