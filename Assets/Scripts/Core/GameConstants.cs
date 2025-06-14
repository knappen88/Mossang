using UnityEngine;

/// <summary>
/// Глобальные константы игры для избежания магических чисел
/// </summary>
public static class GameConstants
{
    // Хотбар
    public const int DEFAULT_HOTBAR_SLOTS = 4;
    public const int MAX_HOTBAR_SLOTS = 10;

    // Инвентарь
    public const int DEFAULT_INVENTORY_SLOTS = 42;
    public const int MAX_STACK_SIZE = 99;

    // Анимации
    public const float DEFAULT_ANIMATION_DURATION = 0.2f;
    public const float UI_FADE_DURATION = 0.3f;
    public const float SLOT_ANIMATION_DELAY = 0.02f;

    // Боевая система
    public const float DEFAULT_ATTACK_RANGE = 1.5f;
    public const float DEFAULT_ATTACK_DURATION = 1f;
    public const int TEST_DAMAGE_AMOUNT = 10; // Для тестирования

    // UI
    public const float DOUBLE_CLICK_THRESHOLD = 0.25f;
    public const float HOVER_SCALE = 1.05f;
    public const float DRAG_ALPHA = 0.5f;

    // Эффекты
    public const float DAMAGE_FLASH_DURATION = 0.1f;
    public const float INVULNERABILITY_DURATION = 1.5f;
    public const float INVULNERABILITY_BLINK_INTERVAL = 0.1f;

    // Физика
    public const float ITEM_PICKUP_MAGNET_DISTANCE = 2f;
    public const float ITEM_PICKUP_MAGNET_SPEED = 5f;
    public const float ITEM_BOB_HEIGHT = 0.1f;
    public const float ITEM_BOB_SPEED = 2f;

    // Звук
    public const float DEFAULT_SOUND_VOLUME = 1f;
    public const float PICKUP_SOUND_VOLUME = 0.8f;
    public const float HURT_SOUND_VOLUME = 0.9f;

    // Слои
    public const string SORTING_LAYER_DEFAULT = "Default";
    public const string SORTING_LAYER_PLAYER = "Player";
    public const string SORTING_LAYER_UI = "UI";

    // Теги
    public const string TAG_PLAYER = "Player";
    public const string TAG_ENEMY = "Enemy";
    public const string TAG_ITEM = "Item";

    // Input
    public const KeyCode KEY_INVENTORY = KeyCode.I;
    public const KeyCode KEY_JUMP = KeyCode.Space;
    public const KeyCode KEY_ESCAPE = KeyCode.Escape;
    public const KeyCode KEY_TEST_DAMAGE = KeyCode.P;
}