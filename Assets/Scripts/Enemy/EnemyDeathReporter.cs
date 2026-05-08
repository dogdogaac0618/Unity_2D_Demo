using System;
using UnityEngine;

/// <summary>
/// 敌人死亡上报器
/// 
/// 当前职责：
/// 1. 监听当前敌人身上的 Health 死亡事件
/// 2. 敌人死亡时，向外广播“有一个敌人死亡了”
/// 
/// 注意：
/// 它不负责扣血。
/// 它不负责开门。
/// 它不负责统计剩余敌人数。
/// 
/// 扣血由 Health 负责。
/// 统计敌人数和开门由 RoomController 负责。
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyDeathReporter : MonoBehaviour 
{
    /// <summary>
    /// 任意敌人死亡时广播。
    /// 参数 GameObject：
    /// 传出去的是死亡的敌人对象。
    /// 
    /// RoomController 会监听这个事件，
    /// 然后判断死掉的敌人是不是属于自己的房间。
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
        hasReportedDeath = false;
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