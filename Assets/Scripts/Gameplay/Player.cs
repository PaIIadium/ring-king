using System.Collections;
using System.Collections.Generic;
using GameUtils;
using Signals;
using UnityEngine;
using Zenject;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Gameplay
{
    public class Player : MonoBehaviour
    {
        [Inject]
        private SignalBus signalBus;

        [Inject] 
        private LevelProvider levelProvider;
        
        private float maxSpeed;
        private float speed;
        private const float RacingAcceleration = 60f;
        private const float SphereRadius = 0.25f;
        private const float SphereDiameter = SphereRadius * 2;
        private const float RingRadius = 0.5f;
        private float sphereSqrRadius;
        private const float BlockSize = 1f;
        private float leavingSqrDistance;
        
        private IEnumerator moveCoroutine;
        private Vector3 moveDirection;
        private bool isReturning;
        
        private const int MaxClosestRings = 10;
        private readonly List<ClosestRing> closestRings = new List<ClosestRing>();
        private readonly RaycastHit[] hits = new RaycastHit[MaxClosestRings];
        
        private const float MaxSecondsBetweenSwipeAndBallStop = 0.4f;
        private Vector2 nextSwipeDirection;
        private float detectingSwipeTime;

        private void Start()
        {
            signalBus.Subscribe<PlayerCollidedWithRingSignal>(OnPlayerCollidedWithRing);
            signalBus.Subscribe<SwipeDetectedSignal>(OnSwipeDetected);
            moveDirection = Vector3.zero;
            CalculateConstants();
        }

        private void CalculateConstants()
        {
            maxSpeed = levelProvider.ProvideCurrentLevel().ballSpeed;
            leavingSqrDistance = Mathf.Pow(SphereRadius + RingRadius, 2);
            sphereSqrRadius = Mathf.Pow(SphereRadius, 2);
        }
        
        private void OnSwipeDetected(SwipeDetectedSignal signal)
        {
            if (moveDirection != Vector3.zero)
            {
                nextSwipeDirection = signal.direction;
                detectingSwipeTime = Time.time;
                return;
            }
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);

            var direction = signal.direction;
            if (direction == Vector2.up) moveCoroutine = Move(new Vector3(0, 0, 1));
            else if (direction == Vector2.right) moveCoroutine = Move(new Vector3(1, 0, 0));
            else if (direction == Vector2.left) moveCoroutine = Move(new Vector3(-1, 0, 0));
            else moveCoroutine = Move(new Vector3(0, 0, -1));

           StartCoroutine(moveCoroutine);
        }
        
        private void OnPlayerCollidedWithRing()
        {
            RevertMoveDirection();
        }

        private IEnumerator Move(Vector3 direction)
        {
            moveDirection = direction;
            var oppositeBlockHitInfo = GetHitInfo();
            var oppositeBlockHitPoint = oppositeBlockHitInfo.point;
            var stopPoint = oppositeBlockHitPoint - moveDirection * (BlockSize / 2);
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                var sqrDistanceToBlock = (oppositeBlockHitPoint - transform.position).sqrMagnitude;
                if (sqrDistanceToBlock < sphereSqrRadius)
                {
                    var bounceCoroutine = Bounce(stopPoint);
                    StartCoroutine(bounceCoroutine);
                    BlockFlash(oppositeBlockHitInfo);
                    break;
                }

                UpdateSpeed();
                if (!isReturning) CheckPassingThroughRings();
            
                transform.Translate(moveDirection * (speed * Time.deltaTime), Space.World);
                yield return waitForFixedUpdate;
            }
        }
        
        private void RevertMoveDirection()
        {
            isReturning = true;
            GetComponent<SphereCollider>().enabled = false;
            foreach (var ring in closestRings) ring.Collider.enabled = true;
        
            closestRings.Clear();
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            var oppositeDirection = -moveDirection;
            moveCoroutine = Move(oppositeDirection);
            StartCoroutine(moveCoroutine);
        }
        
        private RaycastHit GetHitInfo()
        {
            var ray = new Ray(transform.position, moveDirection);
            Physics.Raycast(ray, out var hitInfo, 15f, 256);
            return hitInfo;
        }

        private IEnumerator Bounce(Vector3 stopPoint)
        {
            moveDirection *= -1;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                var dirToStopPoint = stopPoint - transform.position;
                var distance = dirToStopPoint.magnitude;
                var bouncingAcceleration = -Mathf.Pow(speed, 2) / (2 * distance);
                speed += bouncingAcceleration * Time.deltaTime;
                transform.Translate(moveDirection * (speed * Time.deltaTime), Space.World);

                if (speed <= 0.2f || distance <= 0)
                {
                    Stop(stopPoint);
                    CheckNextSwipe();
                    yield break;
                }
                yield return waitForFixedUpdate;
            }
        }

        private void BlockFlash(RaycastHit blockHitInfo)
        {
            var flashImpulse = Mathf.Pow(speed / maxSpeed, 2);
            blockHitInfo.collider.gameObject.GetComponent<FieldBlock>().Flash(flashImpulse);
        }
        
        private void UpdateSpeed()
        {
            speed += RacingAcceleration * Time.deltaTime;
            if (speed >= maxSpeed) speed = maxSpeed;
        }
        
        private void CheckPassingThroughRings()
        {
            var toRemove = new List<ClosestRing>();
            SetClosestRings();
            foreach (var ring in closestRings)
            {
                if (!ring.PassedThroughCenter && ring.Checker.CheckCollecting(transform.position))
                {
                    ring.PassedThroughCenter = true;
                }
                else
                {
                    var distanceVector = ring.Collider.gameObject.transform.position - transform.position;
                    var sqrDistanceToCenter = Vector3.SqrMagnitude(distanceVector);
                    if (sqrDistanceToCenter < leavingSqrDistance) continue;
                
                    if (ring.PassedThroughCenter)
                    {
                        CollectRing(ring.Collider.gameObject);
                        toRemove.Add(ring);
                    }
                    else
                    {
                        ring.Collider.enabled = true;
                        toRemove.Add(ring);
                    }
                }
            }
            foreach (var ring in toRemove) closestRings.Remove(ring);
        }

        private void Stop(Vector3 stopPoint)
        {
            transform.position = stopPoint;
            moveDirection = Vector3.zero;
            speed = 0;
            isReturning = false;
            GetComponent<SphereCollider>().enabled = true;
        }
        
        private void CheckNextSwipe()
        {
            if (nextSwipeDirection != Vector2.zero && Time.time - detectingSwipeTime <= MaxSecondsBetweenSwipeAndBallStop)
            {
                OnSwipeDetected(new SwipeDetectedSignal {direction = nextSwipeDirection});
                nextSwipeDirection = Vector2.zero;
            }
        }
        
        private void SetClosestRings()
        {
            var ray = new Ray(transform.position - moveDirection * SphereRadius, moveDirection);

            var size = Physics.RaycastNonAlloc(ray, hits, SphereDiameter);
            for (var i = 0; i < size; i++)
            {
                var collider = hits[i].collider;
                if (collider.GetComponent<RingDestroyer>() == null) continue;
                collider.enabled = false;
                closestRings.Add(new ClosestRing {
                    Collider = collider as SphereCollider, 
                    PassedThroughCenter = false,
                    Checker = collider.gameObject.GetComponentInChildren<CollectingRingChecker>()
                });
            }
        }
        
        private void CollectRing(GameObject ring)
        {
            ring.GetComponent<RingDestroyer>().Destroy();
            signalBus.Fire<RingDestroyedSignal>();
        }

        private void OnDestroy()
        {
            signalBus.Unsubscribe<PlayerCollidedWithRingSignal>(OnPlayerCollidedWithRing);
            signalBus.Unsubscribe<SwipeDetectedSignal>(OnSwipeDetected);
        }
    }
}
