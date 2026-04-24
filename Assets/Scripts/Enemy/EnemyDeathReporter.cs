using System;
using UnityEngine;

/// <summary>
/// 敌人死亡上报器
/// 作用：
/// 1. 监听当前对象上的 Health 死亡事件
/// 2. 当敌人死亡时，向外统一发出“某个敌人死了”的消息

/// 后续可以怎么扩展：
/// - 房间系统统计剩余敌人数
/// - 刷怪系统判断一波怪是否清完
/// - 掉落系统在敌人死亡时生成掉落物
/// - UI 做击杀计数
/// </summary>
public class EnemyDeathReporter : MonoBehaviour
{
    /// <summary>
    /// 全局静态事件：
    /// 任何系统只要关心“敌人死亡了”，都可以订阅这个事件。
    /// 参数传出的是“死亡的这个敌人自己”。
    /// 
    ///
    /// - 直接传 GameObject 最通用，房间系统/刷怪系统/掉落系统都能先用
    /// - 后续如果需要更细的信息，再升级成传 EnemyInfo / EnemyType 都可以
    /// </summary>
    public static event Action<GameObject> OnAnyEnemyDeath;
  
    private Health health;

    /// 防止同一个敌人死亡时被重复上报
    private bool hasReportedDeath = false;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        // 只有拿到 Health，才去订阅死亡事件
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        // 解绑事件，避免对象复用或销毁时留下脏订阅
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// 当前敌人死亡时执行
    /// </summary>
    private void HandleDeath()
    {
        // 做一次保护，避免重复上报
        if (hasReportedDeath)
        {
            return;
        }

        hasReportedDeath = true;

        // 向外广播：有一个敌人死亡了，并把自己传出去
        OnAnyEnemyDeath?.Invoke(gameObject);

        Debug.Log($"敌人死亡已上报：{gameObject.name}");
    }
}