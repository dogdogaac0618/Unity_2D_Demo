/// <summary>
/// 通用受伤接口
/// 只要某个对象“可以受到伤害”，就实现这个接口
/// 以后玩家、怪物、Boss、可破坏物，都可以走这套接口
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    void TakeDamage(int damage);
}

