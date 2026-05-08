using UnityEngine;

/// <summary>
/// 玩家攻击脚本
/// 
/// 当前职责：
/// 1. 根据 PlayerInputReader 的攻击输入触发攻击
/// 2. 根据 PlayerController 的最后移动方向决定发射方向
/// 3. 生成玩家子弹并初始化方向
/// 
/// 注意：
/// 这个脚本不直接读取键盘按键。
/// 攻击键由 PlayerInputReader 统一读取。
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public GameObject projectilePrefab;
    public float attackCooldown = 0.3f;
    public float spawnOffset = 0.6f;

    private PlayerInputReader inputReader;
    private PlayerController playerController;
    private Vector2 fireDir;
    private float attackTimer = 0f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        UpdateAttackCooldown();
        HandleAttackInput();
    }
    /// <summary>
    /// 更新冷却计时
    /// </summary>
    private void UpdateAttackCooldown()
    {
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        } 
    }
    /// <summary>
    /// 处理攻击输入
    /// </summary>
    private void HandleAttackInput()
    {
        if( !inputReader.AttackPressed)
        {
            return;
        }
        if( attackTimer > 0 )
        {
            return;
        }

        Fire();
        attackTimer = attackCooldown;
    }
    /// <summary>
    /// 发射子弹
    /// </summary>
    private void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("没有指定 projectilePrefab");
            return;
        }

        // 获取最后移动方向；如果没有移动过，默认向右
        fireDir = playerController.LastMoveDir;
        if (fireDir == Vector2.zero)
        {
            fireDir = Vector2.right;
        }

        // 生成位置稍微偏离玩家中心，避免一生成就撞自己
        Vector3 spawnPos = transform.position + (Vector3)(fireDir * spawnOffset);

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(fireDir);
        }
    }
}