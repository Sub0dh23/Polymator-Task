using UnityEngine;

namespace Game.Task2
{
    [RequireComponent(typeof(PlayerVehicleController))]
    public class PlayerCollisionHandler : MonoBehaviour
    {
        private PlayerVehicleController playerController;

        public System.Action OnPlayerCrashed;

        private void Awake()
        {
            playerController = GetComponent<PlayerVehicleController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.gameObject);
        }

        private void HandleCollision(GameObject otherObject)
        {

            if (otherObject.GetComponent<AIVehicleController>() != null ||
                otherObject.name.Contains("Obstacle"))
            {

                playerController.SetControlEnabled(false);

                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddForce((transform.up + Random.onUnitSphere) * 5f, ForceMode.Impulse);
                    rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);
                }

                OnPlayerCrashed?.Invoke();
            }
        }
    }
}