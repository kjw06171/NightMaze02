using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("사망 UI")]
    public GameObject deathUI;

    [Header("피격 효과 설정")]
    public float flashDuration = 0.1f;
    public int flashCount = 2;
    private SpriteRenderer[] renderers;

    public delegate void HealthChanged(int currentHealth, int maxHealth);
    public event HealthChanged OnHealthChanged;

    void Start()
    {
        // 🔥 씬이 Lv_00_1 또는 Lv_00_2라면 저장된 HP 불러오기
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "Lv_00_1" || scene == "Lv_00_2")
        {
            currentHealth = GameState.SharedHealth <= 0
                ? maxHealth
                : GameState.SharedHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        // Sprite 렌더러 자동 수집
        renderers = GetComponentsInChildren<SpriteRenderer>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (deathUI != null)
            deathUI.SetActive(false);
    }

    
    // =========================================================
    // 데미지 처리
    // =========================================================
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);

        // 🔥 HP 저장
        SaveSharedHealth();

        Debug.Log($"플레이어 데미지 → 남은 HP: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =========================================================
    // 회복 처리
    // =========================================================
    public void Heal(int amount)
    {
        if (amount > 0 && currentHealth >= maxHealth)
        {
            Debug.Log("최대 체력입니다.");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 🔥 HP 저장
        SaveSharedHealth();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }


    // =========================================================
    // 사망 처리
    // =========================================================
    private void Die()
    {
        Debug.Log("💀 플레이어 사망! 게임 오버");

        Time.timeScale = 0;

        if (deathUI != null)
            deathUI.SetActive(true);
    }


    // =========================================================
    // HP 공유 저장 함수
    // =========================================================
    private void SaveSharedHealth()
    {
        string scene = SceneManager.GetActiveScene().name;

        // only Lv_00_1, Lv_00_2 두 씬만 HP 공유!
        if (scene == "Lv_00_1" || scene == "Lv_00_2")
        {
            GameState.SharedHealth = currentHealth;
        }
    }


    // =========================================================
    // 피격 깜빡임 효과
    // =========================================================
    private IEnumerator HitFlash()
    {
        if (renderers == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            SetPlayerColor(new Color(1f, 0.3f, 0.3f));
            yield return new WaitForSeconds(flashDuration);

            SetPlayerColor(Color.white);
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void SetPlayerColor(Color color)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.color = color;
        }
    }

    
}
