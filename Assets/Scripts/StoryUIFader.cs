using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class StoryUIFader : MonoBehaviour
{
    [Header("UI 페이드 설정")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 1f;
    public float showDuration = 1.5f;
    public float fadeOutDuration = 1f;

    public static bool IsStoryPlaying = false; // ESC 차단용

    private bool isPlaying = false;

    void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;   // 초기엔 UI가 클릭 막지 않음
            canvasGroup.interactable = false;

            
        }
    }


    // ===============================================================
    // 🔥 ItemPickup.cs에서 호출하는 함수
    // ===============================================================
    public void Play(Action onComplete = null)
    {
        StartCoroutine(PlayStorySequence(onComplete));
    }


    // ===============================================================
    // 🔥 스토리 UI 재생 코루틴
    // ===============================================================
    private IEnumerator PlayStorySequence(Action onComplete)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup이 StoryUIFader에 연결되지 않음!");
            onComplete?.Invoke();
            yield break;
        }

        isPlaying = true;
        IsStoryPlaying = true;  // ESC 차단 ON

        // UI가 화면을 막기 시작
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // 🔥 게임 멈춤
        Time.timeScale = 0f;

        // 🔥 페이드 인
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        // 🔥 유지 시간
        yield return new WaitForSecondsRealtime(showDuration);

        // 🔥 페이드 아웃
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        // UI 클릭 방지 OFF
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // 🔥 게임 재개
        Time.timeScale = 1f;

        IsStoryPlaying = false;
        isPlaying = false;

        // 🔥 콜백 실행 → 이제 대화창 열림
        onComplete?.Invoke();
    }
}
