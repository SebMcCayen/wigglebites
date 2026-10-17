using UnityEngine;
using Wigglebites.Simulation;

namespace Wigglebites.Input
{
    public sealed class TouchSteeringInput : MonoBehaviour
    {
        private const float MinimumDragPixels = 12f;
        private int _trackedFingerId = -1;
        private int _boostFingerId = -1;
        private Vector2 _touchOrigin;
        private SimVector2 _desiredHeading = SimVector2.Right;

        public SimVector2 DesiredHeading => _desiredHeading;
        public bool IsBoostHeld => UnityEngine.Input.GetKey(KeyCode.Space) || _boostFingerId >= 0;

        public void Sample()
        {
            SampleKeyboard();
            SampleTouch();
        }

        private void SampleKeyboard()
        {
            var horizontal = 0f;
            var vertical = 0f;
            if (UnityEngine.Input.GetKey(KeyCode.LeftArrow) || UnityEngine.Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.RightArrow) || UnityEngine.Input.GetKey(KeyCode.D)) horizontal += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.DownArrow) || UnityEngine.Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.UpArrow) || UnityEngine.Input.GetKey(KeyCode.W)) vertical += 1f;

            var keyboardDirection = new Vector2(horizontal, vertical);
            if (keyboardDirection.sqrMagnitude > 0.01f)
            {
                keyboardDirection.Normalize();
                _desiredHeading = new SimVector2(keyboardDirection.x, keyboardDirection.y);
            }
        }

        private void SampleTouch()
        {
            for (var index = 0; index < UnityEngine.Input.touchCount; index++)
            {
                var touch = UnityEngine.Input.GetTouch(index);
                if (touch.phase == TouchPhase.Began)
                {
                    if (touch.position.x > Screen.width * 0.6f && _boostFingerId < 0)
                    {
                        _boostFingerId = touch.fingerId;
                    }
                    else if (_trackedFingerId < 0)
                    {
                        _trackedFingerId = touch.fingerId;
                        _touchOrigin = touch.position;
                    }
                }

                if (touch.fingerId == _boostFingerId)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        _boostFingerId = -1;
                    }
                    continue;
                }

                if (touch.fingerId != _trackedFingerId) continue;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _trackedFingerId = -1;
                    continue;
                }

                var drag = touch.position - _touchOrigin;
                if (drag.sqrMagnitude < MinimumDragPixels * MinimumDragPixels) continue;

                drag.Normalize();
                _desiredHeading = new SimVector2(drag.x, drag.y);
            }
        }
    }
}
