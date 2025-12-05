using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [Header("데미지 설정")]
    public int damageAmount = 1; // 함정이 플레이어에게 줄 데미지 양
    
    // 💡 Trigger 콜라이더에 다른 오브젝트가 들어왔을 때 호출됩니다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가졌는지 확인
        if (other.CompareTag("Player"))
        {
            // PlayerHealth 스크립트 찾기 (플레이어의 루트 오브젝트에 붙어있다고 가정)
            // other.transform.root는 플레이어의 최상위 오브젝트를 찾습니다.
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 데미지를 줍니다.
                playerHealth.TakeDamage(damageAmount);
                
                Debug.Log("함정 발동! 플레이어가 데미지를 입었습니다.");
                
                // 💡 (선택 사항) 데미지를 준 후 함정을 비활성화하거나 파괴할 수 있습니다.
                // Destroy(gameObject); // 한 번만 발동하는 함정일 경우
                // gameObject.SetActive(false); 
            }
        }
    }
}