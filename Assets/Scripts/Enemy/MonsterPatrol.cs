using UnityEngine;
using Pathfinding;
using System.Collections;

public class MonsterPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] PatrolPoints;
    public float PatrolSpeed = 1.5f;
    public float WaypointWaitTime = 2f; 
    public float EndReachedDistance = 1.0f; // AIPath와 통일 (1.0 권장)

    // A* 컴포넌트 참조
    private AIPath aiPath;
    private Seeker seeker;

    // 순찰 상태 변수
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool patrollingForward = true; // 왕복 순찰을 위한 변수
    
    // 순찰을 시작했는지 여부 (EnemyDadChase에서 제어)
    private bool patrolActive = false;
    public bool IsPatrolling => patrolActive; // 외부에서 순찰 상태를 읽을 수 있도록 제공

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();

        // AIPath 설정 적용
        if (aiPath != null)
        {
            // [AIPath 설정 점검] End Reached Distance를 스크립트에서 설정합니다.
            aiPath.endReachedDistance = EndReachedDistance; 
        }

        // 초기에는 순찰 비활성화
        if (aiPath != null) aiPath.enabled = false;
    }

    void Update()
    {
        if (!patrolActive || PatrolPoints == null || PatrolPoints.Length == 0 || isWaiting)
        {
            return; // 순찰 활성화 상태가 아니거나 대기 중이면 아무것도 하지 않음
        }

        // 현재 Waypoint에 도착했는지 확인
        if (aiPath.reachedDestination)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    // --------------------------------------------------------
    // 외부 제어 함수 (EnemyDadChase.cs에서 호출)
    // --------------------------------------------------------

    public void StartPatrolling()
    {
        if (PatrolPoints.Length == 0) return;
        
        patrolActive = true;
        isWaiting = false;
        
        // AIPath 속도 및 목표 설정
        aiPath.enabled = true;
        aiPath.maxSpeed = PatrolSpeed;
        aiPath.target = PatrolPoints[currentWaypointIndex]; 
        
        Debug.Log("순찰 시스템 ON!");
    }

    public void StopPatrolling()
    {
        patrolActive = false;
        isWaiting = false;
        
        StopAllCoroutines(); // 진행 중인 대기 코루틴 중지
        aiPath.enabled = false;
        seeker.CancelCurrentPathRequest();
        
        Debug.Log("순찰 시스템 OFF!");
    }
    
    // --------------------------------------------------------
    // 내부 순찰 이동 로직 (왕복 방식 Ping-Pong)
    // --------------------------------------------------------

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        
        // 몬스터 멈춤
        aiPath.enabled = false;
        
        yield return new WaitForSeconds(WaypointWaitTime);
        
        GoToNextWaypoint(); // 다음 지점으로 이동 준비
        
        isWaiting = false;
    }
    
    private void GoToNextWaypoint()
    {
        if (PatrolPoints.Length <= 1) return;

        // 왕복 순찰 로직
        if (patrollingForward)
        {
            if (currentWaypointIndex < PatrolPoints.Length - 1)
            {
                currentWaypointIndex++;
            }
            else
            {
                patrollingForward = false;
                currentWaypointIndex--;
            }
        }
        else 
        {
            if (currentWaypointIndex > 0)
            {
                currentWaypointIndex--;
            }
            else
            {
                patrollingForward = true;
                currentWaypointIndex++;
            }
        }
        
        // 다음 Waypoint를 목표로 설정하고 AIPath 다시 활성화
        aiPath.target = PatrolPoints[currentWaypointIndex];
        aiPath.enabled = true;
    }
}