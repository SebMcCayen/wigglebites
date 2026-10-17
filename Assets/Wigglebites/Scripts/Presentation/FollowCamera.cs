using UnityEngine;

namespace Wigglebites.Presentation
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float damping = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 24f, -18f);

        private Vector3 _targetPosition;

        public void ConfigureCamera()
        {
            var cameraComponent = GetComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 13f;
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.025f, 0.04f, 0.1f);
            transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        }

        public void SetTarget(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        private void LateUpdate()
        {
            var desiredPosition = _targetPosition + offset;
            var interpolation = 1f - Mathf.Exp(-damping * Time.unscaledDeltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, interpolation);
        }
    }
}
