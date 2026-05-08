using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 通用血量组件
/// 
/// 当前职责：
/// 1. 管理最大生命值和当前生命值
/// 2. 提供 TakeDamage() 受伤方法
/// 3. 提供 Heal() 回血方法
/// 4. 提供 ResetHealth() 重置血量方法
/// 5. 处理可选的受击无敌和受击闪烁
/// 6. 血量归零时广播死亡事件
/// 
/// 注意：
/// Health 只负责“血量本身”。
/// 它不负责玩家死亡切场景。
/// 它不负责敌人死亡开门。
/// 它不负责掉落、分数、UI 页面跳转。
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("基础血量设置")]
    [SerializeField] private int maxHp = 5;                 // 最大生命值
    private int currentHp;                 // 当前生命值

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

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

    // 当前受击反馈协程
    // 为什么要保存它：
    // 如果短时间连续受击，可以先停止旧协程，再启动新协程，
    // 避免多个闪烁协程同时修改颜色。
    private Coroutine hitFeedbackCoroutine;

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

        ResetHealth();
    }

    /// <summary>
    /// 重置血量
    /// 
    /// 用途：
    /// - 游戏开始初始化
    /// - 重新开始游戏
    /// - 敌人对象池复用
    /// - 房间重新生成敌人
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        isInvincible = false;

        currentHp = maxHp;

        StopHitFeedback();
        RestoreOriginalColor();

        OnHpChanged?.Invoke(currentHp, maxHp, 0);
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

        StartHitFeedbackIfNeeded();
    }
    /// <summary>
    /// 回血方法
    /// 
    /// 用途：
    /// - 吃血包
    /// - 治疗技能
    /// - 房间奖励
    /// - 后续吸血效果
    /// </summary>
    /// <param name="amount">回血量</param>
    public void Heal(int amount)
    {
        if(isDead)
        {
            return;
        }
        if(amount <= 0)
        {
            return;
        }

        int oldHp = currentHp;

        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        
        int realHealedAmount = currentHp - oldHp;

        Debug.Log($"{gameObject.name} 恢复了{realHealedAmount}点生命值");

        OnHpChanged?.Invoke(currentHp,maxHp,-realHealedAmount);
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
    /// 

    /// <summary>
    /// 是否需要启动受击反馈
    /// </summary>
    private void StartHitFeedbackIfNeeded()
    {
        if(!useInvincible && !flashOnHit)
        {
            // 如果两种反馈都不需要，那就直接返回，不启动协程
            return;
        }

        StopHitFeedback();

        hitFeedbackCoroutine = StartCoroutine(HitFeedbackCoroutine());
    }

    /// <summary>
    /// 停止当前受击反馈协程
    /// </summary>
    private void StopHitFeedback()
    {
        if(hitFeedbackCoroutine !=null)
        {
            StopCoroutine(hitFeedbackCoroutine);
            hitFeedbackCoroutine = null;
        }
    }
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

        hitFeedbackCoroutine = null;
    }

    /// <summary>
    /// 死亡逻辑
    /// 这里只做“通用层”的事情
    /// 不在这里写玩家专属逻辑，比如切场景
    /// </summary>
    private void Die()
    {
        if(isDead)
        {
            return;
        }
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
    /// <summary>
    /// 恢复角色原始颜色
    /// </summary>
    private void RestoreOriginalColor()
    {
        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

}