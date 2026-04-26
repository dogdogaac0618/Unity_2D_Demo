using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 通用血量组件
/// 作用：
/// 1. 管理最大生命、当前生命
/// 2. 提供通用的 TakeDamage 方法
/// 3. 提供可选的无敌时间
/// 4. 提供可选的受击闪烁反馈
/// 5. 血量归零时，触发死亡事件
/// 6.
///
/// 这个组件的设计目标是“玩家和怪物都能共用”
/// 所以这里不直接写“跳结算界面”这种玩家专属逻辑
/// 玩家专属逻辑会放到单独脚本里处理
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("基础血量设置")]
    public int maxHp = 5;                 // 最大生命值
    public int currentHp;                 // 当前生命值

    [Header("受击保护设置")]
    public bool useInvincible = false;    // 是否启用受击后短暂无敌
    public float invincibleTime = 1f;     // 无敌持续时间

    [Header("受击反馈设置")]
    public bool flashOnHit = true;        // 是否启用受击闪烁
    public float flashInterval = 0.1f;    // 闪烁间隔，越小闪得越快

    [Header("死亡设置")]
    public bool destroyOnDeath = false;   // 死亡后是否直接销毁对象
    public bool disableOnDeath = true;    // 死亡后是否先禁用对象

    // 当前是否已经死亡
    private bool isDead = false;

    // 当前是否处于无敌状态
    private bool isInvincible = false;

    // 用来做闪烁反馈
    private SpriteRenderer sr;

    // 记录角色原来的颜色，闪烁结束后要恢复
    private Color originalColor;

    /// <summary>
    /// 死亡事件
    /// 以后别的脚本可以订阅它
    /// 例如：
    /// - 玩家死亡后切结果界面
    /// - 怪物死亡后播放掉落或通知房间系统
    /// </summary>
    public event Action OnDeath;

    public event Action<int,int,int> OnHpChanged;

    private void Awake()
    {
        // 初始化当前血量
        currentHp = maxHp;

        OnHpChanged ?.Invoke(currentHp, maxHp,0);

        // 获取精灵渲染器
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    /// <summary>
    /// 对外提供的受伤方法
    /// 任何攻击系统只要拿到 IDamageable，就能调用这里
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        // 如果已经死亡，就不再处理
        if (isDead)
        {
            return;
        }

        // 如果正在无敌，也不扣血
        if (isInvincible)
        {
            return;
        }

        // 为了保险，伤害值如果 <= 0，就不处理
        if (damage <= 0)
        {
            return;
        }

        // 扣血
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log(gameObject.name + " 受到伤害，当前血量：" + currentHp);

        //更新血量事件
        OnHpChanged?.Invoke(currentHp, maxHp, damage);
        // 如果血量归零，直接死亡

        if (currentHp <= 0)
        {           
            Die();
            return;
        }
        
        // 如果启用了无敌或闪烁，就启动受击反馈协程
        if (useInvincible || flashOnHit)
        {
            StartCoroutine(HitFeedbackCoroutine());
        }
    }

    /// <summary>
    /// 受击反馈协程
    /// 这里同时处理两类东西：
    /// 1. 是否进入无敌状态
    /// 2. 是否闪烁
    /// 
    /// 这样玩家和怪物都能共用
    /// 玩家可以开无敌 + 闪烁
    /// 普通怪物可以只开闪烁，不开无敌
    /// </summary>
    private IEnumerator HitFeedbackCoroutine()
    {
        // 如果启用无敌，先进入无敌状态
        if (useInvincible)
        {
            isInvincible = true;
        }

        // 如果没启用闪烁，那只需要等无敌时间结束即可
        if (!flashOnHit)
        {
            yield return new WaitForSeconds(invincibleTime);
            isInvincible = false;
            yield break;
        }

        float timer = 0f;
        bool lowAlpha = false;

        // 这里决定总时长：
        // 如果启用了无敌，就按无敌时间闪
        // 如果没启用无敌，就给一个很短的默认闪烁时间
        float totalTime = useInvincible ? invincibleTime : 0.2f;

        while (timer < totalTime)
        {
            if (sr != null)
            {
                // 通过改变透明度制造闪烁效果
                if (lowAlpha)
                {
                    sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
                }
                else
                {
                    sr.color = originalColor;
                }

                lowAlpha = !lowAlpha;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        // 闪烁结束后恢复原来的颜色
        if (sr != null)
        {
            sr.color = originalColor;
        }

        // 无敌时间结束，关闭无敌状态
        isInvincible = false;
    }

    /// <summary>
    /// 死亡逻辑
    /// 这里只做“通用层”的事情
    /// 不在这里写玩家专属逻辑，比如切场景
    /// </summary>
    private void Die()
    {
        isDead = true;
        currentHp = 0;

        Debug.Log(gameObject.name + " 死亡");

        // 先抛出死亡事件，给外部系统处理
        OnDeath?.Invoke();

        // 是否禁用对象
        if (disableOnDeath)
        {
            gameObject.SetActive(false);
        }

        // 是否销毁对象
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 提供给外部读取的死亡状态
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}