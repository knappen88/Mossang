// ===== CUTE RPG COLOR GRADING SYSTEM =====
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CuteRPGColorGrading : MonoBehaviour
{
    [Header("🌈 Основные настройки")]
    [Range(-50f, 50f)] public float brightness = 10f;
    [Range(-50f, 100f)] public float contrast = 15f;
    [Range(-50f, 100f)] public float saturation = 20f;
    [Range(0f, 1f)] public float vibrance = 0.3f;

    [Header("🎨 Цветовые настройки")]
    public Color tintColor = new Color(1f, 0.98f, 0.95f);
    [Range(0f, 1f)] public float tintStrength = 0.1f;
    public Color shadowColor = new Color(0.8f, 0.7f, 0.9f);
    public Color highlightColor = new Color(1f, 1f, 0.9f);

    [Header("✨ Cute эффекты")]
    [Range(0f, 1f)] public float pastelStrength = 0.3f;
    [Range(0f, 1f)] public float bloomThreshold = 0.8f;
    public bool enableSparkles = true;

    [Header("🏞️ Пресеты локаций")]
    public LocationPreset currentLocation = LocationPreset.GreenMeadow;

    [Header("🕐 Время суток")]
    public bool enableDayNightCycle = true;
    [Range(0f, 24f)] public float currentHour = 12f;
    public AnimationCurve sunIntensityCurve;

    [Header("😊 Состояния персонажа")]
    public EmotionalState playerEmotion = EmotionalState.Happy;

    private Material colorGradingMaterial;
    private float emotionTransitionSpeed = 2f;
    private Color currentEmotionTint = Color.white;

    public enum LocationPreset
    {
        GreenMeadow,        // Зеленые луга
        CherryBlossom,      // Сакура
        CandyLand,          // Конфетная страна
        MushroomForest,     // Грибной лес
        CrystalCave,        // Кристальная пещера
        CloudKingdom,       // Облачное королевство
        BeachParadise,      // Пляжный рай
        AutumnForest,       // Осенний лес
        SnowVillage,        // Снежная деревня
        MagicalNight        // Волшебная ночь
    }

    public enum EmotionalState
    {
        Happy,      // 😊 Счастливый
        Excited,    // 🤩 Взволнованный
        Sleepy,     // 😴 Сонный
        Sad,        // 😢 Грустный
        Love,       // 💕 Влюбленный
        Hungry,     // 🤤 Голодный
        Confused,   // 😵 Запутанный
        Magical     // ✨ Под магией
    }

    void Start()
    {
        CreateMaterial();
        if (sunIntensityCurve == null || sunIntensityCurve.length == 0)
        {
            SetupDefaultDayNightCurve();
        }
    }

    void CreateMaterial()
    {
        Shader shader = Shader.Find("Hidden/CuteRPGColorGrading");
        if (shader != null && shader.isSupported)
        {
            colorGradingMaterial = new Material(shader);
        }
        else
        {
            Debug.LogError("Cute RPG Color Grading shader not found!");
        }
    }

    void SetupDefaultDayNightCurve()
    {
        sunIntensityCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f);
        sunIntensityCurve.AddKey(new Keyframe(6f, 0.3f));  // Рассвет
        sunIntensityCurve.AddKey(new Keyframe(12f, 1f));   // Полдень
        sunIntensityCurve.AddKey(new Keyframe(18f, 0.7f)); // Закат
        sunIntensityCurve.AddKey(new Keyframe(24f, 0.2f)); // Полночь
    }

    void Update()
    {
        // Обновление времени суток
        if (enableDayNightCycle && Application.isPlaying)
        {
            currentHour += Time.deltaTime * 0.5f; // 1 игровой час = 2 реальные секунды
            if (currentHour >= 24f) currentHour -= 24f;
        }

        // Плавная смена эмоций
        Color targetEmotionTint = GetEmotionColor(playerEmotion);
        currentEmotionTint = Color.Lerp(currentEmotionTint, targetEmotionTint, Time.deltaTime * emotionTransitionSpeed);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (colorGradingMaterial != null)
        {
            // Применяем пресет локации
            ApplyLocationPreset(currentLocation);

            // Время суток
            float dayNightIntensity = enableDayNightCycle ? sunIntensityCurve.Evaluate(currentHour) : 1f;
            Color timeOfDayTint = GetTimeOfDayColor(currentHour);

            // Передаем параметры в шейдер
            colorGradingMaterial.SetFloat("_Brightness", brightness / 100f);
            colorGradingMaterial.SetFloat("_Contrast", 1f + contrast / 100f);
            colorGradingMaterial.SetFloat("_Saturation", 1f + saturation / 100f);
            colorGradingMaterial.SetFloat("_Vibrance", vibrance);

            colorGradingMaterial.SetColor("_TintColor", tintColor);
            colorGradingMaterial.SetFloat("_TintStrength", tintStrength);
            colorGradingMaterial.SetColor("_ShadowColor", shadowColor);
            colorGradingMaterial.SetColor("_HighlightColor", highlightColor);

            colorGradingMaterial.SetFloat("_PastelStrength", pastelStrength);
            colorGradingMaterial.SetFloat("_BloomThreshold", bloomThreshold);

            // Комбинируем все тинты
            Color finalTint = tintColor * timeOfDayTint * currentEmotionTint;
            colorGradingMaterial.SetColor("_FinalTint", finalTint);
            colorGradingMaterial.SetFloat("_DayNightIntensity", dayNightIntensity);

            Graphics.Blit(source, destination, colorGradingMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void ApplyLocationPreset(LocationPreset preset)
    {
        switch (preset)
        {
            case LocationPreset.GreenMeadow:
                tintColor = new Color(0.95f, 1f, 0.9f);
                shadowColor = new Color(0.7f, 0.8f, 0.6f);
                highlightColor = new Color(1f, 1f, 0.85f);
                saturation = 25f;
                pastelStrength = 0.2f;
                break;

            case LocationPreset.CherryBlossom:
                tintColor = new Color(1f, 0.92f, 0.95f);
                shadowColor = new Color(0.9f, 0.7f, 0.8f);
                highlightColor = new Color(1f, 0.95f, 0.98f);
                saturation = 20f;
                pastelStrength = 0.4f;
                break;

            case LocationPreset.CandyLand:
                tintColor = new Color(1f, 0.95f, 0.9f);
                shadowColor = new Color(0.9f, 0.6f, 0.8f);
                highlightColor = new Color(1f, 1f, 0.8f);
                saturation = 40f;
                contrast = 20f;
                pastelStrength = 0.5f;
                break;

            case LocationPreset.MushroomForest:
                tintColor = new Color(0.95f, 0.98f, 0.9f);
                shadowColor = new Color(0.6f, 0.7f, 0.8f);
                highlightColor = new Color(1f, 0.95f, 0.85f);
                saturation = 15f;
                pastelStrength = 0.3f;
                break;

            case LocationPreset.CrystalCave:
                tintColor = new Color(0.9f, 0.95f, 1f);
                shadowColor = new Color(0.6f, 0.7f, 0.9f);
                highlightColor = new Color(0.95f, 1f, 1f);
                saturation = 10f;
                contrast = 25f;
                pastelStrength = 0.1f;
                break;

            case LocationPreset.CloudKingdom:
                tintColor = new Color(0.95f, 0.98f, 1f);
                shadowColor = new Color(0.8f, 0.85f, 0.95f);
                highlightColor = new Color(1f, 1f, 0.95f);
                brightness = 20f;
                saturation = 5f;
                pastelStrength = 0.6f;
                break;

            case LocationPreset.BeachParadise:
                tintColor = new Color(1f, 0.98f, 0.9f);
                shadowColor = new Color(0.7f, 0.85f, 0.9f);
                highlightColor = new Color(1f, 1f, 0.85f);
                saturation = 30f;
                brightness = 15f;
                pastelStrength = 0.2f;
                break;

            case LocationPreset.AutumnForest:
                tintColor = new Color(1f, 0.9f, 0.8f);
                shadowColor = new Color(0.8f, 0.6f, 0.5f);
                highlightColor = new Color(1f, 0.95f, 0.7f);
                saturation = 25f;
                pastelStrength = 0.15f;
                break;

            case LocationPreset.SnowVillage:
                tintColor = new Color(0.95f, 0.97f, 1f);
                shadowColor = new Color(0.7f, 0.8f, 0.9f);
                highlightColor = new Color(1f, 1f, 0.98f);
                brightness = 10f;
                saturation = -10f;
                pastelStrength = 0.3f;
                break;

            case LocationPreset.MagicalNight:
                tintColor = new Color(0.85f, 0.85f, 1f);
                shadowColor = new Color(0.5f, 0.4f, 0.7f);
                highlightColor = new Color(0.9f, 0.9f, 1f);
                brightness = -10f;
                contrast = 20f;
                saturation = 15f;
                pastelStrength = 0.2f;
                break;
        }
    }

    Color GetTimeOfDayColor(float hour)
    {
        if (hour < 6f) // Ночь
            return new Color(0.8f, 0.8f, 0.95f);
        else if (hour < 8f) // Рассвет
            return Color.Lerp(new Color(0.8f, 0.8f, 0.95f), new Color(1f, 0.9f, 0.8f), (hour - 6f) / 2f);
        else if (hour < 10f) // Утро
            return Color.Lerp(new Color(1f, 0.9f, 0.8f), Color.white, (hour - 8f) / 2f);
        else if (hour < 16f) // День
            return Color.white;
        else if (hour < 19f) // Закат
            return Color.Lerp(Color.white, new Color(1f, 0.8f, 0.7f), (hour - 16f) / 3f);
        else if (hour < 21f) // Сумерки
            return Color.Lerp(new Color(1f, 0.8f, 0.7f), new Color(0.8f, 0.8f, 0.95f), (hour - 19f) / 2f);
        else // Ночь
            return new Color(0.8f, 0.8f, 0.95f);
    }

    Color GetEmotionColor(EmotionalState emotion)
    {
        switch (emotion)
        {
            case EmotionalState.Happy:
                return new Color(1f, 1f, 0.9f); // Теплый желтоватый

            case EmotionalState.Excited:
                return new Color(1f, 0.95f, 0.9f); // Яркий оранжеватый

            case EmotionalState.Sleepy:
                return new Color(0.9f, 0.9f, 0.95f); // Приглушенный синеватый

            case EmotionalState.Sad:
                return new Color(0.85f, 0.85f, 0.9f); // Грустный голубой

            case EmotionalState.Love:
                return new Color(1f, 0.9f, 0.95f); // Розовый

            case EmotionalState.Hungry:
                return new Color(1f, 0.95f, 0.85f); // Теплый оранжевый

            case EmotionalState.Confused:
                return new Color(0.95f, 0.9f, 0.95f); // Легкий фиолетовый

            case EmotionalState.Magical:
                return new Color(0.9f, 0.95f, 1f); // Волшебный голубой

            default:
                return Color.white;
        }
    }

    // Публичные методы для игровой логики
    public void ChangeLocation(LocationPreset newLocation, float transitionTime = 1f)
    {
        StartCoroutine(TransitionToLocation(newLocation, transitionTime));
    }

    public void SetPlayerEmotion(EmotionalState newEmotion)
    {
        playerEmotion = newEmotion;
    }

    public void TriggerMagicalEffect(float duration = 2f)
    {
        StartCoroutine(MagicalColorBurst(duration));
    }

    IEnumerator TransitionToLocation(LocationPreset newLocation, float duration)
    {
        LocationPreset startLocation = currentLocation;

        // Сохраняем начальные значения
        float startBrightness = brightness;
        float startContrast = contrast;
        float startSaturation = saturation;
        float startPastel = pastelStrength;
        Color startTint = tintColor;
        Color startShadow = shadowColor;
        Color startHighlight = highlightColor;

        // Получаем целевые значения
        currentLocation = newLocation;
        ApplyLocationPreset(newLocation);

        float targetBrightness = brightness;
        float targetContrast = contrast;
        float targetSaturation = saturation;
        float targetPastel = pastelStrength;
        Color targetTint = tintColor;
        Color targetShadow = shadowColor;
        Color targetHighlight = highlightColor;

        // Возвращаем начальные значения для плавного перехода
        brightness = startBrightness;
        contrast = startContrast;
        saturation = startSaturation;
        pastelStrength = startPastel;
        tintColor = startTint;
        shadowColor = startShadow;
        highlightColor = startHighlight;

        // Плавный переход
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            brightness = Mathf.Lerp(startBrightness, targetBrightness, t);
            contrast = Mathf.Lerp(startContrast, targetContrast, t);
            saturation = Mathf.Lerp(startSaturation, targetSaturation, t);
            pastelStrength = Mathf.Lerp(startPastel, targetPastel, t);
            tintColor = Color.Lerp(startTint, targetTint, t);
            shadowColor = Color.Lerp(startShadow, targetShadow, t);
            highlightColor = Color.Lerp(startHighlight, targetHighlight, t);

            yield return null;
        }
    }

    IEnumerator MagicalColorBurst(float duration)
    {
        EmotionalState previousEmotion = playerEmotion;
        playerEmotion = EmotionalState.Magical;

        float originalSaturation = saturation;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Пульсирующая насыщенность
            saturation = originalSaturation + Mathf.Sin(t * Mathf.PI * 4) * 20f;

            // Радужный эффект
            float hue = Mathf.PingPong(t * 2, 1);
            tintColor = Color.HSVToRGB(hue, 0.3f, 1f);

            yield return null;
        }

        saturation = originalSaturation;
        playerEmotion = previousEmotion;
    }
}
