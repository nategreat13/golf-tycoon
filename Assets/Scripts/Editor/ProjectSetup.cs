#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using GolfGame.Core;
using GolfGame.Golf;
using GolfGame.Golf.Ball;
using GolfGame.Golf.Course;
using GolfGame.Golf.Scoring;
using GolfGame.Golf.ShotMechanic;
using GolfGame.Golf.Camera;
using GolfGame.Building;
using GolfGame.UI;
using GolfGame.UI.Screens;
using GolfGame.UI.Components;
using GolfGame.Utility;
using TMPro;

/// <summary>
/// One-click project setup. Creates all scenes, prefabs, ScriptableObjects,
/// materials, and wires the game together so it's playable immediately.
/// Menu: Golf Tycoon > Setup Entire Project
/// </summary>
public class ProjectSetup : EditorWindow
{
    [MenuItem("Golf Tycoon/Setup Entire Project")]
    public static void Setup()
    {
        if (!EditorUtility.DisplayDialog("Golf Tycoon Setup",
            "This will create all scenes, prefabs, materials, and ScriptableObjects.\n\n" +
            "Any existing assets with the same names will be overwritten.\n\nContinue?",
            "Yes, Set Up Everything", "Cancel"))
            return;

        try
        {
            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating folders...", 0f);
            CreateFolders();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating URP settings...", 0.05f);
            CreateURPSettings();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating physics materials...", 0.1f);
            CreatePhysicsMaterials();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating materials...", 0.15f);
            CreateMaterials();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating club ScriptableObjects...", 0.2f);
            CreateClubAssets();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating hole ScriptableObjects...", 0.25f);
            CreateHoleAssets();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating course ScriptableObjects...", 0.3f);
            CreateCourseAssets();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating prefabs...", 0.4f);
            CreatePrefabs();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating Boot scene...", 0.5f);
            CreateBootScene();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating MainMenu scene...", 0.6f);
            CreateMainMenuScene();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating Gameplay scene...", 0.7f);
            CreateGameplayScene();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating Builder scene...", 0.8f);
            CreateBuilderScene();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Creating DrivingRange scene...", 0.85f);
            CreateDrivingRangeScene();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Configuring build settings...", 0.9f);
            ConfigureBuildSettings();

            EditorUtility.DisplayProgressBar("Setting Up Golf Tycoon", "Setting tags and layers...", 0.95f);
            SetupTagsAndLayers();

            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("Setup Complete!",
                "Golf Tycoon project is ready!\n\n" +
                "1. Open the Boot scene (Assets/Scenes/Boot.unity)\n" +
                "2. Press Play to test the game\n\n" +
                "The game starts at the main menu. Press 'Play Course' to try the golf gameplay.",
                "Got it!");

            // Open the Boot scene
            EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"Setup failed: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Setup Failed", $"Error: {e.Message}\n\nCheck the Console for details.", "OK");
        }
    }

    static void CreateFolders()
    {
        string[] folders = {
            "Assets/Scenes",
            "Assets/ScriptableObjects/Clubs",
            "Assets/ScriptableObjects/Holes",
            "Assets/ScriptableObjects/Courses",
            "Assets/Prefabs/Golf",
            "Assets/Prefabs/Hazards",
            "Assets/Prefabs/Building",
            "Assets/Prefabs/UI",
            "Assets/Materials/Terrain",
            "Assets/Materials/Water",
            "Assets/PhysicsMaterials",
            "Assets/Settings"
        };

        foreach (var folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                string name = Path.GetFileName(folder);
                if (AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  URP SETTINGS
    // ─────────────────────────────────────────────────────────────
    static void CreateURPSettings()
    {
        // Create URP Asset if none exists
        var existingURPAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        if (existingURPAssets.Length > 0)
        {
            // Use existing URP asset
            string path = AssetDatabase.GUIDToAssetPath(existingURPAssets[0]);
            var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
            Debug.Log($"Using existing URP asset: {path}");
            return;
        }

        // Create new URP asset
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(rendererData, "Assets/Settings/URP_Renderer.asset");

        var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
        AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/URP_Settings.asset");

        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;

        Debug.Log("Created URP render pipeline settings");
    }

    // ─────────────────────────────────────────────────────────────
    //  PHYSICS MATERIALS
    // ─────────────────────────────────────────────────────────────
    static PhysicsMaterial fairwayPhysMat, greenPhysMat, roughPhysMat, bunkerPhysMat, ballPhysMat;

    static void CreatePhysicsMaterials()
    {
        fairwayPhysMat = CreatePhysMat("Fairway", 0.4f, 0.3f, PhysicsMaterialCombine.Average);
        greenPhysMat = CreatePhysMat("Green", 0.7f, 0.2f, PhysicsMaterialCombine.Average);
        roughPhysMat = CreatePhysMat("Rough", 0.6f, 0.2f, PhysicsMaterialCombine.Average);
        bunkerPhysMat = CreatePhysMat("Bunker", 0.9f, 0.05f, PhysicsMaterialCombine.Average);
        ballPhysMat = CreatePhysMat("GolfBall", 0.3f, 0.4f, PhysicsMaterialCombine.Average);
    }

    static PhysicsMaterial CreatePhysMat(string name, float friction, float bounce, PhysicsMaterialCombine combine)
    {
        string path = $"Assets/PhysicsMaterials/{name}.physicMaterial";
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial(name)
            {
                dynamicFriction = friction,
                staticFriction = friction,
                bounciness = bounce,
                frictionCombine = combine,
                bounceCombine = combine
            };
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    // ─────────────────────────────────────────────────────────────
    //  MATERIALS
    // ─────────────────────────────────────────────────────────────
    static Material fairwayMat, greenMat, roughMat, bunkerMat, waterMat, teeMat, skyMat;

    static void CreateMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard"); // fallback

        fairwayMat = CreateMat("Fairway", urpLit, new Color(0.3f, 0.65f, 0.2f));
        greenMat = CreateMat("Green", urpLit, new Color(0.2f, 0.75f, 0.25f));
        roughMat = CreateMat("Rough", urpLit, new Color(0.25f, 0.45f, 0.15f));
        bunkerMat = CreateMat("Bunker", urpLit, new Color(0.9f, 0.85f, 0.6f));
        teeMat = CreateMat("Tee", urpLit, new Color(0.7f, 0.65f, 0.5f));

        // Water - use a blue transparent material
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit == null) urpUnlit = urpLit;
        waterMat = CreateMat("Water", urpUnlit, new Color(0.2f, 0.5f, 0.85f, 0.8f));
        waterMat.SetFloat("_Surface", 1); // Transparent
        waterMat.SetFloat("_Blend", 0);
        waterMat.renderQueue = 3000;
    }

    static Material CreateMat(string name, Shader shader, Color color)
    {
        string path = $"Assets/Materials/Terrain/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader) { color = color };
            mat.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    // ─────────────────────────────────────────────────────────────
    //  CLUB SCRIPTABLE OBJECTS
    // ─────────────────────────────────────────────────────────────
    static ClubData[] clubAssets;

    static void CreateClubAssets()
    {
        clubAssets = new ClubData[]
        {
            CreateClub("Driver", "driver", ClubType.Driver, 280, 12, 0.7f, 0, 15, 0.08f, 0.02f, 2000),
            CreateClub("3-Wood", "3wood", ClubType.Wood, 230, 16, 0.8f, 0, 12, 0.07f, 0.03f, 1500),
            CreateClub("5-Iron", "5iron", ClubType.Iron, 180, 24, 0.9f, 0, 10, 0.06f, 0.04f, 1000),
            CreateClub("7-Iron", "7iron", ClubType.Iron, 150, 30, 1.0f, 0, 8, 0.05f, 0.05f, 0),
            CreateClub("Pitching Wedge", "pw", ClubType.Wedge, 110, 42, 1.1f, 0.1f, 7, 0.04f, 0.06f, 800),
            CreateClub("Sand Wedge", "sw", ClubType.Wedge, 80, 54, 1.2f, 0.2f, 5, 0.03f, 0.07f, 600),
            CreateClub("Putter", "putter", ClubType.Putter, 30, 4, 1.5f, 0.3f, 3, 0.02f, 0.05f, 0)
        };
    }

    static ClubData CreateClub(string name, string id, ClubType type, float dist, float loft,
        float accuracy, float spin, float distUpg, float accUpg, float spinUpg, long cost)
    {
        string path = $"Assets/ScriptableObjects/Clubs/{id}.asset";
        var club = AssetDatabase.LoadAssetAtPath<ClubData>(path);
        if (club == null)
        {
            club = ScriptableObject.CreateInstance<ClubData>();
            AssetDatabase.CreateAsset(club, path);
        }

        club.clubId = id;
        club.clubName = name;
        club.clubType = type;
        club.maxDistance = dist;
        club.loftAngle = loft;
        club.accuracyModifier = accuracy;
        club.spinControl = spin;
        club.distancePerUpgrade = distUpg;
        club.accuracyPerUpgrade = accUpg;
        club.spinPerUpgrade = spinUpg;
        club.purchaseCost = cost;
        club.upgradeCostBase = 200;

        EditorUtility.SetDirty(club);
        return club;
    }

    // ─────────────────────────────────────────────────────────────
    //  HOLE SCRIPTABLE OBJECTS
    // ─────────────────────────────────────────────────────────────
    static HoleData[] holeAssets;

    static void CreateHoleAssets()
    {
        // Hole 1: Simple par 3
        var hole1 = CreateHoleData("hole_1", "The Opener", 3, 150, 1f, 1f, 0.1f, Vector2.right, 0, 0, 0.5f);
        hole1.hazards = new HazardPlacement[]
        {
            new HazardPlacement { type = HazardType.Bunker, longitudinalPosition = 0.85f, lateralOffset = 0.3f, size = 0.8f },
            new HazardPlacement { type = HazardType.Bunker, longitudinalPosition = 0.9f, lateralOffset = -0.25f, size = 0.7f },
            new HazardPlacement { type = HazardType.Trees, longitudinalPosition = 0.3f, lateralOffset = 0.8f, size = 1f },
            new HazardPlacement { type = HazardType.Trees, longitudinalPosition = 0.5f, lateralOffset = -0.8f, size = 1.2f },
            new HazardPlacement { type = HazardType.Trees, longitudinalPosition = 0.7f, lateralOffset = 0.9f, size = 0.9f },
            new HazardPlacement { type = HazardType.Trees, longitudinalPosition = 0.15f, lateralOffset = -0.7f, size = 1f },
        };
        EditorUtility.SetDirty(hole1);

        // Hole 2: Par 4 with bunker
        var hole2 = CreateHoleData("hole_2", "Bunker Valley", 4, 320, 0.9f, 0.9f, 0.2f, new Vector2(0.5f, 0.5f), 0, 0, 0.5f);
        hole2.hazards = new HazardPlacement[]
        {
            new HazardPlacement { type = HazardType.Bunker, longitudinalPosition = 0.65f, lateralOffset = 0.3f, size = 1f },
            new HazardPlacement { type = HazardType.Bunker, longitudinalPosition = 0.85f, lateralOffset = -0.2f, size = 0.8f }
        };
        EditorUtility.SetDirty(hole2);

        // Hole 3: Par 3 with water
        var hole3 = CreateHoleData("hole_3", "Island Green", 3, 165, 0.8f, 0.8f, 0.3f, new Vector2(-1, 0), 0, 0, 0.5f);
        hole3.hazards = new HazardPlacement[]
        {
            new HazardPlacement { type = HazardType.Water, longitudinalPosition = 0.5f, lateralOffset = 0f, size = 2f }
        };
        EditorUtility.SetDirty(hole3);

        holeAssets = new HoleData[] { hole1, hole2, hole3 };
    }

    static HoleData CreateHoleData(string id, string name, int par, float yardage,
        float fairwayWidth, float greenSize, float greenSlope, Vector2 slopeDir,
        float elevation, float dogleg, float doglegPos)
    {
        string path = $"Assets/ScriptableObjects/Holes/{id}.asset";
        var hole = AssetDatabase.LoadAssetAtPath<HoleData>(path);
        if (hole == null)
        {
            hole = ScriptableObject.CreateInstance<HoleData>();
            AssetDatabase.CreateAsset(hole, path);
        }

        hole.holeName = name;
        hole.par = par;
        hole.yardage = yardage;
        hole.fairwayWidth = fairwayWidth;
        hole.greenSize = greenSize;
        hole.greenSlope = greenSlope;
        hole.greenSlopeDirection = slopeDir;
        hole.elevationChange = elevation;
        hole.doglegAngle = dogleg;
        hole.doglegPosition = doglegPos;
        hole.length = yardage < 200 ? GolfGame.Data.HoleLength.Short :
                      yardage < 400 ? GolfGame.Data.HoleLength.Medium :
                      GolfGame.Data.HoleLength.Long;

        EditorUtility.SetDirty(hole);
        return hole;
    }

    // ─────────────────────────────────────────────────────────────
    //  COURSE SCRIPTABLE OBJECTS
    // ─────────────────────────────────────────────────────────────
    static CourseData starterCourse;

    static void CreateCourseAssets()
    {
        string path = "Assets/ScriptableObjects/Courses/StarterCourse.asset";
        starterCourse = AssetDatabase.LoadAssetAtPath<CourseData>(path);
        if (starterCourse == null)
        {
            starterCourse = ScriptableObject.CreateInstance<CourseData>();
            AssetDatabase.CreateAsset(starterCourse, path);
        }

        starterCourse.courseName = "Starter Course";
        starterCourse.courseDescription = "Your first course. Simple but fun!";
        starterCourse.holes = new System.Collections.Generic.List<HoleData>(holeAssets);
        starterCourse.RecalculatePar();
        starterCourse.difficultyRating = 2;

        EditorUtility.SetDirty(starterCourse);
    }

    // ─────────────────────────────────────────────────────────────
    //  PREFABS
    // ─────────────────────────────────────────────────────────────
    static GameObject ballPrefab, teeMarkerPrefab, flagPinPrefab, holeTriggerPrefab;
    static GameObject bunkerPrefab, waterHazardPrefab, treePrefab;
    static GameObject holeTemplatePrefab;

    static void CreatePrefabs()
    {
        // Golf Ball
        ballPrefab = CreateBallPrefab();

        // Tee Marker
        teeMarkerPrefab = CreateTeeMarkerPrefab();

        // Flag Pin
        flagPinPrefab = CreateFlagPinPrefab();

        // Hole Trigger (invisible trigger collider)
        holeTriggerPrefab = CreateHoleTriggerPrefab();

        // Hazards
        bunkerPrefab = CreateHazardPrefab("Bunker", bunkerMat, "Bunker", new Vector3(5, 0.1f, 5));
        waterHazardPrefab = CreateHazardPrefab("WaterHazard", waterMat, "Water", new Vector3(8, 0.05f, 8));
        treePrefab = CreateTreePrefab();

        // Hole Template (full hole with tee, fairway, green, pin)
        holeTemplatePrefab = CreateHoleTemplatePrefab();

        AssetDatabase.SaveAssets();
    }

    static GameObject CreateBallPrefab()
    {
        string path = "Assets/Prefabs/Golf/GolfBall.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "GolfBall";
        ball.tag = "GolfBall";
        ball.transform.localScale = Vector3.one * 0.12f; // Golf ball is ~4.3cm diameter

        // Material
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = Color.white;
        AssetDatabase.CreateAsset(mat, "Assets/Materials/Terrain/GolfBall.mat");
        ball.GetComponent<Renderer>().material = mat;

        // Physics
        var rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.045f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        if (ballPhysMat != null)
            ball.GetComponent<SphereCollider>().material = ballPhysMat;

        // Scripts
        ball.AddComponent<GolfBall>();
        ball.AddComponent<BallPhysics>();
        ball.AddComponent<BallTrail>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(ball, path);
        Object.DestroyImmediate(ball);
        return prefab;
    }

    static GameObject CreateTeeMarkerPrefab()
    {
        string path = "Assets/Prefabs/Golf/TeeMarker.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var tee = new GameObject("TeeMarker");

        // Tee box ground
        var teeBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        teeBox.name = "TeeBox";
        teeBox.tag = "Tee";
        teeBox.transform.SetParent(tee.transform);
        teeBox.transform.localScale = new Vector3(3, 0.05f, 2);
        teeBox.transform.localPosition = Vector3.zero;
        if (teeMat != null) teeBox.GetComponent<Renderer>().material = teeMat;

        // Tee marker posts
        for (int i = -1; i <= 1; i += 2)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"Marker_{(i < 0 ? "L" : "R")}";
            marker.transform.SetParent(tee.transform);
            marker.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
            marker.transform.localPosition = new Vector3(i * 1f, 0.15f, 0);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = Color.red;
            marker.GetComponent<Renderer>().material = mat;
            // Don't save individual marker materials as assets to avoid clutter
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(tee, path);
        Object.DestroyImmediate(tee);
        return prefab;
    }

    static GameObject CreateFlagPinPrefab()
    {
        string path = "Assets/Prefabs/Golf/FlagPin.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var pin = new GameObject("FlagPin");

        // Pole
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(pin.transform);
        pole.transform.localScale = new Vector3(0.04f, 2.5f, 0.04f);
        pole.transform.localPosition = new Vector3(0, 2.5f, 0);
        var poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        poleMat.color = Color.white;
        pole.GetComponent<Renderer>().material = poleMat;
        Object.DestroyImmediate(pole.GetComponent<Collider>());

        // Flag
        var flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flag.name = "Flag";
        flag.transform.SetParent(pin.transform);
        flag.transform.localScale = new Vector3(0.9f, 0.5f, 0.02f);
        flag.transform.localPosition = new Vector3(0.45f, 4.5f, 0);
        var flagMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        flagMat.color = new Color(1f, 0.9f, 0.1f);
        flag.GetComponent<Renderer>().material = flagMat;
        Object.DestroyImmediate(flag.GetComponent<Collider>());

        // Hole trigger (cup in the ground)
        var holeTrigger = new GameObject("HoleTrigger");
        holeTrigger.tag = "Hole";
        holeTrigger.transform.SetParent(pin.transform);
        holeTrigger.transform.localPosition = new Vector3(0, 0.05f, 0);
        var col = holeTrigger.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.15f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(pin, path);
        Object.DestroyImmediate(pin);
        return prefab;
    }

    static GameObject CreateHoleTriggerPrefab()
    {
        string path = "Assets/Prefabs/Golf/HoleTrigger.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var trigger = new GameObject("HoleTrigger");
        trigger.tag = "Hole";
        var col = trigger.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.15f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(trigger, path);
        Object.DestroyImmediate(trigger);
        return prefab;
    }

    static GameObject CreateHazardPrefab(string name, Material mat, string tag, Vector3 scale)
    {
        string path = $"Assets/Prefabs/Hazards/{name}.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hazard.name = name;
        hazard.tag = tag;
        hazard.transform.localScale = scale;
        if (mat != null) hazard.GetComponent<Renderer>().material = mat;

        // Add hazard zone component
        var hz = hazard.AddComponent<HazardZone>();

        // Water and OB need trigger colliders
        if (tag == "Water" || tag == "OutOfBounds")
        {
            // Add a trigger collider on top of the regular collider
            var triggerCol = hazard.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = Vector3.one * 1.2f; // Slightly larger
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(hazard, path);
        Object.DestroyImmediate(hazard);
        return prefab;
    }

    static GameObject CreateTreePrefab()
    {
        string path = "Assets/Prefabs/Hazards/Tree.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var tree = new GameObject("Tree");

        // Trunk
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localScale = new Vector3(0.3f, 2, 0.3f);
        trunk.transform.localPosition = new Vector3(0, 2, 0);
        var trunkMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        trunkMat.SetColor("_BaseColor", new Color(0.4f, 0.25f, 0.1f));
        trunkMat.color = new Color(0.4f, 0.25f, 0.1f);
        trunk.GetComponent<Renderer>().material = trunkMat;

        // Canopy
        var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Canopy";
        canopy.transform.SetParent(tree.transform);
        canopy.transform.localScale = new Vector3(3, 3, 3);
        canopy.transform.localPosition = new Vector3(0, 5, 0);
        var canopyMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        canopyMat.SetColor("_BaseColor", new Color(0.15f, 0.5f, 0.12f));
        canopyMat.color = new Color(0.15f, 0.5f, 0.12f);
        canopy.GetComponent<Renderer>().material = canopyMat;

        var prefab = PrefabUtility.SaveAsPrefabAsset(tree, path);
        Object.DestroyImmediate(tree);
        return prefab;
    }

    static GameObject CreateHoleTemplatePrefab()
    {
        string path = "Assets/Prefabs/Golf/HoleTemplate.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var hole = new GameObject("HoleTemplate");
        var instance = hole.AddComponent<HoleInstance>();

        // Tee
        var tee = new GameObject("Tee");
        tee.transform.SetParent(hole.transform);
        tee.transform.localPosition = Vector3.zero;

        // Tee box visual
        var teeBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        teeBox.name = "TeeBox";
        teeBox.transform.SetParent(tee.transform);
        teeBox.transform.localScale = new Vector3(4, 0.02f, 3);
        teeBox.transform.localPosition = new Vector3(0, 0.01f, 0);
        if (teeMat != null) teeBox.GetComponent<Renderer>().material = teeMat;
        Object.DestroyImmediate(teeBox.GetComponent<Collider>());

        // Fairway (long rectangle)
        var fairway = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fairway.name = "Fairway";
        fairway.tag = "Fairway";
        fairway.transform.SetParent(hole.transform);
        fairway.transform.localScale = new Vector3(20, 0.1f, 150);
        fairway.transform.localPosition = new Vector3(0, -0.05f, 75);
        if (fairwayMat != null) fairway.GetComponent<Renderer>().material = fairwayMat;
        if (fairwayPhysMat != null) fairway.GetComponent<BoxCollider>().material = fairwayPhysMat;

        // Rough (wider rectangle on each side)
        for (int side = -1; side <= 1; side += 2)
        {
            var rough = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rough.name = $"Rough_{(side < 0 ? "L" : "R")}";
            rough.tag = "Rough";
            rough.transform.SetParent(hole.transform);
            rough.transform.localScale = new Vector3(15, 0.1f, 150);
            rough.transform.localPosition = new Vector3(side * 17.5f, -0.06f, 75);
            if (roughMat != null) rough.GetComponent<Renderer>().material = roughMat;
            if (roughPhysMat != null) rough.GetComponent<BoxCollider>().material = roughPhysMat;
        }

        // Fairway edge strips
        for (int side2 = -1; side2 <= 1; side2 += 2)
        {
            var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = $"FairwayEdge_{(side2 < 0 ? "L" : "R")}";
            edge.transform.SetParent(hole.transform);
            edge.transform.localScale = new Vector3(0.5f, 0.12f, 150);
            edge.transform.localPosition = new Vector3(side2 * 10f, -0.04f, 75);
            var edgeMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            edgeMat.color = new Color(0.2f, 0.5f, 0.15f);
            edge.GetComponent<Renderer>().material = edgeMat;
            Object.DestroyImmediate(edge.GetComponent<Collider>());
        }

        // Green
        var green = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        green.name = "Green";
        green.tag = "Green";
        green.transform.SetParent(hole.transform);
        green.transform.localScale = new Vector3(12, 0.05f, 12);
        green.transform.localPosition = new Vector3(0, -0.02f, 150);
        if (greenMat != null) green.GetComponent<Renderer>().material = greenMat;
        // Note: physics material applied after replacing collider below
        // Cylinder uses a MeshCollider by default, but let's add a box collider
        Object.DestroyImmediate(green.GetComponent<Collider>());
        var greenCol = green.AddComponent<BoxCollider>();
        greenCol.size = new Vector3(1, 1, 1);
        if (greenPhysMat != null) greenCol.material = greenPhysMat;

        // Green slope trigger
        var greenTriggerObj = new GameObject("GreenTrigger");
        greenTriggerObj.tag = "Green";
        greenTriggerObj.transform.SetParent(green.transform);
        greenTriggerObj.transform.localPosition = Vector3.zero;
        var greenTrigger = greenTriggerObj.AddComponent<BoxCollider>();
        greenTrigger.isTrigger = true;
        greenTrigger.size = new Vector3(1.1f, 2f, 1.1f);
        greenTriggerObj.AddComponent<GreenPhysics>();

        // Green fringe ring (slightly larger darker cylinder underneath)
        var greenFringe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        greenFringe.name = "GreenFringe";
        greenFringe.tag = "Green";
        greenFringe.transform.SetParent(hole.transform);
        greenFringe.transform.localScale = new Vector3(15, 0.04f, 15);
        greenFringe.transform.localPosition = new Vector3(0, -0.03f, 150);
        var fringeMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        fringeMat.color = new Color(0.22f, 0.68f, 0.22f);
        greenFringe.GetComponent<Renderer>().material = fringeMat;
        Object.DestroyImmediate(greenFringe.GetComponent<Collider>());

        // Pin position marker with FlagPin
        GameObject pinPos;
        if (flagPinPrefab != null)
        {
            pinPos = PrefabUtility.InstantiatePrefab(flagPinPrefab) as GameObject;
            pinPos.name = "PinPosition";
        }
        else
        {
            pinPos = new GameObject("PinPosition");
        }
        pinPos.transform.SetParent(hole.transform);
        pinPos.transform.localPosition = new Vector3(0, 0, 150);

        // Visible hole cup
        var holeCup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        holeCup.name = "HoleCup";
        holeCup.transform.SetParent(pinPos.transform);
        holeCup.transform.localScale = new Vector3(0.4f, 0.01f, 0.4f);
        holeCup.transform.localPosition = new Vector3(0, 0.02f, 0);
        var cupMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        cupMat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.1f));
        cupMat.color = new Color(0.1f, 0.1f, 0.1f);
        holeCup.GetComponent<Renderer>().material = cupMat;
        Object.DestroyImmediate(holeCup.GetComponent<Collider>());

        // Hazard container
        var hazardContainer = new GameObject("Hazards");
        hazardContainer.transform.SetParent(hole.transform);

        // Decorative trees along the rough edges
        if (treePrefab != null)
        {
            float[] treeZPositions = { 20f, 50f, 80f, 110f, 135f };
            float[] treeXOffsets = { 22f, -24f, 25f, -22f, 23f };
            for (int i = 0; i < treeZPositions.Length; i++)
            {
                var tree = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;
                if (tree != null)
                {
                    tree.transform.SetParent(hole.transform);
                    tree.transform.localPosition = new Vector3(treeXOffsets[i], 0, treeZPositions[i]);
                    tree.name = $"DecoTree_{i}";
                }
            }
        }

        // Wire up HoleInstance via serialized fields using SerializedObject
        var prefab = PrefabUtility.SaveAsPrefabAsset(hole, path);
        Object.DestroyImmediate(hole);

        // Wire serialized fields on the prefab
        var prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (prefabInstance != null)
        {
            var hi = prefabInstance.GetComponent<HoleInstance>();
            var so = new SerializedObject(hi);

            so.FindProperty("teeMarker").objectReferenceValue = prefabInstance.transform.Find("Tee");
            so.FindProperty("pinPosition").objectReferenceValue = prefabInstance.transform.Find("PinPosition");
            so.FindProperty("greenCenter").objectReferenceValue = prefabInstance.transform.Find("Green");
            so.FindProperty("hazardContainer").objectReferenceValue = prefabInstance.transform.Find("Hazards");
            var holeTriggerTransform = prefabInstance.transform.Find("PinPosition/HoleTrigger");
            if (holeTriggerTransform != null)
                so.FindProperty("holeCollider").objectReferenceValue = holeTriggerTransform.GetComponent<Collider>();
            so.FindProperty("greenPhysics").objectReferenceValue =
                prefabInstance.transform.Find("Green/GreenTrigger")?.GetComponent<GreenPhysics>();
            so.FindProperty("bunkerPrefab").objectReferenceValue = bunkerPrefab;
            so.FindProperty("waterPrefab").objectReferenceValue = waterHazardPrefab;
            so.FindProperty("treePrefab").objectReferenceValue = treePrefab;

            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            Object.DestroyImmediate(prefabInstance);
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    // ─────────────────────────────────────────────────────────────
    //  SCENES
    // ─────────────────────────────────────────────────────────────

    static void CreateBootScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Game Manager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();
        // Set skipMainMenu to false via serialized property
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("skipMainMenu").boolValue = false;
        gmSO.ApplyModifiedProperties();

        // Touch Input Handler
        var inputObj = new GameObject("TouchInputHandler");
        inputObj.AddComponent<TouchInputHandler>();

        // Camera (minimal — each scene will have its own)
        // Boot scene just needs a camera so it doesn't show a black screen during load
        var camObj = new GameObject("BootCamera");
        var cam = camObj.AddComponent<UnityEngine.Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.13f, 0.45f, 0.13f); // Golf green
        cam.tag = "MainCamera";

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
    }

    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera - darker, richer green for better contrast
        var camObj = new GameObject("Camera");
        var cam = camObj.AddComponent<UnityEngine.Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.3f, 0.08f);

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // UI Manager
        var uiManager = canvasObj.AddComponent<UIManager>();

        // Main Menu Panel
        var menuPanel = CreateUIPanel(canvasObj.transform, "MainMenuPanel");
        var menuScreen = menuPanel.AddComponent<MainMenuScreen>();

        // Title shadow (dark green, offset +2px down and +2px right)
        var titleShadow = CreateUIText(menuPanel.transform, "TitleShadow", "GOLF TYCOON", 64, new Vector2(2, 698));
        titleShadow.color = new Color(0.05f, 0.18f, 0.05f);
        titleShadow.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

        // Title (large, white, on top of shadow)
        var titleMain = CreateUIText(menuPanel.transform, "TitleText", "GOLF TYCOON", 64, new Vector2(0, 700));
        titleMain.color = Color.white;
        titleMain.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

        // Decorative separator line below title
        var separatorObj = new GameObject("Separator");
        separatorObj.transform.SetParent(menuPanel.transform, false);
        var sepRect = separatorObj.AddComponent<RectTransform>();
        sepRect.anchoredPosition = new Vector2(0, 660);
        sepRect.sizeDelta = new Vector2(350, 3);
        var sepImage = separatorObj.AddComponent<UnityEngine.UI.Image>();
        sepImage.color = new Color(1f, 0.85f, 0.2f, 0.6f);

        // Currency display - large, gold/yellow
        var currencyText = CreateUIText(menuPanel.transform, "CurrencyText", "$5,000", 36, new Vector2(0, 610));
        currencyText.color = new Color(1f, 0.85f, 0.2f);
        currencyText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);

        // Info texts - compact group, smaller font, light gray
        var repText = CreateUIText(menuPanel.transform, "ReputationText", "Rep: 0 (Lv.1)", 15, new Vector2(0, 575));
        repText.color = new Color(0.8f, 0.85f, 0.8f);
        var courseText = CreateUIText(menuPanel.transform, "CourseNameText", "My Course", 16, new Vector2(0, 555));
        courseText.color = new Color(0.8f, 0.85f, 0.8f);
        var tierText = CreateUIText(menuPanel.transform, "TierText", "1/1 Holes", 14, new Vector2(0, 535));
        tierText.color = new Color(0.7f, 0.75f, 0.7f);
        var incomeText = CreateUIText(menuPanel.transform, "IncomeText", "Income: 5/min", 14, new Vector2(0, 515));
        incomeText.color = new Color(0.7f, 0.75f, 0.7f);
        var titleRoleText = CreateUIText(menuPanel.transform, "TitleRoleText", "Groundskeeper", 14, new Vector2(0, 495));
        titleRoleText.color = new Color(0.7f, 0.75f, 0.7f);

        // Buttons - wider (300px), taller (55px)
        // Play Course = bright green primary action
        var playBtn = CreateUIButton(menuPanel.transform, "PlayButton", "Play Course", new Vector2(0, 300), new Vector2(300, 55));
        SetButtonColors(playBtn, new Color(0.2f, 0.7f, 0.3f, 1f), new Color(0.25f, 0.8f, 0.35f, 1f), new Color(0.15f, 0.55f, 0.2f, 1f));
        var playLabel = playBtn.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (playLabel != null) playLabel.fontSize = 24;

        // Secondary buttons - darker green
        var buildBtn = CreateUIButton(menuPanel.transform, "BuildButton", "My Property", new Vector2(0, 230), new Vector2(300, 55));
        SetButtonColors(buildBtn, new Color(0.15f, 0.4f, 0.15f, 0.95f), new Color(0.2f, 0.5f, 0.2f, 1f), new Color(0.1f, 0.3f, 0.1f, 1f));

        var visitBtn = CreateUIButton(menuPanel.transform, "VisitButton", "Visit Courses", new Vector2(0, 160), new Vector2(300, 55));
        SetButtonColors(visitBtn, new Color(0.15f, 0.4f, 0.15f, 0.95f), new Color(0.2f, 0.5f, 0.2f, 1f), new Color(0.1f, 0.3f, 0.1f, 1f));

        // Spacing gap before utility buttons
        var rangeBtn = CreateUIButton(menuPanel.transform, "DrivingRangeButton", "Driving Range", new Vector2(0, 50), new Vector2(300, 55));
        SetButtonColors(rangeBtn, new Color(0.15f, 0.4f, 0.15f, 0.95f), new Color(0.2f, 0.5f, 0.2f, 1f), new Color(0.1f, 0.3f, 0.1f, 1f));

        var shopBtn = CreateUIButton(menuPanel.transform, "ShopButton", "Shop", new Vector2(0, -20), new Vector2(300, 55));
        SetButtonColors(shopBtn, new Color(0.15f, 0.4f, 0.15f, 0.95f), new Color(0.2f, 0.5f, 0.2f, 1f), new Color(0.1f, 0.3f, 0.1f, 1f));

        // Version text at the bottom
        var versionText = CreateUIText(menuPanel.transform, "VersionText", "v0.1 - Early Access", 12, new Vector2(0, -850));
        versionText.color = new Color(0.5f, 0.55f, 0.5f, 0.7f);

        // Wire up MainMenuScreen serialized fields
        var so = new SerializedObject(menuScreen);
        so.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("buildButton").objectReferenceValue = buildBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("visitButton").objectReferenceValue = visitBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("drivingRangeButton").objectReferenceValue = rangeBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("shopButton").objectReferenceValue = shopBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("currencyText").objectReferenceValue = currencyText;
        so.FindProperty("reputationText").objectReferenceValue = repText;
        so.FindProperty("courseNameText").objectReferenceValue = courseText;
        so.FindProperty("courseTierText").objectReferenceValue = tierText;
        so.FindProperty("incomeRateText").objectReferenceValue = incomeText;
        so.FindProperty("titleText").objectReferenceValue = titleRoleText;
        so.ApplyModifiedProperties();

        // Wire UIManager
        var uiSO = new SerializedObject(uiManager);
        var screensList = uiSO.FindProperty("allScreens");
        screensList.arraySize = 1;
        screensList.GetArrayElementAtIndex(0).objectReferenceValue = menuScreen;
        uiSO.ApplyModifiedProperties();

        // Event System
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    // Helper to set button colors consistently
    static void SetButtonColors(GameObject btnObj, Color normal, Color highlighted, Color pressed)
    {
        var image = btnObj.GetComponent<UnityEngine.UI.Image>();
        if (image != null) image.color = normal;

        var btn = btnObj.GetComponent<UnityEngine.UI.Button>();
        if (btn != null)
        {
            var colors = btn.colors;
            colors.normalColor = normal;
            colors.highlightedColor = highlighted;
            colors.pressedColor = pressed;
            btn.colors = colors;
        }
    }

    static void CreateGameplayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Directional Light
        var lightObj = new GameObject("Directional Light");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.84f);
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Camera
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<UnityEngine.Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.75f, 0.95f); // Sky blue
        cam.orthographic = true;
        cam.orthographicSize = 25f;
        camObj.transform.position = new Vector3(0, 50, 75);
        camObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Ground (safety net for out-of-bounds)
        var gameGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        gameGround.name = "Ground";
        gameGround.tag = "Fairway";
        gameGround.transform.position = new Vector3(0, -0.5f, 0);
        gameGround.transform.localScale = new Vector3(200, 1, 200);
        if (fairwayMat != null) gameGround.GetComponent<Renderer>().material = fairwayMat;

        // Golf Ball
        var ballObj = PrefabUtility.InstantiatePrefab(ballPrefab) as GameObject;
        if (ballObj != null)
            ballObj.transform.position = new Vector3(0, 0.1f, 0);

        // Shot Controller
        var shotObj = new GameObject("ShotController");
        var shotController = shotObj.AddComponent<ShotController>();
        var powerBar = shotObj.AddComponent<PowerBar>();
        var accuracyBar = shotObj.AddComponent<AccuracyBar>();

        // Shot Arc on ball
        if (ballObj != null)
        {
            var arcObj = new GameObject("ShotArc");
            arcObj.transform.SetParent(ballObj.transform);
            arcObj.AddComponent<LineRenderer>();
            arcObj.AddComponent<ShotArc>();
        }

        // Course Loader
        var courseObj = new GameObject("CourseLoader");
        var courseLoader = courseObj.AddComponent<CourseLoader>();

        // Hole Container
        var holeContainer = new GameObject("Holes");

        // Scorecard
        var scorecardObj = new GameObject("Scorecard");
        var scorecard = scorecardObj.AddComponent<ScorecardManager>();

        // Club Inventory & Selector
        var clubObj = new GameObject("ClubSystem");
        var clubInv = clubObj.AddComponent<ClubInventory>();
        var clubSel = clubObj.AddComponent<ClubSelector>();

        // Wire club inventory with all clubs
        var clubInvSO = new SerializedObject(clubInv);
        var clubList = clubInvSO.FindProperty("allClubs");
        clubList.arraySize = clubAssets.Length;
        for (int i = 0; i < clubAssets.Length; i++)
            clubList.GetArrayElementAtIndex(i).objectReferenceValue = clubAssets[i];
        clubInvSO.ApplyModifiedProperties();

        // Wire club selector
        var clubSelSO = new SerializedObject(clubSel);
        clubSelSO.FindProperty("inventory").objectReferenceValue = clubInv;
        clubSelSO.ApplyModifiedProperties();

        // Camera Controller
        var camController = camObj.AddComponent<GolfCameraController>();

        // Canvas for HUD
        var canvasObj = new GameObject("GameCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var gameScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        gameScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameScaler.referenceResolution = new Vector2(1080, 1920);
        gameScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // HUD Panel
        var hudPanel = CreateUIPanel(canvasObj.transform, "HUDPanel");
        var hudScreen = hudPanel.AddComponent<HUDScreen>();

        // HUD elements
        var holeNumText = CreateUIText(hudPanel.transform, "HoleNumber", "Hole 1", 24,
            new Vector2(-400, 900), TextAlignmentOptions.Left);
        var parTextUI = CreateUIText(hudPanel.transform, "ParText", "Par 3", 20,
            new Vector2(-400, 870), TextAlignmentOptions.Left);
        var yardageTextUI = CreateUIText(hudPanel.transform, "YardageText", "150 yds", 18,
            new Vector2(-400, 845), TextAlignmentOptions.Left);
        var strokeTextUI = CreateUIText(hudPanel.transform, "StrokeText", "Strokes: 0", 22,
            new Vector2(400, 900), TextAlignmentOptions.Right);
        var clubNameUI = CreateUIText(hudPanel.transform, "ClubName", "7-Iron", 22,
            new Vector2(0, -800));
        var clubDistUI = CreateUIText(hudPanel.transform, "ClubDist", "150 yds", 16,
            new Vector2(0, -830));
        var statusUI = CreateUIText(hudPanel.transform, "StatusText", "Tap to set power", 28,
            new Vector2(0, -700));
        var relScoreUI = CreateUIText(hudPanel.transform, "RelativeScore", "E", 20,
            new Vector2(400, 870), TextAlignmentOptions.Right);
        var distanceUI = CreateUIText(hudPanel.transform, "DistanceText", "0 yds to pin", 20,
            new Vector2(0, 900));

        // Pin target marker (invisible, positioned at hole location for distance calc)
        var pinTargetObj = new GameObject("PinTarget");
        pinTargetObj.transform.position = new Vector3(0, 0, 150); // Default forward position

        // Club switch buttons
        var prevClubBtn = CreateUIButton(hudPanel.transform, "PrevClub", "<", new Vector2(-120, -800), new Vector2(50, 40));
        var nextClubBtn = CreateUIButton(hudPanel.transform, "NextClub", ">", new Vector2(120, -800), new Vector2(50, 40));

        // Wire HUD
        var hudSO = new SerializedObject(hudScreen);
        hudSO.FindProperty("holeNumberText").objectReferenceValue = holeNumText;
        hudSO.FindProperty("parText").objectReferenceValue = parTextUI;
        hudSO.FindProperty("yardageText").objectReferenceValue = yardageTextUI;
        hudSO.FindProperty("strokeCountText").objectReferenceValue = strokeTextUI;
        hudSO.FindProperty("clubNameText").objectReferenceValue = clubNameUI;
        hudSO.FindProperty("clubDistanceText").objectReferenceValue = clubDistUI;
        hudSO.FindProperty("statusText").objectReferenceValue = statusUI;
        hudSO.FindProperty("scoreRelativeText").objectReferenceValue = relScoreUI;
        hudSO.FindProperty("prevClubButton").objectReferenceValue = prevClubBtn.GetComponent<UnityEngine.UI.Button>();
        hudSO.FindProperty("nextClubButton").objectReferenceValue = nextClubBtn.GetComponent<UnityEngine.UI.Button>();
        hudSO.FindProperty("shotController").objectReferenceValue = shotController;
        hudSO.FindProperty("clubSelector").objectReferenceValue = clubSel;
        hudSO.FindProperty("scorecard").objectReferenceValue = scorecard;
        hudSO.FindProperty("distanceText").objectReferenceValue = distanceUI;
        hudSO.FindProperty("pinTarget").objectReferenceValue = pinTargetObj.transform;
        if (ballObj != null)
            hudSO.FindProperty("ballTransform").objectReferenceValue = ballObj.transform;

        // PowerBarUI holder
        var powerBarUIObj = new GameObject("PowerBarUI");
        powerBarUIObj.transform.SetParent(canvasObj.transform, false);
        var powerBarUIRect = powerBarUIObj.AddComponent<RectTransform>();
        powerBarUIRect.anchorMin = Vector2.zero;
        powerBarUIRect.anchorMax = Vector2.one;
        powerBarUIRect.sizeDelta = Vector2.zero;
        var powerBarUIComp = powerBarUIObj.AddComponent<GolfGame.UI.Components.PowerBarUI>();

        // AccuracyBarUI holder
        var accuracyBarUIObj = new GameObject("AccuracyBarUI");
        accuracyBarUIObj.transform.SetParent(canvasObj.transform, false);
        var accuracyBarUIRect = accuracyBarUIObj.AddComponent<RectTransform>();
        accuracyBarUIRect.anchorMin = Vector2.zero;
        accuracyBarUIRect.anchorMax = Vector2.one;
        accuracyBarUIRect.sizeDelta = Vector2.zero;
        var accuracyBarUIComp = accuracyBarUIObj.AddComponent<GolfGame.UI.Components.AccuracyBarUI>();

        // Wire PowerBarUI / AccuracyBarUI serialized fields
        var pbuiSO = new SerializedObject(powerBarUIComp);
        pbuiSO.FindProperty("powerBar").objectReferenceValue = powerBar;
        pbuiSO.ApplyModifiedProperties();

        var abuiSO = new SerializedObject(accuracyBarUIComp);
        abuiSO.FindProperty("accuracyBar").objectReferenceValue = accuracyBar;
        abuiSO.ApplyModifiedProperties();

        // Wire HUD references to the bar UIs
        hudSO.FindProperty("powerBarUI").objectReferenceValue = powerBarUIComp;
        hudSO.FindProperty("accuracyBarUI").objectReferenceValue = accuracyBarUIComp;

        hudSO.ApplyModifiedProperties();

        // Score Popup (floating text for Birdie/Par/Bogey etc.)
        var scorePopupObj = new GameObject("ScorePopup");
        scorePopupObj.transform.SetParent(canvasObj.transform, false);
        scorePopupObj.AddComponent<ScorePopup>();

        // Scorecard Screen (shown at end of round)
        var scorecardPanel = CreateUIPanel(canvasObj.transform, "ScorecardPanel");
        var scorecardScreen = scorecardPanel.AddComponent<ScorecardScreen>();
        scorecardPanel.SetActive(false);

        var totalScoreUI = CreateUIText(scorecardPanel.transform, "TotalScore", "0", 48, new Vector2(0, 200));
        var relativeUI = CreateUIText(scorecardPanel.transform, "RelativeScore", "E", 28, new Vector2(0, 140));
        var summaryUI = CreateUIText(scorecardPanel.transform, "Summary", "", 20, new Vector2(0, 100));
        var repGainUI = CreateUIText(scorecardPanel.transform, "RepGain", "+10 Reputation", 18, new Vector2(0, 50));
        var playAgainBtn = CreateUIButton(scorecardPanel.transform, "PlayAgain", "Play Again", new Vector2(-100, -150));
        var menuBtn = CreateUIButton(scorecardPanel.transform, "MainMenu", "Main Menu", new Vector2(100, -150));

        var scSO = new SerializedObject(scorecardScreen);
        scSO.FindProperty("totalScoreText").objectReferenceValue = totalScoreUI;
        scSO.FindProperty("relativeScoreText").objectReferenceValue = relativeUI;
        scSO.FindProperty("summaryText").objectReferenceValue = summaryUI;
        scSO.FindProperty("reputationGainText").objectReferenceValue = repGainUI;
        scSO.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn.GetComponent<UnityEngine.UI.Button>();
        scSO.FindProperty("mainMenuButton").objectReferenceValue = menuBtn.GetComponent<UnityEngine.UI.Button>();
        scSO.ApplyModifiedProperties();

        // GolfGameplayManager — orchestrates everything
        var gameplayMgrObj = new GameObject("GolfGameplayManager");
        var gameplayMgr = gameplayMgrObj.AddComponent<GolfGameplayManager>();

        // Wire ShotController
        var shotSO = new SerializedObject(shotController);
        shotSO.FindProperty("powerBar").objectReferenceValue = powerBar;
        shotSO.FindProperty("accuracyBar").objectReferenceValue = accuracyBar;
        if (ballObj != null)
        {
            shotSO.FindProperty("golfBall").objectReferenceValue = ballObj.GetComponent<GolfBall>();
            var arcComp = ballObj.GetComponentInChildren<ShotArc>();
            if (arcComp != null)
                shotSO.FindProperty("shotArc").objectReferenceValue = arcComp;
        }
        shotSO.ApplyModifiedProperties();

        // Wire Camera Controller
        var camSO = new SerializedObject(camController);
        camSO.FindProperty("mainCamera").objectReferenceValue = cam;
        if (ballObj != null)
            camSO.FindProperty("golfBall").objectReferenceValue = ballObj.GetComponent<GolfBall>();
        camSO.FindProperty("shotController").objectReferenceValue = shotController;
        camSO.ApplyModifiedProperties();

        // Wire Course Loader
        var clSO = new SerializedObject(courseLoader);
        clSO.FindProperty("holeTemplatePrefab").objectReferenceValue = holeTemplatePrefab;
        clSO.FindProperty("holeContainer").objectReferenceValue = holeContainer.transform;
        clSO.ApplyModifiedProperties();

        // Wire Gameplay Manager
        var gpSO = new SerializedObject(gameplayMgr);
        if (ballObj != null)
            gpSO.FindProperty("golfBall").objectReferenceValue = ballObj.GetComponent<GolfBall>();
        gpSO.FindProperty("shotController").objectReferenceValue = shotController;
        gpSO.FindProperty("courseLoader").objectReferenceValue = courseLoader;
        gpSO.FindProperty("scorecard").objectReferenceValue = scorecard;
        gpSO.FindProperty("cameraController").objectReferenceValue = camController;
        gpSO.FindProperty("clubSelector").objectReferenceValue = clubSel;
        gpSO.FindProperty("clubInventory").objectReferenceValue = clubInv;
        gpSO.FindProperty("hudScreen").objectReferenceValue = hudScreen;
        gpSO.FindProperty("scorecardScreen").objectReferenceValue = scorecardScreen;
        gpSO.FindProperty("defaultCourse").objectReferenceValue = starterCourse;
        gpSO.ApplyModifiedProperties();

        // Golfer Visual
        var golferObj = new GameObject("Golfer");
        var golferVisual = golferObj.AddComponent<GolferVisual>();
        var golferSO = new SerializedObject(golferVisual);
        if (ballObj != null)
            golferSO.FindProperty("golfBall").objectReferenceValue = ballObj.GetComponent<GolfBall>();
        golferSO.FindProperty("shotController").objectReferenceValue = shotController;
        golferSO.ApplyModifiedProperties();

        // Shot Feedback (on canvas)
        var feedbackObj = new GameObject("ShotFeedback");
        feedbackObj.transform.SetParent(canvasObj.transform, false);
        feedbackObj.AddComponent<RectTransform>();
        feedbackObj.AddComponent<GolfGame.Golf.ShotFeedback>();

        // Event System
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GolfGameplay.unity");
    }

    static void CreateBuilderScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camObj = new GameObject("BuilderCamera");
        var cam = camObj.AddComponent<UnityEngine.Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
        cam.orthographic = true;
        cam.orthographicSize = 30;
        camObj.transform.position = new Vector3(0, 40, 0);
        camObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        camObj.AddComponent<BuildingCamera>();

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);
        if (roughMat != null) ground.GetComponent<Renderer>().material = roughMat;

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var builderScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        builderScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        builderScaler.referenceResolution = new Vector2(1080, 1920);
        builderScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var builderPanel = CreateUIPanel(canvasObj.transform, "BuilderPanel");
        var builderScreen = builderPanel.AddComponent<BuilderScreen>();

        var courseNameUI = CreateUIText(builderPanel.transform, "CourseName", "My Course", 28, new Vector2(0, 400));
        var tierUI = CreateUIText(builderPanel.transform, "Tier", "Tier: 1-Hole Course", 18, new Vector2(0, 365));
        var holeCountUI = CreateUIText(builderPanel.transform, "HoleCount", "Built: 1/1", 16, new Vector2(0, 340));
        var currencyUI = CreateUIText(builderPanel.transform, "Currency", "$5,000", 24, new Vector2(300, 430));
        var drLevelUI = CreateUIText(builderPanel.transform, "DRLevel", "Driving Range Lv.1", 16, new Vector2(-300, -300));
        var drIncomeUI = CreateUIText(builderPanel.transform, "DRIncome", "$5/min", 14, new Vector2(-300, -325));
        var expCostUI = CreateUIText(builderPanel.transform, "ExpCost", "", 14, new Vector2(0, -200));
        var expStatusUI = CreateUIText(builderPanel.transform, "ExpStatus", "", 14, new Vector2(0, -225));

        var buildBtn = CreateUIButton(builderPanel.transform, "BuildHole", "Build New Hole", new Vector2(0, 50));
        var expandBtn = CreateUIButton(builderPanel.transform, "Expand", "Expand Course", new Vector2(0, -120));
        var upgDRBtn = CreateUIButton(builderPanel.transform, "UpgradeDR", "Upgrade Range", new Vector2(-300, -360));
        var backBtn = CreateUIButton(builderPanel.transform, "Back", "Back", new Vector2(350, -430));

        var bsSO = new SerializedObject(builderScreen);
        bsSO.FindProperty("courseNameText").objectReferenceValue = courseNameUI;
        bsSO.FindProperty("tierText").objectReferenceValue = tierUI;
        bsSO.FindProperty("holeCountText").objectReferenceValue = holeCountUI;
        bsSO.FindProperty("currencyText").objectReferenceValue = currencyUI;
        bsSO.FindProperty("drivingRangeLevelText").objectReferenceValue = drLevelUI;
        bsSO.FindProperty("drivingRangeIncomeText").objectReferenceValue = drIncomeUI;
        bsSO.FindProperty("expansionCostText").objectReferenceValue = expCostUI;
        bsSO.FindProperty("expansionStatusText").objectReferenceValue = expStatusUI;
        bsSO.FindProperty("buildHoleButton").objectReferenceValue = buildBtn.GetComponent<UnityEngine.UI.Button>();
        bsSO.FindProperty("expandButton").objectReferenceValue = expandBtn.GetComponent<UnityEngine.UI.Button>();
        bsSO.FindProperty("upgradeDrivingRangeButton").objectReferenceValue = upgDRBtn.GetComponent<UnityEngine.UI.Button>();
        bsSO.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<UnityEngine.UI.Button>();
        bsSO.ApplyModifiedProperties();

        // Event System
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/CourseBuilder.unity");
    }

    static void CreateDrivingRangeScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Simple placeholder scene
        var camObj = new GameObject("Camera");
        var cam = camObj.AddComponent<UnityEngine.Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
        camObj.transform.position = new Vector3(0, 5, -10);
        camObj.transform.rotation = Quaternion.Euler(20, 0, 0);

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "RangeGround";
        ground.transform.localScale = new Vector3(5, 1, 10);
        if (fairwayMat != null) ground.GetComponent<Renderer>().material = fairwayMat;

        // Target markers
        float[] distances = { 20, 40, 60, 80 };
        foreach (float d in distances)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            target.name = $"Target_{d}yds";
            target.transform.position = new Vector3(0, 0.05f, d);
            target.transform.localScale = new Vector3(4, 0.05f, 4);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = Color.white;
            target.GetComponent<Renderer>().material = mat;
        }

        // Canvas with back button
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var rangeScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        rangeScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        rangeScaler.referenceResolution = new Vector2(1080, 1920);
        rangeScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        CreateUIText(canvasObj.transform, "Title", "DRIVING RANGE", 32, new Vector2(0, 400));
        CreateUIText(canvasObj.transform, "Info", "Hit balls at targets to earn bonus income!", 18, new Vector2(0, 350));
        var backBtn = CreateUIButton(canvasObj.transform, "Back", "Back to Menu", new Vector2(0, -400));

        // Event System
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/DrivingRange.unity");
    }

    // ─────────────────────────────────────────────────────────────
    //  BUILD SETTINGS
    // ─────────────────────────────────────────────────────────────

    static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GolfGameplay.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/CourseBuilder.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/DrivingRange.unity", true)
        };
    }

    // ─────────────────────────────────────────────────────────────
    //  TAGS & LAYERS
    // ─────────────────────────────────────────────────────────────

    static void SetupTagsAndLayers()
    {
        // Tags are already defined in TagManager.asset
        // But let's make sure they exist via code as well
        string[] requiredTags = { "GolfBall", "Fairway", "Rough", "Green", "Bunker", "Water", "OutOfBounds", "Hole", "Tee" };

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in requiredTags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    // ─────────────────────────────────────────────────────────────
    //  UI HELPERS
    // ─────────────────────────────────────────────────────────────

    static GameObject CreateUIPanel(Transform parent, string name)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        return panel;
    }

    static TMP_FontAsset _cachedFont;
    static TMP_FontAsset GetDefaultFont()
    {
        if (_cachedFont != null) return _cachedFont;
        // Try loading from TMP resources
        _cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (_cachedFont != null) return _cachedFont;
        // Fallback: search project
        var guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (guids.Length > 0)
            _cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return _cachedFont;
    }

    static TextMeshProUGUI CreateUIText(Transform parent, string name, string text, int fontSize,
        Vector2 position, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultFont();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return tmp;
    }

    static GameObject CreateUIButton(Transform parent, string name, string label, Vector2 position,
        Vector2? size = null)
    {
        Vector2 btnSize = size ?? new Vector2(250, 50);

        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = btnSize;

        var image = btnObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.5f, 0.2f, 0.9f);

        var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.15f, 1f);
        btn.colors = colors;

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultFont();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnObj;
    }
}
#endif
