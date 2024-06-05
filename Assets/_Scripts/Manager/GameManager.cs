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

#if UNITY_EDITOR
#endif

namespace _Scripts.Manager
{
    /// <summary>
    /// Manages the generation of a mesh using a compute shader.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float noiseScale;

        // General settings for the mesh generation.
        [Header("General Settings")] [SerializeField]
        private GeneralSettings generalSettings; // General settings ScriptableObject

        [Header("Shader Settings")] [SerializeField]
        private ShaderSettings shaderSettings;

        [Header("Generators")] [SerializeField]
        private GroundGenerator groundGenerator;

        [SerializeField] private WaterGenerator waterGenerator;

        [Header("Ground Generation")] [SerializeField]
        private NoiseSettings groundNoiseSettings; // Noise settings for mesh generation

        [Header("Water Generation")] [SerializeField]
        private NoiseSettings waterNoiseSettings; // Noise settings for mesh generation

        [SerializeField] private GameObject player;

        // Reference to managers for noise, mesh and colourGradient generation.
        private NoiseGenerationManager _noiseGenerationManager; // Manager for noise generation
        private FalloffMapManager _falloffMapManager; // Manager for falloffMapManager generation
        private MeshGenerationManager _meshGenerationManager; // Manager for mesh generation
        private ColourGenerationManager _colourGenerationManager; // Manager for colourGradient generation

        private List<Vector4> _colourPaletteWater;
        private List<Vector4> _colourPaletteGround;

        private GroundGenerator ground;
        private WaterGenerator water;

        #endregion

        #region Methods

        /// <summary>
        /// This method is called on the start of the game.
        /// It generates a mesh, colours it, and adjusts the height of the mesh vertices.
        /// </summary>
        private void Start()
        {
            groundNoiseSettings.noiseScale = noiseScale;
            
            InitializeManagers();
            InitializeColourPalette();
            InitializeBuffers();
            bool success = GenerateTerrain();

            if (success)
            {
                Vector3 position = new Vector3(transform.position.x, transform.position.y + groundNoiseSettings.maxTerrainHeight, transform.position.z);
                Instantiate(player, position, Quaternion.identity);
            }

            ReleaseBuffers();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeManagers()
        {
            _noiseGenerationManager = new NoiseGenerationManager();

            _falloffMapManager = new FalloffMapManager();

            // Initialize the mesh generation manager and the compute buffers.
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

        private void InitializeBuffers()
        {
            int chunkSize = generalSettings.chunkSize;

            _noiseGenerationManager.InitializeBuffers(chunkSize);

            _falloffMapManager.InitializeBuffers(chunkSize);

            GeneratorFunctions.InitializeBuffers(chunkSize);
        }

        private void ReleaseBuffers()
        {
            _noiseGenerationManager.ReleaseBuffers();

            _falloffMapManager.ReleaseBuffers();

            GeneratorFunctions.ReleaseBuffers();
        }

        /// <summary>
        /// Generates a mesh using the provided chunkSize and noise settings.
        /// </summary>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateTerrain()
        {
            if (ground == null)
            {
                ground = Instantiate(groundGenerator, transform.position, Quaternion.identity, transform);
            }

            if (water == null)
            {
                water = Instantiate(waterGenerator, transform.position, Quaternion.identity, transform);
            }

            try
            {
                // Cache MeshFilter and MeshCollider components
                MeshFilter groundMeshFilter = ground.GetComponent<MeshFilter>();
                MeshCollider groundCollider = ground.GetComponent<MeshCollider>();
                MeshFilter waterMeshFilter = water.GetComponent<MeshFilter>();
                MeshCollider waterCollider = water.GetComponent<MeshCollider>();

                // If MeshCollider component doesn't exist, add it and assign sharedMesh
                if (groundMeshFilter != null)
                {
                    if (groundCollider == null)
                    {
                        groundCollider = ground.gameObject.AddComponent<MeshCollider>();
                        groundCollider.cookingOptions = MeshColliderCookingOptions.None;
                    }

                    ground.GenerateGround(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager,
                        _falloffMapManager,
                        shaderSettings, generalSettings, groundNoiseSettings, _colourPaletteGround, groundMeshFilter);
                    
                    groundCollider.sharedMesh = groundMeshFilter.sharedMesh;

                    // If MeshCollider component doesn't exist, add it and assign sharedMesh
                    if (waterMeshFilter != null)
                    {
                        if (waterCollider == null)
                        {
                            waterCollider = water.gameObject.AddComponent<MeshCollider>();
                            waterCollider.cookingOptions = MeshColliderCookingOptions.None;
                        }
                    }

                    water.GenerateWater(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager,
                        _falloffMapManager,
                        shaderSettings, generalSettings, waterNoiseSettings, _colourPaletteWater, waterMeshFilter,
                        groundNoiseSettings.maxTerrainHeight);
                    
                    waterCollider.sharedMesh = waterMeshFilter.sharedMesh;
                }

                else
                {
                    throw new System.Exception("MeshFilter component not found on the ground object.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in GenerateTerrain: {ex.Message}");
                return false;
            }

            return true;
        }

        #endregion
    }
}