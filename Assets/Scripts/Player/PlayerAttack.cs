using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public GameObject projectilePrefab;
    public float attackCooldown = 0.3f;
    public float spawnOffset = 0.6f;

    private PlayerController playerController;
    private Projectile projectile;
    private float attackTimer = 0f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 冷却计时
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 按 J 发射
        if (Input.GetKeyDown(KeyCode.J) && attackTimer <= 0f)
        {
            Fire();
            attackTimer = attackCooldown;
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("没有指定 projectilePrefab");
            return;
        }

        // 获取最后移动方向；如果没有移动过，默认向右
        Vector2 fireDir = playerController.LastMoveDir;
        if (fireDir == Vector2.zero)
        {
            fireDir = Vector2.right;
        }

        // 生成位置稍微偏离玩家中心，避免一生成就撞自己
        Vector3 spawnPos = transform.position + (Vector3)(fireDir * spawnOffset);

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(fireDir);
        }
    }
}