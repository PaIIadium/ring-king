using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class RingDestroyer : MonoBehaviour
    {
        [SerializeField]
        private float flashDuration;

        [SerializeField]
        private Color flashColor;

        [SerializeField]
        private float flashEmissiveIntensity;

        [SerializeField]
        private float flashPointLightIntensity;

        [SerializeField]
        private float flashRange;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private Color defaultColor;
        private float defaultEmissiveIntensity;
        private float defaultPointLightIntensity;
        private Vector3 defaultScale;
        private float defaultRange;
        private Renderer blockRenderer;
        private Light[] lightPoints;
        private const float ExplosionDurationFraction = 0.2f;
        
        private void Start()
        {
            blockRenderer = GetComponent<Renderer>();
            defaultColor = blockRenderer.material.GetColor(EmissionColor);
            defaultScale = transform.localScale;
            defaultEmissiveIntensity = 1f;
            lightPoints = GetComponentsInChildren<Light>();
            defaultRange = lightPoints[0].range;
            defaultPointLightIntensity = lightPoints[0].intensity;
        }

        public void Destroy()
        {
            StartCoroutine(nameof(Destroying));
        }

        public IEnumerator Destroying()
        {
            var coroutinesList = new List<Coroutine>
            {
                StartCoroutine(LightningAnimation()),
                StartCoroutine(ChangeScale())
            };

            yield return WaitForAllCoroutines(coroutinesList);
            Destroy(gameObject);
        }
        
        private IEnumerator LightningAnimation()
        {
            foreach (var lightPoint in lightPoints) lightPoint.shadows = LightShadows.Hard;
            yield return StartCoroutine(Flash());
            yield return StartCoroutine(Extinct());
        }
        
        private IEnumerator ChangeScale()
        {
            var stopwatch = 0f;
            while (stopwatch < flashDuration)
            {
                var progress = stopwatch / flashDuration;
                var scale = 1 - progress;
                transform.localScale = defaultScale * scale;
                stopwatch += Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator Flash()
        {
            var explosionDuration = ExplosionDurationFraction * flashDuration;
            var stopwatch = 0f;
            while (stopwatch < explosionDuration)
            {
                var progress = Mathf.Pow(stopwatch / explosionDuration, 2);
                var emissiveIntensity = Mathf.Lerp(defaultEmissiveIntensity, flashEmissiveIntensity, progress);
                var lightPointIntensity =
                    Mathf.Lerp(defaultPointLightIntensity, flashPointLightIntensity, progress);
                var color = Color.Lerp(defaultColor, flashColor, progress);

                SetupRingColor(color * emissiveIntensity);
                SetupLightColor(color, lightPointIntensity, progress);
                stopwatch += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Extinct()
        {
            var extinctionDuration = flashDuration - ExplosionDurationFraction * flashDuration;
            var stopwatch = 0f;
            while (stopwatch < extinctionDuration)
            {
                var progress = 1 - stopwatch / extinctionDuration;
                var emissiveIntensity = Mathf.Lerp(defaultEmissiveIntensity, flashEmissiveIntensity, progress);
                var lightPointIntensity =
                    Mathf.Lerp(0f, flashPointLightIntensity, progress);
                var color = flashColor;
                
                SetupRingColor(color * emissiveIntensity);
                SetupLightColor(color, lightPointIntensity, progress);
                stopwatch += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator WaitForAllCoroutines(List<Coroutine> coroutines)
        {
            foreach (var coroutine in coroutines) yield return coroutine;
        }

        private void SetupLightColor(Color color, float intensity, float progress)
        {
            var range = Mathf.Lerp(defaultRange, flashRange, progress);
            foreach (var lightPoint in lightPoints)
            {
                lightPoint.color = color;
                lightPoint.intensity = intensity;
                lightPoint.range = range;
            }
        }

        private void SetupRingColor(Color color)
        {
            DynamicGI.SetEmissive(blockRenderer, color);
            blockRenderer.material.SetColor(EmissionColor, color);
        }
    }
}