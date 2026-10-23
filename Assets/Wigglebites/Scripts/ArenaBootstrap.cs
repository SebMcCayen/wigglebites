using System;
using System.IO;
using UnityEngine;
using Wigglebites.AI;
using Wigglebites.Application;
using Wigglebites.Config;
using Wigglebites.Input;
using Wigglebites.Persistence;
using Wigglebites.Presentation;
using Wigglebites.Simulation;
using Wigglebites.UI;

namespace Wigglebites
{
    public sealed class ArenaBootstrap : MonoBehaviour
    {
        private const int PlayerSnakeId = 1;
        private const int RandomSeed = 1337;

        private GameBalanceConfig _balance;
        private GameSimulation _simulation;
        private BotRosterController _botRoster;
        private TouchSteeringInput _input;
        private ArenaView _view;
        private FollowCamera _followCamera;
        private MatchController _match;
        private LocalSaveStore _saveStore;
        private LocalSaveData _saveData;
        private float _accumulator;
        private bool _resultPersisted;

        public MatchController Match => _match;
        public SnakeState Player => _simulation?.FindSnake(PlayerSnakeId);
        public float BestMass => _saveData?.bestMass ?? 0f;
        public bool ReducedEffects => _saveData?.reducedEffects ?? false;
        public bool SoundEnabled => _saveData?.soundEnabled ?? true;
        public SimVector2 DesiredHeading => _input?.DesiredHeading ?? SimVector2.Right;
        public bool IsBoostHeld => _input?.IsBoostHeld ?? false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntime()
        {
            if (FindFirstObjectByType<ArenaBootstrap>() != null) return;
            new GameObject("Wigglebites Runtime").AddComponent<ArenaBootstrap>();
        }

