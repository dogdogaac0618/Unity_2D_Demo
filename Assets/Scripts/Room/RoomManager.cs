using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RoomManager
/// 作用：
/// 1. 统一维护“房间ID -> RoomController”的映射关系。
/// 2. 统一维护“房间ID -> 进入该房间的落点 Transform”的映射关系。
/// 3. 作为门脚本与房间脚本之间的中间层原型。
///
/// 为什么这一步要新增“房间入口点映射”：
/// - 当前文档里明确写到，门手动拖 nextRoomPoint / nextRoomController 的维护成本会随着房间变多而快速上升。
/// - 我们前几步已经把 nextRoomController 收口到 RoomManager 了。
/// - 现在要继续往前推进，就需要先让 RoomManager 能按房间 ID 找到进入该房间的落点。
///
/// 当前边界：
/// - 这一版只负责“查询房间入口点”，还不直接修改门脚本的传送逻辑。
/// - 相机点位暂时不收口到这里，文档里写的是“后续再决定是否统一到 RoomManager”。
/// </summary>
public class RoomManager : MonoBehaviour
{
    /// <summary>
    /// 简单单例入口，方便门脚本统一找到 RoomManager。
    /// </summary>
    public static RoomManager Instance { get; private set; }

    [System.Serializable]
    public class RoomEntry
    {
        [Header("房间唯一标识，建议与 RoomController 上的 Room Id 保持一致")]
        public string roomId;

        [Header("这个房间对应的 RoomController")]
        public RoomController roomController;
    }

    [System.Serializable]
    public class RoomSpawnPointEntry
    {
        [Header("目标房间 ID")]
        public string roomId;

        [Header("进入这个房间时，玩家应该落到哪个点")]
        public Transform entryPoint;
    }

    [Header("旧的房间注册表（当前仅作补充来源保留）")]
    [SerializeField] private RoomEntry[] rooms;

    [Header("房间入口点配置表（这一步新增）")]
    [SerializeField] private RoomSpawnPointEntry[] roomEntryPoints;

    [Header("当前激活中的房间ID（原型阶段先只做记录）")]
    [SerializeField] private string currentRoomId;

    /// <summary>
    /// 房间ID -> RoomController
    /// </summary>
    private Dictionary<string, RoomController> roomMap = new Dictionary<string, RoomController>();

    /// <summary>
    /// 房间ID -> 房间入口点
    /// </summary>
    private Dictionary<string, Transform> roomEntryPointMap = new Dictionary<string, Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在多个 RoomManager，后创建的这个将被销毁。");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildRoomMap();
        BuildRoomEntryPointMap();
    }

    /// <summary>
    /// 建立“房间ID -> RoomController”映射。
    /// 这一版优先从场景里自动收集所有 RoomController。
    /// </summary>
    private void BuildRoomMap()
    {
        roomMap.Clear();

        // 第一步：自动收集场景中的所有 RoomController
        RoomController[] allRoomControllers = FindObjectsOfType<RoomController>(true);

        for (int i = 0; i < allRoomControllers.Length; i++)
        {
            RoomController controller = allRoomControllers[i];

            if (controller == null)
            {
                continue;
            }

            string id = controller.GetRoomId();

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"RoomManager：发现一个 RoomController 没有有效的 Room Id，对象名 = {controller.name}");
                continue;
            }

            if (roomMap.ContainsKey(id))
            {
                Debug.LogWarning($"RoomManager：自动收集时发现重复的房间ID [{id}]，后出现的对象名 = {controller.name}，已跳过。");
                continue;
            }

            roomMap.Add(id, controller);
        }

        // 第二步：保留 Inspector 旧配置作为补充来源
        if (rooms == null || rooms.Length == 0)
        {
            return;
        }

        for (int i = 0; i < rooms.Length; i++)
        {
            RoomEntry entry = rooms[i];

            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.roomId))
            {
                continue;
            }

            if (entry.roomController == null)
            {
                continue;
            }

            if (roomMap.ContainsKey(entry.roomId))
            {
                continue;
            }

            roomMap.Add(entry.roomId, entry.roomController);
        }
    }

    /// <summary>
    /// 建立“房间ID -> 房间入口点”映射。
    /// 这一版先使用 Inspector 手动配置，
    /// 因为当前文档里只有 Room_02 的落点 NextRoomPoint_Test 是明确存在的。
    /// </summary>
    private void BuildRoomEntryPointMap()
    {
        roomEntryPointMap.Clear();

        if (roomEntryPoints == null || roomEntryPoints.Length == 0)
        {
            return;
        }

        for (int i = 0; i < roomEntryPoints.Length; i++)
        {
            RoomSpawnPointEntry entry = roomEntryPoints[i];

            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.roomId))
            {
                Debug.LogWarning($"RoomManager：第 {i} 个房间入口点条目的 roomId 为空，已跳过。");
                continue;
            }

            if (entry.entryPoint == null)
            {
                Debug.LogWarning($"RoomManager：房间 [{entry.roomId}] 没有配置入口点，已跳过。");
                continue;
            }

            if (roomEntryPointMap.ContainsKey(entry.roomId))
            {
                Debug.LogWarning($"RoomManager：发现重复的房间入口点配置 [{entry.roomId}]，后面的配置将被忽略。");
                continue;
            }

            roomEntryPointMap.Add(entry.roomId, entry.entryPoint);
        }
    }

    /// <summary>
    /// 尝试根据房间ID获取 RoomController。
    /// </summary>
    public bool TryGetRoomController(string roomId, out RoomController roomController)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            roomController = null;
            return false;
        }

        return roomMap.TryGetValue(roomId, out roomController);
    }

    /// <summary>
    /// 尝试根据房间ID获取“进入该房间的落点”。
    /// 这是为下一步让门不再直接拖 nextRoomPoint 做准备。
    /// </summary>
    public bool TryGetRoomEntryPoint(string roomId, out Transform entryPoint)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            entryPoint = null;
            return false;
        }

        return roomEntryPointMap.TryGetValue(roomId, out entryPoint);
    }

    /// <summary>
    /// 激活指定房间。
    /// 当前仍然只负责：
    /// 1. 根据 roomId 找到目标 RoomController
    /// 2. 调用现有的 ActivateRoom()
    /// </summary>
    public bool ActivateRoomById(string roomId)
    {
        if (!TryGetRoomController(roomId, out RoomController targetRoom))
        {
            Debug.LogWarning($"RoomManager：找不到房间ID [{roomId}] 对应的 RoomController。");
            return false;
        }

        currentRoomId = roomId;
        targetRoom.ActivateRoom();

        return true;
    }

    /// <summary>
    /// 手动设置当前房间ID。
    /// </summary>
    public void SetCurrentRoom(string roomId)
    {
        currentRoomId = roomId;
    }

    /// <summary>
    /// 给其他脚本读取当前房间ID用。
    /// </summary>
    public string GetCurrentRoomId()
    {
        return currentRoomId;
    }

    /// <summary>
    /// 方便调试：根据当前 Inspector 配置重建映射。
    /// </summary>
    [ContextMenu("Rebuild Room Data From Inspector")]
    public void RebuildRoomDataFromInspector()
    {
        BuildRoomMap();
        BuildRoomEntryPointMap();
        Debug.Log("RoomManager：已根据当前配置重新建立房间数据。");
    }
}