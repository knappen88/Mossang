using UnityEngine;
using Combat.Data;

/// <summary>
/// Специальный тип инструмента для атаки без оружия
/// </summary>
[CreateAssetMenu(menuName = "Items/Unarmed Tool", fileName = "UnarmedTool")]
public class UnarmedToolData : ToolData
{
    [Header("Unarmed Combat")]
    [SerializeField] private bool isDefaultUnarmed = true;

    [Header("Attack Animations - Body")]
    [SerializeField] private AnimationClip attackAnimationFront;
    [SerializeField] private AnimationClip attackAnimationSide;
    [SerializeField] private AnimationClip attackAnimationBack;

    [Header("Attack Animations - Arms")]
    [SerializeField] private AnimationClip armsAttackAnimationFront;
    [SerializeField] private AnimationClip armsAttackAnimationSide;
    [SerializeField] private AnimationClip armsAttackAnimationBack;

    [Header("Animation Settings")]
    [SerializeField] private float attackAnimationDuration = 0.5f;
    [SerializeField] private float attackHitTime = 0.2f; // Момент нанесения урона

    private void OnValidate()
    {
        // Устанавливаем базовые параметры для кулака
        if (Application.isEditor)
        {
            damagePerUse = 5;
            efficiency = 0.5f;
            gatherRadius = 1f;
        }

        // Базовые настройки для кулака
        if (gatherableResources == null || gatherableResources.Length == 0)
        {
            gatherableResources = new ResourceType[] { ResourceType.Wood, ResourceType.Plants };
        }
    }

    public override void Use(GameObject user)
    {
        // Unarmed нельзя "использовать" как обычный предмет
        // Он автоматически активен когда слот пуст
        Debug.Log("Unarmed is always equipped when slot is empty!");
    }

    /// <summary>
    /// Получить анимацию атаки для тела по направлению
    /// </summary>
    public AnimationClip GetBodyAttackAnimation(int direction)
    {
        switch (direction)
        {
            case 0: return attackAnimationFront;  // Front
            case 1: return attackAnimationBack;   // Back  
            case 2: return attackAnimationSide;   // Side
            default: return attackAnimationFront;
        }
    }

    /// <summary>
    /// Получить анимацию атаки для рук по направлению
    /// </summary>
    public AnimationClip GetArmsAttackAnimation(int direction)
    {
        switch (direction)
        {
            case 0: return armsAttackAnimationFront;  // Front
            case 1: return armsAttackAnimationBack;   // Back
            case 2: return armsAttackAnimationSide;   // Side
            default: return armsAttackAnimationFront;
        }
    }

    public float GetAttackDuration() => attackAnimationDuration;
    public float GetAttackHitTime() => attackHitTime;
}