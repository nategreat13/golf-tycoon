using UnityEngine;
using GolfGame.Economy;
using GolfGame.Building;
using GolfGame.Progression;

namespace GolfGame.Core
{
    public enum GameState
    {
        Loading,
        MainMenu,
        Playing,
        Building,
        DrivingRange,
        Visiting
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool skipMainMenu;

        public GameState CurrentState { get; private set; } = GameState.Loading;

        private SaveSystem saveSystem;
        private TimeManager timeManager;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void InitializeSystems()
        {
            saveSystem = gameObject.AddComponent<SaveSystem>();
            timeManager = gameObject.AddComponent<TimeManager>();

            ServiceLocator.Register(saveSystem);
            ServiceLocator.Register(timeManager);

            var currencyManager = gameObject.AddComponent<CurrencyManager>();
            ServiceLocator.Register(currencyManager);

            var propertyManager = gameObject.AddComponent<PropertyManager>();
            ServiceLocator.Register(propertyManager);

            var incomeCalculator = gameObject.AddComponent<IncomeCalculator>();
            ServiceLocator.Register(incomeCalculator);

            var aiGolferSim = gameObject.AddComponent<AIGolferSimulator>();
            ServiceLocator.Register(aiGolferSim);

            var reputationSystem = gameObject.AddComponent<ReputationSystem>();
            ServiceLocator.Register(reputationSystem);

            var unlockManager = gameObject.AddComponent<UnlockManager>();
            ServiceLocator.Register(unlockManager);

            // Load saved data
            saveSystem.Load();

            // Calculate offline progress
            var offlineCalc = new OfflineProgressCalculator();
            long offlineEarnings = offlineCalc.Calculate(
                timeManager.GetLastSaveTime(),
                incomeCalculator.GetIncomePerSecond()
            );
            if (offlineEarnings > 0)
            {
                currencyManager.Add(offlineEarnings);
                Debug.Log($"Offline earnings: {offlineEarnings}");
            }

            timeManager.UpdateLastSaveTime();
        }

        private void Start()
        {
            if (skipMainMenu)
            {
                ChangeState(GameState.Playing);
            }
            else
            {
                ChangeState(GameState.MainMenu);
            }
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"Game state changed to: {newState}");

            switch (newState)
            {
                case GameState.MainMenu:
                    SceneLoader.LoadScene("MainMenu");
                    break;
                case GameState.Playing:
                    SceneLoader.LoadScene("GolfGameplay");
                    break;
                case GameState.Building:
                    SceneLoader.LoadScene("CourseBuilder");
                    break;
                case GameState.DrivingRange:
                    SceneLoader.LoadScene("DrivingRange");
                    break;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                saveSystem.Save();
                timeManager.UpdateLastSaveTime();
            }
        }

        private void OnApplicationQuit()
        {
            saveSystem.Save();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Clear();
                EventBus.Clear();
            }
        }
    }
}
