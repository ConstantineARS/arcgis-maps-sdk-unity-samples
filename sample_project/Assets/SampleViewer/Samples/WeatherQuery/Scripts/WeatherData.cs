using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;
using static UnityEngine.Rendering.HighDefinition.CloudLayer;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class WeatherData : MonoBehaviour
{
    public string currentWeather;
    public string skyCondition;
    public int tempurature;
    public double longitude;
    public double latitude;

    [SerializeField] private ArcGISCameraComponent ArcGISCamera;
    [SerializeField] private ArcGISMapComponent ArcGISMap;
    private ArcGISLocationComponent cameraLocationComponent;
    [SerializeField] private MeshRenderer CloudLayer;
    [SerializeField] private Material CloudMat;
    [SerializeField] private Light directionalLight;
    [SerializeField] private Animation lightningAnim;
    private AudioSource lightningAudio;
    [SerializeField] private AudioClip[] lightningClips;
    private float lightningTimer;
    private ArcGISLocationComponent locationComponent;
    private bool thunderAndLightning;
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile volumeProfile;
    private VolumetricClouds vClouds;
    private WeatherQuery weatherQuery;

    [Header("WeatherVFX")]
    [SerializeField] private GameObject lightning;
    [SerializeField] private GameObject rain;
    [SerializeField] private GameObject snow;

    private void Awake()
    {
        ArcGISMap = FindObjectOfType<ArcGISMapComponent>();
        ArcGISCamera = FindObjectOfType<ArcGISCameraComponent>();
        cameraLocationComponent = ArcGISCamera.gameObject.GetComponent<ArcGISLocationComponent>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
        directionalLight = FindObjectOfType<Light>();
        weatherQuery = FindObjectOfType<WeatherQuery>();
        lightningAnim = directionalLight.GetComponent<Animation>();
        lightningAudio = lightning.GetComponent<AudioSource>();
        if (GraphicsSettings.renderPipelineAsset.ToString().Contains("Universal"))
        {
            CloudLayer = GameObject.Find("Cloud Layer").GetComponent<MeshRenderer>();
            CloudMat = CloudLayer.material;
        }
        else
        {
            volume = FindObjectOfType<Volume>();
            volumeProfile = volume.profile;
        }
    }

    public float ConvertTemp(float Temp, bool IsCelcius)
    {
        return IsCelcius ? Mathf.Round((Temp - 32) * 0.55555555555f) : Temp;
    }

    private void DisableWeather()
    {
        lightning.SetActive(false);
        thunderAndLightning = false;
        rain.SetActive(false);
        snow.SetActive(false);
    }

    private void FollowCamera()
    {
        locationComponent.Position = cameraLocationComponent.Position;
    }

    private void LateUpdate()
    {
        FollowCamera();
    }

    public void MoveCamera()
    {
        ArcGISMap.OriginPosition = new ArcGISPoint(longitude, latitude, 0, ArcGISSpatialReference.WGS84());
        cameraLocationComponent.Position = new ArcGISPoint(longitude, latitude, 3000.0f, ArcGISSpatialReference.WGS84());
    }

    public void MoveLightning()
    {
        var randomForward = Camera.main.transform.forward * Random.Range(10.0f, 50.0f);
        var randomUp = lightning.transform.up;
        var randomRight = Camera.main.transform.right * Random.Range(-10.0f, 10.0f);
        lightning.transform.localPosition = randomRight + randomUp + randomForward;
        var vfx = lightning.GetComponent<VisualEffect>();
        var eventAttribute = vfx.CreateVFXEventAttribute();
        vfx.SendEvent("Lightning", eventAttribute);
        lightningAnim.Play();
        lightningTimer = Random.Range(2.0f, 30.0f);
        lightningAudio.PlayOneShot(lightningClips[Random.Range(0, lightningClips.Length)]);
    }

    public void SetSky()
    {
        if (GraphicsSettings.renderPipelineAsset.ToString().Contains("Universal"))
        {


            string pipelineName = GraphicsSettings.renderPipelineAsset.GetType().ToString();
            if (GraphicsSettings.renderPipelineAsset.ToString().Contains("Universal"))
            {
                if (CloudMat != null)
                {
                    if (skyCondition.ToLower().Contains("overcast"))
                    {
                        CloudMat.SetFloat("_CloudSize", 40f);
                        CloudMat.SetFloat("_CloudDensity", 1f);
                        directionalLight.color = new Color(0.1803922f, 0.1803922f, 0.1803922f, 1.0f);
                    }
                    else if (skyCondition.ToLower().Contains("cloud"))
                    {
                        CloudMat.SetFloat("_CloudSize", 120f);
                        CloudMat.SetFloat("_CloudDensity", 5.4f);
                        directionalLight.color = new Color(1, 1, 1, 1);
                    }
                    else if (currentWeather.ToLower().Contains("thunder"))
                    {
                        CloudMat.SetFloat("_CloudSize", 19.87f);
                        CloudMat.SetFloat("_CloudDensity", 0.5f);
                        directionalLight.color = new Color(1, 1, 1, 1);
                    }
                    else
                    {
                        CloudMat.SetFloat("_CloudSize", 120f);
                        CloudMat.SetFloat("_CloudDensity", 4.3f);
                        directionalLight.color = new Color(1, 1, 1, 1);
                    }
                }
            }
        }
        else
        {


            if (volumeProfile.TryGet(out vClouds))
            {
                if (skyCondition.ToLower().Contains("overcast"))
                {
                    vClouds.cloudPreset = VolumetricClouds.CloudPresets.Overcast;
                    directionalLight.color = new Color(0.1803922f, 0.1803922f, 0.1803922f, 1.0f);
                }
                else if (skyCondition.ToLower().Contains("cloud"))
                {
                    vClouds.cloudPreset = VolumetricClouds.CloudPresets.Cloudy;
                    directionalLight.color = new Color(1, 1, 1, 1);
                }
                else if (currentWeather.ToLower().Contains("thunder"))
                {
                    vClouds.cloudPreset = VolumetricClouds.CloudPresets.Stormy;
                    directionalLight.color = new Color(1, 1, 1, 1);
                }
                else
                {
                    vClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
                    directionalLight.color = new Color(1, 1, 1, 1);
                }
            }
        }
    }

    public void SetWeather()
    {
        if (currentWeather.ToLower().Contains("thunder"))
        {
            DisableWeather();
            rain.SetActive(true);
            lightning.SetActive(true);
            thunderAndLightning = true;
        }
        else if (currentWeather.ToLower().Contains("snow"))
        {
            DisableWeather();
            snow.SetActive(true);
        }
        else if (currentWeather.ToLower().Contains("rain"))
        {
            DisableWeather();
            rain.SetActive(true);
        }
        else
        {
            vClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
            directionalLight.color = new Color(1, 1, 1, 1);
            DisableWeather();
        }
    }

    private void Start()
    {
        MoveCamera();

        weatherQuery.TempuratureToggle.onValueChanged.AddListener(delegate (bool value)
        {
            weatherQuery.TempText.text = ConvertTemp(tempurature, value).ToString() + "°";
        });
    }

    private void Update()
    {
        if (thunderAndLightning)
        {
            if (lightningTimer > 0)
            {
                lightningTimer -= Time.deltaTime;
            }
            else
            {
                MoveLightning();
            }
        }
    }
}