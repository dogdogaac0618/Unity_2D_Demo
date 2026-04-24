using UnityEngine;

public class EnemyRangedController : MonoBehaviour
{
    [Header("目标引用")]
    [SerializeField] private Transform playerTarget;

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float detectRange = 350f;

    [Tooltip("大于这个距离时，敌人会往玩家方向靠近")]
    [SerializeField] private float preferredDistance = 180f;

    [Tooltip("小于这个距离时，敌人会后退，避免一直贴脸")]
    [SerializeField] private float retreatDistance = 120f;

    [Header("发射参数")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireCooldown = 1.2f;
    //离身体外缘再推开多少”的额外边距
    [SerializeField] private float fireOffset = 1.2f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private float lastFireTime = -999f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if(rb == null )
        {
            Debug.LogWarning("EnemyRanged没有挂载RB2D组件");
        }
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {        
        // 优先手动拖引用；如果忘了拖，再在启动时兜底找一次
        // 注意：不是在 Update 里反复找
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
    }

    /// <summary>
    /// 给以后刷怪系统预留的外部设置接口
    /// </summary>
    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    private void FixedUpdate()
    {
        if (playerTarget == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        
        Vector2 toPlayer = playerTarget.position - transform.position;
        float distance = toPlayer.magnitude;

        // 玩家不在侦测范围内时，不移动
        if (distance > detectRange)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 dir = distance > 0.001f ? toPlayer / distance : Vector2.zero;

        // 三段式距离控制：
        // 1. 太远 -> 靠近
        // 2. 太近 -> 后退
        // 3. 合适距离 -> 停住
        if (distance > preferredDistance)
        {
            rb.velocity = dir * moveSpeed;
        }
        else if (distance < retreatDistance)
        {
            rb.velocity = -dir * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (playerTarget == null || projectilePrefab == null)
        {
            return;
        }

        Vector2 toPlayer = playerTarget.position - transform.position;
        float distance = toPlayer.magnitude;

        // 玩家不在侦测范围内，不开火
        if (distance > detectRange)
        {
            return;
        }

        // 按冷却发射
        if (Time.time < lastFireTime + fireCooldown)
        {
            return;
        }

        lastFireTime = Time.time;
        Fire(toPlayer.normalized);
    }

    private void Fire(Vector2 dir)
    {
        // 恢复成正常发射：
        // 从敌人当前位置朝玩家方向发射
        Vector3 spawnPos = transform.position + (Vector3)(dir.normalized * fireOffset);

        GameObject bulletObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        EnemyProjectile enemyProjectile = bulletObj.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.Init(dir, transform);
        }
        else
        {
            Debug.LogError("EnemyRangedController 发射失败：实例上没有 EnemyProjectile");
        }
    }
}