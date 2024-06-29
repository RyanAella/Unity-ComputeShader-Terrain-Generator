/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: This script manages the generation of terrain meshes (ground and water) using compute shaders in Unity.
 *              It handles initialization, validation of parameters, and spawning of the player character on the generated terrain.
 * License: MIT License
 */

using System.Collections.Generic;
using UnityEngine;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using _Scripts.TerrainGenerators;

namespace _Scripts.Manager
{
    /// <summary>
    /// Manages the generation of a mesh using a compute shader.
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        #region Variables

        [Header("Settings")] 
        [SerializeField] private GeneralSettings generalSettings; // General settings for the game
        [SerializeField] private NoiseSettings noiseSettings; // Noise settings
        
        [SerializeField] private Shaders shaders; // Shader settings for the compute shader

        [SerializeField] private bool useColourPalette;
        [SerializeField] private ColourGradient colourGradient; // Colour gradient for the ground
        [SerializeField] private ColourGradient greyscaleGradient; // Colour palette for the ground

        [Header("Generators")] [SerializeField]
        private GroundGenerator groundGenerator; // Ground generator object

        [SerializeField] private WaterGenerator waterGenerator; // Water generator object

        [Header("References")] [SerializeField]
        private bool spawnPlayer;

        [SerializeField] private GameObject player; // Player object

        private TerrainSettings _terrainSettings; // TerrainGenerators settings
        private TerrainGenerationManagers _managers; // TerrainGenerators generation managers

        private List<Vector4> _colourPalette; // Colour palette for the ground

        private int _colourCount; // Represents the number of colours in the ground colour palette

        private GroundGenerator _ground; // Ground generator object
        private WaterGenerator _water; // Water generator object

        private MeshFilter _groundMeshFilter; // MeshFilter component of the ground object
        private MeshCollider _groundCollider; // MeshCollider component of the ground object
        private MeshFilter _waterMeshFilter; // MeshFilter component of the water object
        private MeshCollider _waterCollider; // MeshCollider component of the water object
        
        private List<float> _colourHeights;

        #endregion

        #region Methods

        /// <summary>
        /// This method is called on the start of the game.
        /// It generates a mesh, colours it, and adjusts the height of the mesh vertices.
        /// </summary>
        private void Start()
        {
            // Initialize the necessary components
            Init();

            // Initialize the buffers for the various managers
            ComputeBufferManager.InitializeBuffers(generalSettings.resolution, _terrainSettings, _colourCount);

            // Generate the terrain mesh
            GenerateTerrain();

            // If the terrain generation was successful, instantiate the player at the top of the terrain
            if (spawnPlayer)
            {
                Vector3 position = new Vector3(transform.position.x,
                    transform.position.y + generalSettings.maxTerrainHeight, transform.position.z);
                Instantiate(player, position, Quaternion.identity);
            }

            // Release the buffers
            ComputeBufferManager.Instance.ReleaseBuffers();
        }

        /// <summary>
        /// Validates the parameters and sets the lacunarity of noise settings to 1 if it's less than 1.
        /// Instantiates the ground and water objects if they are null.
        /// </summary>
        private void ValidateParameters()
        {
            // Ensure that the lacunarity of ground noise settings is at least 1
            if (noiseSettings.lacunarity < 1)
            {
                noiseSettings.lacunarity = 1;
            }

            // Instantiate the ground object if it is null
            if (_ground == null)
            {
                _ground = Instantiate(groundGenerator, transform.position, Quaternion.identity, transform);
            }

            // Instantiate the water object if it is null
            if (_water == null)
            {
                _water = Instantiate(waterGenerator, transform.position, Quaternion.identity, transform);
            }
        }

