using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("子弹基础参数")]
    public float speed = 10f;      // 子弹飞行速度
    public float lifeTime = 2f;    // 子弹存在时间
    public int damage = 1;         // 子弹伤害值

    // 记录子弹飞行方向
    private Vector2 moveDir = Vector2.right;

    // 获取子弹自身的 Rigidbody2D
    private Rigidbody2D rb;

    private void Awake()
    {
        // 在对象创建时缓存 Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 初始化子弹
    /// 这个函数会在玩家发射子弹时被调用
    /// 作用：
    /// 1. 接收玩家传进来的发射方向
    /// 2. 给子弹一个初速度
    /// 3. 设置子弹的自动销毁时间
    /// </summary>
    /// <param name="dir">发射方向</param>
    public void Init(Vector2 dir)
    {
        // 如果传进来的方向是 0，说明当前没有有效方向
        // 这里为了保险，默认让子弹朝右飞
        if (dir == Vector2.zero)
        {
            moveDir = Vector2.right;
        }
        else
        {
            moveDir = dir.normalized;
        }

        // 给 Dynamic Rigidbody2D 一个速度       
        if (rb != null)
        {
            rb.velocity = moveDir * speed;
        }

        // 防止重复调用时叠加旧的 Invoke
        CancelInvoke();

        // 子弹到时间后自动销毁，避免场上留太多子弹
        Invoke(nameof(DestroySelf), lifeTime);
    }

    private void FixedUpdate()
    {
        // 这里再次维持速度，防止某些情况下子弹速度被意外改掉       
        if (rb != null)
        {
            rb.velocity = moveDir * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 先打印一下，方便你观察子弹碰到了谁
        //Debug.Log("子弹碰到：" + other.name);

        // 忽略玩家自己
        if (other.CompareTag("Player"))
        {
            return;
        }

        // 忽略别的子弹
        if (other.CompareTag("Projectile"))
        {
            return;
        }

        // 对实现了 IDamageable 的对象进行受伤
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 到时间后自动销毁子弹
    /// </summary>
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}