using UnityEngine;
using UnityEngine.UI;

namespace Game.Task1
{
    public class Task1UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private GameObject controlPanel;

        [Header("UI Buttons")]
        [SerializeField] private Button rotateTyreButton;
        [SerializeField] private Button stopTyreButton;
        [SerializeField] private Button toggleDoorButton;
        [SerializeField] private Button toggleHoodBootButton;

        private CarAnimationController activeCarController;

        private void Start()
        {

            if (controlPanel != null) controlPanel.SetActive(false);
            if (instructionPanel != null) instructionPanel.SetActive(true);

            if (rotateTyreButton != null) rotateTyreButton.onClick.AddListener(OnRotateTyresClicked);
            if (stopTyreButton != null) stopTyreButton.onClick.AddListener(OnStopTyresClicked);
            if (toggleDoorButton != null) toggleDoorButton.onClick.AddListener(OnToggleDoorsClicked);
            if (toggleHoodBootButton != null) toggleHoodBootButton.onClick.AddListener(OnToggleHoodBootClicked);

            if (placementManager != null)
            {
                placementManager.OnCarPlaced += HandleCarPlaced;
            }
        }

        private void OnDestroy()
        {
            if (placementManager != null)
            {
                placementManager.OnCarPlaced -= HandleCarPlaced;
            }
        }

        private void HandleCarPlaced(GameObject carInstance)
        {
            if (carInstance != null)
            {
                activeCarController = carInstance.GetComponent<CarAnimationController>();

                if (instructionPanel != null) instructionPanel.SetActive(false);
                if (controlPanel != null) controlPanel.SetActive(true);
            }
        }

        private void OnRotateTyresClicked()
        {
            if (activeCarController != null)
            {
                activeCarController.StartTyreRotation();
            }
        }

        private void OnStopTyresClicked()
        {
            if (activeCarController != null)
            {
                activeCarController.StopTyreRotation();
            }
        }

        private void OnToggleDoorsClicked()
        {
            if (activeCarController != null)
            {
                activeCarController.ToggleDoors();
            }
        }

        private void OnToggleHoodBootClicked()
        {
            if (activeCarController != null)
            {
                activeCarController.ToggleHoodAndBoot();
            }
        }
    }
}