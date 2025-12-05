
/// <summary>
/// 게임의 전반적인 상태를 관리하는 정적(Static) 클래스입니다.
/// 플레이어 인벤토리, 핵심 아이템 획득 여부 등을 전역적으로 추적하는 데 사용됩니다.
/// </summary>
public static class GameState
{
    // 💡 플레이어가 촛불을 획득했는지 여부를 추적하는 플래그입니다.
    // 기본값은 false이며, 획득해야만 true로 설정되어 빛 조절이 가능해집니다.
    public static bool HasCandle { get; set; } = false;

    // 💡 필요하다면 다른 전역 상태 변수들을 여기에 추가할 수 있습니다.
}