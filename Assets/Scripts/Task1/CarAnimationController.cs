using System.Collections;
using UnityEngine;

namespace Game.Task1
{
    public class CarAnimationController : MonoBehaviour
    {
        [System.Serializable]
        public class AnimatedPart
        {
            public string name;
            public Transform partTransform;
            public Vector3 closedRotationEuler;
            public Vector3 openRotationEuler;
            public bool isOpen;
            [HideInInspector] public Coroutine activeCoroutine;
        }

        [Header("Tyre Rotation Settings")]
        [SerializeField] private Transform[] wheels;
        [SerializeField] private float tyreRotationSpeed = 360f;
        [SerializeField] private Vector3 tyreRotationAxis = Vector3.right;
        private bool isRotatingTyres = false;

        [Header("Door Settings")]
        [SerializeField] private AnimatedPart[] doors;

        [Header("Hood and Boot Settings")]
        [SerializeField] private AnimatedPart[] hoodAndBoot;

        [Header("Animation Options")]
        [SerializeField] private float animationDuration = 0.6f;

        private void Update()
        {
            if (isRotatingTyres)
            {
                RotateTyres();
            }
        }

        private void RotateTyres()
        {
            if (wheels == null) return;
            foreach (Transform wheel in wheels)
            {
                if (wheel != null)
                {

                    wheel.Rotate(tyreRotationAxis * tyreRotationSpeed * Time.deltaTime, Space.Self);
                }
            }
        }

        public void StartTyreRotation()
        {
            isRotatingTyres = true;
        }

        public void StopTyreRotation()
        {
            isRotatingTyres = false;
        }

        public void ToggleDoors()
        {
            if (doors == null) return;
            foreach (var door in doors)
            {
                if (door.partTransform != null)
                {
                    door.isOpen = !door.isOpen;
                    if (door.activeCoroutine != null) StopCoroutine(door.activeCoroutine);
                    door.activeCoroutine = StartCoroutine(AnimatePartCoroutine(door));
                }
            }
        }

        public void ToggleHoodAndBoot()
        {
            if (hoodAndBoot == null) return;
            foreach (var part in hoodAndBoot)
            {
                if (part.partTransform != null)
                {
                    part.isOpen = !part.isOpen;
                    if (part.activeCoroutine != null) StopCoroutine(part.activeCoroutine);
                    part.activeCoroutine = StartCoroutine(AnimatePartCoroutine(part));
                }
            }
        }

        private IEnumerator AnimatePartCoroutine(AnimatedPart part)
        {
            Vector3 startRot = part.partTransform.localEulerAngles;
            Vector3 targetRot = part.isOpen ? part.openRotationEuler : part.closedRotationEuler;

            startRot = NormalizeEulerAngles(startRot);
            targetRot = NormalizeEulerAngles(targetRot);

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;

                t = Mathf.SmoothStep(0f, 1f, t);

                part.partTransform.localRotation = Quaternion.Euler(Vector3.Lerp(startRot, targetRot, t));
                yield return null;
            }

            part.partTransform.localRotation = Quaternion.Euler(targetRot);
            part.activeCoroutine = null;
        }

        private Vector3 NormalizeEulerAngles(Vector3 angles)
        {
            float x = NormalizeAngle(angles.x);
            float y = NormalizeAngle(angles.y);
            float z = NormalizeAngle(angles.z);
            return new Vector3(x, y, z);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}