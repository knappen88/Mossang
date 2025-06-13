using UnityEngine;

public class AdvancedPixelPerfectCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Pixel Perfect")]
    public int pixelsPerUnit = 16; // Изменено на 16 для ваших тайлов

    [Header("Camera View")]
    public int targetPixelHeight = 180; // Сколько пикселей видно по высоте
    public bool autoAdjustToScreen = false; // Новая опция

    [Header("Camera Bounds")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Follow Behavior")]
    public Vector3 offset = new Vector3(0, 0, -10);
    public bool lockX = false;
    public bool lockY = false;

    [Header("Smoothing")]
    public bool useSmoothing = false;
    [Range(0.01f, 1f)]
    public float smoothTime = 0.1f;

    [Header("Shake")]
    public bool enableShake = false;
    public float shakeDuration = 0f;
    public float shakeIntensity = 0.1f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private Camera cam;
    private Vector3 velocity;
    private float shakeTimer;
    private Vector3 originalOffset;
    private int lastScreenHeight;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalOffset = offset;
        lastScreenHeight = Screen.height;

        SetupCamera();
    }

    void SetupCamera()
    {
        // Убеждаемся, что камера ортографическая
        cam.orthographic = true;

        // Вычисляем размер камеры
        float orthographicSize;

        if (autoAdjustToScreen)
        {
            // Старый способ - подстраивается под экран
            orthographicSize = Screen.height / (pixelsPerUnit * 2f);
        }
        else
        {
            // Новый способ - фиксированное количество видимых пикселей
            orthographicSize = targetPixelHeight / (pixelsPerUnit * 2f);
        }

        cam.orthographicSize = orthographicSize;

        if (showDebugInfo)
        {
            Debug.Log($"Camera Setup - Orthographic Size: {orthographicSize}");
            Debug.Log($"Visible tiles: {orthographicSize * 2} высота x {orthographicSize * 2 * cam.aspect} ширина");
            Debug.Log($"Visible pixels: {targetPixelHeight} высота x {targetPixelHeight * cam.aspect} ширина");
        }
    }

    void Update()
    {
        // Проверяем изменение разрешения экрана
        if (autoAdjustToScreen && Screen.height != lastScreenHeight)
        {
            lastScreenHeight = Screen.height;
            SetupCamera();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Базовая позиция цели
        Vector3 targetPosition = target.position + offset;

        // Применение блокировок осей
        if (lockX) targetPosition.x = transform.position.x;
        if (lockY) targetPosition.y = transform.position.y;

        // Применение границ
        if (useBounds)
        {
            // Учитываем размер видимой области при ограничении
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
        }

        // Округление до пикселей
        targetPosition = PixelPerfectClamp(targetPosition);

        // Применение сглаживания или прямое следование
        if (useSmoothing)
        {
            Vector3 smoothedPos = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
            transform.position = PixelPerfectClamp(smoothedPos);
        }
        else
        {
            transform.position = targetPosition;
        }

        // Применение тряски
        if (enableShake && shakeTimer > 0)
        {
            ApplyCameraShake();
        }
    }

    Vector3 PixelPerfectClamp(Vector3 position)
    {
        float pixelSize = 1f / pixelsPerUnit;
        return new Vector3(
            Mathf.Round(position.x / pixelSize) * pixelSize,
            Mathf.Round(position.y / pixelSize) * pixelSize,
            position.z
        );
    }

    void ApplyCameraShake()
    {
        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0)
        {
            enableShake = false;
            offset = originalOffset;
            return;
        }

        float x = Random.Range(-1f, 1f) * shakeIntensity;
        float y = Random.Range(-1f, 1f) * shakeIntensity;

        // Округляем тряску до пикселей
        x = Mathf.Round(x * pixelsPerUnit) / pixelsPerUnit;
        y = Mathf.Round(y * pixelsPerUnit) / pixelsPerUnit;

        offset = originalOffset + new Vector3(x, y, 0);
    }

    public void TriggerShake(float duration, float intensity)
    {
        enableShake = true;
        shakeTimer = duration;
        shakeDuration = duration;
        shakeIntensity = intensity;
    }

    // Публичные методы для изменения размера камеры в рантайме
    public void SetTargetPixelHeight(int newHeight)
    {
        targetPixelHeight = newHeight;
        if (!autoAdjustToScreen)
        {
            SetupCamera();
        }
    }

    public void ZoomIn()
    {
        SetTargetPixelHeight(Mathf.Max(90, targetPixelHeight - 30));
    }

    public void ZoomOut()
    {
        SetTargetPixelHeight(Mathf.Min(360, targetPixelHeight + 30));
    }

    // Вспомогательные методы для отладки
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.red;
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);

        // Показываем видимую область камеры
        if (Application.isPlaying && cam != null)
        {
            Gizmos.color = Color.yellow;
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 camPos = transform.position;

            Vector3 camBottomLeft = new Vector3(camPos.x - halfWidth, camPos.y - halfHeight, 0);
            Vector3 camTopRight = new Vector3(camPos.x + halfWidth, camPos.y + halfHeight, 0);
            Vector3 camTopLeft = new Vector3(camPos.x - halfWidth, camPos.y + halfHeight, 0);
            Vector3 camBottomRight = new Vector3(camPos.x + halfWidth, camPos.y - halfHeight, 0);

            Gizmos.DrawLine(camBottomLeft, camTopLeft);
            Gizmos.DrawLine(camTopLeft, camTopRight);
            Gizmos.DrawLine(camTopRight, camBottomRight);
            Gizmos.DrawLine(camBottomRight, camBottomLeft);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        float y = 10;
        GUI.Label(new Rect(10, y, 300, 20), $"Orthographic Size: {cam.orthographicSize:F2}", style);
        y += 20;
        GUI.Label(new Rect(10, y, 300, 20), $"Visible Tiles: {cam.orthographicSize * 2:F1} x {cam.orthographicSize * 2 * cam.aspect:F1}", style);
        y += 20;
        GUI.Label(new Rect(10, y, 300, 20), $"Target Pixel Height: {targetPixelHeight}", style);
        y += 20;
        GUI.Label(new Rect(10, y, 300, 20), $"Screen: {Screen.width}x{Screen.height}", style);
    }
}