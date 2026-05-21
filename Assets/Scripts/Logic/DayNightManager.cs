using ProjectDaydream.Common.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ProjectDaydream.Logic
{
    public class DayNightManager : MonoBehaviour
    { 
        [SerializeField] private Light sun;
        [SerializeField] private Light moon;
        [SerializeField] private AnimationCurve lightIntensityCurve;
        [SerializeField] private float maxSunIntensity = 1;
        [SerializeField] private float maxMoonIntensity = 0.5f;
        
        [SerializeField] private Color dayAmbientLight;
        [SerializeField] private Color nightAmbientLight;
        [SerializeField] private Volume volume;
        [SerializeField] private Material skyboxMaterial;
        
        
        private ColorAdjustments colorAdjustments;
        
        [SerializeField] private TimeManager timeManager;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RenderSettings.skybox = skyboxMaterial;
            RenderSettings.sun = sun;
            DynamicGI.UpdateEnvironment();
        }
        
        private void Start() {
            volume.profile.TryGet(out colorAdjustments);
        }

        private void Update() {
            RotateSunAndMoon();
            UpdateLightSettings();
            UpdateSkyBlend();
            UpdateSkyRotation();
        }

        private void UpdateSkyBlend() {
            float dotProduct = Vector3.Dot(sun.transform.forward, Vector3.up);
            float blend = Mathf.Lerp(0, 1, lightIntensityCurve.Evaluate(dotProduct));
            skyboxMaterial.SetFloat("_Blend", blend);
        }
        
        private void UpdateSkyRotation() {
            float dayFraction = (float)TimeManager.Instance.CurrentTime.TimeOfDay.TotalSeconds / 86400f;
            // Rotate skybox - adjust speed to taste
            float skyRotation = Mathf.Repeat(dayFraction * 360f, 360f);
            RenderSettings.skybox.SetFloat("_Rotation", skyRotation);
            
            // TODO: This does work but we need a better cubemap that supports an offset angle
            // OR we separate stars/clouds from the skybox and rotate them separately
            /*
            float latitudeDeg = 37.5f;
            float latitudeRad = latitudeDeg * Mathf.Deg2Rad;
            Vector4 axis = new Vector4(Mathf.Cos(latitudeRad), Mathf.Sin(latitudeRad), 0f, 0f);
            RenderSettings.skybox.SetVector("_RotationAxis", axis);
            */
            RenderSettings.skybox.SetVector("_RotationAxis", new Vector4(0, 90, 0, 0));
        }
        
        private void UpdateLightSettings() {
            float dotProduct = Vector3.Dot(sun.transform.forward, Vector3.down);
            float lightIntensity = lightIntensityCurve.Evaluate(dotProduct);
            
            sun.intensity = Mathf.Lerp(0, maxSunIntensity, lightIntensity);
            moon.intensity = Mathf.Lerp(maxMoonIntensity, 0, lightIntensity);
            
            if (colorAdjustments)
            {
                colorAdjustments.colorFilter.value = Color.Lerp(nightAmbientLight, dayAmbientLight, lightIntensity);
            }
        }

        private float CalculateSunAngle() {
            var isDay = timeManager.IsDayTime();
            var startDegree = isDay ? 0f : 180f;
            var start = isDay ? timeManager.SunriseTime : timeManager.SunsetTime;
            var end = isDay ? timeManager.SunsetTime : timeManager.SunriseTime;
        
            var totalTime = start.CalculateDifference(end);
            var elapsedTime = start.CalculateDifference(timeManager.CurrentTime.TimeOfDay);

            double percentage = elapsedTime.TotalMinutes / totalTime.TotalMinutes;
            return Mathf.Lerp(startDegree, startDegree + 180, (float) percentage);
        }
        
        private void RotateSunAndMoon() {
            var rotation = CalculateSunAngle();
            sun.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.right);
            moon.transform.rotation = Quaternion.AngleAxis(rotation + 180f, Vector3.right);
        }
    }
}
