using System;
using UnityEngine;
using Wigglebites.AI;
using Wigglebites.Config;
using Wigglebites.Input;
using Wigglebites.Presentation;
using Wigglebites.Simulation;

namespace Wigglebites
{
    public sealed class ArenaBootstrap : MonoBehaviour
    {
        private const int PlayerSnakeId = 1;
        private const int BotSnakeId = 2;
        private const int RandomSeed = 1337;

        private readonly CollectorBotBrain _botBrain = new CollectorBotBrain();
        private GameBalanceConfig _balance;
        private GameSimulation _simulation;
        private TouchSteeringInput _input;
        private ArenaView _view;
        private FollowCamera _followCamera;
        private float _accumulator;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntime()
        {
            if (FindFirstObjectByType<ArenaBootstrap>() != null) return;
            new GameObject("Wigglebites Runtime").AddComponent<ArenaBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
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

            BuildSimulation();
            _view.Render(_simulation);
            _followCamera.SetTarget(_view.GetSnakeHeadPosition(PlayerSnakeId));
        }

        private void Update()
        {
            _input.Sample();
            _accumulator = Mathf.Min(_accumulator + Time.deltaTime, _balance.SimulationStep * 4f);

            while (_accumulator >= _balance.SimulationStep)
            {
                _simulation.SetDesiredHeading(PlayerSnakeId, _input.DesiredHeading);
                _simulation.SetBoostRequested(PlayerSnakeId, _input.IsBoostHeld);
                var bot = _simulation.FindSnake(BotSnakeId);
                _simulation.SetDesiredHeading(BotSnakeId, _botBrain.ChooseHeading(bot, _simulation.Food));
                _simulation.Tick(_balance.SimulationStep);
                _accumulator -= _balance.SimulationStep;
            }

            _view.Render(_simulation);
            _followCamera.SetTarget(_view.GetSnakeHeadPosition(PlayerSnakeId));
        }

        private void BuildSimulation()
        {
            _simulation = new GameSimulation(_balance.CreateSimulationConfig());
            _simulation.AddSnake(PlayerSnakeId, true, new SimVector2(-4f, 0f), SimVector2.Right);
            _simulation.AddSnake(BotSnakeId, false, new SimVector2(4f, 0f), new SimVector2(-1f, 0f));

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
