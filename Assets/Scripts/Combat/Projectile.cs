using UnityEngine;

/// <summary>
/// 通用子弹脚本
///
/// 当前主要用于玩家子弹。
///
/// 职责：
/// 1. 接收发射方向
/// 2. 控制子弹移动
/// 3. 忽略发射者自己
/// 4. 命中目标后造成伤害
/// 5. 到时间后自动销毁
///
/// 注意：
/// 这一轮先只让玩家子弹使用这个脚本。
/// 敌人子弹 EnemyProjectile 暂时不动，因为它有更复杂的扫掠命中检测。
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("子弹数据")]
    [SerializeField] private BulletData bulletData;
    [Header("子弹基础参数")]
    [SerializeField] private float speed = 10f;       // 子弹飞行速度
    [SerializeField] private float lifeTime = 2f;     // 子弹存在时间
    [SerializeField] private int damage = 1;          // 子弹伤害值

    [Header("命中规则")]
    [SerializeField] private string targetTag = "Enemy";

    private float Speed => bulletData != null ? bulletData.speed : speed;
    private float LifeTime => bulletData != null ? bulletData.lifeTime : lifeTime;
    private int Damage => bulletData != null ? bulletData.damage : damage;
    // 当前飞行方向
    private Vector2 moveDir = Vector2.right;

    // 子弹自己的 Rigidbody2D
    private Rigidbody2D rb;

    // 子弹自己的 Collider2D
    private Collider2D projectileCollider;

    // 发射者根物体
    // 例如：玩家发射，就记录 Player 的 Transform
    private Transform ownerRoot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// 初始化子弹
    /// </summary>
    /// <param name="dir">子弹飞行方向</param>
    /// <param name="owner">发射者</param>
    /// <param name="targetTag">目标 Tag，例如 Enemy / Player</param>
    public void Init(Vector2 dir, Transform owner, string targetTag)
    {
        ownerRoot = owner != null ? owner.root : null;
        this.targetTag = targetTag;

        // 防止传进来的方向是 0
        if (dir.sqrMagnitude <= 0.0001f)
        {
            moveDir = Vector2.right;
        }
        else
        {
            moveDir = dir.normalized;
        }

        // 忽略发射者自己的 Collider
        IgnoreOwnerCollision();

        // 设置初速度
        if (rb != null)
        {
            rb.velocity = moveDir * Speed;
        }

        // 防止重复调用 Init 时叠加旧的 Invoke
        CancelInvoke();

        // 到时间自动销毁
        Invoke(nameof(DestroySelf), LifeTime);
    }

    /// <summary>
    /// 兼容旧代码：
    /// 如果还有地方调用 Init(dir)，默认当作玩家子弹，目标是 Enemy。
    /// </summary>
    public void Init(Vector2 dir)
    {
        Init(dir, null, "Enemy");
    }

    private void FixedUpdate()
    {
        // 维持速度，避免子弹被物理碰撞影响后变慢
        if (rb != null)
        {
            rb.velocity = moveDir * Speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitTarget(other);
    }

    /// <summary>
    /// 尝试命中目标
    /// </summary>
    private void TryHitTarget(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        Transform otherRoot = other.transform.root;

        // 忽略发射者自己
        if (ownerRoot != null && otherRoot == ownerRoot)
        {
            return;
        }

        // 忽略其他子弹
        if (other.CompareTag("Projectile"))
        {
            return;
        }

        // 如果设置了目标 Tag，就只攻击对应目标
        if (!string.IsNullOrWhiteSpace(targetTag))
        {
            bool isTarget = false;

            if (other.CompareTag(targetTag))
            {
                isTarget = true;
            }

            if (otherRoot != null && otherRoot.CompareTag(targetTag))
            {
                isTarget = true;
            }

            if (!isTarget)
            {
                return;
            }
        }

        // 优先从被碰到的 Collider 所在物体找 IDamageable
        IDamageable damageable = other.GetComponent<IDamageable>();

        // 如果子物体上没有，就从根物体找
        if (damageable == null && otherRoot != null)
        {
            damageable = otherRoot.GetComponent<IDamageable>();
        }

        if (damageable == null)
        {
            Debug.LogWarning($"{name} 命中了 {other.name}，但目标没有 IDamageable。");
            return;
        }

        damageable.TakeDamage(Damage);
        DestroySelf();
    }

    /// <summary>
    /// 忽略发射者自己的碰撞体
    /// 例如玩家发射子弹后，不应该立刻打到自己。
    /// </summary>
    private void IgnoreOwnerCollision()
    {
        if (ownerRoot == null || projectileCollider == null)
        {
            return;
        }

        Collider2D[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider2D>(true);

        for (int i = 0; i < ownerColliders.Length; i++)
        {
            if (ownerColliders[i] != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, ownerColliders[i], true);
            }
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}