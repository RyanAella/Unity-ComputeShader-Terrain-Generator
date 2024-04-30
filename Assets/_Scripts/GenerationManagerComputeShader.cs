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
    /// This class is responsible for managing the generation of a mesh using a compute shader.
    /// It initializes a compute buffer, generates a noise map, and then uses that noise map to generate a mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GenerationManagerComputeShader : MonoBehaviour
    {
        #region Variables

        // General settings for the mesh generation.
        [Header("General Settings")] [SerializeField]
        private Vector2Int resolution = new(20, 20);

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")] [SerializeField]
        private ComputeShader computeShader;

        [SerializeField] private NoiseSettings noiseSettings;

        // Manager for handling compute shader operations and mesh generation.
        public ComputeShaderManager computeShaderManager;
        public MeshGenerationManager meshGenerationManager;

        /*[HideInInspector]*/
        public Vector3[] vertices;

        /*[HideInInspector]*/
        public int[] triangles;

        // Reference to the MeshFilter component attached to the GameObject.
        private MeshFilter _meshFilter;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            // Initialize the compute shader manager and mesh generation manager.
            computeShaderManager = new ComputeShaderManager();
            meshGenerationManager = new MeshGenerationManager();

            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Initialize the compute buffers with the specified resolution.
            computeShaderManager.InitializeBuffers(resolution);
        }

        private void Start()
        {
            // Generate the mesh using the compute shader and the specified resolution.
            vertices = new Vector3[resolution.x * resolution.y];
            triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            computeShaderManager.GenerateMeshParameters(computeShader, resolution, noiseSettings, vertices, triangles);

            // Create the mesh using the generated vertices and triangles.
            meshGenerationManager.CreateMesh(_meshFilter, vertices, triangles);
        }

        private void OnDestroy()
        {
            // Release the compute buffers when the GameObject is destroyed.
            computeShaderManager.ReleaseBuffers();
        }

        #endregion
    }
}