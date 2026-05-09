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
/// </summary>
public class RoomDoor : MonoBehaviour
{
    [Header("可选：门的反馈文字")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("门自身组件")]
    [SerializeField] private BoxCollider2D exitTrigger;
    [SerializeField] private SpriteRenderer exitRenderer;

    [Header("目标房间 ID（由 RoomManager 查询目标房间与目标落点）")]
    [SerializeField] private string targetRoomId;

    [Header("是否在使用门后移动主相机")]
    [SerializeField] private bool moveMainCameraOnUse = false;

    [Header("下一房间相机 XY 位置（当前继续保留手动方案）")]
    [SerializeField] private Vector2 nextRoomCameraPosition;

    /// <summary>
    /// 当前门是否解锁。
    /// RoomController 会通过 SetDoorUnlocked() 控制它。
    /// </summary>
    private bool isUnlocked = false;

    /// <summary>
    /// 防止玩家一次穿门时重复触发多次。
    /// </summary>
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
    }

    /// <summary>
    /// 供 RoomController 调用：
    /// true  = 门解锁并显示
    /// false = 门锁住并隐藏
    /// </summary>
    public void SetDoorUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;

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
        

        isUsed = true;

        RoomTransitionService.Instance.TryTransition(other, 
            targetRoomId, moveMainCameraOnUse, nextRoomCameraPosition, name);
    }

    /// <summary>
    /// 传送玩家到目标点。
    /// 当前正式方案：
    /// - 必须通过 RoomManager 按 targetRoomId 查询目标房间入口点。
    /// - 不再使用门自己手拖的 nextRoomPoint。
    ///
    /// 这样做的目的：
    /// - 彻底消除“门 -> 玩家落点”的旧手动耦合。
    /// - 让房间入口点的维护统一收口到 RoomManager。
    /// </summary>
    private void TeleportPlayer(Collider2D playerCollider)
    {
        Transform targetPoint = ResolveTargetEntryPoint();

        if (targetPoint == null)
        {
            Debug.LogWarning($"{name}：没有可用的目标落点，无法传送玩家。");
            return;
        }

        Rigidbody2D playerRb = playerCollider.attachedRigidbody;

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.position = targetPoint.position;
        }
        else
        {
            playerCollider.transform.position = targetPoint.position;
        }
    }

    /// <summary>
    /// 解析这扇门真正应该把玩家送到哪里。
    /// 当前正式方案只认 RoomManager。
    /// </summary>
    private Transform ResolveTargetEntryPoint()
    {
        if (string.IsNullOrWhiteSpace(targetRoomId))
        {
            return null;
        }

        if (RoomManager.Instance == null)
        {
            Debug.LogWarning($"{name}：场景中没有 RoomManager，无法查询目标房间入口点。");
            return null;
        }

        if (RoomManager.Instance.TryGetRoomEntryPoint(targetRoomId, out Transform entryPoint))
        {
            return entryPoint;
        }

        Debug.LogWarning($"{name}：RoomManager 中找不到目标房间 [{targetRoomId}] 的入口点配置。");
        return null;
    }

    /// <summary>
    /// 当前项目的相机方案仍是门里配置一个下一房间相机 XY。
    /// 这一步不动它。
    /// </summary>
    private void MoveMainCameraIfNeeded()
    {
        if (!moveMainCameraOnUse)
        {
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogWarning($"{name}：场景里找不到 Main Camera。");
            return;
        }

        Vector3 oldPos = Camera.main.transform.position;

        Camera.main.transform.position = new Vector3(
            nextRoomCameraPosition.x,
            nextRoomCameraPosition.y,
            oldPos.z
        );
    }

    /// <summary>
    /// 通过 RoomManager 按房间 ID 激活目标房间。
    /// </summary>
    private void ActivateTargetRoomIfNeeded()
    {
        // SecondRoomExit_Test 当前仍可能没有正式目标房间，这里允许为空
        if (string.IsNullOrWhiteSpace(targetRoomId))
        {
            return;
        }

        if (RoomManager.Instance == null)
        {
            Debug.LogWarning($"{name}：场景中没有 RoomManager，无法按房间 ID 激活目标房间。");
            return;
        }

        bool success = RoomManager.Instance.ActivateRoomById(targetRoomId);

        if (!success)
        {
            Debug.LogWarning($"{name}：通过 RoomManager 激活目标房间失败，目标房间 ID = {targetRoomId}");
        }
    }

    /// <summary>
    /// 判断这次碰撞是否允许使用门
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
        if(other == null)
        {
            return false;
        }
        if (other.CompareTag("Player"))
        {
            return true;
        }
        return false;
    }
}