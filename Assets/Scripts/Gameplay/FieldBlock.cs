using System.Collections;
using UnityEngine;

namespace Gameplay
{
    public class FieldBlock : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private bool isFlashing;
        private Color defaultColor;
        private float defaultIntensity;
        private Renderer blockRenderer;
        private float maxIntensity;
    
        [SerializeField]
        private float flashDuration;
    
        [SerializeField]
        private Color flashColor;
    
        [SerializeField]
        private float flashIntensity;
        private void Start()
        {
            blockRenderer = GetComponent<Renderer>();
            defaultColor = blockRenderer.material.GetColor(EmissionColor);
            defaultIntensity = 1f;
        }

        public void Flash(float impulse)
        {
            maxIntensity = (1 + impulse) * flashIntensity;
            StartCoroutine(nameof(Flashing));
        }

        private IEnumerator Flashing()
        {
            var stopwatch = 0f;
            var explosionDuration = flashDuration / 4;
            var extinctionDuration = flashDuration - explosionDuration;
            while (true)
            {
                var progress = stopwatch < explosionDuration ? stopwatch / explosionDuration :
                    1 - (stopwatch - explosionDuration) / extinctionDuration;

                var intensity = Mathf.Lerp(defaultIntensity, maxIntensity, progress);
                var color = Color.Lerp(defaultColor, flashColor, progress);
            
                DynamicGI.SetEmissive(blockRenderer, color * intensity);
                blockRenderer.material.SetColor(EmissionColor, color * intensity);
            
                stopwatch += Time.deltaTime;
                if (stopwatch > flashDuration)
                {
                    DynamicGI.SetEmissive(blockRenderer, defaultColor);
                    blockRenderer.material.SetColor(EmissionColor, defaultColor);
                    yield break;
                }
                yield return null;
            }
        }
    }
}
