using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景名称常量表
/// 
/// 作用：
/// 1. 统一管理项目里所有场景名字
/// 2. 避免在多个脚本里重复手写字符串
/// 3. 以后如果场景改名，只需要改这里一个地方
/// 
/// 注意：
/// 这里的名字必须和 Unity Build Settings 里的场景名字完全一致。
/// </summary>
public static class SceneNames
{
   public const string MainMenu = "MainMenu";
   public const string GameScene = "GameScene";
   public const string ResultUI = "ResultUI";
}
