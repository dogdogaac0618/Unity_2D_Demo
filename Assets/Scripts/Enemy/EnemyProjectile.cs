using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("子弹基础参数")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;

    [Header("命中检测参数")]
    [SerializeField] private float hitCheckRadius = 0.6f;

    [Header("命中规则")]
    [SerializeField] private string targetTag = "Player";

    // 当前飞行方向
    private Vector2 moveDir = Vector2.right;

    // 缓存组件，避免重复 GetComponent
    private Rigidbody2D rb;
    private Collider2D bulletCollider;

    // 记录发射者，用于忽略自己
    private Transform ownerRoot;

    // 记录上一帧位置
    // 为什么要记：
    // 因为我们不再只靠 OnTriggerEnter2D，
    // 而是每一帧主动检查“从上一帧到这一帧之间，这颗子弹有没有扫到 Player”
    private Vector2 lastPosition;

    // 防止一颗子弹重复命中多次
    private bool hasHitTarget = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(DestroySelf), lifeTime);

        hasHitTarget = false;

        if (rb != null)
        {
            lastPosition = rb.position;
        }
        else
        {
            lastPosition = transform.position;
        }
    }

    /// <summary>
    /// 由外部在生成子弹后调用
    /// dir：子弹飞行方向
    /// owner：发射这颗子弹的敌人
    /// </summary>
    public void Init(Vector2 dir, Transform owner)
    {
        ownerRoot = owner.root;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            moveDir = Vector2.right;
        }
        else
        {
            moveDir = dir.normalized;
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (bulletCollider == null)
        {
            bulletCollider = GetComponent<Collider2D>();
        }

        // 从物理层忽略发射者自己的碰撞体
        // 目的：避免子弹出膛后先和敌人自己发生碰撞
       
        IgnoreOwnerCollision();

        // 记录初始化后的起点
        if (rb != null)
        {
            lastPosition = rb.position;
            rb.velocity = moveDir * speed;
        }
        else
        {
            lastPosition = transform.position;
        }
    }
    /// <summary>
    /// 忽略发射者自己的碰撞体
    /// 例如敌人发射子弹后，子弹不应该马上打到敌人自己。
    /// </summary>
    private void IgnoreOwnerCollision()
    {
        Collider2D[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider2D>(true);
        for(int i = 0;i < ownerColliders.Length;i++)
        {
            if(ownerColliders[i] != null)
            {
                Physics2D.IgnoreCollision(bulletCollider,ownerColliders[i], true);
            }
        }
        
    }
    private void FixedUpdate()
    {
        if (hasHitTarget)
        {
            return;
        }

        CheckHitBySweep();

        if (rb != null)
        {
            lastPosition = rb.position;
        }
        else
        {
            lastPosition = transform.position;
        }
    }

    /// <summary>
    /// 主动检测命中
    /// 1. 先检查当前位置是否已经和 Player 重叠
    /// 2. 再检查“上一帧 -> 当前帧”这段路径有没有扫到 Player
    /// 这样就不再依赖 Trigger 回调本身。
    /// </summary>
    private void CheckHitBySweep()
    {
        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;

        // 先查当前位置重叠
        TryHitPlayerAtPosition(currentPosition);

        if (hasHitTarget)
        {
            return;
        }

        Vector2 delta = currentPosition - lastPosition;
        float distance = delta.magnitude;

        // 如果这一帧几乎没移动，就不用继续做路径扫掠
        if (distance <= 0.0001f)
        {
            return;
        }

        Vector2 castDir = delta / distance;

        // 用 CircleCast 检查“从上一帧到当前帧”的整段路径
        RaycastHit2D[] hits = Physics2D.CircleCastAll(lastPosition, hitCheckRadius, castDir, distance);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D other = hits[i].collider;
            TryHitPlayerFromCollider(other);

            if (hasHitTarget)
            {
                return;
            }
        }
    }

    /// <summary>
    /// 检查当前位置是否已经和某个碰撞体重叠
    /// </summary>
    private void TryHitPlayerAtPosition(Vector2 position)
    {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, hitCheckRadius);

        for (int i = 0; i < overlaps.Length; i++)
        {
            TryHitPlayerFromCollider(overlaps[i]);

            if (hasHitTarget)
            {
                return;
            }
        }
    }

    /// <summary>
    /// 只要拿到一个碰撞体，就尝试判断它是不是 Player
    /// 如果是 Player，就直接从 Player 根对象拿 Health 扣血
    /// </summary>
    private void TryHitPlayerFromCollider(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        Transform root = other.transform.root;

        // 忽略发射者自己
        if (ownerRoot != null && root == ownerRoot)
        {
            return;
        }
        //忽略子弹
        if(other.CompareTag("Projectile"))
        {
            return;
        }

        // 处理玩家
        bool isTarget = false;
        if (other.CompareTag(targetTag))
        {
            isTarget = true;
        }
        if(root.CompareTag(targetTag))
        {
            isTarget = true;
        }
        if (!isTarget)
        {
            return;
        }

        IDamageable damageable = root.GetComponent<IDamageable>();

        // 如果当前碰撞体上没有，就从根物体找
        if (damageable == null && root != null)
        {
            damageable = root.GetComponent<IDamageable>();
        }
        damageable.TakeDamage(damage);
        hasHitTarget = true;

        Destroy(gameObject);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitCheckRadius);
    }
#endif
}