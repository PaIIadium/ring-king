using UnityEngine;

namespace Gameplay
{
    public class RingRotator : MonoBehaviour
    {
        [SerializeField]
        private float rotationSpeed = 120f;

        [SerializeField] 
        private bool isReversed;
        
        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            var rotation = transform.rotation;
            var delta = rotationSpeed * Time.deltaTime * Vector3.right;
            delta *= isReversed ? -1 : 1;
            rotation *= Quaternion.Euler(delta);
            transform.rotation = rotation;
        }
    }
}
