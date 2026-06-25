using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Game.Task1
{
    public class ARPlacementManager : MonoBehaviour
    {
        [Header("Placement Configuration")]
        [SerializeField] private GameObject placementPrefab;
        [SerializeField] private Camera arCamera;

        [Header("Editor Fallback Settings")]
        [SerializeField] private bool enableEditorFallback = true;
        [SerializeField] private LayerMask editorGroundLayer;
        [SerializeField] private GameObject editorFallbackFloor;

        private ARRaycastManager arRaycastManager;
        private GameObject spawnedObject;
        private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        public GameObject SpawnedObject => spawnedObject;

        public System.Action<GameObject> OnCarPlaced;

        private void Awake()
        {

            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                arRaycastManager = GetComponent<ARRaycastManager>();
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            if (editorFallbackFloor == null)
            {
                editorFallbackFloor = GameObject.Find("EditorFallbackFloor");
            }

            if (Application.isMobilePlatform && editorFallbackFloor != null)
            {
                editorFallbackFloor.SetActive(false);
                Debug.Log("Mobile platform detected: Disabled EditorFallbackFloor.");
            }
        }

        private void Start()
        {
            SetupPlaneVisualizer();
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Pointer.current != null &&
                UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
            {
                Vector2 touchPosition = UnityEngine.InputSystem.Pointer.current.position.ReadValue();

                if (IsPointerOverUI(touchPosition))
                {
                    return;
                }

                bool placed = false;

                if (arRaycastManager != null && arRaycastManager.enabled)
                {

                    if (arRaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                    {
                        Pose hitPose = s_Hits[0].pose;
                        PlaceOrMoveObject(hitPose.position, hitPose.rotation);
                        placed = true;
                    }
                }

                if (!placed && enableEditorFallback && !Application.isMobilePlatform)
                {
                    Ray ray = arCamera.ScreenPointToRay(touchPosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 100f, editorGroundLayer))
                    {

                        Vector3 lookPos = arCamera.transform.position - hit.point;
                        lookPos.y = 0;
                        Quaternion rotation = Quaternion.LookRotation(lookPos);
                        PlaceOrMoveObject(hit.point, rotation);
                    }
                }
            }
        }

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return false;

            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                foreach (var touch in UnityEngine.InputSystem.Touchscreen.current.touches)
                {
                    if (touch.press.isPressed)
                    {
                        int pointerId = touch.touchId.ReadValue();
                        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerId))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void SetupPlaneVisualizer()
        {
            var planeManager = FindObjectOfType<ARPlaneManager>();
            if (planeManager != null && planeManager.planePrefab == null)
            {

                GameObject planeTemplate = new GameObject("AR_Default_Plane");
                planeTemplate.SetActive(false);

                planeTemplate.AddComponent<MeshFilter>();
                var meshRenderer = planeTemplate.AddComponent<MeshRenderer>();
                planeTemplate.AddComponent<ARPlane>();
                planeTemplate.AddComponent<ARPlaneMeshVisualizer>();
                planeTemplate.AddComponent<MeshCollider>();

                var lineRenderer = planeTemplate.AddComponent<LineRenderer>();

                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");

                Material planeMat = new Material(shader);
                if (planeMat != null)
                {
                    planeMat.color = new Color(0.12f, 0.58f, 0.95f, 0.35f);

                    planeMat.SetFloat("_Surface", 1.0f);
                    planeMat.SetFloat("_Blend", 0.0f);
                    planeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    planeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    planeMat.SetInt("_ZWrite", 0);
                    planeMat.DisableKeyword("_ALPHATEST_ON");
                    planeMat.EnableKeyword("_ALPHABLEND_ON");
                    planeMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    planeMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    meshRenderer.sharedMaterial = planeMat;
                }

                if (lineRenderer != null && planeMat != null)
                {
                    lineRenderer.startWidth = 0.02f;
                    lineRenderer.endWidth = 0.02f;
                    lineRenderer.sharedMaterial = planeMat;
                    lineRenderer.useWorldSpace = false;
                    lineRenderer.loop = true;
                    lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    lineRenderer.receiveShadows = false;
                }

                planeManager.planePrefab = planeTemplate;
                Debug.Log("Procedurally generated and registered AR plane visualizer prefab with LineRenderer.");
            }
        }

        private void PlaceOrMoveObject(Vector3 position, Quaternion rotation)
        {
            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placementPrefab, position, rotation);
                OnCarPlaced?.Invoke(spawnedObject);
            }
            else
            {
                spawnedObject.transform.position = position;
                spawnedObject.transform.rotation = rotation;
                OnCarPlaced?.Invoke(spawnedObject);
            }
        }
    }
}