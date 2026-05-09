using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpController : MonoBehaviour
{
    [SerializeField] private Image HpBar; // 用于显示HP的Image组件数组
    [SerializeField] private Image DelayBar;
    [SerializeField] private int maxHpRealWidth = 300; // HP满血时的宽度
    [SerializeField] private float delayTime = 0.5f; // 延迟血条的更新延迟时间
    [SerializeField] private float reduceDuration = 0.35f; // 延迟血条减少动画时长

    private RectTransform rectHpBar;
    private RectTransform rectDelayBar;

    //受伤之前的血量宽度
    private float previousWidth;

    private Coroutine delayCoroutine;
    void Awake()
    {
        rectHpBar = HpBar.GetComponent<RectTransform>();
        rectDelayBar = DelayBar.GetComponent<RectTransform>();
        
    }
    //更新血量
    public void UpdateHp(int currentHp, int maxHp,int damge)
    {   
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        //记录受伤前的血量宽度，用于延迟血条的动画
        previousWidth = rectHpBar.sizeDelta.x;

        //更新血条Hp
        //计算血条宽度，当前血量占最大血量的比例乘以满血时的宽度
        float width = (float)currentHp / maxHp * maxHpRealWidth;
        rectHpBar.sizeDelta = new Vector2(width, rectHpBar.sizeDelta.y);
        
        //开始更新延迟血条的协程
        if( delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }
        delayCoroutine = StartCoroutine(UpdateDelayHpBar(previousWidth,currentHp,width));
    }
    //更新延迟血量
    IEnumerator UpdateDelayHpBar(float previousWidth,int currentHp,float width)
    {
        //计时器
        float t = 0f;
        float progress = 0f;
        yield return new WaitForSeconds(delayTime); // 等待0.5秒后更新延迟血量
        float targetWidth;
        while ( t< reduceDuration)
        {
            t += Time.deltaTime;
            progress = Mathf.Clamp01(t / reduceDuration); // 将t归一化到0-1之间
            // 线性插值计算当前宽度
            targetWidth = Mathf.Lerp(previousWidth, width, progress);
            rectDelayBar.sizeDelta = new Vector2(targetWidth, rectDelayBar.sizeDelta.y);
            yield return null;
        }
        
    }
}
