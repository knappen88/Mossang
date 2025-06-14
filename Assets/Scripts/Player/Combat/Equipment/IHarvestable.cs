using UnityEngine;

public interface IHarvestable
{
    /// <summary>
    /// Попытка собрать ресурс с помощью инструмента
    /// </summary>
    void Harvest(ToolData tool, GameObject harvester);

    /// <summary>
    /// Проверка, можно ли собрать этот ресурс данным инструментом
    /// </summary>
    bool CanBeHarvestedWith(ToolData tool);

    /// <summary>
    /// Получить тип ресурса
    /// </summary>
    ResourceType GetResourceType();

    /// <summary>
    /// Проверка, уничтожен ли ресурс
    /// </summary>
    bool IsDestroyed { get; }
}