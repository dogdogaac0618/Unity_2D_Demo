using TMPro;
using UnityEngine;

/// <summary>
/// RoomDoor
///
/// 当前职责：
/// 1. 控制门自己的显示 / 隐藏
/// 2. 控制门自己的触发开关
/// 3. 检测玩家是否进入门
/// 4. 通知 RoomTransitionService 执行房间切换
///
/// 注意：
/// RoomDoor 不再负责传送玩家。
/// RoomDoor 不再负责移动相机。
/// RoomDoor 不再负责激活目标房间。
/// </summary>
public class RoomDoor : MonoBehaviour
{
    [Header("可选：门的反馈文字")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("门自身组件")]
    [SerializeField] private BoxCollider2D exitTrigger;
    [SerializeField] private SpriteRenderer exitRenderer;

    [Header("目标房间 ID，由 RoomManager 查询目标房间与目标落点")]
    [SerializeField] private string targetRoomId;

    [Header("是否在使用门后移动主相机")]
    [SerializeField] private bool moveMainCameraOnUse = false;

    [Header("下一房间相机 XY 位置")]
    [SerializeField] private Vector2 nextRoomCameraPosition;

    // 当前门是否解锁
    private bool isUnlocked = false;

    // 防止玩家一次穿门时重复触发多次
    private bool isUsed = false;

    private void Awake()
    {
        if (exitTrigger == null)
        {
            exitTrigger = GetComponent<BoxCollider2D>();
        }

        if (exitRenderer == null)
        {
            exitRenderer = GetComponent<SpriteRenderer>();
        }

        if (exitTrigger != null)
        {
            exitTrigger.isTrigger = true;
        }

        RefreshDoorState();
        HideFeedbackText();
    }

    /// <summary>
    /// 供 RoomController 调用。
    /// true = 门解锁并显示
    /// false = 门锁住并隐藏
    /// </summary>
    public void SetDoorUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;

        // 门重新锁住时，允许下次解锁后再次使用
        if (!isUnlocked)
        {
            isUsed = false;
        }

        RefreshDoorState();
    }

    /// <summary>
    /// 统一刷新门的显示和触发状态。
    /// </summary>
    private void RefreshDoorState()
    {
        if (exitRenderer != null)
        {
            exitRenderer.enabled = isUnlocked;
        }

        if (exitTrigger != null)
        {
            exitTrigger.enabled = isUnlocked;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanUseDoor(other))
        {
            return;
        }

        if (RoomTransitionService.Instance == null)
        {
            Debug.LogWarning($"{name}：场景中没有 RoomTransitionService，无法切换房间。");
            ShowFeedbackText("无法切换房间");
            return;
        }

        // 先标记为已使用，防止连续触发
        isUsed = true;

        bool success = RoomTransitionService.Instance.TryTransition(
            other,
            targetRoomId,
            moveMainCameraOnUse,
            nextRoomCameraPosition,
            name
        );

        // 如果切换失败，允许玩家重新触发这扇门
        if (!success)
        {
            isUsed = false;
            ShowFeedbackText("门暂时无法使用");
        }
        else
        {
            HideFeedbackText();
        }
    }

    /// <summary>
    /// 判断这次碰撞是否允许使用门。
    /// </summary>
    private bool CanUseDoor(Collider2D other)
    {
        if (!isUnlocked)
        {
            return false;
        }

        if (isUsed)
        {
            return false;
        }

        if (other == null)
        {
            return false;
        }

        return other.CompareTag("Player");
    }

    private void ShowFeedbackText(string message)
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
    }

    private void HideFeedbackText()
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.gameObject.SetActive(false);
    }
}