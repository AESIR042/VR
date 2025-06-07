using UnityEngine;
using UnityEngine.AI;

public class Rabbit : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 12f;         // 兔子游荡半径略大
    public float wanderTimer = 4f;
    public float walkSpeed = 2f;             // 兔子走路速度更快
    public float runSpeed = 7f;              // 兔子逃跑速度更快
    public float detectionRange = 7f;        // 兔子警觉范围更大
    public float fleeRange = 12f;            // 兔子逃跑距离更远
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

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
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            isScared = true;
            FleeFromPlayer();
        }
        else if (distanceToPlayer > fleeRange)
        {
            isScared = false;

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                timer += Time.deltaTime;
                idleTime -= Time.deltaTime;

                if (idleTime <= 0)
                {
                    Wander();
                    idleTime = Random.Range(minIdleTime, maxIdleTime);
                }
            }
        }

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
        if (animator != null)
        {
            animator.SetBool("IsScared", isScared);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 bestPoint = origin;
        float minDist = float.MaxValue;
        int maxTries = 10;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection.y = 0;
            Vector3 candidate = origin + randDirection;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(candidate, out navHit, 2.0f, layermask))
            {
                float d = Vector3.Distance(origin, navHit.position);
                if (d < minDist && d > 1.0f)
                {
                    minDist = d;
                    bestPoint = navHit.position;
                }
            }
        }
        return bestPoint;
    }
}