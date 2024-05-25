/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System.Collections.Generic;
using System.Linq;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using _Scripts.Terrain;
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
        [SerializeField] private Vector2Int resolution = new(20, 20); // Resolution of the generated mesh

        [SerializeField] private float islandRadius; // Radius of the island.
        
        [SerializeField] private GroundGenerator groundGenerator;
        [SerializeField] private WaterGenerator waterGenerator;

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")] 
        [SerializeField] private ComputeShader noiseGenerationComputeShader; // Compute shader for noise generation
        [SerializeField] private ComputeShader valueClampComputeShader; // Compute shader for noise generation
        [SerializeField] private ComputeShader colourGenerationComputeShader; // Compute shader for colourGradient generation

        [Header("Ground Generation")]
        [SerializeField] private NoiseSettings groundNoiseSettings; // Noise settings for mesh generation
        [SerializeField] private Gradient groundColourGradient; // Gradient for colourGradient mapping
        [SerializeField] private bool generateMultipleLayers;
        
        [Header("Water Generation")]
        [SerializeField] private NoiseSettings waterNoiseSettings; // Noise settings for mesh generation
        [SerializeField] private Gradient waterColourGradient; // Gradient for colourGradient mapping
        
        [SerializeField] private Gradient colourGradient; // Gradient for colourGradient mapping

        // Reference to managers for noise, mesh and colourGradient generation.
        private NoiseGenerationManager _noiseGenerationManager; // Manager for noise generation
        private MeshGenerationManager _meshGenerationManager; // Manager for mesh generation
        private ColourGenerationManager _colourGenerationManager; // Manager for colourGradient generation
        
        private List<Vector4> _colourPaletteWater;
        private List<Vector4> _colourPaletteGround;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes the GameManager component.
        /// This method is called when the GameManager component is first created.
        /// </summary>
        private void Awake()
        {
            _noiseGenerationManager = new NoiseGenerationManager();
            _noiseGenerationManager.InitializeBuffers(resolution);

            // Initialize the mesh generation manager and the compute buffers.
            _meshGenerationManager = new MeshGenerationManager();

            _colourGenerationManager = new ColourGenerationManager();
        }

        /// <summary>
        /// This method is called on the start of the game.
        /// It generates a mesh, colours it, and adjusts the height of the mesh vertices.
        /// </summary>
        private void Start()
        {
            _colourPaletteWater = GetColorPalette(colourGradient, 0.0f, 0.3f);
            _colourPaletteGround = GetColorPalette(colourGradient, 0.3f, 1.0f);
            
            GenerateTerrain();
        }

        /// <summary>
        /// Generates a mesh using the provided resolution and noise settings.
        /// </summary>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private void GenerateTerrain()
        {
            var ground = Instantiate(groundGenerator, transform.position, Quaternion.identity);
            ground.transform.parent = transform;
            
            ground.GenerateGround(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager, noiseGenerationComputeShader, valueClampComputeShader, colourGenerationComputeShader, resolution, groundNoiseSettings, _colourPaletteGround, islandRadius, generateMultipleLayers);
            
            // Generate ground and get the MeshFilter component
            MeshFilter groundMeshFilter = ground.GetComponent<MeshFilter>();

            // Check if MeshFilter component exists
            if (groundMeshFilter != null)
            {
                float maxHeight = groundMeshFilter.sharedMesh.vertices.Max(vertex => vertex.y);
                
                var water = Instantiate(waterGenerator, transform.position, Quaternion.identity);
                water.transform.parent = transform;
            
                water.GenerateWater(_noiseGenerationManager, _meshGenerationManager, _colourGenerationManager, noiseGenerationComputeShader, valueClampComputeShader, colourGenerationComputeShader, resolution, waterNoiseSettings, _colourPaletteWater, islandRadius, maxHeight);
                
                MeshFilter waterMeshFilter = water.GetComponent<MeshFilter>();
            }
            else
            {
                Debug.LogError("MeshFilter component not found on the ground object.");
            } 
        }
        
        private List<Vector4> GetColorPalette(Gradient gradient, float minRange, float maxRange)
        {
            List<Vector4> palette = new List<Vector4>();

            foreach (var colorKey in gradient.colorKeys)
            {
                if (colorKey.time >= minRange && colorKey.time <= maxRange)
                {
                    // Find the corresponding alpha key
                    float alpha = 1.0f; // Default alpha
                    foreach (var alphaKey in gradient.alphaKeys)
                    {
                        if (Mathf.Approximately(alphaKey.time, colorKey.time))
                        {
                            alpha = alphaKey.alpha;
                            break;
                        }
                    }

                    palette.Add(new Vector4(colorKey.color.r, colorKey.color.g, colorKey.color.b, alpha));
                }
            }

            return palette;
        }

        #endregion
    }
}