using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Wigglebites.Simulation;

namespace Wigglebites.Presentation
{
    public sealed class ArenaView : MonoBehaviour
    {
        private readonly Dictionary<int, List<Transform>> _snakeSegments = new Dictionary<int, List<Transform>>();
        private readonly Dictionary<int, GameObject> _foodObjects = new Dictionary<int, GameObject>();
        private Material _playerMaterial;
        private Material[] _botMaterials;
        private Material _protectedMaterial;
        private Material[] _foodMaterials;
        private Transform _dynamicRoot;
        private Transform _decorativeRoot;
        private bool _reducedEffects;

        public void Build(float arenaSize)
        {
            _playerMaterial = CreateMaterial(new Color(0.09f, 0.7f, 1f), true);
            _botMaterials = new[]
            {
                CreateMaterial(new Color(1f, 0.28f, 0.12f), true),
                CreateMaterial(new Color(0.49f, 0.92f, 0.3f), true),
                CreateMaterial(new Color(0.6f, 0.28f, 1f), true),
                CreateMaterial(new Color(0.2f, 0.78f, 1f), true)
            };
            _protectedMaterial = CreateMaterial(new Color(0.75f, 0.95f, 1f), true);
            _foodMaterials = new[]
            {
                CreateMaterial(new Color(0.14f, 0.87f, 1f), true),
                CreateMaterial(new Color(1f, 0.31f, 0.86f), true),
                CreateMaterial(new Color(1f, 0.83f, 0.24f), true),
                CreateMaterial(new Color(0.49f, 0.92f, 0.3f), true)
            };

            CreateArena(arenaSize);
            CreateLighting();
            _dynamicRoot = new GameObject("DynamicObjects").transform;
            _dynamicRoot.SetParent(transform, false);
        }

        public void Render(GameSimulation simulation)
        {
            RenderSnakes(simulation.Snakes);
            RenderFood(simulation.Food);
        }

        public void SetReducedEffects(bool reduced)
        {
            _reducedEffects = reduced;
            if (_decorativeRoot != null) _decorativeRoot.gameObject.SetActive(!reduced);
        }

        public Vector3 GetSnakeHeadPosition(int snakeId)
        {
            return _snakeSegments.TryGetValue(snakeId, out var segments) && segments.Count > 0
                ? segments[0].position
                : Vector3.zero;
        }

        public void ResetSimulationObjects()
        {
            if (_dynamicRoot != null)
            {
                _dynamicRoot.gameObject.SetActive(false);
                Destroy(_dynamicRoot.gameObject);
            }
            _snakeSegments.Clear();
            _foodObjects.Clear();
            _dynamicRoot = new GameObject("DynamicObjects").transform;
            _dynamicRoot.SetParent(transform, false);
        }

