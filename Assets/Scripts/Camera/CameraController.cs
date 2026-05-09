using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相机控制器
///
/// 当前职责：
/// 1. 管理主相机
/// 2. 提供移动相机的方法
///
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    [Header("要控制的相机")]
    [SerializeField] private Camera targetCamera;

    void Awake()
    {
        if(Instance !=null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void MoveCameraToPosition(Vector3 newPosition)
    {
        targetCamera.transform.position = new Vector3(newPosition.x, newPosition.y, targetCamera.transform.position.z);
    }
}
