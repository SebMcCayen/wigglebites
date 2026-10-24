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
        private SnakePalette[] _snakePalettes;
        private Material _protectedMaterial;
        private Material _eyeMaterial;
        private Material _pupilMaterial;
        private Material _mouthMaterial;
        private Material _tongueMaterial;
        private Material[] _foodMaterials;
        private Transform _dynamicRoot;
        private Transform _decorativeRoot;
        private bool _reducedEffects;

        public void Build(float arenaSize)
        {
            _snakePalettes = new[]
            {
                new SnakePalette(
                    CreateMaterial(new Color(0.03f, 0.72f, 1f), true, 0.88f),
                    CreateMaterial(new Color(0.14f, 0.34f, 1f), true, 0.82f)),
                new SnakePalette(
                    CreateMaterial(new Color(1f, 0.25f, 0.12f), true, 0.82f),
                    CreateMaterial(new Color(1f, 0.66f, 0.08f), true, 0.78f)),
                new SnakePalette(
                    CreateMaterial(new Color(0.38f, 0.94f, 0.18f), true, 0.82f),
                    CreateMaterial(new Color(0.03f, 0.58f, 0.28f), true, 0.78f)),
                new SnakePalette(
                    CreateMaterial(new Color(0.64f, 0.23f, 1f), true, 0.86f),
                    CreateMaterial(new Color(1f, 0.22f, 0.82f), true, 0.82f)),
                new SnakePalette(
                    CreateMaterial(new Color(0.62f, 0.9f, 1f), true, 0.9f),
                    CreateMaterial(new Color(0.1f, 0.57f, 1f), true, 0.84f))
            };
            _protectedMaterial = CreateMaterial(new Color(0.82f, 0.98f, 1f), true, 0.95f);
            _eyeMaterial = CreateMaterial(new Color(0.98f, 1f, 1f), false, 0.94f);
            _pupilMaterial = CreateMaterial(new Color(0.015f, 0.025f, 0.08f), false, 0.9f);
            _mouthMaterial = CreateMaterial(new Color(0.22f, 0.025f, 0.09f), false, 0.82f);
            _tongueMaterial = CreateMaterial(new Color(1f, 0.2f, 0.52f), true, 0.84f);
            _foodMaterials = new[]
            {
                CreateMaterial(new Color(0.14f, 0.87f, 1f), true, 0.9f),
                CreateMaterial(new Color(1f, 0.31f, 0.86f), true, 0.9f),
                CreateMaterial(new Color(1f, 0.83f, 0.24f), true, 0.9f),
                CreateMaterial(new Color(0.49f, 0.92f, 0.3f), true, 0.9f)
            };

            CreateArena(arenaSize);
            CreateLighting();
            CreateDynamicRoot();
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
            CreateDynamicRoot();
        }

        private void CreateDynamicRoot()
        {
            _dynamicRoot = new GameObject("Dynamic Objects").transform;
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
                    segments.Add(CreateSnakeSegment(snake, segments.Count));
                }

                var palette = PaletteForSnake(snake);
                var thickness = 0.9f + Mathf.Clamp01((snake.Mass - 10f) / 190f) * 0.42f;
                for (var segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                {
                    var segment = segments[segmentIndex];
                    if (segmentIndex >= snake.BodySegmentCount)
                    {
                        segment.gameObject.SetActive(false);
                        continue;
                    }

                    segment.gameObject.SetActive(true);
                    var renderer = segment.GetComponent<Renderer>();
                    renderer.sharedMaterial = snake.IsCombatProtected
                        ? _protectedMaterial
                        : segmentIndex > 0 && segmentIndex % 3 == 0
                            ? palette.Secondary
                            : palette.Primary;

                    var position = snake.GetBodyPosition(segmentIndex);
                    segment.position = new Vector3(position.X, segmentIndex == 0 ? 0.73f : 0.61f, position.Y);
                    if (segmentIndex == 0)
                    {
                        segment.rotation = Quaternion.LookRotation(
                            new Vector3(snake.Heading.X, 0f, snake.Heading.Y),
                            Vector3.up);
                        segment.localScale = new Vector3(1.28f, 0.94f, 1.48f) * thickness;
                    }
                    else
                    {
                        var tailProgress = segmentIndex / Mathf.Max(1f, snake.BodySegmentCount - 1f);
                        var taper = Mathf.Lerp(1f, 0.56f, tailProgress * tailProgress);
                        segment.localScale = Vector3.one * thickness * taper;
                    }
                }
            }
        }

        private Transform CreateSnakeSegment(SnakeState snake, int segmentIndex)
        {
            var segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            segment.name = segmentIndex == 0 ? $"Snake {snake.Id} Friendly Head" : $"Snake {snake.Id} Body {segmentIndex}";
            segment.transform.SetParent(_dynamicRoot, false);
            segment.GetComponent<Renderer>().sharedMaterial = PaletteForSnake(snake).Primary;
            Destroy(segment.GetComponent<Collider>());
            if (segmentIndex == 0) CreateFriendlyFace(segment.transform);
            return segment.transform;
        }

        private void CreateFriendlyFace(Transform head)
        {
            CreateFaceFeature("Left Eye", head, new Vector3(-0.29f, 0.25f, 0.43f), new Vector3(0.34f, 0.42f, 0.22f), _eyeMaterial);
            CreateFaceFeature("Right Eye", head, new Vector3(0.29f, 0.25f, 0.43f), new Vector3(0.34f, 0.42f, 0.22f), _eyeMaterial);
            CreateFaceFeature("Left Pupil", head, new Vector3(-0.29f, 0.27f, 0.61f), new Vector3(0.14f, 0.2f, 0.08f), _pupilMaterial);
            CreateFaceFeature("Right Pupil", head, new Vector3(0.29f, 0.27f, 0.61f), new Vector3(0.14f, 0.2f, 0.08f), _pupilMaterial);
            CreateFaceFeature("Smile", head, new Vector3(0f, -0.18f, 0.59f), new Vector3(0.42f, 0.18f, 0.08f), _mouthMaterial);
            CreateFaceFeature("Tongue", head, new Vector3(0f, -0.23f, 0.66f), new Vector3(0.19f, 0.08f, 0.06f), _tongueMaterial);
            CreateFaceFeature("Left Cheek", head, new Vector3(-0.43f, -0.03f, 0.48f), new Vector3(0.12f, 0.09f, 0.05f), _tongueMaterial);
            CreateFaceFeature("Right Cheek", head, new Vector3(0.43f, -0.03f, 0.48f), new Vector3(0.12f, 0.09f, 0.05f), _tongueMaterial);
        }

        private static Transform CreateFaceFeature(
            string objectName,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            var feature = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            feature.name = objectName;
            feature.transform.SetParent(parent, false);
            feature.transform.localPosition = localPosition;
            feature.transform.localScale = localScale;
            feature.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(feature.GetComponent<Collider>());
            return feature.transform;
        }

        private void RenderFood(IReadOnlyList<FoodState> food)
        {
            for (var index = 0; index < food.Count; index++)
            {
                var item = food[index];
                if (!item.IsActive)
                {
                    if (_foodObjects.TryGetValue(item.Id, out var consumed)) consumed.SetActive(false);
                    continue;
                }

                if (!_foodObjects.TryGetValue(item.Id, out var foodObject))
                {
                    var primitive = item.Id % 3 == 0
                        ? PrimitiveType.Cube
                        : item.Id % 3 == 1
                            ? PrimitiveType.Sphere
                            : PrimitiveType.Capsule;
                    foodObject = GameObject.CreatePrimitive(primitive);
                    foodObject.name = $"Candy {item.Id}";
                    foodObject.transform.SetParent(_dynamicRoot, false);
                    foodObject.transform.localScale = primitive == PrimitiveType.Capsule
                        ? new Vector3(0.32f, 0.26f, 0.32f)
                        : Vector3.one * 0.46f;
                    foodObject.GetComponent<Renderer>().sharedMaterial = _foodMaterials[item.Id % _foodMaterials.Length];
                    Destroy(foodObject.GetComponent<Collider>());
                    _foodObjects.Add(item.Id, foodObject);
                }

                foodObject.SetActive(true);
                var bob = _reducedEffects ? 0f : Mathf.Sin((Time.unscaledTime * 2f) + (item.Id * 0.37f)) * 0.08f;
                foodObject.transform.position = new Vector3(item.Position.X, 0.48f + bob, item.Position.Y);
                if (!_reducedEffects) foodObject.transform.Rotate(34f * Time.unscaledDeltaTime, 58f * Time.unscaledDeltaTime, 0f, Space.World);
            }
        }

        private void CreateArena(float arenaSize)
        {
            var floorMaterial = CreateMaterial(new Color(0.035f, 0.055f, 0.14f), false, 0.38f);
            var wallMaterial = CreateMaterial(new Color(0.07f, 0.09f, 0.22f), false, 0.58f);
            var cyanMaterial = CreateMaterial(new Color(0.08f, 0.78f, 1f), true, 0.82f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Deep Navy Arena Base";
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(arenaSize, 0.25f, arenaSize);
            floor.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            Destroy(floor.GetComponent<Collider>());

            CreateHexField(arenaSize);
            var half = arenaSize * 0.5f;
            CreateWall("North Wall", new Vector3(0f, 0.55f, half), new Vector3(arenaSize, 1.1f, 0.7f), wallMaterial, cyanMaterial);
            CreateWall("South Wall", new Vector3(0f, 0.55f, -half), new Vector3(arenaSize, 1.1f, 0.7f), wallMaterial, cyanMaterial);
            CreateWall("East Wall", new Vector3(half, 0.55f, 0f), new Vector3(0.7f, 1.1f, arenaSize), wallMaterial, cyanMaterial);
            CreateWall("West Wall", new Vector3(-half, 0.55f, 0f), new Vector3(0.7f, 1.1f, arenaSize), wallMaterial, cyanMaterial);
            CreateDecorations(arenaSize);
        }

        private void CreateHexField(float arenaSize)
        {
            const float radius = 2.25f;
            var drawRadius = radius * 0.91f;
            var horizontalStep = Mathf.Sqrt(3f) * radius;
            var verticalStep = 1.5f * radius;
            var half = (arenaSize * 0.5f) - radius;
            var vertices = new List<Vector3>();
            var triangles = new[] { new List<int>(), new List<int>(), new List<int>() };
            var rowLimit = Mathf.CeilToInt(arenaSize / verticalStep) + 2;
            var columnLimit = Mathf.CeilToInt(arenaSize / horizontalStep) + 2;

            for (var row = -rowLimit; row <= rowLimit; row++)
            {
                var centerZ = row * verticalStep;
                var rowOffset = (Mathf.Abs(row) % 2) * horizontalStep * 0.5f;
                for (var column = -columnLimit; column <= columnLimit; column++)
                {
                    var centerX = (column * horizontalStep) + rowOffset;
                    if (Mathf.Abs(centerX) > half || Mathf.Abs(centerZ) > half) continue;

                    var start = vertices.Count;
                    vertices.Add(new Vector3(centerX, -0.045f, centerZ));
                    for (var corner = 0; corner < 6; corner++)
                    {
                        var angle = Mathf.Deg2Rad * (30f + (corner * 60f));
                        vertices.Add(new Vector3(
                            centerX + (Mathf.Cos(angle) * drawRadius),
                            -0.045f,
                            centerZ + (Mathf.Sin(angle) * drawRadius)));
                    }

                    var tileTriangles = triangles[Mathf.Abs(row + column) % triangles.Length];
                    for (var corner = 0; corner < 6; corner++)
                    {
                        tileTriangles.Add(start);
                        tileTriangles.Add(start + 1 + corner);
                        tileTriangles.Add(start + 1 + ((corner + 1) % 6));
                    }
                }
            }

            var mesh = new Mesh { name = "Neon Arena Hex Field" };
            mesh.SetVertices(vertices);
            mesh.subMeshCount = triangles.Length;
            for (var index = 0; index < triangles.Length; index++) mesh.SetTriangles(triangles[index], index);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var field = new GameObject("Layered Hex Floor");
            field.transform.SetParent(transform, false);
            field.AddComponent<MeshFilter>().sharedMesh = mesh;
            field.AddComponent<MeshRenderer>().sharedMaterials = new[]
            {
                CreateMaterial(new Color(0.075f, 0.13f, 0.28f), false, 0.54f),
                CreateMaterial(new Color(0.09f, 0.17f, 0.34f), false, 0.58f),
                CreateMaterial(new Color(0.11f, 0.15f, 0.32f), false, 0.52f)
            };
        }

        private void CreateDecorations(float arenaSize)
        {
            _decorativeRoot = new GameObject("Neon Arena Decorations").transform;
            _decorativeRoot.SetParent(transform, false);
            var half = arenaSize * 0.5f;
            var outerEdge = half + 1.4f;
            var cyan = CreateMaterial(new Color(0.08f, 0.86f, 1f), true, 0.86f);
            var violet = CreateMaterial(new Color(0.63f, 0.22f, 1f), true, 0.88f);
            var pink = CreateMaterial(new Color(1f, 0.2f, 0.72f), true, 0.84f);
            var rock = CreateMaterial(new Color(0.18f, 0.23f, 0.38f), false, 0.36f);
            var foliage = CreateMaterial(new Color(0.16f, 0.64f, 0.3f), false, 0.52f);

            CreateCrystalCluster(new Vector3(outerEdge, 0f, outerEdge), cyan, violet);
            CreateCrystalCluster(new Vector3(-outerEdge, 0f, -outerEdge), violet, pink);
            CreateCrystalCluster(new Vector3(-outerEdge, 0f, outerEdge), cyan, pink);
            CreateRockGarden(new Vector3(outerEdge, 0f, -outerEdge), rock, foliage);
            CreateRockGarden(new Vector3(-outerEdge, 0f, 8f), rock, foliage);
            CreateRockGarden(new Vector3(outerEdge, 0f, -10f), rock, foliage);
        }

        private void CreateCrystalCluster(Vector3 center, Material primary, Material secondary)
        {
            var cluster = new GameObject("Glowing Crystal Cluster").transform;
            cluster.SetParent(_decorativeRoot, false);
            cluster.localPosition = center;
            CreateDecorationPrimitive("Tall Crystal", PrimitiveType.Cube, cluster, new Vector3(0f, 1.2f, 0f), new Vector3(0.55f, 2.4f, 0.55f), Quaternion.Euler(0f, 24f, 8f), primary);
            CreateDecorationPrimitive("Side Crystal A", PrimitiveType.Cube, cluster, new Vector3(-0.7f, 0.72f, 0.1f), new Vector3(0.45f, 1.45f, 0.45f), Quaternion.Euler(0f, -18f, -19f), secondary);
            CreateDecorationPrimitive("Side Crystal B", PrimitiveType.Cube, cluster, new Vector3(0.68f, 0.62f, -0.16f), new Vector3(0.42f, 1.25f, 0.42f), Quaternion.Euler(0f, 32f, 22f), primary);
        }

        private void CreateRockGarden(Vector3 center, Material rock, Material foliage)
        {
            var garden = new GameObject("Rounded Rock Garden").transform;
            garden.SetParent(_decorativeRoot, false);
            garden.localPosition = center;
            CreateDecorationPrimitive("Rock A", PrimitiveType.Sphere, garden, new Vector3(0f, 0.42f, 0f), new Vector3(1.55f, 0.82f, 1.3f), Quaternion.Euler(0f, 18f, 0f), rock);
            CreateDecorationPrimitive("Rock B", PrimitiveType.Sphere, garden, new Vector3(1.1f, 0.3f, 0.32f), new Vector3(0.9f, 0.62f, 0.78f), Quaternion.Euler(0f, -22f, 0f), rock);
            CreateDecorationPrimitive("Shrub A", PrimitiveType.Sphere, garden, new Vector3(-0.72f, 0.34f, 0.74f), new Vector3(0.7f, 0.62f, 0.7f), Quaternion.identity, foliage);
            CreateDecorationPrimitive("Shrub B", PrimitiveType.Sphere, garden, new Vector3(-1.05f, 0.25f, 0.35f), new Vector3(0.52f, 0.48f, 0.52f), Quaternion.identity, foliage);
        }

        private static void CreateDecorationPrimitive(
            string objectName,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            var decoration = GameObject.CreatePrimitive(primitiveType);
            decoration.name = objectName;
            decoration.transform.SetParent(parent, false);
            decoration.transform.localPosition = localPosition;
            decoration.transform.localScale = localScale;
            decoration.transform.localRotation = localRotation;
            decoration.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(decoration.GetComponent<Collider>());
        }

        private SnakePalette PaletteForSnake(SnakeState snake)
        {
            if (snake.IsPlayer) return _snakePalettes[0];
            return _snakePalettes[1 + (Mathf.Abs(snake.Id) % (_snakePalettes.Length - 1))];
        }

        private void CreateWall(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material wallMaterial,
            Material trimMaterial)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = objectName;
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            Destroy(wall.GetComponent<Collider>());

            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = $"{objectName} Neon Trim";
            trim.transform.SetParent(transform, false);
            trim.transform.localPosition = position + (Vector3.up * 0.62f);
            trim.transform.localScale = new Vector3(
                scale.x > scale.z ? scale.x : 0.18f,
                0.18f,
                scale.z >= scale.x ? scale.z : 0.18f);
            trim.GetComponent<Renderer>().sharedMaterial = trimMaterial;
            Destroy(trim.GetComponent<Collider>());
        }

        private void CreateLighting()
        {
            var lightObject = new GameObject("Soft Arena Key Light");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var lightComponent = lightObject.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.color = new Color(0.77f, 0.86f, 1f);
            lightComponent.intensity = 1.3f;
            lightComponent.shadows = LightShadows.Soft;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.19f, 0.25f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.12f, 0.25f);
            RenderSettings.ambientGroundColor = new Color(0.025f, 0.035f, 0.09f);
        }

        private static Material CreateMaterial(Color color, bool emissive, float smoothness)
        {
            var shader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            shader = shader ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.05f);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.55f);
            }
            return material;
        }

        private readonly struct SnakePalette
        {
            public SnakePalette(Material primary, Material secondary)
            {
                Primary = primary;
                Secondary = secondary;
            }

            public Material Primary { get; }
            public Material Secondary { get; }
        }
    }
}
