using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelationEffect : MonoBehaviour
{
    [Header("Pixelation Settings")]
    [Range(1, 256)]
    public int pixelSize = 128;

    [Header("Advanced Settings")]
    public bool preserveAspectRatio = true;
    [Range(0f, 1f)]
    public float pixelationStrength = 1f;

    private Material pixelationMaterial;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        CreateMaterial();
    }

    void CreateMaterial()
    {
        // Создаем материал с нашим шейдером
        Shader pixelationShader = Shader.Find("Hidden/PixelationEffect");
        if (pixelationShader != null && pixelationShader.isSupported)
        {
            pixelationMaterial = new Material(pixelationShader);
        }
        else
        {
            Debug.LogError("Pixelation shader not found or not supported!");
            enabled = false;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (pixelationMaterial != null)
        {
            // Передаем параметры в шейдер
            pixelationMaterial.SetInt("_PixelSize", pixelSize);
            pixelationMaterial.SetFloat("_PixelationStrength", pixelationStrength);
            pixelationMaterial.SetFloat("_AspectRatio", cam.aspect);
            pixelationMaterial.SetInt("_PreserveAspect", preserveAspectRatio ? 1 : 0);

            // Применяем эффект
            Graphics.Blit(source, destination, pixelationMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void OnDestroy()
    {
        if (pixelationMaterial != null)
        {
            DestroyImmediate(pixelationMaterial);
        }
    }

    // Методы для управления в рантайме
    public void SetPixelSize(int size)
    {
        pixelSize = Mathf.Clamp(size, 1, 256);
    }

    public void IncreasePixelation()
    {
        SetPixelSize(pixelSize / 2);
    }

    public void DecreasePixelation()
    {
        SetPixelSize(pixelSize * 2);
    }
}