using UnityEngine;
using UnityEngine.UI;

namespace Game.Task2
{
    public class Task2GameManager : MonoBehaviour
    {
        [System.Serializable]
        public class LevelData
        {
            public string levelName;
            public GameObject levelRoot;
            public WaypointPath playerPath;
            public int maxScore = 1000;
            public float timePenaltyMultiplier = 10f;
            public int minScore = 100;
        }

        [Header("Player Reference")]
        [SerializeField] private PlayerVehicleController playerVehicle;
        [SerializeField] private PlayerCollisionHandler playerCollision;

        [Header("Levels Configuration")]
        [SerializeField] private LevelData[] levels;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject failPanel;

        [Header("HUD Outlets")]
        [SerializeField] private Text levelTitleText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Slider progressSlider;

        [Header("Result Outlets")]
        [SerializeField] private Text winResultText;
        [SerializeField] private Text failResultText;

        private int activeLevelIndex = -1;
        private bool isLevelActive = false;
        private float levelElapsedTime = 0f;
        private int currentScore = 0;

        private void Start()
        {

            ShowPanel(mainMenuPanel);

            foreach (var level in levels)
            {
                if (level.levelRoot != null)
                {
                    level.levelRoot.SetActive(false);
                }
            }

            if (playerVehicle != null)
            {
                playerVehicle.SetControlEnabled(false);
                playerVehicle.gameObject.SetActive(false);
            }

            if (playerCollision != null)
            {
                playerCollision.OnPlayerCrashed += HandlePlayerCrashed;
            }

            if (playerVehicle != null)
            {
                playerVehicle.OnPathCompleted += HandlePlayerFinished;
            }
        }

        private void OnDestroy()
        {
            if (playerCollision != null)
            {
                playerCollision.OnPlayerCrashed -= HandlePlayerCrashed;
            }

            if (playerVehicle != null)
            {
                playerVehicle.OnPathCompleted -= HandlePlayerFinished;
            }
        }

        private void Update()
        {
            if (!isLevelActive) return;

            levelElapsedTime += Time.deltaTime;

            LevelData data = levels[activeLevelIndex];
            int penalty = Mathf.RoundToInt(levelElapsedTime * data.timePenaltyMultiplier);
            currentScore = Mathf.Max(data.maxScore - penalty, data.minScore);

            if (timerText != null) timerText.text = $"Time: {levelElapsedTime:F1}s";
            if (scoreText != null) scoreText.text = $"Score: {currentScore}";

            if (progressSlider != null && playerVehicle != null && playerVehicle.Path != null)
            {
                float progress = (float)playerVehicle.CurrentWaypointIndex / playerVehicle.Path.WaypointCount;
                progressSlider.value = Mathf.Clamp01(progress);
            }
        }

        public void StartLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levels.Length) return;

            if (activeLevelIndex != -1 && levels[activeLevelIndex].levelRoot != null)
            {
                levels[activeLevelIndex].levelRoot.SetActive(false);
            }

            activeLevelIndex = levelIndex;
            LevelData activeLevel = levels[activeLevelIndex];

            if (activeLevel.levelRoot != null)
            {
                activeLevel.levelRoot.SetActive(true);

                AIVehicleController[] aiVehicles = activeLevel.levelRoot.GetComponentsInChildren<AIVehicleController>(true);
                foreach (var ai in aiVehicles)
                {
                    ai.gameObject.SetActive(true);
                    ai.InitializeOnPath();
                    ai.SetActive(true);
                }
            }

            levelElapsedTime = 0f;
            currentScore = activeLevel.maxScore;

            if (playerVehicle != null)
            {
                playerVehicle.gameObject.SetActive(true);

                Rigidbody rb = playerVehicle.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    if (!rb.isKinematic)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    rb.isKinematic = true;
                }

                playerVehicle.SetPath(activeLevel.playerPath);
                playerVehicle.ConfigureMovement(14f, 24f, 36f, 15f, 1.2f);
                playerVehicle.SetControlEnabled(true);
            }

            Camera mainCam = Camera.main;
            if (mainCam != null && activeLevel.playerPath != null)
            {
                Vector3 pathPos = activeLevel.playerPath.transform.position;
                mainCam.transform.position = new Vector3(pathPos.x, 15f, -12f);
            }

            if (levelTitleText != null) levelTitleText.text = activeLevel.levelName;
            if (progressSlider != null) progressSlider.value = 0f;

            ShowPanel(hudPanel);
            isLevelActive = true;
        }

        public void RestartActiveLevel()
        {
            if (activeLevelIndex != -1)
            {
                StartLevel(activeLevelIndex);
            }
        }

        public void ReturnToMainMenu()
        {
            isLevelActive = false;

            if (activeLevelIndex != -1 && levels[activeLevelIndex].levelRoot != null)
            {
                levels[activeLevelIndex].levelRoot.SetActive(false);
            }

            activeLevelIndex = -1;

            if (playerVehicle != null)
            {
                playerVehicle.gameObject.SetActive(false);
            }

            ShowPanel(mainMenuPanel);
        }

        private void HandlePlayerCrashed()
        {
            isLevelActive = false;

            StopAllAIVehicles();

            if (failResultText != null)
            {
                failResultText.text = $"Difficulty: {levels[activeLevelIndex].levelName}\nYou crashed into traffic!";
            }

            ShowPanel(failPanel);
        }

        private void HandlePlayerFinished()
        {
            isLevelActive = false;

            if (playerVehicle != null)
            {
                playerVehicle.SetControlEnabled(false);
            }

            StopAllAIVehicles();

            string hsKey = $"HighScore_Level_{activeLevelIndex}";
            int highScore = PlayerPrefs.GetInt(hsKey, 0);
            bool newHighScore = currentScore > highScore;
            if (newHighScore)
            {
                PlayerPrefs.SetInt(hsKey, currentScore);
                PlayerPrefs.Save();
                highScore = currentScore;
            }

            if (winResultText != null)
            {
                winResultText.text = $"Cleared in {levelElapsedTime:F1}s!\nScore: {currentScore}\n{(newHighScore ? "NEW " : "")}High Score: {highScore}";
            }

            ShowPanel(winPanel);
        }

        private void StopAllAIVehicles()
        {
            if (activeLevelIndex == -1) return;
            GameObject root = levels[activeLevelIndex].levelRoot;
            if (root != null)
            {
                AIVehicleController[] aiVehicles = root.GetComponentsInChildren<AIVehicleController>();
                foreach (var ai in aiVehicles)
                {
                    ai.SetActive(false);
                }
            }
        }

        private void ShowPanel(GameObject panelToShow)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == panelToShow);
            if (hudPanel != null) hudPanel.SetActive(hudPanel == panelToShow);
            if (winPanel != null) winPanel.SetActive(winPanel == panelToShow);
            if (failPanel != null) failPanel.SetActive(failPanel == panelToShow);
        }
    }
}