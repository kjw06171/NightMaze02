using UnityEngine;

public class AutoFadeOnPlayer : MonoBehaviour
{
    [Header("투명도 설정")]
    [Range(0f, 1f)]
    public float fadeAlpha = 0.3f;   // 센서가 안에 있을 때
    public float normalAlpha = 1f;   // 기본 알파

    [Header("적용할 SpriteRenderer들")]
    public SpriteRenderer[] renderers; // 여러 개 처리 가능

    private void Awake()
    {
        // 자동으로 자식 SpriteRenderer 가져오기
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🔥 Player → X  
        // 🔥 PlayerSensor → O
        if (other.CompareTag("PlayerSensor"))
            SetAlpha(fadeAlpha);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerSensor"))
            SetAlpha(normalAlpha);
    }

    private void SetAlpha(float a)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            Color c = r.color;
            c.a = a;
            r.color = c;
        }
    }
}
