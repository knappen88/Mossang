using UnityEngine;
using System.Text;

public class UIStructureChecker : MonoBehaviour
{
    [ContextMenu("Check UI Structure")]
    public void CheckStructure()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== UI STRUCTURE CHECK ===");

        // Находим InventoryToggleUI
        var inventoryToggle = FindObjectOfType<InventoryToggleUI>(true);
        if (inventoryToggle != null)
        {
            report.AppendLine($"\n[InventoryToggleUI] Найден на: {GetPath(inventoryToggle.transform)}");
            report.AppendLine($"  - Активен: {inventoryToggle.gameObject.activeInHierarchy}");

            // Получаем приватное поле через рефлексию
            var inventoryPanelField = inventoryToggle.GetType().GetField("inventoryPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (inventoryPanelField != null)
            {
                var inventoryPanel = inventoryPanelField.GetValue(inventoryToggle) as GameObject;
                if (inventoryPanel != null)
                {
                    report.AppendLine($"  - inventoryPanel указывает на: {GetPath(inventoryPanel.transform)}");
                    report.AppendLine($"  - inventoryPanel активен: {inventoryPanel.activeInHierarchy}");
                }
                else
                {
                    report.AppendLine("  - inventoryPanel: НЕ НАЗНАЧЕН!");
                }
            }
        }
        else
        {
            report.AppendLine("[InventoryToggleUI] НЕ НАЙДЕН!");
        }

        // Находим HotbarManager
        var hotbarManager = FindObjectOfType<HotbarManager>(true);
        if (hotbarManager != null)
        {
            report.AppendLine($"\n[HotbarManager] Найден на: {GetPath(hotbarManager.transform)}");
            report.AppendLine($"  - Активен: {hotbarManager.gameObject.activeInHierarchy}");
            report.AppendLine($"  - Родитель: {hotbarManager.transform.parent?.name ?? "нет"}");

            // Проверяем всех родителей
            Transform current = hotbarManager.transform;
            while (current.parent != null)
            {
                current = current.parent;
                if (!current.gameObject.activeSelf)
                {
                    report.AppendLine($"  - ПРОБЛЕМА: Родитель '{current.name}' НЕАКТИВЕН!");
                }
            }
        }
        else
        {
            report.AppendLine("[HotbarManager] НЕ НАЙДЕН!");
        }

        // Проверяем связь
        if (inventoryToggle != null && hotbarManager != null)
        {
            report.AppendLine("\n[АНАЛИЗ ПРОБЛЕМЫ]");

            // Проверяем, не являются ли они родственниками
            if (IsChildOf(hotbarManager.transform, inventoryToggle.transform))
            {
                report.AppendLine("  - ПРОБЛЕМА: HotbarManager находится внутри InventoryToggleUI!");
                report.AppendLine("  - РЕШЕНИЕ: Вынесите хотбар из иерархии инвентаря");
            }

            var inventoryPanelField = inventoryToggle.GetType().GetField("inventoryPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (inventoryPanelField != null)
            {
                var inventoryPanel = inventoryPanelField.GetValue(inventoryToggle) as GameObject;
                if (inventoryPanel != null && IsChildOf(hotbarManager.transform, inventoryPanel.transform))
                {
                    report.AppendLine("  - ПРОБЛЕМА: HotbarManager находится внутри inventoryPanel!");
                    report.AppendLine("  - Это объясняет почему хотбар скрывается!");
                }
            }
        }

        Debug.Log(report.ToString());
    }

    private string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    private bool IsChildOf(Transform child, Transform parent)
    {
        Transform current = child;
        while (current != null)
        {
            if (current == parent) return true;
            current = current.parent;
        }
        return false;
    }

    private void Start()
    {
        // Автоматически проверяем структуру при старте
        Invoke(nameof(CheckStructure), 0.1f);
    }
}