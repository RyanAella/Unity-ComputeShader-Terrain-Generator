/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System.Collections.Generic;
using UnityEngine;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using _Scripts.Terrain;

namespace _Scripts.Manager
{
    /// <summary>
    /// Manages the generation of a mesh using a compute shader.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Variables

        [Header("General Settings")] [SerializeField]
        private GeneralSettings generalSettings;

        [Header("Shader Settings")] [SerializeField]
        private ShaderSettings shaderSettings;

        [Header("Generators")] [SerializeField]
        private GroundGenerator groundGenerator;

        [SerializeField] private WaterGenerator waterGenerator;

        [Header("Ground Generation")] [SerializeField]
        private NoiseSettings groundNoiseSettings;

        [Header("Water Generation")] [SerializeField]
        private NoiseSettings waterNoiseSettings;

        [SerializeField] private GameObject player;

        // Reference to managers for noise, mesh and colourGradient generation.
        private NoiseGenerationManager _noiseGenerationManager; // Manager for noise generation
        private FalloffMapManager _falloffMapManager; // Manager for falloffMapManager generation
        private MeshGenerationManager _meshGenerationManager; // Manager for mesh generation
        private ColourGenerationManager _colourGenerationManager; // Manager for colourGradient generation

        private List<Vector4> _colourPaletteWater;
        private List<Vector4> _colourPaletteGround;

        private Vector2[] _offsetVectorsGround;
        private Vector2[] _offsetVectorsWater;

        private GroundGenerator _ground;
        private WaterGenerator _water;

        #endregion

        #region Methods

        /// <summary>
        /// This method is called on the start of the game.
        /// It generates a mesh, colours it, and adjusts the height of the mesh vertices.
        /// </summary>
        private void Start()
        {
            ValidateParameters();

            InitializeManagers();
            InitializeColourPalette();
            InitializeParameters();
            InitializeBuffers();

            bool success = GenerateTerrain();

            // if (success)
            // {
            //     Vector3 position = new Vector3(transform.position.x, transform.position.y + groundNoiseSettings.maxTerrainHeight, transform.position.z);
            //     Instantiate(player, position, Quaternion.identity);
            // }

            ReleaseBuffers();
        }

        private void ValidateParameters()
        {
            if (groundNoiseSettings.lacunarity < 1)
            {
                groundNoiseSettings.lacunarity = 1;
            }

            if (waterNoiseSettings.lacunarity < 1)
            {
                waterNoiseSettings.lacunarity = 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeManagers()
        {
            _noiseGenerationManager = new NoiseGenerationManager();

            _falloffMapManager = new FalloffMapManager();

            _meshGenerationManager = new MeshGenerationManager();

            _colourGenerationManager = new ColourGenerationManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeColourPalette()
        {
            _colourPaletteGround = ColourGenerationManager.GetColorPalette(generalSettings.colourGradient, 0.3f, 1.0f);
            _colourPaletteWater = ColourGenerationManager.GetColorPalette(generalSettings.colourGradient, 0.0f, 0.3f);
        }

        private void InitializeParameters()
        {
            InitializeOffsetVectors(groundNoiseSettings);
            InitializeOffsetVectors(waterNoiseSettings);
        }

        private void InitializeOffsetVectors(NoiseSettings settings)
        {
            System.Random pseudoRandom = new System.Random(settings.seed.GetHashCode());

            // Offset vectors for fractional brownian motion
            settings.noiseLayerOffsetVectors = new Vector2[settings.noiseLayers.Length];
            for (int i = 0; i < settings.noiseLayers.Length; i++)
            {
                settings.noiseLayerOffsetVectors[i] = new Vector2(
                    pseudoRandom.Next(-100000, 100000) + settings.offset.x,
                    pseudoRandom.Next(-100000, 100000) + settings.offset.y);
            }

            // Offset vectors for domain warping
            int warpSteps = settings.warpSteps;
            settings.offsetVectors = new Vector2[warpSteps * 2];

            for (int i = 0; i < warpSteps; i++)
            {
                settings.offsetVectors[i] =
                    new Vector2((float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.x,
                        (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.y);
                settings.offsetVectors[i + 1] =
                    new Vector2((float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.x,
                        (float)pseudoRandom.NextDouble() * settings.warpStepSize + settings.offset.y);

                if (i == 0) settings.offsetVectors[0] = Vector2.zero;
            }
        }

        private void InitializeBuffers()
        {
            Vector2Int chunkSize = GeneralSettings.chunkSize;

            _noiseGenerationManager.InitializeBuffers(chunkSize, groundNoiseSettings);

            _falloffMapManager.InitializeBuffers(chunkSize);

            GeneratorFunctions.InitializeBuffers(chunkSize);

            // ToDo:
            // _colourGenerationManager.InitializeBuffers();
        }

        private void ReleaseBuffers()
        {
            _noiseGenerationManager.ReleaseBuffers();

            _falloffMapManager.ReleaseBuffers();

            GeneratorFunctions.ReleaseBuffers();

            // ToDo:
            // _colourGenerationManager.ReleaseBuffers();
        }

        /// <summary>
        /// Generates a mesh using the provided chunkSize and noise settings.
        /// </summary>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateTerrain()
        {
            if (_ground == null)
            {
                _ground = Instantiate(groundGenerator, transform.position, Quaternion.identity, transform);
            }

            if (_water == null)
            {
                _water = Instantiate(waterGenerator, transform.position, Quaternion.identity, transform);
            }

            // try
            // {
            // Cache MeshFilter and MeshCollider components
            MeshFilter groundMeshFilter = _ground.GetComponent<MeshFilter>();
            MeshCollider groundCollider = _ground.GetComponent<MeshCollider>();
            MeshFilter waterMeshFilter = _water.GetComponent<MeshFilter>();
            MeshCollider waterCollider = _water.GetComponent<MeshCollider>();

            // If MeshCollider component doesn't exist, add it and assign sharedMesh
            if (groundMeshFilter != null)
            {
                if (groundCollider == null)
                {
                    groundCollider = _ground.gameObject.AddComponent<MeshCollider>();
                    groundCollider.cookingOptions = MeshColliderCookingOptions.None;
                }

                _ground.GenerateGround(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager,
                    _falloffMapManager, generalSettings, shaderSettings, groundNoiseSettings, _colourPaletteGround,
                    groundMeshFilter);

                groundCollider.sharedMesh = groundMeshFilter.sharedMesh;

                // If MeshCollider component doesn't exist, add it and assign sharedMesh
                if (waterMeshFilter != null)
                {
                    if (waterCollider == null)
                    {
                        waterCollider = _water.gameObject.AddComponent<MeshCollider>();
                        waterCollider.cookingOptions = MeshColliderCookingOptions.None;
                    }
                }

                _water.GenerateWater(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager,
                    _falloffMapManager, generalSettings, shaderSettings, waterNoiseSettings, _colourPaletteWater,
                    waterMeshFilter,
                    groundNoiseSettings.maxTerrainHeight);

                waterCollider.sharedMesh = waterMeshFilter.sharedMesh;
            }

            else
            {
                throw new System.Exception("MeshFilter component not found on the ground object.");
            }
            // }
            // catch (System.Exception ex)
            // {
            //     Debug.LogError($"Error in GenerateTerrain: {ex.Message}");
            //     return false;
            // }

            return true;
        }

        #endregion
    }
}