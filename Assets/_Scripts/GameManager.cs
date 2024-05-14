/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts
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
        [Header("General Settings")] [SerializeField]
        private Vector2Int resolution = new(20, 20); // Resolution of the generated mesh

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")] [SerializeField]
        private ComputeShader noiseShader; // Compute shader for noise generation

        [SerializeField] private ComputeShader colourShader; // Compute shader for colour generation
        [SerializeField] private NoiseSettings noiseSettings; // Noise settings for mesh generation
        [SerializeField] private Gradient colour; // Gradient for colour mapping

        // Public arrays for mesh data.
        /*[HideInInspector]*/ public Vector3[] vertices; // Array to store vertices of the mesh
        [HideInInspector] public int[] triangles; // Array to store triangles of the mesh

        // Reference to the MeshFilter component attached to the GameObject.
        private MeshFilter _meshFilter; // Reference to the MeshFilter component

        // Reference to managers for mesh and colour generation.
        private MeshGenerationManager _meshGenerationManager; // Manager for mesh generation
        private ColourGenerationManager _colourGenerationManager; // Manager for colour generation

        // Array to store minimum and maximum values.
        private float[] _minMax; // Array to store minimum and maximum values

        #endregion

        #region Unity Methods

        /// <summary>
        /// Executes the Start method which generates a mesh and colours it.
        /// </summary>
        /// <returns>No return value.</returns>
        private void Start()
        {
            bool meshGenerated = GenerateMesh();

            ColourMesh(meshGenerated);
        }

        /// <summary>
        /// Generates a mesh using the provided resolution and noise settings.
        /// </summary>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateMesh()
        {
            // Initialize the compute shader manager and mesh generation manager.
            _meshGenerationManager = new MeshGenerationManager();

            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Initialize the compute buffers with the specified resolution.
            _meshGenerationManager.InitializeBuffers(resolution);

            // Create arrays to store the vertices and triangles of the mesh.
            vertices = new Vector3[resolution.x * resolution.y];
            triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Initialize an array to store the minimum and maximum values of the mesh.
            _minMax = new float[2];

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The noiseShader, resolution, noiseSettings, vertices, and triangles arrays are passed as arguments.
            _minMax = _meshGenerationManager.GenerateMeshParameters(noiseShader, resolution, noiseSettings, vertices,
                triangles, noiseSettings.maxTerrainHeight);

            // Create the mesh using the generated vertices and triangles.
            // The _meshFilter, vertices, and triangles arrays are passed as arguments.
            _meshGenerationManager.CreateMesh(_meshFilter, vertices, triangles);

            // Release the compute buffers used by the mesh generation manager.
            _meshGenerationManager.ReleaseBuffers();

            return true;
        }

        /// <summary>
        /// Colours the mesh based on the generated mesh and the specified colour palette.
        /// </summary>
        /// <param name="meshGenerated">Indicates whether the mesh has been generated or not.</param>
        public void ColourMesh(bool meshGenerated)
        {
            // Create a new instance of the ColourGenerationManager class.
            _colourGenerationManager = new ColourGenerationManager();

            // If the mesh has not been generated, return early.
            if (!meshGenerated) return;

            // Create a new array to store the colour palette.
            Vector4[] colourPalette = new Vector4[colour.colorKeys.Length];

            // Loop through each colour key in the colour gradient and convert it to a Vector4.
            for (int i = 0; i < colour.colorKeys.Length; i++)
            {
                Color c = colour.colorKeys[i].color;
                colourPalette[i] = new Vector4(c.r, c.g, c.b, c.a);
                
                // Debug.Log(c);
                // Debug.Log(colourPalette[i]);
            }
            

            // Use the ColourGenerationManager class to colour the mesh using the specified compute shader, mesh filter, resolution, min/max values, and colour palette.
            _colourGenerationManager.ColourMesh(colourShader, _meshFilter, resolution, _minMax, colourPalette);

            // Release the buffers used by the ColourGenerationManager class.
            // _colourGenerationManager.ReleaseBuffers();
        }

        #endregion
    }
}