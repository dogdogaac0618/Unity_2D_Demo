using System.Collections;
using UnityEngine;

/// <summary>
/// 敌人攻击范围脚本
///
/// 当前职责：
/// 1. 玩家进入攻击范围 -> 开始攻击循环
/// 2. 玩家离开攻击范围 -> 停止攻击循环
/// 3. 攻击范围被禁用时 -> 自动清理攻击状态
///
/// 为什么要这样写：
/// Unity 不允许 inactive 的 GameObject 启动 Coroutine。
/// 所以在 StartCoroutine 前必须确认当前脚本和 GameObject 是激活状态。
/// </summary>
public class EnemyAttackRange : MonoBehaviour
{
    [Header("伤害设置")]
    [SerializeField] private int damage = 1;              // 每次攻击造成的伤害
    [SerializeField] private float damageCooldown = 1f;   // 两次攻击之间的间隔

    // 当前是否有玩家待在攻击范围内
    private bool playerInRange = false;

    // 当前范围内的玩家受伤接口缓存
    private IDamageable currentTarget;

    // 当前攻击循环协程
    private Coroutine attackCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 保护 1：
        // 如果这个脚本或 GameObject 已经不是激活状态，就不要启动协程。
        // 这就是你现在报错的核心修复点。
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            return;
        }

        // 只处理玩家
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 尝试获取玩家的受伤接口
        // 用 GetComponentInParent 更稳：
        // 因为有时候碰撞体在 Player 子物体上，而 Health/IDamageable 在 Player 根物体上。
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            Debug.LogWarning("玩家进入攻击范围，但没有找到 IDamageable：" + other.name);
            return;
        }

        // 标记玩家已进入范围
        playerInRange = true;
        currentTarget = damageable;

        // 如果当前还没有攻击循环，就启动一个
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackLoopCoroutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 只处理玩家
        if (!other.CompareTag("Player"))
        {
            return;
        }

        StopAttack();
    }

    private void OnDisable()
    {
        // 重点：
        // 当敌人、房间、AttackRange 被 SetActive(false) 时，
        // Unity 不一定会正常帮你走 OnTriggerExit2D。
        // 所以这里必须手动清理攻击状态。
        StopAttack();
    }

    /// <summary>
    /// 停止攻击，并清理当前目标。
    /// </summary>
    private void StopAttack()
    {
        playerInRange = false;
        currentTarget = null;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    /// <summary>
    /// 攻击循环协程。
    /// 只要玩家还在范围里，就每隔一段时间打一次。
    /// </summary>
    private IEnumerator AttackLoopCoroutine()
    {
        while (isActiveAndEnabled && playerInRange && currentTarget != null)
        {
            currentTarget.TakeDamage(damage);

            yield return new WaitForSeconds(damageCooldown);
        }

        attackCoroutine = null;
    }
}