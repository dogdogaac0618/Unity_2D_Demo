using TMPro;
using UnityEngine;

/// <summary>
/// RoomDoor
/// 作用：
/// 1. 控制门自己的显示 / 隐藏。
/// 2. 控制门自己的触发开关。
/// 3. 玩家进入后执行传送。
/// 4. 按需要移动主相机。
/// 5. 通过 targetRoomId + RoomManager 激活目标房间。
///
/// 本次修改重点：
/// - 彻底移除 nextRoomPoint 旧方案。
/// - 门的传送落点现在必须通过 RoomManager 按 targetRoomId 查询。
///
/// 为什么现在可以这样收口：
/// - 前一步你已经验证过：即使把 RoomExit_Test 的 Next Room Point 清空，
///   从 Room_01 进入 Room_02 仍然正常。
/// - 说明当前双房间原型已经不再依赖门自己手拖 nextRoomPoint。
/// - 所以这一步就把旧兜底正式删除，避免后续继续保留重复配置入口。
///
/// 当前边界：
/// - 相机仍然继续使用门上的手动 XY 坐标字段。
/// - SecondRoomExit_Test 目前还没有正式接到第三房间，所以 targetRoomId 允许为空。
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

    private void Reset()
    {
        exitTrigger = GetComponent<BoxCollider2D>();
        exitRenderer = GetComponent<SpriteRenderer>();
    }

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
        if (!isUnlocked)
        {
            return;
        }

        if (isUsed)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        isUsed = true;

        TeleportPlayer(other);
        MoveMainCameraIfNeeded();
        ActivateTargetRoomIfNeeded();
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (exitTrigger == null)
        {
            exitTrigger = GetComponent<BoxCollider2D>();
        }

        if (exitRenderer == null)
        {
            exitRenderer = GetComponent<SpriteRenderer>();
        }
    }
#endif
}