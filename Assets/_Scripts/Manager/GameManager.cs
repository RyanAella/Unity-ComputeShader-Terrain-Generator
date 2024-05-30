/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System.Collections.Generic;
using System.Linq;
using _Scripts.ScriptableObjects;
using _Scripts.Terrain;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Manager
{
    /// <summary>
    ///     This class is responsible for managing the generation of a mesh using a compute shader.
    ///     It initializes a compute buffer, generates a noise map, and then uses that noise map to generate a mesh.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Variables
        
        // General settings for the mesh generation.
        [Header("General Settings")] 
        [SerializeField] private GeneralSettings generalSettings; // General settings ScriptableObject
        
        [Header("Shader Settings")] 
        [SerializeField] private ShaderSettings shaderSettings;
        
        [Header("Generators")]
        [SerializeField] private GroundGenerator groundGenerator;
        [SerializeField] private WaterGenerator waterGenerator;

        [Header("Ground Generation")]
        [SerializeField] private NoiseSettings groundNoiseSettings; // Noise settings for mesh generation
        [SerializeField] private bool generateMultipleLayers;
        
        [Header("Water Generation")]
        [SerializeField] private NoiseSettings waterNoiseSettings; // Noise settings for mesh generation

        [SerializeField] private GameObject player;
        
        // Reference to managers for noise, mesh and colourGradient generation.
        private NoiseGenerationManager _noiseGenerationManager; // Manager for noise generation
        private MeshGenerationManager _meshGenerationManager; // Manager for mesh generation
        private ColourGenerationManager _colourGenerationManager; // Manager for colourGradient generation
        
        private List<Vector4> _colourPaletteWater;
        private List<Vector4> _colourPaletteGround;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the GameManager component.
        /// This method is called when the GameManager component is first created.
        /// </summary>
        private void Awake()
        {
            InitializeManagers();
        }

        /// <summary>
        /// This method is called on the start of the game.
        /// It generates a mesh, colours it, and adjusts the height of the mesh vertices.
        /// </summary>
        private void Start()
        {
            InitializeColourPalette();

            bool success = GenerateTerrain();

            if (success)
            {
                Vector3 position = new Vector3(transform.position.x, transform.position.y + groundNoiseSettings.maxTerrainHeight, transform.position.z);
                Instantiate(player, position, Quaternion.identity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeManagers()
        {
            _noiseGenerationManager = new NoiseGenerationManager();
            _noiseGenerationManager.InitializeBuffers(generalSettings.resolution);

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

        /// <summary>
        /// Generates a mesh using the provided resolution and noise settings.
        /// </summary>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateTerrain()
        {
            var ground = Instantiate(groundGenerator, transform.position, Quaternion.identity);
            ground.transform.parent = transform;
            
            ground.GenerateGround(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager, shaderSettings, generalSettings, groundNoiseSettings, _colourPaletteGround, generateMultipleLayers);
            
            // Generate ground and get the MeshFilter component
            MeshFilter groundMeshFilter = ground.GetComponent<MeshFilter>();

            if (ground.GetComponent<MeshCollider>() == null)
            {
                MeshCollider groundCollider = ground.AddComponent<MeshCollider>();
                groundCollider.cookingOptions = MeshColliderCookingOptions.None;
                groundCollider.sharedMesh = groundMeshFilter.sharedMesh;
            }

            // Check if MeshFilter component exists
            if (groundMeshFilter != null)
            {
                var water = Instantiate(waterGenerator, transform.position, Quaternion.identity);
                water.transform.parent = transform;
            
                water.GenerateWater(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager, shaderSettings, generalSettings, waterNoiseSettings, _colourPaletteWater, groundNoiseSettings.maxTerrainHeight);
            }
            else
            {
                Debug.LogError("MeshFilter component not found on the ground object.");
            } 
            
            return true;
        }

        #endregion
    }
}