        private void RenderSnakes(IReadOnlyList<SnakeState> snakes)
        {
            for (var snakeIndex = 0; snakeIndex < snakes.Count; snakeIndex++)
            {
                var snake = snakes[snakeIndex];
                if (!_snakeSegments.TryGetValue(snake.Id, out var segments))
                {
                    segments = new List<Transform>();
                    _snakeSegments.Add(snake.Id, segments);
                }

                if (!snake.IsAlive)
                {
                    for (var segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                    {
                        segments[segmentIndex].gameObject.SetActive(false);
                    }
                    continue;
                }

                while (segments.Count < snake.BodySegmentCount)
                {
                    var segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    segment.name = segments.Count == 0 ? $"Snake {snake.Id} Head" : $"Snake {snake.Id} Segment {segments.Count}";
                    segment.transform.SetParent(_dynamicRoot, false);
                    segment.transform.localScale = Vector3.one * (segments.Count == 0 ? 1.35f : 1f);
                    segment.GetComponent<Renderer>().sharedMaterial = MaterialForSnake(snake);
                    Destroy(segment.GetComponent<Collider>());
                    segments.Add(segment.transform);
                }

                for (var segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                {
                    if (segmentIndex >= snake.BodySegmentCount)
                    {
                        segments[segmentIndex].gameObject.SetActive(false);
                        continue;
                    }
                    segments[segmentIndex].gameObject.SetActive(true);
                    segments[segmentIndex].GetComponent<Renderer>().sharedMaterial = snake.IsCombatProtected
                        ? _protectedMaterial
                        : MaterialForSnake(snake);
                    var position = snake.GetBodyPosition(segmentIndex);
                    segments[segmentIndex].position = new Vector3(position.X, 0.65f, position.Y);
                }
            }
        }

        private void RenderFood(IReadOnlyList<FoodState> food)
        {
            for (var index = 0; index < food.Count; index++)
            {
                var item = food[index];
                if (!item.IsActive)
                {
                    if (_foodObjects.TryGetValue(item.Id, out var consumed))
                    {
                        consumed.SetActive(false);
                    }
                    continue;
                }

                if (!_foodObjects.TryGetValue(item.Id, out var foodObject))
                {
                    foodObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    foodObject.name = $"Food {item.Id}";
                    foodObject.transform.SetParent(_dynamicRoot, false);
                    foodObject.transform.localScale = Vector3.one * 0.45f;
                    foodObject.GetComponent<Renderer>().sharedMaterial = _foodMaterials[item.Id % _foodMaterials.Length];
                    Destroy(foodObject.GetComponent<Collider>());
                    _foodObjects.Add(item.Id, foodObject);
                }

                foodObject.SetActive(true);
                var bob = _reducedEffects ? 0f : Mathf.Sin((Time.unscaledTime * 2f) + (item.Id * 0.37f)) * 0.08f;
                foodObject.transform.position = new Vector3(item.Position.X, 0.45f + bob, item.Position.Y);
                if (!_reducedEffects) foodObject.transform.Rotate(0f, 50f * Time.unscaledDeltaTime, 0f, Space.World);
            }
        }

        private void CreateArena(float arenaSize)
        {
            var floorMaterial = CreateMaterial(new Color(0.06f, 0.1f, 0.23f), false);
            var wallMaterial = CreateMaterial(new Color(0.08f, 0.6f, 0.95f), true);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Arena Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(arenaSize, 0.25f, arenaSize);
            floor.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

            var half = arenaSize * 0.5f;
            CreateWall("North Wall", new Vector3(0f, 0.75f, half), new Vector3(arenaSize, 1.5f, 0.5f), wallMaterial);
            CreateWall("South Wall", new Vector3(0f, 0.75f, -half), new Vector3(arenaSize, 1.5f, 0.5f), wallMaterial);
            CreateWall("East Wall", new Vector3(half, 0.75f, 0f), new Vector3(0.5f, 1.5f, arenaSize), wallMaterial);
            CreateWall("West Wall", new Vector3(-half, 0.75f, 0f), new Vector3(0.5f, 1.5f, arenaSize), wallMaterial);
            CreateDecorations(arenaSize, wallMaterial);
        }

        private void CreateDecorations(float arenaSize, Material neonMaterial)
        {
            _decorativeRoot = new GameObject("Neon Decorations").transform;
            _decorativeRoot.SetParent(transform, false);
            var gridMaterial = CreateMaterial(new Color(0.08f, 0.35f, 0.58f), true);
            var half = arenaSize * 0.5f;
            const int lineCount = 9;
            for (var index = 1; index < lineCount; index++)
            {
                var offset = Mathf.Lerp(-half, half, index / (float)lineCount);
                CreateDecoration($"Grid X {index}", new Vector3(offset, -0.055f, 0f), new Vector3(0.035f, 0.02f, arenaSize), gridMaterial);
                CreateDecoration($"Grid Z {index}", new Vector3(0f, -0.05f, offset), new Vector3(arenaSize, 0.02f, 0.035f), gridMaterial);
            }

            var pylonOffset = half - 2f;
            CreateDecoration("North East Pylon", new Vector3(pylonOffset, 1.25f, pylonOffset), new Vector3(0.65f, 2.5f, 0.65f), neonMaterial);
            CreateDecoration("North West Pylon", new Vector3(-pylonOffset, 1.25f, pylonOffset), new Vector3(0.65f, 2.5f, 0.65f), neonMaterial);
            CreateDecoration("South East Pylon", new Vector3(pylonOffset, 1.25f, -pylonOffset), new Vector3(0.65f, 2.5f, 0.65f), neonMaterial);
            CreateDecoration("South West Pylon", new Vector3(-pylonOffset, 1.25f, -pylonOffset), new Vector3(0.65f, 2.5f, 0.65f), neonMaterial);
        }

        private void CreateDecoration(string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var decoration = GameObject.CreatePrimitive(PrimitiveType.Cube);
            decoration.name = objectName;
            decoration.transform.SetParent(_decorativeRoot, false);
            decoration.transform.localPosition = position;
            decoration.transform.localScale = scale;
            decoration.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(decoration.GetComponent<Collider>());
        }

        private Material MaterialForSnake(SnakeState snake) => snake.IsPlayer
            ? _playerMaterial
            : _botMaterials[Mathf.Abs(snake.Id) % _botMaterials.Length];

        private void CreateWall(string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = objectName;
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
        }

        private void CreateLighting()
        {
            var lightObject = new GameObject("Arena Light");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var lightComponent = lightObject.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.color = new Color(0.75f, 0.84f, 1f);
            lightComponent.intensity = 1.2f;
            lightComponent.shadows = LightShadows.Soft;
        }

        private static Material CreateMaterial(Color color, bool emissive)
        {
            var shader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            shader = shader ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.8f);
            }
            return material;
        }
    }
}