        private void Awake()
        {
            UnityEngine.Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            QualitySettings.vSyncCount = 0;

            _balance = Resources.Load<GameBalanceConfig>("GameBalance");
            if (_balance == null)
            {
                Debug.LogWarning("GameBalance asset was not found. Using serialized defaults.");
                _balance = ScriptableObject.CreateInstance<GameBalanceConfig>();
            }

            _input = gameObject.AddComponent<TouchSteeringInput>();
            _view = new GameObject("Arena View").AddComponent<ArenaView>();
            _view.transform.SetParent(transform, false);
            _view.Build(_balance.ArenaSize);

            var cameraObject = new GameObject("Arena Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<Camera>();
            _followCamera = cameraObject.AddComponent<FollowCamera>();
            _followCamera.ConfigureCamera();

            _saveStore = new LocalSaveStore(Path.Combine(UnityEngine.Application.persistentDataPath, "wigglebites-save.json"));
            _saveData = _saveStore.Load();
            _view.SetReducedEffects(_saveData.reducedEffects);
            BuildSimulation();
            _match = new MatchController(_simulation, PlayerSnakeId, _balance.MatchDurationSeconds);
            _view.Render(_simulation);
            _followCamera.SetTarget(_view.GetSnakeHeadPosition(PlayerSnakeId));

            var hud = gameObject.AddComponent<RuntimeHud>();
            hud.Configure(this);
            var feedback = gameObject.AddComponent<ArenaFeedback>();
            feedback.Configure(this, _view);
        }

        private void Update()
        {
            _input.Sample();
            if (_match.Phase != MatchPhase.Playing)
            {
                _accumulator = 0f;
                if (_match.Phase == MatchPhase.Results) PersistResult();
                _view.Render(_simulation);
                return;
            }

            if (Time.unscaledDeltaTime > 0f) _match.Advance(Time.unscaledDeltaTime);
            if (_match.Phase == MatchPhase.Results)
            {
                PersistResult();
                _view.Render(_simulation);
                return;
            }
            _accumulator = Mathf.Min(_accumulator + Time.deltaTime, _balance.SimulationStep * 4f);

            while (_accumulator >= _balance.SimulationStep)
            {
                _simulation.SetDesiredHeading(PlayerSnakeId, _input.DesiredHeading);
                _simulation.SetBoostRequested(PlayerSnakeId, _input.IsBoostHeld);
                _botRoster.Tick(_balance.SimulationStep);
                _simulation.Tick(_balance.SimulationStep);
                _match.EvaluateCurrentState();
                _accumulator -= _balance.SimulationStep;
                if (_match.Phase == MatchPhase.Results) break;
            }

            if (_match.Phase == MatchPhase.Results) PersistResult();

            _view.Render(_simulation);
            _followCamera.SetTarget(_view.GetSnakeHeadPosition(PlayerSnakeId));
        }

        public void StartMatch()
        {
            if (_match.Phase == MatchPhase.Menu) _match.Start();
        }

        public void TogglePause()
        {
            if (_match.Phase == MatchPhase.Playing)
            {
                _match.Pause();
                _accumulator = 0f;
            }
            else if (_match.Phase == MatchPhase.Paused)
            {
                _match.Resume();
            }
        }

        public void Replay()
        {
            _accumulator = 0f;
            _resultPersisted = false;
            _view.ResetSimulationObjects();
            BuildSimulation();
            _match = new MatchController(_simulation, PlayerSnakeId, _balance.MatchDurationSeconds);
            _match.Start();
            _view.Render(_simulation);
            _followCamera.SetTarget(_view.GetSnakeHeadPosition(PlayerSnakeId));
        }

        public void ToggleReducedEffects()
        {
            var updatedData = CopySaveData();
            updatedData.reducedEffects = !updatedData.reducedEffects;
            if (!TrySaveData(updatedData, "effects setting")) return;
            _saveData = updatedData;
            _view.SetReducedEffects(_saveData.reducedEffects);
        }

        public void ToggleSound()
        {
            var updatedData = CopySaveData();
            updatedData.soundEnabled = !updatedData.soundEnabled;
            if (TrySaveData(updatedData, "sound setting")) _saveData = updatedData;
        }

        private void PersistResult()
        {
            if (_resultPersisted) return;
            if (_match.FinalPlayerMass <= _saveData.bestMass)
            {
                _resultPersisted = true;
                return;
            }

            var updatedData = CopySaveData();
            updatedData.bestMass = _match.FinalPlayerMass;
            if (TrySaveData(updatedData, "best mass")) _saveData = updatedData;
            _resultPersisted = true;
        }

        private LocalSaveData CopySaveData() => new LocalSaveData
        {
            bestMass = _saveData.bestMass,
            reducedEffects = _saveData.reducedEffects,
            soundEnabled = _saveData.soundEnabled
        };

        private bool TrySaveData(LocalSaveData data, string description)
        {
            try
            {
                _saveStore.Save(data);
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                Debug.LogWarning($"Could not save local {description}: {exception.Message}");
                return false;
            }
        }

        private void BuildSimulation()
        {
            _simulation = new GameSimulation(_balance.CreateSimulationConfig());
            _simulation.AddSnake(PlayerSnakeId, true, SimVector2.Zero, SimVector2.Right);
            _botRoster = new BotRosterController(
                _simulation,
                RandomSeed,
                _balance.BotPerceptionRadius,
                _balance.BotRespawnDelaySeconds);
            _botRoster.SpawnInitialRoster(_balance.BotCount);

            var random = new System.Random(RandomSeed);
            var spawnExtent = (_balance.ArenaSize * 0.5f) - 2f;
            for (var index = 0; index < _balance.InitialFoodCount; index++)
            {
                var x = Lerp(-spawnExtent, spawnExtent, (float)random.NextDouble());
                var y = Lerp(-spawnExtent, spawnExtent, (float)random.NextDouble());
                var mass = Lerp(_balance.MinimumFoodMass, _balance.MaximumFoodMass, (float)random.NextDouble());
                _simulation.AddFood(index + 1, new SimVector2(x, y), mass);
            }
        }

        private static float Lerp(float minimum, float maximum, float amount) =>
            minimum + ((maximum - minimum) * amount);
    }
}
