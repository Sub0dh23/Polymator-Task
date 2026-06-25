using UnityEngine;

namespace Game.Task2
{
    public class AIVehicleController : PathFollower
    {
        [Header("AI Configuration")]
        [SerializeField] private bool teleportToStartOnComplete = true;

        private bool isActive = true;

        private bool isFirstInit = true;
        private Vector3 originalEditorPosition;
        private int originalEditorWaypointIndex;

        protected override void Start()
        {

            if (name != "AIVehicle" && transform.parent != null)
            {
                Transform mainAI = transform.parent.Find("AIVehicle");
                if (mainAI != null)
                {
                    transform.localScale = mainAI.localScale;

                    Transform mainModel = mainAI.Find("CarModel");
                    Transform myModel = transform.Find("CarModel");
                    if (mainModel != null && myModel != null)
                    {
                        myModel.localScale = mainModel.localScale;
                    }
                }
            }

            originalEditorPosition = transform.position;
            originalEditorWaypointIndex = 1;

            base.Start();

            currentSpeed = maxSpeed;
        }

        public override void InitializeOnPath()
        {
            base.InitializeOnPath();

            if (isFirstInit && name.Contains("Staggered"))
            {
                transform.position = originalEditorPosition;
                currentWaypointIndex = originalEditorWaypointIndex;
                isFirstInit = false;
            }
        }

        private void Update()
        {
            if (!isActive || path == null || path.WaypointCount == 0) return;

            MoveAlongPath(maxSpeed);
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        protected override void AdvanceToNextWaypoint()
        {
            int nextIndex = path.GetNextWaypointIndex(currentWaypointIndex);

            if (nextIndex == -1)
            {
                if (teleportToStartOnComplete)
                {

                    InitializeOnPath();
                    currentSpeed = maxSpeed;
                }
                else
                {
                    isPathCompleted = true;
                    currentSpeed = 0f;
                    OnPathCompleted?.Invoke();
                }
            }
            else
            {
                currentWaypointIndex = nextIndex;
            }
        }
    }
}