using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace DayNightCycles
{
    public class DayAndNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [Range(0f, 24f)]
        [Tooltip("Slider allows you to set the starting time. Range 0-24")]
        public float currentTime;
        [Tooltip("Time elapsed multiplier. When set to 1, one second of real time equals one minute of script time. A negative value turns back time.")]
        public float timeSpeed = 1f; // time speed multiplier
        private float timeDivider = 60f; // divides the time so that you can obtain the exact passage of seconds

        [Header("Current Time")]
        [Tooltip("Current time in the hh:mm:ss system.")]
        public string currentTimeString; // shows time in the hh:mm system in the inspector

        [Header("Sun Settings")]
        [Tooltip("A light source simulating the Sun.")]
        public Light sunLight; // sun light object
        [Range(0f, 90f)] // sun latitude range
        [Tooltip("Sun latitude determines the maximum height of the Sun. Range 0-90")]
        public float sunLatitude = 20f; // sun latitude
        [Range(-180f, 180f)] // sun longitude range
        [Tooltip("Sun longitude determines position of the Sun. Range -180, 180")]
        public float sunLongitude = -90f; // sun longitude
        [Tooltip("Basic Sun intensity value. Together with Sun Intensity Multiplier affects the brightness of the Sun during the cycle.")]
        public float sunIntensity = 60000f; // sun base intensity
        [Tooltip("Decreases or increases Sun intensity over time. 0 - Midnight | 0.25 - Dawn | 0.5 - Noon | 0.75 - Dusk | 1 - Midnight")]
        public AnimationCurve sunIntensityMultiplier; // a curve that decreases or increases sun intensity over time
        [Range(1500f, 7000f)]
        [Tooltip("Basic Sun temperature value in Kelvin. Together with Sun Temperature Curve affects the temperature of the Sun during the cycle.")]
        public float sunTemperature = 6500f; // sun base temperature
        [Tooltip("Decreases or increases Sun temperature over time. 0 - Midnight | 0.25 - Dawn | 0.5 - Noon | 0.75 - Dusk | 1 - Midnight")]
        public AnimationCurve sunTemperatureCurve; // a curve that decreases or increases sun temperature over time

        [Header("Moon Settings")]
        [Tooltip("A light source simulating the Moon.")]
        public Light moonLight; // moon light object
        [Range(0f, 90f)] // moon latitude range
        [Tooltip("Moon latitude determines the maximum height of the Moon. For best results, the value should be the same as star latitude. Range 0-90")]
        public float moonLatitude = 40f; // moon latitude
        [Range(-180f, 180f)] //moon latitude range
        [Tooltip("Moon longitude determines position of the Moon. For best results, the value should be the same as star longitude. Range -180, 180")]
        public float moonLongitude = 90f; // moon longitude
        [Tooltip("Basic moon intensity value. Together with Moon Intensity Multiplier affects the brightness of the Moon during the cycle.")]
        public float moonIntensity = 12000f; // moon base intensity
        public AnimationCurve moonIntensityMultiplier; // curve that decreases or increases moon intensity over time
        [Range(6500f, 20000f)] // moon temperature range
        [Tooltip("Basic Moon temperature value in Kelvin. Together with Moon Temperature Curve affects the temperature of the Moon during the cycle.")]
        public float moonTemperature = 10000f; // moon base temperature
        [Tooltip("Decreases or increases Moon temperature over time. 0 - Midnight | 0.25 - Dawn | 0.5 - Noon | 0.75 - Dusk | 1 - Midnight")]
        public AnimationCurve moonTemperatureCurve;  // a curve that decreases or increases moon intensity over time

        [Header("Stars")]
        public VolumeProfile volumeProfile; // volume profile
        private PhysicallyBasedSky skySettings; // access to physically based sky
        [Range(0f, 90f)] // star latitude range
        [Tooltip("Star latitude determines the height of the stars rotation point (Polar Star). Range 0-90")]
        public float polarStarLatitude = 40f; // star latitude
        [Range(-180f, 180f)] // star longitude range
        [Tooltip("Star longitude determines the position of the stars rotation point (Polar Star). Range -180, 180")]
        public float polarStarLongitude = 90f; // star longitude
        [Tooltip("Star intensity value. Together with Star Curve affects the brightness of the skybox during the cycle.")]
        public float starsIntensity = 8000f; // star intensity
        [Tooltip("Decreases or increases skybox intensity over time. 0 - Midnight | 0.25 - Dawn | 0.5 - Noon | 0.75 - Dusk | 1 - Midnight")]
        public AnimationCurve starsCurve; // curve that decreases or increases star intensity over time
        [Tooltip("The curve of the horizon tint changing over time")]
        public AnimationCurve horizonTintCurve; // horizon tint curve
        [Tooltip("The curve of the zenit tint changing over time")]
        public AnimationCurve zenithTintCurve; // zenit tint curve

        [Header("Control Indicators")]
        [Tooltip("Displays a marker whether it is day or night")]
        public bool isDay = true; // displays a marker whether it is day or night


        private HDAdditionalLightData sunLightData; // cached HDAdditionalLightData for sun
        private HDAdditionalLightData moonLightData; // cached HDAdditionalLightData for moon
        private Light sunLightComponent; // sun light component
        private Light moonLightComponent; // moon light component

        void Awake()
        {
            sunLightData = sunLight.GetComponent<HDAdditionalLightData>(); // cache HDAdditionalLightData for sun
            moonLightData = moonLight.GetComponent<HDAdditionalLightData>(); // cache HDAdditionalLightData for moon
            sunLightComponent = sunLight.GetComponent<Light>(); // sun light component
            moonLightComponent = moonLight.GetComponent<Light>(); // moon light component
        }

        // Update is called once per frame
        void Update()
        {
            currentTime += Time.deltaTime * timeSpeed / timeDivider; // time generator

            if (currentTime >= 24)
            {
                currentTime = 0;
            }
            if (currentTime < 0)
            {
                currentTime = 23.99999f;
            }

            UpdateTimeText();
            UpdateLight();
            CheckShadowStatus();
            SkyStar();

        }

        private void OnValidate()  //perform an action after a value changes in the Inspector
        {
            if (sunLightData == null & sunLightComponent == null || moonLightData == null & moonLightComponent == null)
                Awake();
            UpdateLight();
            CheckShadowStatus();
            SkyStar();
        }

        void UpdateTimeText()
        {

            currentTimeString = Mathf.Floor(currentTime).ToString("00") + ":" + Mathf.Floor((currentTime * 60) % 60).ToString("00") + ":" + Mathf.Floor((currentTime * 3600) % 60).ToString("00"); // conversion to a 24-hour system

        }

        void UpdateLight()
        {
            float sunRotation = currentTime / 24f * 360f; // the sun's rotation relative to time
            sunLight.transform.localRotation = (Quaternion.Euler(sunLatitude - 90, sunLongitude, 0) * Quaternion.Euler(0, sunRotation, 0)); // sun rotation with longitude and latitude
            moonLight.transform.localRotation = (Quaternion.Euler(90 - moonLatitude, moonLongitude, 0) * Quaternion.Euler(0, sunRotation, 0)); // moon rotation with longitude and latitude


            float normalizedTime = currentTime / 24f;
            float sunIntensityCurve = sunIntensityMultiplier.Evaluate(normalizedTime); // sun intensity curve
            float moonIntensityCurve = moonIntensityMultiplier.Evaluate(normalizedTime); // moon intensity curve
            float sunTemperatureMultiplier = sunTemperatureCurve.Evaluate(normalizedTime); // sun temperature
            float moonTemperatureMultiplier = moonTemperatureCurve.Evaluate(normalizedTime); // moon temperature

            // U¿ywanie w³aœciwoœci intensity bezpoœrednio na obiekcie Light
            if (sunLightComponent != null)
            {
                sunLightComponent.intensity = sunIntensityCurve * sunIntensity;  // sun intensity considering the curve
                sunLightComponent.colorTemperature = sunTemperatureMultiplier * sunTemperature; // sun light temperature with temperature curve
            }

            if (moonLightComponent != null)
            {
                moonLightComponent.intensity = moonIntensityCurve * moonIntensity; // moon intensity considering the curve
                moonLightComponent.colorTemperature = moonTemperatureMultiplier * moonTemperature; // moon light temperature with temperature curve
            }
        }

        void CheckShadowStatus() // turning sun and moon shadows depending on the current time value
        {
            float currentSunRotation = currentTime;
            if (currentSunRotation >= 5.9f && currentSunRotation <= 18.1f)
            {
                sunLightData.EnableShadows(true);
                moonLightData.EnableShadows(false);
                isDay = true;
            }

            else
            {
                sunLightData.EnableShadows(false);
                moonLightData.EnableShadows(true);
                isDay = false;
            }

        }

        void SkyStar()
        {
            volumeProfile.TryGet<PhysicallyBasedSky>(out skySettings); //  volume profile with physicaly based sky
            skySettings.spaceEmissionMultiplier.value = starsCurve.Evaluate(currentTime / 24.0f) * starsIntensity; // intensity of the skybox with stars taking into account the curve

            skySettings.spaceRotation.value = (Quaternion.Euler(90 - polarStarLatitude, polarStarLongitude, 0) * Quaternion.Euler(0, currentTime / 24.0f * 360.0f, 60)).eulerAngles; // skybox rotation

            float horizonTintCurveValue = horizonTintCurve.Evaluate(currentTime / 24.0f); // changing the horizon tint over time taking into account the curve
            skySettings.horizonTint.value = new Color(horizonTintCurveValue, horizonTintCurveValue, horizonTintCurveValue); // horizon tint
            float zenithTintCurveValue = zenithTintCurve.Evaluate(currentTime / 24.0f); // changing the zenit tint over time taking into account the curve
            skySettings.zenithTint.value = new Color(zenithTintCurveValue, zenithTintCurveValue, zenithTintCurveValue); // zenit tint
        }
    }
}
