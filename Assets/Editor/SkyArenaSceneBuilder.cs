using System.IO;
using AetherFrame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AetherFrame.EditorTools
{
    // Creates the complete first playable from primitives so the repository does not need
    // imported assets, prefab files, or a preauthored Unity scene to be useful.
    public static class SkyArenaSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SkyArenaPrototype.unity";

        [InitializeOnLoadMethod]
        private static void CreateSceneOnProjectLoad()
        {
            if (!File.Exists(ScenePath))
            {
                EditorApplication.delayCall += () => GenerateSkyArenaPrototype(false);
            }
        }

        [MenuItem("AETHER FRAME/Build SkyArenaPrototype Scene")]
        public static void GenerateSkyArenaPrototype()
        {
            GenerateSkyArenaPrototype(true);
        }

        private static void GenerateSkyArenaPrototype(bool showDialog)
        {
            Directory.CreateDirectory("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SkyArenaPrototype";

            Material groundMaterial = CreateMaterial("Aether Ground", new Color(0.13f, 0.17f, 0.19f));
            Material platformMaterial = CreateMaterial("Aether Platforms", new Color(0.28f, 0.32f, 0.34f));
            Material towerMaterial = CreateMaterial("Aether Towers", new Color(0.18f, 0.22f, 0.27f));
            Material playerMaterial = CreateMaterial("Player Mecha Blue", new Color(0.1f, 0.55f, 0.95f));
            Material enemyMaterial = CreateMaterial("Drone Amber", new Color(0.95f, 0.42f, 0.12f));
            Material laserMaterial = CreateMaterial("Laser Cyan", new Color(0.2f, 1f, 1f));
            Material missileMaterial = CreateMaterial("Missile White", new Color(0.9f, 0.95f, 1f));
            Material enemyShotMaterial = CreateMaterial("Enemy Shot Red", new Color(1f, 0.18f, 0.1f));

            SetupLighting();
            SetupArena(groundMaterial, platformMaterial, towerMaterial);

            Projectile laserPrefab = CreateProjectilePrefab("Player Laser Projectile", laserMaterial, 82f, 14f, 2.2f, new Vector3(0.18f, 0.18f, 1.5f));
            HomingMissile missilePrefab = CreateMissilePrefab(missileMaterial);
            Projectile enemyShotPrefab = CreateProjectilePrefab("Enemy Drone Bolt", enemyShotMaterial, 24f, 9f, 5f, new Vector3(0.35f, 0.35f, 0.35f));

            GameObject player = CreatePlayer(playerMaterial, laserPrefab, missilePrefab);
            CreateCamera(player);
            CreateDrones(player.transform, enemyMaterial, enemyShotPrefab);
            CreateHud(player);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.Refresh();

            if (showDialog && !Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("AETHER FRAME", "SkyArenaPrototype scene generated successfully.", "Launch");
            }
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }

        private static void SetupLighting()
        {
            RenderSettings.ambientLight = new Color(0.28f, 0.34f, 0.42f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.42f, 0.56f, 0.68f);
            RenderSettings.fogDensity = 0.006f;

            GameObject sun = new GameObject("Cold Dawn Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(0.85f, 0.95f, 1f);
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        }

        private static void SetupArena(Material groundMaterial, Material platformMaterial, Material towerMaterial)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Sky Arena Ground Plane";
            ground.transform.position = new Vector3(0f, -18f, 0f);
            ground.transform.localScale = new Vector3(260f, 2f, 260f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            Vector3[] platformPositions =
            {
                new Vector3(-42f, 2f, 30f),
                new Vector3(36f, 8f, -20f),
                new Vector3(0f, 18f, 58f),
                new Vector3(58f, -4f, 48f),
                new Vector3(-64f, 12f, -52f)
            };

            foreach (Vector3 position in platformPositions)
            {
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = "Floating Combat Platform";
                platform.transform.position = position;
                platform.transform.localScale = new Vector3(24f, 2.2f, 18f);
                platform.GetComponent<Renderer>().sharedMaterial = platformMaterial;
            }

            Vector3[] towerPositions =
            {
                new Vector3(-80f, 12f, 0f),
                new Vector3(82f, 18f, -10f),
                new Vector3(-18f, 24f, -78f),
                new Vector3(24f, 28f, 82f),
                new Vector3(0f, 36f, 0f)
            };

            foreach (Vector3 position in towerPositions)
            {
                GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = "Vertical Arena Obstacle";
                tower.transform.position = position;
                tower.transform.localScale = new Vector3(5f, 28f, 5f);
                tower.GetComponent<Renderer>().sharedMaterial = towerMaterial;
            }
        }

        private static GameObject CreatePlayer(Material playerMaterial, Projectile laserPrefab, HomingMissile missilePrefab)
        {
            GameObject player = new GameObject("AetherFrame Player Mecha");
            player.transform.position = new Vector3(0f, 14f, -42f);
            player.AddComponent<TeamMember>().team = Team.Player;

            Health health = player.AddComponent<Health>();
            health.maxHealth = 150f;
            health.reloadSceneOnDeath = true;

            PlayerFlightController flight = player.AddComponent<PlayerFlightController>();
            LockOnSystem lockOn = player.AddComponent<LockOnSystem>();
            WeaponController weapons = player.AddComponent<WeaponController>();
            weapons.lockOnSystem = lockOn;
            weapons.laserPrefab = laserPrefab;
            weapons.missilePrefab = missilePrefab;

            CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
            collider.radius = 1.4f;
            collider.height = 4.2f;
            collider.direction = 2;

            GameObject visualRoot = new GameObject("Mecha Visual Root");
            visualRoot.transform.SetParent(player.transform, false);
            flight.visualRoot = visualRoot.transform;

            CreateChildPrimitive(visualRoot.transform, PrimitiveType.Capsule, "Core Fuselage", Vector3.zero, new Vector3(1.2f, 1.7f, 1.2f), playerMaterial);
            CreateChildPrimitive(visualRoot.transform, PrimitiveType.Cube, "Shoulder Wing L", new Vector3(-1.45f, 0.15f, 0f), new Vector3(1.8f, 0.22f, 0.75f), playerMaterial);
            CreateChildPrimitive(visualRoot.transform, PrimitiveType.Cube, "Shoulder Wing R", new Vector3(1.45f, 0.15f, 0f), new Vector3(1.8f, 0.22f, 0.75f), playerMaterial);
            CreateChildPrimitive(visualRoot.transform, PrimitiveType.Cube, "Forward Sensor", new Vector3(0f, 0.25f, 1.1f), new Vector3(0.45f, 0.35f, 0.65f), playerMaterial);

            GameObject laserMuzzle = new GameObject("Laser Muzzle");
            laserMuzzle.transform.SetParent(player.transform, false);
            laserMuzzle.transform.localPosition = new Vector3(0.6f, -0.2f, 2.15f);
            weapons.laserMuzzle = laserMuzzle.transform;

            GameObject missileMuzzle = new GameObject("Missile Muzzle");
            missileMuzzle.transform.SetParent(player.transform, false);
            missileMuzzle.transform.localPosition = new Vector3(-0.75f, -0.25f, 1.7f);
            weapons.missileMuzzle = missileMuzzle.transform;

            return player;
        }

        private static void CreateCamera(GameObject player)
        {
            GameObject cameraObject = new GameObject("Third Person Combat Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.43f, 0.57f, 0.7f);
            camera.fieldOfView = 68f;
            camera.tag = "MainCamera";

            ThirdPersonCameraController follow = cameraObject.AddComponent<ThirdPersonCameraController>();
            follow.target = player.transform;
            follow.playerFlight = player.GetComponent<PlayerFlightController>();
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 5f, -12f);
        }

        private static void CreateDrones(Transform player, Material enemyMaterial, Projectile enemyShotPrefab)
        {
            Vector3[] positions =
            {
                new Vector3(-38f, 18f, 38f),
                new Vector3(36f, 22f, 26f),
                new Vector3(8f, 34f, 72f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject drone = new GameObject("Aether Drone " + (i + 1));
                drone.transform.position = positions[i];
                drone.AddComponent<TeamMember>().team = Team.Enemy;

                Health health = drone.AddComponent<Health>();
                health.maxHealth = 55f;

                SphereCollider collider = drone.AddComponent<SphereCollider>();
                collider.radius = 1.7f;

                EnemyDroneAI ai = drone.AddComponent<EnemyDroneAI>();
                ai.player = player;
                ai.projectilePrefab = enemyShotPrefab;

                CreateChildPrimitive(drone.transform, PrimitiveType.Sphere, "Drone Core", Vector3.zero, new Vector3(1.4f, 1.4f, 1.4f), enemyMaterial);
                CreateChildPrimitive(drone.transform, PrimitiveType.Cube, "Drone Stabilizer", new Vector3(0f, 0f, -1.1f), new Vector3(2.5f, 0.18f, 0.45f), enemyMaterial);

                GameObject firePoint = new GameObject("Drone Fire Point");
                firePoint.transform.SetParent(drone.transform, false);
                firePoint.transform.localPosition = new Vector3(0f, 0f, 1.8f);
                ai.firePoint = firePoint.transform;
            }
        }

        private static void CreateHud(GameObject player)
        {
            GameObject canvasObject = new GameObject("Aether HUD Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            HUDController hud = canvasObject.AddComponent<HUDController>();
            hud.playerHealth = player.GetComponent<Health>();
            hud.playerFlight = player.GetComponent<PlayerFlightController>();
            hud.lockOnSystem = player.GetComponent<LockOnSystem>();

            hud.healthFill = CreateBar(canvas.transform, "Health Bar", new Vector2(24f, -26f), new Color(0.95f, 0.2f, 0.24f));
            hud.energyFill = CreateBar(canvas.transform, "Aether Energy Bar", new Vector2(24f, -56f), new Color(0.2f, 0.75f, 1f));
            hud.targetText = CreateText(canvas.transform, "Target Text", new Vector2(24f, -92f), "TARGET: NO LOCK", 18, TextAnchor.MiddleLeft);
            hud.enemyCountText = CreateText(canvas.transform, "Enemy Count Text", new Vector2(24f, -120f), "DRONES: 3", 18, TextAnchor.MiddleLeft);

            Text crosshair = CreateText(canvas.transform, "Crosshair", Vector2.zero, "+", 34, TextAnchor.MiddleCenter);
            RectTransform crosshairRect = crosshair.rectTransform;
            crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.sizeDelta = new Vector2(80f, 80f);
            crosshair.color = new Color(0.78f, 0.95f, 1f, 0.9f);
        }

        private static Image CreateBar(Transform parent, string name, Vector2 anchoredPosition, Color fillColor)
        {
            GameObject back = new GameObject(name + " Backing");
            back.transform.SetParent(parent, false);
            Image backing = back.AddComponent<Image>();
            backing.color = new Color(0f, 0f, 0f, 0.55f);
            RectTransform backRect = backing.rectTransform;
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = anchoredPosition;
            backRect.sizeDelta = new Vector2(260f, 20f);

            GameObject fill = new GameObject(name + " Fill");
            fill.transform.SetParent(back.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            return fillImage;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, string value, int size, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(0.82f, 0.95f, 1f);

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(320f, 28f);
            return text;
        }

        private static Projectile CreateProjectilePrefab(string name, Material material, float speed, float damage, float lifetime, Vector3 scale)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = name;
            projectileObject.transform.localScale = scale;
            projectileObject.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(projectileObject.GetComponent<Collider>());

            SphereCollider trigger = projectileObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.6f;

            Rigidbody body = projectileObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            Projectile projectile = projectileObject.AddComponent<Projectile>();
            projectile.speed = speed;
            projectile.damage = damage;
            projectile.lifetime = lifetime;
            projectileObject.SetActive(false);
            return projectile;
        }

        private static HomingMissile CreateMissilePrefab(Material material)
        {
            GameObject missileObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            missileObject.name = "Player Homing Missile";
            missileObject.transform.localScale = new Vector3(0.35f, 0.35f, 1.2f);
            missileObject.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(missileObject.GetComponent<Collider>());

            CapsuleCollider trigger = missileObject.AddComponent<CapsuleCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.35f;
            trigger.height = 1.5f;
            trigger.direction = 2;

            Rigidbody body = missileObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            HomingMissile missile = missileObject.AddComponent<HomingMissile>();
            missile.speed = 34f;
            missile.damage = 38f;
            missile.lifetime = 6f;
            missileObject.SetActive(false);
            return missile;
        }

        private static GameObject CreateChildPrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(primitive.GetComponent<Collider>());
            return primitive;
        }
    }
}
