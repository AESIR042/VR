using UnityEngine;
using UnityEngine.AI;

public class Boar : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    public float walkSpeed = 1f;
    public float runSpeed = 4f;
    public float detectionRange = 5f;
    public float fleeRange = 8f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float timer;
    private float idleTime;
    private bool isScared;
    private Vector3 startPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        
        timer = wanderTimer;
        idleTime = Random.Range(minIdleTime, maxIdleTime);
    }

    void Update()
    {
        // 检测玩家距离
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer < detectionRange)
        {
            // 玩家靠近，逃跑
            isScared = true;
            FleeFromPlayer();
        }
        else if (distanceToPlayer > fleeRange)
        {
            // 玩家远离，恢复正常行为
            isScared = false;
            
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // 到达目的地后的行为
                timer += Time.deltaTime;
                idleTime -= Time.deltaTime;
                
                if (idleTime <= 0)
                {
                    // 空闲时间结束，开始游荡
                    Wander();
                    idleTime = Random.Range(minIdleTime, maxIdleTime);
                }
            }
        }
        
        // 更新动画参数
        UpdateAnimation();
    }
    
    void Wander()
    {
        Vector3 newPos = RandomNavSphere(startPosition, wanderRadius, -1);
        agent.speed = walkSpeed;
        agent.SetDestination(newPos);
        timer = 0;
    }
    
    void FleeFromPlayer()
    {
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeRange;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeRange, NavMesh.AllAreas))
        {
            agent.speed = runSpeed;
            agent.SetDestination(hit.position);
        }
    }
    
    void UpdateAnimation()
    {
        animator.SetBool("IsScared", isScared);
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
    
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 bestPoint = origin;
        float minDist = float.MaxValue;
        int maxTries = 10;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection.y = 0; // 保持在地面
            Vector3 candidate = origin + randDirection;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(candidate, out navHit, 2.0f, layermask))
            {
                float d = Vector3.Distance(origin, navHit.position);
                if (d < minDist && d > 1.0f) // 距离不能太近
                {
                    minDist = d;
                    bestPoint = navHit.position;
                }
            }
        }
        return bestPoint;
    }
}
