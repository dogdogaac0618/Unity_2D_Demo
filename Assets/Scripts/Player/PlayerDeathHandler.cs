using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家死亡处理脚本
/// 作用：
/// 1. 监听玩家 Health 的死亡事件
/// 2. 当玩家死亡时，切换到结果界面
/// 
/// 为什么不把这个逻辑写进 Health？
/// 因为 Health 是通用组件，怪物也会用
/// 如果把切场景写进 Health，怪物死了也会跳结算，那就错了
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// 玩家死亡时执行
    /// </summary>
    private void HandleDeath()
    {
        Debug.Log("玩家死亡，进入结算界面");

        SceneManager.LoadScene(SceneNames.ResultUI);
    }
}