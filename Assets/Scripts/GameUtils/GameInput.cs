using Lean.Touch;
using Signals;
using UnityEngine;
using Zenject;

namespace GameUtils
{
    public class GameInput : MonoBehaviour
    {
        [Inject] 
        private SignalBus signalBus;

        private float minSqrSwipeSpeed = 4_000f;
        private float minSqrSwipeDistance = 2_500f;
        private bool isSwiping;
        private bool swipeEnded = true;
        private Vector2 startPoint;

        private void Start()
        {
            minSqrSwipeSpeed /= LeanTouch.ScalingFactor * LeanTouch.ScalingFactor;
            minSqrSwipeDistance /= LeanTouch.ScalingFactor * LeanTouch.ScalingFactor;
            LeanTouch.OnFingerUpdate += HandleFinger;
        }

        private void HandleSwipe(LeanFinger finger)
        {
            var direction = finger.ScaledDelta;
            var sinAndCos45 = Mathf.Sqrt(2) / 2;
            var angle45 = new Vector2(sinAndCos45, sinAndCos45);
            var angle135 = new Vector2(-sinAndCos45, sinAndCos45);
        
            var angleWith45 = Vector2.Angle(angle45, direction);
            var angleWith135 = Vector2.Angle(angle135, direction);

            Vector2 vector;
            if (angleWith45 < 90 && angleWith135 < 90) vector = Vector2.up;
            else if (angleWith45 < 90) vector = Vector2.right;
            else if (angleWith135 < 90) vector = Vector2.left;
            else vector = Vector2.down;
            signalBus.Fire(new SwipeDetectedSignal {direction = vector});
        }

        private void HandleFinger(LeanFinger finger)
        {
            var sqrSpeed = (finger.ScreenDelta / Time.deltaTime).sqrMagnitude;
            if (sqrSpeed >= minSqrSwipeSpeed)
            {
                if (!isSwiping)
                {
                    isSwiping = true;
                    startPoint = finger.ScreenPosition;
                }
                else
                {
                    if (!swipeEnded) return;
                    var sqrSwipeDistance = (finger.ScreenPosition - startPoint).sqrMagnitude;
                    if (sqrSwipeDistance < minSqrSwipeDistance) return;
                    HandleSwipe(finger);
                    isSwiping = false;
                    swipeEnded = false;
                }
            }
            else
            {
                swipeEnded = true;
                isSwiping = false;
            }
        }
    }
}
