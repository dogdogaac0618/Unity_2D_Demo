using TMPro;
using UnityEngine;

/// <summary>
/// 清场反馈测试器
/// 
/// 这一版只解决一个问题：
/// 当 EnemyAliveCounter 广播“当前场上敌人已清空”时，
/// 有没有别的系统能真正接住这个事件，并在屏幕上给出可见反馈。
/// 
/// 为什么先做这个，而不是直接做开门/切房间：
/// - 现在我们还没正式进入 N-07 的房间主循环
/// - 如果现在直接把“清场 -> 开门”写死，后面房间系统成型时容易推翻重来
/// - 所以先用一个最小 UI 监听器验证事件链路是通的
/// 
/// 当前职责非常单纯：
/// 1. 监听 EnemyAliveCounter.OnAllEnemiesCleared
/// 2. 收到事件后，把屏幕上的 TMP 文本改成“房间已清空”
/// 
/// 后续可以怎么替换：
/// - 换成真正的房门开启提示
/// - 换成“下一波开始 / 房间已完成”的 UI
/// - 换成播放动画、音效、特效
/// </summary>
public class RoomClearFeedbackTester : MonoBehaviour
{
    /// <summary>
    /// 用来显示提示文字的 TMP 组件
    /// 
    /// 建议直接把这个脚本挂在 Text (TMP) 对象上，
    /// 这样如果 Inspector 没手动拖，也可以自动拿到自己身上的 TMP_Text。
    /// </summary>
    [SerializeField] private TMP_Text feedbackText;

    /// <summary>
    /// 清场后显示的文案
    /// 先单独留成字段，后面你要改成别的字，不用进代码里翻
    /// </summary>
    [SerializeField] private string clearedMessage = "clear";

    private void Awake()
    {
        // 如果没在 Inspector 手动拖引用，
        // 就默认尝试拿当前对象自己身上的 TMP_Text。
        if (feedbackText == null)
        {
            feedbackText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        // 监听“场上敌人已清空”的统一事件
        EnemyAliveCounter.OnAllEnemiesCleared += HandleAllEnemiesCleared;
    }

    private void OnDisable()
    {
        // 解绑事件，避免对象禁用或切场景时留下脏订阅
        EnemyAliveCounter.OnAllEnemiesCleared -= HandleAllEnemiesCleared;
    }

    private void Start()
    {
        // 开场先把文字清空，
        // 避免一运行就看到上一次测试残留的提示。
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    /// <summary>
    /// 当收到“清场完成”事件时执行
    /// </summary>
    private void HandleAllEnemiesCleared()
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("RoomClearFeedbackTester 没有拿到 TMP_Text，无法显示清场提示。");
            return;
        }

        // 把可见提示直接显示到屏幕上
        feedbackText.text = clearedMessage;

        // 保留一条日志，方便确认“UI 监听器确实被触发了”
        Debug.Log("清场反馈测试器已收到事件，并显示文字反馈。");
    }
}