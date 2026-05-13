using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "ScriptableObjects/BulletData", order = 1)]
public class BulletData : ScriptableObject
{
    public float speed = 10f;       // 子弹飞行速度
    public float lifeTime = 2f;     // 子弹存在时间
    public int damage = 1;          // 子弹伤害值
}
