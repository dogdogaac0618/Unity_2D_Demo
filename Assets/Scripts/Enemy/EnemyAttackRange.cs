using System.Collections;
using UnityEngine;

/// <summary>
/// 敌人攻击范围脚本
///
/// 1. 玩家进入攻击范围 -> 开始攻击循环
/// 2. 玩家离开攻击范围 -> 停止攻击循环
///
/// 这样做的好处：
/// - 逻辑更稳定
/// - 更容易排查问题
/// - 后面如果要加攻击前摇、攻击动画、攻击间隔，也更容易扩展
/// </summary>
public class EnemyAttackRange : MonoBehaviour
{
    [Header("伤害设置")]
    public int damage = 1;                 // 每次攻击造成的伤害
    public float damageCooldown = 1f;      // 两次攻击之间的间隔

    // 当前是否有玩家待在攻击范围内
    private bool playerInRange = false;

    // 当前范围内的玩家受伤接口缓存
    private IDamageable currentTarget;

    // 当前攻击循环协程
    private Coroutine attackCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只处理玩家
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 尝试获取玩家的受伤接口
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null)
        {
            //Debug.LogWarning("玩家进入攻击范围，但没有找到 IDamageable：" + other.name);
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

        //Debug.Log("玩家进入攻击范围，开始攻击循环");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 只处理玩家
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 玩家离开范围，停止攻击
        playerInRange = false;
        currentTarget = null;

        // 如果攻击循环存在，就停止它
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        //Debug.Log("玩家离开攻击范围，停止攻击循环");
    }

    /// <summary>
    /// 攻击循环协程
    /// 只要玩家还在范围里，就每隔一段时间打一次
    /// </summary>
    private IEnumerator AttackLoopCoroutine()
    {
        while (playerInRange)
        {
            // 双保险：确认目标还在
            if (currentTarget != null)
            {
                //Debug.Log("敌人成功造成一次伤害");
                currentTarget.TakeDamage(damage);
            }

            // 等待下一次攻击间隔
            yield return new WaitForSeconds(damageCooldown);
        }

        // 循环结束时，把协程引用清空
        attackCoroutine = null;
    }
}