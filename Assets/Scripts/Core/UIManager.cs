using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("玩家血量组件")]
    [SerializeField] private Health playerHealth;

    [Header("玩家血条控制器")]
    [SerializeField] private PlayerHpController playerHpController;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        // 监听玩家血量变化事件
        playerHealth.OnHpChanged += UpdatePlayerHp;
    }

    private void OnDisable()
    {
        // 取消监听玩家血量变化事件
        playerHealth.OnHpChanged -= UpdatePlayerHp;
    }
    public void InitUI()
    {
        Debug.Log("UIManager Init");

    }
    /// <summary>
    /// 更新玩家的血条
    /// </summary>
    /// <param name="currentHp">当前的血量</param>
    /// <param name="maxHp">最大的血量</param>
    /// <param name="damage">受到的伤害</param>
    public void UpdatePlayerHp(int currentHp, int maxHp,int damage)
    {
        playerHpController.UpdateHp(currentHp, maxHp,damage);
    }
}
