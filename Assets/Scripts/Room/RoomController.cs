using UnityEngine;
using System;

/// <summary>
/// 房间控制器（第一版原型）
/// 
/// 这一版只解决一个问题：
/// 把“这个房间有哪些敌人、这个房间有哪些门、清怪后开什么门”
/// 收回到房间自己身上管理。
/// 
/// 当前职责：
/// 1. 管理这个房间自己的敌人列表
/// 2. 管理这个房间自己的门列表
/// 3. 房间激活时：锁门、激活本房间敌人、统计敌人数
/// 4. 本房间敌人清空时：开门
/// </summary>
public class RoomController : MonoBehaviour
{
    /// <summary>
    /// 任意房间被清空时广播。
    /// 
    /// 参数 string：
    /// 传出去的是被清空的房间 ID。
    /// 
    /// 为什么用 static event：
    /// UI 提示系统不需要知道具体是哪一个 RoomController，
    /// 只需要知道“有房间被清空了”。
    /// </summary>
    public static event Action<string> OnAnyRoomCleared;
    /// <summary>
    /// 房间名字，用来打日志，方便你在控制台看清楚是谁清空了
    /// </summary>
    [SerializeField] private string roomId = "Room";

    /// <summary>
    /// 是否开场就激活这个房间
    /// 
    /// 例子：
    /// - 第一房间：true
    /// - 第二房间：false
    /// </summary>
    [SerializeField] private bool activateOnStart = false;

    /// <summary>
    /// 这个房间的敌人根对象列表
    /// 
    /// 注意：
    /// - 这里拖“敌人根对象”
    /// - 也就是挂了 Health / EnemyDeathReporter / 敌人行为脚本的那个对象
    /// - 不要拖 AttackRange 子物体
    /// </summary>
    [SerializeField] private GameObject[] roomEnemies;

    /// <summary>
    /// 这个房间清空后要解锁的门
    /// 
    /// 当前先直接复用 RoomDoor 作为“门对象脚本”。
    /// 后面如果你要把门再拆得更干净，可以再单独拆 Door 脚本。
    /// </summary>
    [SerializeField] private RoomDoor[] roomDoors;

    /// <summary>
    /// 当前房间还活着的敌人数
    /// </summary>
    private int aliveEnemyCount = 0;

    /// <summary>
    /// 当前房间是否已经被激活
    /// </summary>
    private bool isRoomActive = false;

    /// <summary>
    /// 当前房间是否已经清空
    /// </summary>
    private bool isRoomCleared = false;

    private void OnEnable()
    {
        // 监听“任意敌人死亡”事件
        EnemyDeathReporter.OnAnyEnemyDeath += HandleAnyEnemyDeath;
    }

    private void OnDisable()
    {
        // 解绑事件，避免切场景或对象禁用时留下脏订阅
        EnemyDeathReporter.OnAnyEnemyDeath -= HandleAnyEnemyDeath;
    }

    private void Start()
    {
        // 开场先把这个房间的门都锁住
        SetDoorsUnlocked(false);

        // 对于“不是开场房间”的房间，先确保它的敌人保持未激活
        // 这样第二房间、第三房间不会一进游戏就提前参与战斗。
        if (!activateOnStart)
        {
            SetRoomEnemiesActive(false);
            return;
        }

        // 第一房间这种“开场即激活”的房间，直接启动
        ActivateRoom();
    }

    /// <summary>
    /// 激活当前房间
    /// 
    /// 当前会做三件事：
    /// 1. 锁门
    /// 2. 激活本房间敌人
    /// 3. 重新统计本房间敌人数
    /// </summary>
    public void ActivateRoom()
    {
        // 已经激活且还没清空时，不重复激活
        if (isRoomActive && !isRoomCleared)
        {
            Debug.Log($"{roomId} 已经处于激活状态，无需重复 ActivateRoom。");
            return;
        }

        isRoomActive = true;
        isRoomCleared = false;

        SetDoorsUnlocked(false);
        SetRoomEnemiesActive(true);
        RebuildAliveEnemyCount();
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.SetCurrentRoom(roomId);
        }

        Debug.Log($"{roomId} 已激活，当前敌人数：{aliveEnemyCount}");

