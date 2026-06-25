using UnityEngine;

namespace Game.Task2
{
    public class PathFollower : MonoBehaviour
    {
        [Header("Path Settings")]
        [SerializeField] protected WaypointPath path;
        [SerializeField] protected float arrivalThreshold = 0.3f;

        [Header("Movement Settings")]
        [SerializeField] protected float maxSpeed = 14f;
        [SerializeField] protected float rotationSpeed = 15f;

        protected int currentWaypointIndex = 0;
        protected bool isPathCompleted = false;
        protected float currentSpeed = 0f;
        protected Rigidbody rb;

        public WaypointPath Path => path;
        public bool IsPathCompleted => isPathCompleted;
        public float CurrentSpeed => currentSpeed;
        public int CurrentWaypointIndex => currentWaypointIndex;

        public System.Action OnPathCompleted;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            InitializeOnPath();
        }

        public void SetPath(WaypointPath newPath)
        {
            path = newPath;
            InitializeOnPath();
        }

        public virtual void InitializeOnPath()
        {
            if (path == null || path.WaypointCount == 0) return;

            currentWaypointIndex = 0;
            isPathCompleted = false;
            currentSpeed = 0f;

            Vector3 startPos = path.GetPoint(0);
            transform.position = startPos;

            if (path.WaypointCount > 1)
            {
                Vector3 direction = (path.GetPoint(1) - startPos).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }

        protected virtual void MoveAlongPath(float targetSpeed)
        {
            if (path == null || path.WaypointCount == 0 || isPathCompleted) return;

            currentSpeed = targetSpeed;

            if (currentSpeed <= 0.01f) return;

            Vector3 targetPosition = path.GetPoint(currentWaypointIndex);

            Vector3 moveDirection = (targetPosition - transform.position);
            moveDirection.y = 0;

            float distance = moveDirection.magnitude;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

            if (distance <= arrivalThreshold)
            {
                AdvanceToNextWaypoint();
            }
        }

        protected virtual void AdvanceToNextWaypoint()
        {
            int nextIndex = path.GetNextWaypointIndex(currentWaypointIndex);

            if (nextIndex == -1)
            {

                isPathCompleted = true;
                currentSpeed = 0f;
                OnPathCompleted?.Invoke();
            }
            else
            {
                currentWaypointIndex = nextIndex;
            }
        }
    }
}