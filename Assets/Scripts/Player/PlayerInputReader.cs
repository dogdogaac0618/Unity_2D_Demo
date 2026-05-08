using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家输入读取器
/// 
/// 当前职责：
/// 1. 统一读取玩家移动输入
/// 2. 统一读取攻击按键
/// 3. 统一读取冲刺按键
/// 
/// 为什么要单独拆出来：
/// PlayerController 不应该直接关心“按哪个键”。
/// PlayerAttack 也不应该直接关心“按哪个键”。
/// 它们只需要读取这里整理好的输入结果。
/// 
/// 以后如果你换成 Unity New Input System，
/// 主要改这个脚本即可，移动和攻击逻辑不用大改。
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    // 玩家移动输入，x 代表水平输入，y 代表垂直输入
    public Vector2 MoveInput { get; private set; }
    private float x;
    private float y;

    public bool DashPressed {  get; private set; }
    public bool AttackPressed { get; private set; }

    // Update is called once per frame
    void Update()
    {
        ReadMoveInput();
        ReadActionInput();
    }

    public void ReadMoveInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        MoveInput = new Vector2(x, y).normalized;
    }
    public void ReadActionInput()
    {
        DashPressed = Input.GetKeyDown(KeyCode.Space);
        AttackPressed = Input.GetKeyDown(KeyCode.J);
    }
}
