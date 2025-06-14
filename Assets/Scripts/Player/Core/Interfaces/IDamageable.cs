using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector3 damageSource);
    bool IsAlive { get; }
}