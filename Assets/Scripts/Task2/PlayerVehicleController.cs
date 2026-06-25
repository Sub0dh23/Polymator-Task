using UnityEngine;

namespace Game.Task2
{
    public class PlayerVehicleController : PathFollower
    {
        [Header("Acceleration Settings")]
        [SerializeField] private float acceleration = 24f;
        [SerializeField] private float deceleration = 36f;

        [Header("Finish Line Settings")]
        [SerializeField] private float finishLineThreshold = 1.2f;

        private bool canMove = true;

        public void SetControlEnabled(bool enabled)
        {
            canMove = enabled;
            if (!enabled)
            {
                currentSpeed = 0f;
            }
        }

        private void Update()
        {
            if (path == null || path.WaypointCount == 0 || isPathCompleted || !canMove) return;

            bool isHolding = UnityEngine.InputSystem.Pointer.current != null &&
                             UnityEngine.InputSystem.Pointer.current.press.isPressed;

            if (isHolding && UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                isHolding = false;
            }

            float targetSpeed = 0f;
            if (isHolding)
            {

                targetSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            else
            {

                targetSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            MoveAlongPath(targetSpeed);
        }

        protected override void MoveAlongPath(float targetSpeed)
        {
            if (path == null || path.WaypointCount == 0 || isPathCompleted) return;

            float originalThreshold = arrivalThreshold;

            if (currentWaypointIndex == path.WaypointCount - 1)
            {
                arrivalThreshold = finishLineThreshold;
            }

            base.MoveAlongPath(targetSpeed);

            arrivalThreshold = originalThreshold;
        }

        public void ConfigureMovement(float maxSpeedVal, float accelVal, float decelVal, float rotSpeedVal, float thresholdVal)
        {
            maxSpeed = maxSpeedVal;
            acceleration = accelVal;
            deceleration = decelVal;
            rotationSpeed = rotSpeedVal;
            finishLineThreshold = thresholdVal;
        }
    }
}