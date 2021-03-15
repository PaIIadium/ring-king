using System.Collections;
using Signals;
using UnityEngine;
using Zenject;

namespace Gameplay
{
    public class RingTouchPoint : MonoBehaviour
    {
        [SerializeField]
        private GameObject lightPoint;

        [Inject]
        private SignalBus signalBus;
        
        private Ray forwardRay;
        private GameObject player;
        private RingLightPoint ringLightPoint;
        private const float RayLength = 0.1f;

        private void Start()
        {
            player = FindObjectOfType<Player>().gameObject;
            ringLightPoint = lightPoint.GetComponent<RingLightPoint>();

            StartCoroutine(nameof(CheckCollision));
        }

        private IEnumerator CheckCollision()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                if (IsHitPlayer()) OnPlayerHit();
                yield return waitForFixedUpdate;
                UpdateRay();
            } 
        }

        void UpdateRay()
        {
            var rayDirection = GetRayDirection();
            forwardRay = new Ray(transform.position, rayDirection);
        }

        private Vector3 GetRayDirection()
        {
            var rotationAngle = transform.rotation.eulerAngles.y / (180 / Mathf.PI);
            var x = Mathf.Sin(rotationAngle);
            var z = Mathf.Cos(rotationAngle);
            return new Vector3(x, 0, z);
        }

        private void OnPlayerHit()
        {
            signalBus.Fire<PlayerCollidedWithRingSignal>();
            ringLightPoint.Flash();
        }

        private bool IsHitPlayer()
        {
            if (Physics.Raycast(forwardRay, out var hitInfo, RayLength))
            {
                var isPlayer = hitInfo.collider.CompareTag("Player");
                if (isPlayer) return true;
            }
            return IsInsidePlayer();
        }

        private bool IsInsidePlayer()
        {
            var playerBounds = player.GetComponent<SphereCollider>().bounds;
            var isInside = playerBounds.Contains(transform.position);
            return isInside;
        }
    }
}