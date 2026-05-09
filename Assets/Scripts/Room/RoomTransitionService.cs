using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 房间切换服务
///
/// 当前职责：
/// 1. 根据目标房间 ID，向 RoomManager 查询目标入口点
/// 2. 把玩家传送到目标入口点
/// 3. 按需要移动主相机
/// 4. 激活目标房间
/// </summary>
public class RoomTransitionService : MonoBehaviour
{
    public static RoomTransitionService Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 尝试执行房间切换
    /// </summary>
    /// <param name="playerCollider">玩家进入门触发器时的碰撞体</param>
    /// <param name="targetRoomId">目标房间 ID</param>
    /// <param name="moveMainCameraOnUse">是否移动主相机</param>
    /// <param name="nextRoomCameraPosition">目标相机 XY 坐标</param>
    /// <param name="sourceDoorName">是哪一扇门发起的切换，用于 Debug</param>
    /// <returns>切换是否成功</returns>
    public bool TryTransition(Collider2D playerCollider, 
        string targetRoomId,bool moveMainCameraOnUse, 
        Vector2 nextRoomCameraPosition, 
        string sourceDoorName)
    {
        //获取传送点
        Transform targetPosition = GetTargetPosition(targetRoomId);
        //传送玩家
        TeleportPlayer(playerCollider, targetPosition);
        //移动主相机
        if(moveMainCameraOnUse)
        {
           CameraController.Instance.MoveCameraToPosition(nextRoomCameraPosition);
        }
        //激活目标房间
        ActivateTargetRoom(targetRoomId);

        return true;
    }

    /// <summary>
    /// 通过RoomManger查询目标点
    /// </summary>
    private Transform GetTargetPosition(string targetRoomId)
    {
        if(RoomManager.Instance.TryGetRoomEntryPoint (targetRoomId, out Transform targetPosition))
        {
            return targetPosition;
        }
        else
        {
            Debug.LogWarning($"RoomTransitionService：无法找到目标房间 {targetRoomId} 的入口点，无法切换。");
            return null;
        }
    }

    /// <summary>
    /// 传送玩家
    /// </summary>
    private void TeleportPlayer(Collider2D playerCollider, Transform targetPosition)
    {
        Rigidbody2D playerRigidbody = playerCollider.GetComponent<Rigidbody2D>();
        //速度清零 加速度清零
        playerRigidbody.velocity = Vector2.zero;
        playerRigidbody.angularVelocity = 0f;
        //传送玩家到目标位置
        playerRigidbody.position = targetPosition.position;
    }
    ///<summary>
    ///激活目标房间
    ///</summary>
    private void ActivateTargetRoom(string targetRoomId)
    {
        RoomManager.Instance.ActivateRoomById(targetRoomId);
    }
}
