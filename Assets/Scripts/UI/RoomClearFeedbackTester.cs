using TMPro;
using UnityEngine;

/// <summary>
/// 房间清空提示脚本
/// 
/// 当前职责：
/// 1. 监听 RoomController 的房间清空事件
/// 2. 当房间清空时，直接显示提示文字
/// 
/// 注意：
/// 这里不使用协程。
/// 提示显示后不会自动隐藏。
/// </summary>
public class RoomClearFeedbackTester : MonoBehaviour
{
    [Header("清房提示文字")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("提示内容设置")]
    [SerializeField] private string messageFormat = "{0} 已清空，房门已解锁";


    private void Awake()
    {
        // 如果没在 Inspector 手动拖引用，
        // 就默认尝试拿当前对象自己身上的 TMP_Text。
        if (feedbackText == null)
        {
            feedbackText = GetComponent<TMP_Text>();
        }

        HideFeedBack();

    }

    private void OnEnable()
    {
        // 监听“场上敌人已清空”的统一事件
        RoomController.OnAnyRoomCleared += HandleRoomCleared;
    }

    private void OnDisable()
    {
        // 解绑事件，避免对象禁用或切场景时留下脏订阅
        RoomController.OnAnyRoomCleared -= HandleRoomCleared;
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
    private void HandleRoomCleared(string roomId)
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("RoomClearFeedbackTester 没有拿到 TMP_Text，无法显示清场提示。");
            return;
        }

        // 把可见提示直接显示到屏幕上
        feedbackText.text = string.Format(messageFormat, roomId);

        // 保留一条日志，方便确认“UI 监听器确实被触发了”
        Debug.Log("清场反馈测试器已收到事件，并显示文字反馈。");
    }

    private void HideFeedBack()
    {
        feedbackText.gameObject.SetActive(false);
    }
}