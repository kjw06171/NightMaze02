using UnityEngine;

using Pathfinding;

using System.Collections;



public class EnemyDadChase : MonoBehaviour

{

    // A* Pathfinding 컴포넌트

    public Seeker seeker;

    public AIPath aiPath;



    [Header("Target & Light")]

    public Transform Player;

    public LightControl LightControlScript;

    public MonsterPatrol patrolScript;



    [Header("Movement & State")]

    public float ChaseSpeed = 3f;

    public float FleeSpeed = 2f;

    public float ChaseDistance = 5f; // 플레이어 추격 시작 거리

    public float FleeDistance = 10f; // 도망갈 때 플레이어로부터 멀어지려는 거리



    // Collision Filter 변수는 이제 감지 로직을 담당할 MonsterSensorTrigger.cs로 이동합니다.

    // public LayerMask PlayerSensorLayer; // 이 줄을 제거하세요!



    private bool isChasing = false;

    private bool isFleeing = false;



    void Start()

    {

        // 컴포넌트 자동 참조

        if (aiPath == null) aiPath = GetComponent<AIPath>();

        if (seeker == null) seeker = GetComponent<Seeker>();

        if (patrolScript == null) patrolScript = GetComponent<MonsterPatrol>();



        // 초기 상태: AIPath 비활성화

        if (aiPath != null) aiPath.enabled = false;

    }



    void Update()

    {

        // 필수 컴포넌트 및 타겟 확인

        if (Player == null || LightControlScript == null || aiPath == null || patrolScript == null) return;



        bool isLightActive = LightControlScript.IsLightOn;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

       

        // --- 1. 상태 전환 및 제어 로직 (Flee > Chase > Patrol 순) ---

       

        // 1-1. 빛이 켜져 있고 AND 플레이어가 ChaseDistance 이내에 있을 경우 (Flee 최우선)

        if (isLightActive && distanceToPlayer < ChaseDistance)

        {

            if (patrolScript.IsPatrolling) patrolScript.StopPatrolling();

            if (isChasing) StopChasing();

            if (!isFleeing) StartFleeing();

        }

        // 1-2. 빛이 꺼지거나, 도망 조건이 충족되지 않을 경우

        else

        {

            if (isFleeing) StopFleeing(); // 도망 중지



            // (B) 횃불이 꺼져있고, 추격 범위 안에 들어왔을 경우 추격 시작

            if (!isFleeing && distanceToPlayer < ChaseDistance)

            {

                if (!isChasing)

                {

                    if (patrolScript.IsPatrolling) patrolScript.StopPatrolling();

                    StartChasing();

                }

            }

            // (C) 추격 중이었는데, 플레이어가 범위 밖으로 나갔을 경우 추격 중지 및 순찰 시작

            else if (isChasing && distanceToPlayer >= ChaseDistance)

            {

                StopChasing();

                if (!patrolScript.IsPatrolling)

                {

                    patrolScript.StartPatrolling();

                }

            }

            // (D) 추격도 도망도 아닐 때 순찰 시작

            else if (!isChasing && !isFleeing)

            {

                if (!patrolScript.IsPatrolling)

                {

                    patrolScript.StartPatrolling();

                }

            }

        }

       

        // --- 2. 이동 처리 ---

        if (isFleeing)

        {

            HandleFleeMovement();

        }

        else if (isChasing)

        {

            HandleChaseMovement();

        }

    }

   

    // --------------------------------------------------------

    // 💥 Trigger 감지 함수: (제거됨 - MonsterSensorTrigger.cs로 이동)

    // --------------------------------------------------------

    // private void OnTriggerEnter2D(Collider2D other) 함수는 제거되었습니다.

   

    // --------------------------------------------------------

    // 이동 처리 함수 및 상태 변경 도우미 함수 (그대로 유지)

    // --------------------------------------------------------



    private void OnCollisionEnter2D(Collision2D collision)

    {

        // 몬스터의 큰 원 콜리더(몸체, Non-Trigger)가 다른 Non-Trigger 오브젝트(예: 벽, 플레이어 몸체)와

        // 물리적으로 충돌했을 때 사용됩니다.

    }



    private void HandleFleeMovement()

    {

        Vector3 directionToPlayer = Player.position - transform.position;

        Vector3 fleeDirection = -directionToPlayer.normalized;

        Vector3 targetPosition = transform.position + fleeDirection * FleeDistance;



        NNConstraint constraint = NNConstraint.None;

        NNInfo nearestNodeInfo = AstarPath.active.GetNearest(targetPosition, constraint);

        Vector3 nearestValidTarget = nearestNodeInfo.position;



        aiPath.destination = nearestValidTarget;

       

        if (!aiPath.enabled) aiPath.enabled = true;

        aiPath.maxSpeed = FleeSpeed;

    }

   

    private void HandleChaseMovement()

    {

        if (!aiPath.enabled) aiPath.enabled = true;

        aiPath.target = Player;

        aiPath.maxSpeed = ChaseSpeed;

    }



    void StartChasing()

    {

        isChasing = true;

        aiPath.target = Player;

        aiPath.enabled = true;

        aiPath.maxSpeed = ChaseSpeed;

        Debug.Log("추격 시작!");

    }



    void StopChasing()

    {

        isChasing = false;

        if (!isFleeing)

        {

            aiPath.enabled = false;

            seeker.CancelCurrentPathRequest();

        }

        Debug.Log("추격 중지!");

    }

   

    void StartFleeing()

    {

        isFleeing = true;

        aiPath.target = null;

        aiPath.enabled = true;

        aiPath.maxSpeed = FleeSpeed;

        Debug.Log("불 감지! 도망 시작!");

    }



    void StopFleeing()

    {

        isFleeing = false;

        aiPath.enabled = false;

        seeker.CancelCurrentPathRequest();

        Debug.Log("도망 중지!");

    }

}