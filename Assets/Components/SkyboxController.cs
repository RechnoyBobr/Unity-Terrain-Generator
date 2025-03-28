using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SkyboxController : MonoBehaviour
{
    [Header("Skybox Settings")]
    public Color skyColor = new Color(0.5f, 0.7f, 1f);
    public Color horizonColor = new Color(0.8f, 0.8f, 0.8f);
    public Color groundColor = new Color(0.2f, 0.2f, 0.2f);
    public float skyIntensity = 1f;
    public float groundIntensity = 1f;
    public float exposure = 1f;
    public float rotation = 0f;
    public float updateSpeed = 0.1f;

    private Material skyboxMaterial;
    private float currentRotation = 0f;

    void Start()
    {
        skyboxMaterial = new Material(Shader.Find("Skybox/Procedural"));
        RenderSettings.skybox = skyboxMaterial;
        

        UpdateSkybox();
    }

    void Update()
    {
        // Плавно обновляем вращение
        currentRotation = Mathf.Lerp(currentRotation, rotation, Time.deltaTime * updateSpeed);
        skyboxMaterial.SetFloat("_Rotation", currentRotation);
    }

    void UpdateSkybox()
    {
        skyboxMaterial.SetColor("_SkyTint", skyColor);
        skyboxMaterial.SetColor("_GroundColor", groundColor);
        skyboxMaterial.SetFloat("_Exposure", exposure);
        skyboxMaterial.SetFloat("_SkyIntensity", skyIntensity);
        skyboxMaterial.SetFloat("_GroundIntensity", groundIntensity);
        skyboxMaterial.SetFloat("_Rotation", rotation);
    }

    void OnValidate()
    {
        if (skyboxMaterial != null)
        {
            UpdateSkybox();
        }
    }

    void OnDestroy()
    {
        // Очищаем материал при уничтожении
        if (skyboxMaterial != null)
        {
            Destroy(skyboxMaterial);
        }
    }
} 