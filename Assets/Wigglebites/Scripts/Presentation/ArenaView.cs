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
        private Material _botMaterial;
        private Material _protectedMaterial;
        private Material _foodMaterial;
        private Transform _dynamicRoot;

        public void Build(float arenaSize)
        {
            _playerMaterial = CreateMaterial(new Color(0.09f, 0.7f, 1f), true);
            _botMaterial = CreateMaterial(new Color(1f, 0.28f, 0.12f), true);
            _protectedMaterial = CreateMaterial(new Color(0.75f, 0.95f, 1f), true);
            _foodMaterial = CreateMaterial(new Color(1f, 0.28f, 0.85f), true);

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
                    segment.GetComponent<Renderer>().sharedMaterial = snake.IsPlayer ? _playerMaterial : _botMaterial;
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
                        : snake.IsPlayer ? _playerMaterial : _botMaterial;
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
                    foodObject.GetComponent<Renderer>().sharedMaterial = _foodMaterial;
                    Destroy(foodObject.GetComponent<Collider>());
                    _foodObjects.Add(item.Id, foodObject);
                }

                foodObject.SetActive(true);
                foodObject.transform.position = new Vector3(item.Position.X, 0.45f, item.Position.Y);
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
        }

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
