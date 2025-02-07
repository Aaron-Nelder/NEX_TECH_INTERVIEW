using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrol, Chase, Attack }
public enum EnemyPatrolType { Loop, PingPong }

public class EnemyAI : MonoBehaviour
{
    const float NAV_UPDATE_TIME = 0.5f;

    [SerializeField] EnemyState m_currentState = EnemyState.Idle;
    [SerializeField] NavMeshAgent m_navMeshAgent;
    [SerializeField] LaserObstacle m_laserObstacle;
    [SerializeField] LineRenderer m_lineRenderer;
    [SerializeField] float m_attackRange = 3.0f;

    [Header("Patrol")]
    [SerializeField] Transform[] m_patrolPoints;
    [SerializeField] EnemyPatrolType m_patrolType = EnemyPatrolType.Loop;
    [SerializeField] float m_patrolSpeed = 2.0f;
    int m_currentPatrolPoint = 0;
    bool m_patrolForward = true;

    [Header("Chase")]
    [SerializeField] float m_chaseRange = 5.0f;
    [SerializeField] float m_chaseSpeed = 5.0f;
    [SerializeField] float m_turnSpeed = 5.0f;

    Transform m_playerTarget;
    Transform m_playerHead;
    float m_navUpdateTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        m_playerHead = Camera.main.transform;
        ChangeState(m_currentState);
    }

    // Update is called once per frame
    void Update()
    {
        CheckForChase();

        switch (m_currentState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            default:
                break;
        }
    }

    void Attack()
    {
        LookTowardsPlayer();

        // rotate the m_laserObstacle.transform to slowely look towards the players head
        Vector3 headDir = m_playerHead.position - m_laserObstacle.transform.position;
        m_laserObstacle.transform.rotation = Quaternion.Slerp(m_laserObstacle.transform.rotation, Quaternion.LookRotation(headDir), m_turnSpeed * Time.deltaTime);

        // check to see if the enemy can see the player
        m_navUpdateTime += Time.deltaTime;
        if (m_navUpdateTime < NAV_UPDATE_TIME)
            return;

        m_navUpdateTime = 0;
        Vector3 direction = m_playerHead.position - m_laserObstacle.transform.position;
        // Check if the player is in the line of sight
        if (Physics.Raycast(m_laserObstacle.transform.position, direction, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                m_navMeshAgent.SetDestination(m_playerTarget.position);
            }
        }
    }

    void LookTowardsPlayer()
    {
        Vector3 direction = m_playerHead.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), m_turnSpeed * Time.deltaTime);
    }

    void CheckForChase()
    {
        float distance = Vector3.Distance(transform.position, m_playerTarget.position);
        if (m_currentState == EnemyState.Attack)
        {
            if (distance > m_attackRange)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
            return;
        }

        // Check if the player is within range

        if (distance < m_chaseRange)
            ChangeState(EnemyState.Chase);
    }

    void Chase()
    {
        LookTowardsPlayer();

        // get the distance between the player and the enemy
        float distance = Vector3.Distance(transform.position, m_playerTarget.position);
        // Check if the player is within attack range
        if (distance < m_attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        m_navUpdateTime += Time.deltaTime;
        if (m_navUpdateTime < NAV_UPDATE_TIME)
            return;

        m_navUpdateTime = 0.0f;
        m_navMeshAgent.SetDestination(m_playerTarget.position);
    }

    void Patrol()
    {
        if (m_patrolPoints.Length == 0)
            return;

        Vector3 targetPosition = m_navMeshAgent.destination;
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(targetPosition, currentPosition);
        // Check if we are close to the target
        if (distance <= 1.5f)
        {
            // Move to the next patrol point
            m_currentPatrolPoint = m_patrolForward ? m_currentPatrolPoint + 1 : m_currentPatrolPoint - 1;
            // Check if we have reached the end of the patrol points
            if (m_currentPatrolPoint >= m_patrolPoints.Length || m_currentPatrolPoint < 0)
            {
                // Change direction based on patrol type
                if (m_patrolType == EnemyPatrolType.Loop)
                {
                    m_currentPatrolPoint = 0;
                }
                else if (m_patrolType == EnemyPatrolType.PingPong)
                {
                    m_patrolForward = !m_patrolForward;
                    m_currentPatrolPoint = m_patrolForward ? 1 : m_patrolPoints.Length - 2;
                }
            }
            m_navMeshAgent.SetDestination(m_patrolPoints[m_currentPatrolPoint].position);

        }
    }

    void ChangeState(EnemyState newState)
    {
        EnemyState oldState = m_currentState;
        m_currentState = newState;
        m_laserObstacle.enabled = newState == EnemyState.Attack;
        m_navMeshAgent.speed = newState == EnemyState.Patrol ? m_patrolSpeed : m_chaseSpeed;
        m_lineRenderer.enabled = newState == EnemyState.Attack;

        if (newState == EnemyState.Patrol)
            m_navMeshAgent.SetDestination(m_patrolPoints[m_currentPatrolPoint].position);
        else if (newState == EnemyState.Attack)
        {
            m_navMeshAgent.SetDestination(transform.position);
        }
    }
}