        /// <summary>
        ///     Validates the parameters and sets the lacunarity of noise settings to 1 if it's less than 1.
        /// </summary>
        private void Init()
        {
            // Validate the parameters before proceeding
            ValidateParameters();
            
            //
            _terrainSettings = new TerrainSettings
            {
                GeneralSettings =  generalSettings,
                NoiseSettings = noiseSettings
            };
            
            // Initialize Managers
            _managers = new TerrainGenerationManagers
            {
                NoiseGenerationManager = new NoiseGenerationManager(),
                FalloffMapManager = new FalloffMapManager(),
                MeshGenerationManager = new MeshGenerationManager(),
                ColourGenerationManager = new ColourGenerationManager()
            };

            // Initialize Colour Palettes
            _colourPalette =
                ColourGenerationManager.GetColorPalette(colourGradient.colours, out _colourCount, out _colourHeights);

            // Initialize Offset Vectors
            InitializeOffsetVectors(noiseSettings);

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
        /// Initializes the offset vectors for the given noise settings.
        /// </summary>
        /// <param name="settings">The noise settings to initialize the offset vectors for.</param>
        private static void InitializeOffsetVectors(NoiseSettings settings)
        {
            // Create a pseudo-random number generator based on the noise settings seed
            var pseudoRandom = new System.Random(settings.seed.GetHashCode());

            // Generate offset vectors for fractional brownian motion
            settings.noiseLayerOffsetVectors = new Vector2[settings.noiseLayerSettings.Length];
            for (int i = 0; i < settings.noiseLayerSettings.Length; i++)
            {
                int randomX = pseudoRandom.Next(-100000, 100000);
                int randomY = pseudoRandom.Next(-100000, 100000);

                // Generate a random offset within a range and add it to the noise settings offset
                settings.noiseLayerOffsetVectors[i] =
                    new Vector2(randomX + settings.offset.x, randomY + settings.offset.y);
            }

            // Generate offset vectors for domain warping
            int warpSteps = settings.warpSteps;
            settings.offsetVectors = new Vector2[warpSteps * 2];

            // Loop through each warp step
            for (int i = 0; i < warpSteps; i++)
            {
                // Generate random offsets for domain warping
                float xOffset1 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.x;
                float yOffset1 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.y;

                float xOffset2 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.x;
                float yOffset2 = (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.y;

                // Set the offset vectors for the current step
                settings.offsetVectors[i + i] = new Vector2(xOffset1, yOffset1);
                settings.offsetVectors[i + i + 1] = new Vector2(xOffset2, yOffset2);

                // Set the first offset vector to zero for domain warping
                if (i == 0) settings.offsetVectors[0] = Vector2.zero;
            }
        }

        /// <summary>
        ///     Generates terrain meshes for ground and water based on the provided noise settings.
        /// </summary>
        /// <returns>True if the terrain mesh generation was successful, false otherwise.</returns>
        private void GenerateTerrain()
        {
            // Check if MeshFilter and MeshCollider components are present for ground and water
            bool groundComponentsPresent = _groundMeshFilter != null && _groundCollider != null;
            bool waterComponentsPresent = _waterMeshFilter != null && _waterCollider != null;

            // Check if the required components for ground generation are present
            if (groundComponentsPresent)
            {
                // Generate ground terrain
                _ground.GenerateGround(_managers, _terrainSettings, shaders, _colourPalette, _colourHeights.ToArray(),
                    _groundMeshFilter, _colourCount);
            }
            else
            {
                // Log an error if MeshFilter or MeshCollider component is missing on the ground object
                Debug.LogError("MeshFilter or MeshCollider component not found on the ground object.");
            }

            // Check if the required components for water generation are present
            if (waterComponentsPresent)
            {
                // Generate water terrain
                _water.GenerateWater(_managers, _terrainSettings, shaders, _colourPalette, _colourHeights.ToArray(),
                    _waterMeshFilter, generalSettings.maxTerrainHeight, _colourCount);
            }
            else
            {
                // Log an error if MeshFilter or MeshCollider component is missing on the water object
                Debug.LogError("MeshFilter or MeshCollider component not found on the water object.");
            }

            // Assign generated ground mesh to the ground collider if ground components are present
            if (groundComponentsPresent)
            {
                _groundCollider.sharedMesh = _groundMeshFilter.sharedMesh;
            }

            // Assign generated water mesh to the water collider if water components are present
            if (waterComponentsPresent)
            {
                _waterCollider.sharedMesh = _waterMeshFilter.sharedMesh;
            }
        }

        #endregion
    }
}