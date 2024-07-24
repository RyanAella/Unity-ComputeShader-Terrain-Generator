/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: Manages the generation of terrain meshes (ground and water) using compute shaders in Unity.
 *              Handles initialization, parameter validation, and player spawning on the generated terrain.
 * License: MIT License
 */

using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Generators
{
    /// <summary>
    /// Manages the generation of a mesh using a compute shader.
    /// </summary>
    public class EnvironmentGenerator : MonoBehaviour
    {
        #region Variables

        [Header("Settings")] [SerializeField] private GeneralSettings generalSettings; // General game settings
        [SerializeField] private NoiseSettings noiseSettings; // Noise generation settings
        [SerializeField] private Shaders shaders; // Shader settings for compute shaders

        [Tooltip("Toggle between using color or greyscale palette")] 
        [SerializeField] private bool useColourPalette;
        [SerializeField] private ColourGradient colourGradient; // Color gradient for terrain
        [SerializeField] private ColourGradient greyscaleGradient; // Greyscale gradient for terrain

        [Header("Generators")] 
        [Tooltip("Toggle water generation")] 
        [SerializeField] private bool generateWater;
        [SerializeField] private TerrainGenerator groundGenerator; // Prefab for ground generation
        [SerializeField] private TerrainGenerator waterGenerator; // Prefab for water generation

        [Header("References")] [SerializeField]
        private bool spawnPlayer; // Toggle player spawning
        [SerializeField] private GameObject player; // Player object prefab

        private TerrainSettings _terrainSettings; // Settings for terrain generation
        private TerrainGenerationManagers _managers; // Managers for terrain generation

        private List<Vector4> _colourPalette; // List of colors for the terrain
        private int _colourCount; // Number of colors in the palette

        private TerrainGenerator _ground; // Instance of the ground generator
        private TerrainGenerator _water; // Instance of the water generator

        private MeshFilter _groundMeshFilter; // MeshFilter for ground
        private MeshCollider _groundCollider; // MeshCollider for ground
        private MeshFilter _waterMeshFilter; // MeshFilter for water
        private MeshCollider _waterCollider; // MeshCollider for water

        private List<float> _colourHeights; // Heights for color gradients

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the environment by setting up necessary components and generating terrain.
        /// </summary>
        private void Start()
        {
            Init(); // Initialize components and settings

            Generate(); // Generate terrain and possibly instantiate the player

            SpawnPlayer();
        }

        // private void Update()
        // {
        //     Generate();
        // }

        /// <summary>
        /// Initializes settings, managers, and components for terrain generation.
        /// </summary>
        private void Init()
        {
            ValidateParameters(); // Ensure parameters are valid

            // Set up terrain generation settings
            _terrainSettings = new TerrainSettings
            {
                GeneralSettings = generalSettings,
                NoiseSettings = noiseSettings
            };

            InitializeManagers(); // Set up terrain generation managers
            InitializeColourPalette(); // Set up color palettes
            InitializeOffsetVectors(noiseSettings); // Set up noise offset vectors
            CacheMeshComponents(); // Cache MeshFilter and MeshCollider components

            // Initialize buffers for compute shaders
            ComputeBufferManager.InitializeBuffers(generalSettings.resolution, _terrainSettings, _colourCount);
        }

        /// <summary>
        /// Validates parameters and instantiates ground and water objects if they are null.
        /// </summary>
        private void ValidateParameters()
        {
            // Ensure noise settings lacunarity is at least 1
            if (noiseSettings.lacunarity < 1)
            {
                noiseSettings.lacunarity = 1;
            }

            // Instantiate ground generator if not already done
            if (_ground == null)
            {
                _ground = Instantiate(groundGenerator, transform.position, Quaternion.identity, transform);

                if (_ground == null)
                {
                    Debug.LogError("Failed to instantiate ground generator.");
                }
            }

            // Instantiate water generator if not already done
            if (_water == null)
            {
                _water = Instantiate(waterGenerator, transform.position, Quaternion.identity, transform);

                if (_water == null)
                {
                    Debug.LogError("Failed to instantiate water generator.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeManagers()
        {
            // Initialize Managers
            _managers = new TerrainGenerationManagers
            {
                NoiseGenerationManager = new NoiseGenerationManager(),
                ValueClampManager = new ValueClampManager(),
                // FalloffMapManager = new FalloffMapManager(),
                MeshGenerationManager = new MeshGenerationManager(),
                ColourGenerationManager = new ColourGenerationManager()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeColourPalette()
        {
            // Initialize Colour Palettes
            if (useColourPalette)
            {
                _colourPalette =
                    ColourGenerationManager.GetColorPalette(colourGradient.colours, out _colourCount,
                        out _colourHeights);
            }
            else
            {
                _colourPalette =
                    ColourGenerationManager.GetColorPalette(greyscaleGradient.colours, out _colourCount,
                        out _colourHeights);
            }

            if (_colourPalette == null || _colourPalette.Count == 0)
            {
                Debug.LogWarning("Colour palette is null or empty. Using default colours.");
                _colourPalette = new List<Vector4>();
            }
        }

        /// <summary>
        /// Sets up offset vectors for noise settings.
        /// </summary>
        /// <param name="settings">Noise settings to initialize.</param>
        private static void InitializeOffsetVectors(NoiseSettings settings)
        {
            var pseudoRandom = new System.Random(settings.seed.GetHashCode());

            int warpSteps = settings.warpSteps;
            settings.offsetVectors = new Vector2[warpSteps * 2];

            for (int i = 0; i < warpSteps; i++)
            {
                float xOffset1 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.x;
                float yOffset1 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.y;

                float xOffset2 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.x;
                float yOffset2 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.y;

                settings.offsetVectors[i + i] = new Vector2(xOffset1, yOffset1) * 10;
                settings.offsetVectors[i + i + 1] = new Vector2(xOffset2, yOffset2) * 10;

                if (i == 0) settings.offsetVectors[0] = Vector2.zero;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CacheMeshComponents()
        {
            // Cache MeshFilter and MeshCollider components
            _groundMeshFilter = _ground.GetComponent<MeshFilter>();
            _groundCollider = _ground.GetComponent<MeshCollider>();
            _waterMeshFilter = _water.GetComponent<MeshFilter>();
            _waterCollider = _water.GetComponent<MeshCollider>();

            // Set cooking options for MeshColliders
            _groundCollider.cookingOptions = MeshColliderCookingOptions.None;
            _waterCollider.cookingOptions = MeshColliderCookingOptions.None;
        }

        /// <summary>
        /// Generates terrain meshes for ground and water based on noise settings.
        /// </summary>
        private void Generate()
        {
            // Debug.Log("Memory used before generation: " + Profiler.GetTotalAllocatedMemoryLong());

            bool groundComponentsPresent = _groundMeshFilter != null && _groundCollider != null;
            bool waterComponentsPresent = _waterMeshFilter != null && _waterCollider != null;

            if (groundComponentsPresent)
            {
                _ground.GenerateTerrain(_managers, _terrainSettings, shaders, _colourPalette, _colourHeights.ToArray(),
                    _groundMeshFilter, _colourCount, false, _terrainSettings.GeneralSettings.maxTerrainHeight);
            }
            else
            {
                Debug.LogError("MeshFilter or MeshCollider component not found on the ground object.");
            }

            if (groundComponentsPresent)
            {
                _groundCollider.sharedMesh = _groundMeshFilter.sharedMesh;
            }

            if (generateWater)
            {
                if (waterComponentsPresent)
                {
                    _water.GenerateTerrain(_managers, _terrainSettings, shaders, _colourPalette,
                        _colourHeights.ToArray(),
                        _waterMeshFilter, _colourCount, true, _terrainSettings.GeneralSettings.maxTerrainHeight);
                }
                else
                {
                    Debug.LogError("MeshFilter or MeshCollider component not found on the water object.");
                }

                if (waterComponentsPresent)
                {
                    _waterCollider.sharedMesh = _waterMeshFilter.sharedMesh;
                }
            }

            // Release the buffers
            ComputeBufferManager.Instance.ReleaseBuffers();

            // Debug.Log("Memory used after generation: " + Profiler.GetTotalAllocatedMemoryLong());
        }

        private void OnDestroy()
        {
            if (ComputeBufferManager.Instance != null)
            {
                ComputeBufferManager.Instance.ReleaseBuffers();
            }
        }

        private void OnDisable()
        {
            if (ComputeBufferManager.Instance != null)
            {
                ComputeBufferManager.Instance.ReleaseBuffers();
            }
        }

        private void SpawnPlayer()
        {
            // If the terrain generation was successful, instantiate the player at the top of the terrain
            if (spawnPlayer)
            {
                Vector3 position = new Vector3(transform.position.x,
                    transform.position.y + generalSettings.maxTerrainHeight, transform.position.z);
                Instantiate(player, position, Quaternion.identity);
            }
        }

        #endregion
    }
}