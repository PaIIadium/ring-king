using UnityEngine;

namespace Gameplay
{
    public class CollectingRingChecker : MonoBehaviour
    {
        private Vector3 rayDirection;
        private float rotationAngle;
    
        public bool CheckCollecting(Vector3 position)
        {
            rotationAngle = transform.rotation.eulerAngles.y / (180 / Mathf.PI);
            rayDirection.x = Mathf.Sin(rotationAngle);
            rayDirection.z = Mathf.Cos(rotationAngle);
            var sqrDistance = Vector3.Cross(rayDirection, position - transform.position).sqrMagnitude;
            return sqrDistance <= 0.03f;
        }
    }
}