        // 保护：
        // 如果这个房间压根没有敌人，也允许它立刻视为清空
        if (aliveEnemyCount == 0)
        {
            ClearRoom();
        }
    }

    /// <summary>
    /// 任意敌人死亡后执行
    /// 
    /// 这里只处理“属于这个房间”的敌人。
    /// 别的房间敌人死亡，不应该影响本房间计数。
    /// </summary>
    private void HandleAnyEnemyDeath(GameObject deadEnemy)
    {
        if (!isRoomActive || isRoomCleared)
        {
            return;
        }

        if (!IsEnemyFromThisRoom(deadEnemy))
        {
            return;
        }

        if (aliveEnemyCount > 0)
        {
            aliveEnemyCount--;
        }

        Debug.Log($"{roomId} 敌人死亡，剩余敌人数：{aliveEnemyCount}，死亡对象：{deadEnemy.name}");

        if (aliveEnemyCount == 0)
        {
            ClearRoom();
        }
    }

    /// <summary>
    /// 判断某个死亡对象是否属于当前房间
    /// </summary>
    private bool IsEnemyFromThisRoom(GameObject target)
    {
        if (roomEnemies == null || roomEnemies.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < roomEnemies.Length; i++)
        {
            if (roomEnemies[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 重新统计当前房间中“还活着并且处于激活状态”的敌人数
    /// </summary>
    private void RebuildAliveEnemyCount()
    {
        int count = 0;

        if (roomEnemies != null)
        {
            for (int i = 0; i < roomEnemies.Length; i++)
            {
                GameObject enemy = roomEnemies[i];

                if (enemy == null)
                {
                    continue;
                }

                // 这里只统计当前真正处于激活层级中的敌人
                if (!enemy.activeInHierarchy)
                {
                    continue;
                }

                // 同时要求它确实是“正式敌人”，也就是挂了 EnemyDeathReporter
                EnemyDeathReporter reporter = enemy.GetComponent<EnemyDeathReporter>();

                if (reporter == null)
                {
                    Debug.LogWarning($"{roomId} 中对象 {enemy.name} 没有 EnemyDeathReporter，不会参与房间清怪统计。");
                    continue;
                }

                count++;
            }
        }

        aliveEnemyCount = count;
    }

    /// <summary>
    /// 当前房间清空
    /// 
    /// 当前只做一件核心事：
    /// 打开这个房间自己的门
    /// </summary>
    private void ClearRoom()
    {
        if (isRoomCleared)
        {
            return;
        }

        isRoomCleared = true;

        SetDoorsUnlocked(true);

        Debug.Log($"{roomId} 已清空，房门已解锁。");

        // 广播清房事件
        // UI 提示、音效、奖励系统以后都可以监听这个事件
        OnAnyRoomCleared?.Invoke(roomId);
    }

    /// <summary>
    /// 统一设置这个房间的敌人是否激活
    /// </summary>
    private void SetRoomEnemiesActive(bool active)
    {
        if (roomEnemies == null)
        {
            return;
        }

        for (int i = 0; i < roomEnemies.Length; i++)
        {
            if (roomEnemies[i] == null)
            {
                continue;
            }

            roomEnemies[i].SetActive(active);
        }
    }

    /// <summary>
    /// 统一设置这个房间的门是否解锁
    /// 
    /// 当前门对象继续用 RoomDoor 承担“显示 + 触发器开关”。
    /// </summary>
    private void SetDoorsUnlocked(bool unlocked)
    {
        if (roomDoors == null)
        {
            return;
        }

        for (int i = 0; i < roomDoors.Length; i++)
        {
            if (roomDoors[i] == null)
            {
                continue;
            }

            roomDoors[i].SetDoorUnlocked(unlocked);
        }
    }
    /// <summary>
    /// 对外提供当前房间的 Room Id。
    /// 为什么现在要加这个：
    /// - 当前 RoomManager 想要往“自动收集房间”方向推进，
    ///   但它需要一个稳定的方式读取每个 RoomController 的房间标识。
    /// - 不直接暴露字段，而是通过方法读取，后续更容易维护。
    /// </summary>
    public string GetRoomId()
    {
        return roomId;
    }
}