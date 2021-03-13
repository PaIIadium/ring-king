using System.Collections;
using UnityEngine;

namespace Gameplay
{
    public class RingMover : MonoBehaviour
    {
        [SerializeField]
        private GameObject startPoint;

        [SerializeField]
        private GameObject endPoint;

        [SerializeField]
        private float speed = 2f;

        private Vector3 startPosition;
        private Vector3 endPosition;
        private Vector3 moveDirection;
        private bool isMovingForward;
        private const float threshold = 0.1f;

        private void Start()
        {
            endPosition = endPoint.transform.position;
            startPosition = startPoint.transform.position;
            moveDirection = (endPosition - transform.position).normalized;
            isMovingForward = true;
            StartCoroutine(nameof(Move));
        }

        private IEnumerator Move()
        {
            while (true)
            {
                var destination = isMovingForward ? endPosition : startPosition;
                var sqrDistance = Vector3.SqrMagnitude(destination - transform.position);
                if (sqrDistance <= threshold)
                {
                    isMovingForward = !isMovingForward;
                    var position = transform.position;
                    moveDirection = isMovingForward
                        ? (endPosition - position).normalized
                        : (startPosition - position).normalized;
                }

                transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
    }
}