using UnityEngine;

/// <summary>
/// ͨ���˺�����
/// �κν���������򡢲���ʵ���� IDamageable �ӿڵĶ��󣬶����ܵ��˺�
/// �Ժ�����ű�������������
/// - ����
/// - ����
/// - �ҽ�
/// - Boss ���ܷ�Χ
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Header("�˺�������")]
    public int damage = 2;   // �����˺���ʱ�ܵ����˺�ֵ

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("DamageZone �������Ķ����ǣ�" + other.name);

        // ���Դ������Ķ������ϻ�ȡ�������˽ӿڡ�
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable == null)
        {
            return;
        }

        // ����������ʵ���� IDamageable���Ͷ�������˺�
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}