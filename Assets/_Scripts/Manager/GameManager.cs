/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System.Collections.Generic;
using System.Linq;
using _Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Manager
{
    /// <summary>
    ///     This class is responsible for managing the generation of a mesh using a compute shader.
    ///     It initializes a compute buffer, generates a noise map, and then uses that noise map to generate a mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GameManager : MonoBehaviour
    {
        #region Variables

        // General settings for the mesh generation.
        [Header("General Settings")] 
        [SerializeField] private Vector2Int resolution = new(20, 20); // Resolution of the generated mesh

        [SerializeField] private float islandRadius; // Radius of the island.

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")] 
        [SerializeField] private ComputeShader meshGenerationComputeShader; // Compute shader for noise generation
        [SerializeField] private ComputeShader colourGenerationComputeShader; // Compute shader for colourGradient generation

        [SerializeField] private NoiseSettings noiseSettings; // Noise settings for mesh generation
        [SerializeField] private Gradient colourGradient; // Gradient for colourGradient mapping

        // Reference to the MeshFilter component attached to the GameObject.
        private MeshFilter _meshFilter; // Reference to the MeshFilter component

        private Vector3[] _vertices;
        private int[] _triangles;

        // Reference to managers for mesh and colourGradient generation.
        private MeshGenerationManager _meshGenerationManager; // Manager for mesh generation
        private ColourGenerationManager _colourGenerationManager; // Manager for colourGradient generation

        // Array to store minimum and maximum values.
        private float[] _minMaxValues; // Array to store minimum and maximum values
        
        private List<Vector4> _colourPalette;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes the GameManager component.
        /// This method is called when the GameManager component is first created.
        /// </summary>
        private void Awake()
        {
            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Initialize the mesh generation manager and the compute buffers.
            _meshGenerationManager = new MeshGenerationManager();
            _meshGenerationManager.InitializeBuffers(resolution);

            // Create arrays to store the vertices and triangles of the mesh.
            // The number of vertices is determined by the resolution of the mesh.
            _vertices = new Vector3[resolution.x * resolution.y];

            // The number of triangles is determined by the resolution of the mesh minus 1.
            // Each quad in the mesh is represented by 2 triangles, so there are 6 indices per quad.
            _triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Create a list of colour palette vectors based on the color keys in the colour gradient.
            // Each vector represents a color with components for red, green, blue, and alpha.
            _colourPalette = colourGradient.colorKeys
                .Select(colourKey => new Vector4(colourKey.color.r, colourKey.color.g, colourKey.color.b, colourKey.color.a))
                .ToList();
        }

        /// <summary>
        /// This method is called on the start of the game.
        /// It generates a mesh, colours it, and adjusts the height of the mesh vertices.
        /// </summary>
        private void Start()
        {
            // Generate the mesh using the provided resolution and noise settings.
            // Returns true if the mesh generation was successful, false otherwise.
            bool meshGenerated = GenerateMesh();

            // Colour the mesh based on the generated mesh and the specified colourGradient palette.
            // The meshGenerated parameter indicates whether the mesh has been generated or not.
            ColourMesh(meshGenerated);

            // Adjust the height of the mesh vertices based on the specified noise settings.
            AdjustMeshHeight();
        }

        /// <summary>
        /// This method is called when the script is destroyed. It releases the compute buffers used by the mesh generation manager.
        /// It also releases the compute buffers used by the ColourGenerationManager class.
        /// </summary>
        private void OnDestroy()
        {
            // Check if the mesh generation manager is not null before trying to release the buffers.
            // Call the ReleaseBuffers method of the mesh generation manager to release the compute buffers.
            // This is done to free up memory and prevent memory leaks.
            _meshGenerationManager?.ReleaseBuffers();

            // Check if the colour generation manager is not null before trying to release the buffers.
            // Call the ReleaseBuffers method of the colour generation manager to release the compute buffers.
            // This is done to free up memory and prevent memory leaks.
            // _colourGenerationManager?.ReleaseBuffers();
        }

        /// <summary>
        /// Generates a mesh using the provided resolution and noise settings.
        /// </summary>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateMesh()
        {
            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The meshGenerationComputeShader, resolution, noiseSettings, vertices, and triangles arrays are passed as arguments.
            _minMaxValues = _meshGenerationManager.GenerateMeshParameters(meshGenerationComputeShader, resolution,
                noiseSettings, _vertices,
                _triangles, islandRadius);
            
            // Creates a new Mesh object.
            // Sets the mesh of the MeshFilter to the newly created mesh.
            _meshFilter.mesh = new Mesh
            {
                // Sets the index format of the mesh to UInt32, which is required for large meshes.
                indexFormat = IndexFormat.UInt32,
                name = "Procedural Mesh GPU"
            };

            // Create the mesh using the generated vertices and triangles.
            // The _meshFilter, vertices, and triangles arrays are passed as arguments.
            _meshGenerationManager.CreateMesh(_meshFilter.mesh, _vertices, _triangles);

            return true;
        }

        /// <summary>
        /// Colours the mesh based on the generated mesh and the specified colourGradient palette.
        /// </summary>
        /// <param name="meshGenerated">Indicates whether the mesh has been generated or not.</param>
        public void ColourMesh(bool meshGenerated)
        {
            // Create a new instance of the ColourGenerationManager class.
            _colourGenerationManager = new ColourGenerationManager();

            int colourCount = _colourPalette.Count;
            
            // Initialize the buffers for the vertices and colors
            _colourGenerationManager.InitializeBuffers(resolution, colourCount);

            // If the mesh has not been generated, return early.
            if (!meshGenerated) return;

            // Use the ColourGenerationManager class to colourGradient the mesh using the specified compute shader, mesh filter, resolution, min/max values, and colourGradient palette.
            _colourGenerationManager.ColourMesh(colourGenerationComputeShader, _meshFilter, resolution, _minMaxValues,
                _colourPalette.ToArray(), colourCount);
        }

        /// <summary>
        /// Adjusts the height of the mesh vertices based on the specified noise settings.
        /// </summary>
        private void AdjustMeshHeight()
        {
            // Get the mesh from the MeshFilter.
            Mesh mesh = _meshFilter.sharedMesh;

            // Get the vertices of the mesh.
            Vector3[] meshVertices = mesh.vertices;

            float heightMultiplier = noiseSettings.maxTerrainHeight;

            // Adjust the height of each vertex by multiplying it by the maximum terrain height specified in the noise settings.
            for (int i = 0; i < meshVertices.Length; i++)
            {
                meshVertices[i].y *= heightMultiplier;
            }

            // Assign the new height values to the mesh vertices.
            mesh.vertices = meshVertices;

            // Recalculate the normals of the mesh to ensure correct geometry representation.
            mesh.RecalculateNormals();
        }

        #endregion
    }
}