using System.Collections;
using UnityEngine;

namespace Gameplay
{
    public class RingLightPoint : MonoBehaviour
    {
        [SerializeField]
        private float flashDuration;

        [SerializeField]
        private Color flashColor;

        [SerializeField]
        private float flashIntensity;

        [SerializeField]
        private float flashRange;

        [SerializeField]
        private float explosionDurationFraction = 0.25f;
        
        private Color defaultColor;
        private float defaultIntensity;
        private float defaultRange;

        private Light lightPoint;
        private float extinctionDuration;
        private float explosionDuration;
        private float stopwatch;

        private void Start()
        {
            lightPoint = GetComponent<Light>();
            defaultColor = lightPoint.color;
            defaultIntensity = lightPoint.intensity;
            defaultRange = lightPoint.range;
        }

        public void Flash()
        {
            lightPoint.shadows = LightShadows.Hard;
            StartCoroutine(nameof(Flashing));
        }

        private IEnumerator Flashing()
        {
            CalculateConstants();
            while (!isActive)
            {
                UpdateFlash();
                yield return null;
            }
            SetFlashDefaultValues();
        }
        
        private void CalculateConstants()
        {
            stopwatch = 0f;
            explosionDuration = flashDuration * explosionDurationFraction;
            extinctionDuration = flashDuration - explosionDuration;
        }
        
        private bool isActive => stopwatch > flashDuration;

        private void UpdateFlash()
        {
            var progress = CalculateProgress();
            SetupLightPoints(progress);
            stopwatch += Time.deltaTime;
        }

        private float CalculateProgress()
        {
            var isExplosion = stopwatch < explosionDuration;
            if (isExplosion)
            {
                return stopwatch / explosionDuration;
            }
            return 1 - (stopwatch - explosionDuration) / extinctionDuration;
        }

        void SetFlashDefaultValues()
        {
            lightPoint.color = defaultColor;
            lightPoint.intensity = defaultIntensity;
            lightPoint.range = defaultRange;
            lightPoint.shadows = LightShadows.None;
        }

        void SetupLightPoints(float progress)
        {
            lightPoint.color = Color.Lerp(defaultColor, flashColor, progress);
            lightPoint.intensity = Mathf.Lerp(defaultIntensity, flashIntensity, progress);
            lightPoint.range = Mathf.Lerp(defaultRange, flashRange, progress);
        }
    }
}