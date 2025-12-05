using UnityEngine;
using UnityEngine.SceneManagement; 
using System; // Action/Delegate 사용을 위해 추가

// 🚨 파일 내부에 이 클래스 정의가 오직 한 번만 존재해야 합니다.
public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxHealth = 3;   // 최대 목숨 개수 (하트 이미지 개수와 일치해야 함)
    private int currentHealth;  // 현재 목숨 개수

    // 💡 UI 업데이트 이벤트: 목숨이 변경될 때마다 이 이벤트를 통해 UI 스크립트(HealthUI)에 알립니다.
    public delegate void HealthChanged(int currentHealth, int maxHealth);
    public event HealthChanged OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        // 초기 UI 상태 설정
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 플레이어에게 데미지를 줍니다. (몬스터 공격 또는 함정 발동 시 호출됨)
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; 
        
        // 💡 현재 목숨이 데미지보다 적을 경우 0으로 보정
        currentHealth = Mathf.Max(0, currentHealth - damageAmount); 
        
        Debug.Log($"플레이어 데미지 입음. 남은 목숨: {currentHealth}");

        // 💡 UI에 변경 사항 알림
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 플레이어의 체력(목숨)을 회복시키거나 피해를 줍니다.
    /// 양수: 회복, 음수: 피해
    /// </summary>
    /// <param name="amount">체력 변화량 (음수일 경우 피해)</param>
    public void Heal(int amount)
    {
        // 🚨 [수정됨] 회복량(양수)일 때만 최대 목숨 체크를 합니다. 피해량(음수)은 무시하지 않습니다.
        if (amount > 0 && currentHealth >= maxHealth) 
        {
            Debug.Log("최대 목숨입니다. 더 이상 회복할 수 없습니다.");
            return;
        }
        
        // 1. 현재 목숨에 변화량(회복 또는 피해)을 더합니다.
        currentHealth += amount;

        // 2. 목숨이 0 미만으로 내려가는 것을 방지 (피해량 적용 시)
        currentHealth = Mathf.Max(currentHealth, 0);

        // 3. 최대 목숨을 초과하지 않도록 제한합니다. (회복량 적용 시)
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // 4. 로그 출력
        if (amount > 0)
        {
            Debug.Log($"HP 회복: +{amount} / 현재 목숨: {currentHealth}/{maxHealth}");
        }
        else if (amount < 0)
        {
            // 음수 값은 피해로 간주
            Debug.Log($"HP 피해: {amount} / 현재 목숨: {currentHealth}/{maxHealth}");
        }
        
        // 5. UI에 변경 사항 알림
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }
        
        // 6. 사망 체크 (피해 적용 후)
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // 💡 [추가된 함수] 현재 체력이 최대 체력과 같은지 확인
    /// <summary>
    /// 플레이어의 체력이 최대치인지 여부를 반환합니다.
    /// </summary>
    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }

    
    /// <summary>
    /// 플레이어가 사망했을 때 호출되는 함수입니다.
    /// </summary>
    private void Die()
    {
        Debug.Log("플레이어 사망! 게임 오버.");
        
        // 💡 게임 멈추기: 시간 흐름을 0으로 설정하여 모든 움직임을 정지시킵니다.
        Time.timeScale = 0; 
        
        // (여기에 게임 오버 UI를 표시하는 로직을 추가합니다.)
    }
}