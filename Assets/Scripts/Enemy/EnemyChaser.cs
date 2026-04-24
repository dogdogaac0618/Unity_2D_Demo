using UnityEngine;

/// <summary>
/// 近战追击怪 AI
/// 作用：
/// 1. 在场景中自动寻找玩家
/// 2. 当玩家进入追击范围时，朝玩家移动
/// 3. 当距离玩家很近时，停止继续硬顶，避免抖动太明显
///
/// 这是最基础的一种敌人原型：
/// “近战追击型”
/// 后面远程怪、硬壳怪都可以在这个基础上继续分化
/// </summary>
public class EnemyChaser : MonoBehaviour
{// 敌人的追击目标
    //方法
    // 1. 直接在 Inspector 里手动拖 Player 进来
    // 2. 或者后面由刷怪系统调用 SetTarget() 传入
    // 3. 如果前两种都没有，这个脚本会在 Start 里做一次兜底查找
    [Header("目标设置")]
    [SerializeField] private Transform playerTarget;

    // 敌人移动速度
    [Header("追击参数")]
    [SerializeField] private float moveSpeed = 2.5f;

    // 探测范围
    // 当玩家超出这个距离时，敌人停止追击
    [SerializeField] private float detectRange = 8f;

    // 停止距离
    // 当敌人已经非常靠近玩家时，就不再继续往前顶
    [SerializeField] private float stopDistance = 0.8f;

    // 缓存 Rigidbody2D
    private Rigidbody2D rb;

    // 当前移动方向
    private Vector2 moveDir;

    // 用来标记是否已经做过“启动时目标初始化”
    // 防止以后你扩展代码时不小心重复初始化
    private bool hasInitTarget = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        InitTargetOnce();
    }

    private void Update()
    {
        // 如果当前没有目标，就停止移动
        // 注意：这里不再进行任何查找
        // 也就是说，Update 只负责“使用已有引用”，不负责“寻找引用”
        if (playerTarget == null)
        {
            moveDir = Vector2.zero;
            return;
        }

        // 计算敌人与玩家之间的距离
        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // 玩家超出探测范围，不追
        if (distance > detectRange)
        {
            moveDir = Vector2.zero;
            return;
        }

        // 已经足够靠近玩家，不继续往前硬顶
        if (distance <= stopDistance)
        {
            moveDir = Vector2.zero;
            return;
        }

        // 计算朝向玩家的单位方向
        Vector2 dir = (playerTarget.position - transform.position).normalized;
        moveDir = dir;
    }

    private void FixedUpdate()
    {
        // 用 Rigidbody2D 进行移动
        if (rb != null)
        {
            rb.velocity = moveDir * moveSpeed;
        }
    }

    /// <summary>
    /// 只在启动阶段初始化一次目标
    /// 优先级：
    /// 1. Inspector 手动拖拽的 playerTarget
    /// 2. 外部提前调用 SetTarget() 注入的目标
    /// 3. 如果都没有，再兜底用 Tag 查找一次
    /// </summary>
    private void InitTargetOnce()
    {
        if (hasInitTarget)
        {
            return;
        }

        hasInitTarget = true;

        // 如果已经有目标了，就不再查找
        if (playerTarget != null)
        {
            return;
        }

        // 兜底方案：只在 Start 里查一次
        // 不放到 Update 里反复查
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogWarning(gameObject.name + " 没有找到 Player 目标，请检查：");
        }
    }

    /// <summary>
    /// 供外部脚本设置目标
    /// 
    /// 以后做刷怪系统时，在生成敌人的那一刻直接调用：
    /// enemy.SetTarget(player.transform);
    /// 
    /// 这样敌人就完全不需要自己查找玩家。
    /// </summary>
    /// <param name="target">要追击的目标</param>
    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    /// <summary>
    /// 提供给外部读取当前目标
    /// 方便后面调试、扩展和别的系统读取
    /// </summary>
    public Transform GetTarget()
    {
        return playerTarget;
    }

    /// <summary>
    /// 返回当前是否有目标
    /// </summary>
    public bool HasTarget()
    {
        return playerTarget != null;
    }

    /// <summary>
    /// 提供给外部动态清空目标
    /// 例如以后玩家死亡、切场景、重新生成时可以调用
    /// </summary>
    public void ClearTarget()
    {
        playerTarget = null;
        moveDir = Vector2.zero;
    }
}