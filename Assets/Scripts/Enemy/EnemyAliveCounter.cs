using System;
using UnityEngine;

/// <summary>
/// 场上敌人数统计器（可重建版本）
/// 
/// 这一版只解决一个问题：
/// 不仅能在开场统计敌人数，
/// 还支持在“进入下一测试房间”后，重新扫描当前激活的敌人，
/// 把计数器切换到新房间那一批敌人上。

/// 当前职责：
/// 1. 监听任意敌人死亡事件
/// 2. 维护当前房间的活跃敌人数
/// 3. 当数量归零时，广播清场完成
/// 4. 支持外部在房间切换时手动重建当前敌人数
/// </summary>
public class EnemyAliveCounter : MonoBehaviour
{
    /// <summary>
    /// 当当前房间的敌人被清空时，对外广播一次
    /// </summary>
    public static event Action OnAllEnemiesCleared;

    /// <summary>
    /// 当前活着的敌人数
    /// </summary>
    private int aliveEnemyCount = 0;

    /// <summary>
    /// 防止同一轮房间里重复广播“清场完成”
    /// </summary>
    private bool hasRaisedAllEnemiesCleared = false;

    private void OnEnable()
    {
        EnemyDeathReporter.OnAnyEnemyDeath += HandleAnyEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyDeathReporter.OnAnyEnemyDeath -= HandleAnyEnemyDeath;
    }

    private void Start()
    {
        // 开场先按当前激活的敌人重建一次
        RebuildAliveEnemyCount();
    }

    /// <summary>
    /// 对外公开的方法：
    /// 重新扫描场景里“当前激活”的正式敌人，
    /// 并把计数器切换到这批敌人。
    /// 
    /// 什么时候调用：
    /// - 开场时
    /// - 传送进入下一测试房间后
    /// - 后续如果你做刷怪波次切换，也可以继续复用
    /// </summary>
    public void RebuildAliveEnemyCount()
    {
        EnemyDeathReporter[] allReporters = FindObjectsOfType<EnemyDeathReporter>();

        int count = 0;

        for (int i = 0; i < allReporters.Length; i++)
        {
            // 这里只统计“当前激活层级中真的在运行”的敌人
            // 这样被 SetActive(false) 的下一房间敌人不会被提前算进去
            if (allReporters[i].gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        aliveEnemyCount = count;
        hasRaisedAllEnemiesCleared = false;

        Debug.Log($"已重建当前房间敌人数：{aliveEnemyCount}");

        TryRaiseAllEnemiesCleared();
    }

    /// <summary>
    /// 任意敌人死亡时执行
    /// </summary>
    private void HandleAnyEnemyDeath(GameObject deadEnemy)
    {
        if (aliveEnemyCount > 0)
        {
            aliveEnemyCount--;
        }

        Debug.Log($"敌人死亡，剩余敌人数：{aliveEnemyCount}，死亡对象：{deadEnemy.name}");

        TryRaiseAllEnemiesCleared();
    }

    /// <summary>
    /// 尝试广播“当前房间已清空”
    /// </summary>
    private void TryRaiseAllEnemiesCleared()
    {
        if (hasRaisedAllEnemiesCleared)
        {
            return;
        }

        if (aliveEnemyCount == 0)
        {
            hasRaisedAllEnemiesCleared = true;

            Debug.Log("当前场上敌人已清空");
            Debug.Log("已广播清场完成事件");

            OnAllEnemiesCleared?.Invoke();
        }
    }
}