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
        private Vector2Int resolution = new(20, 20);

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")] [SerializeField]
        private ComputeShader noiseShader;

        [SerializeField] private ComputeShader colourShader;

        [SerializeField] private NoiseSettings noiseSettings;

        [SerializeField] private Gradient colour;

        // public bool autoUpdate;

        // Public arrays for mesh data.
        /*[HideInInspector]*/ public Vector3[] vertices;
        [HideInInspector] public int[] triangles;

        // Reference to the MeshFilter component attached to the GameObject.
        private MeshFilter _meshFilter;

        private MeshGenerationManager _meshGenerationManager;
        private ColourGenerationManager _colourGenerationManager;

        private float[] _minMax;

        #endregion

        #region Unity Methods

        private void Start()
        {
            bool meshGenerated = GenerateMesh();
        
            ColourMesh(meshGenerated);
        }

        public bool GenerateMesh()
        {
            // Initialize the compute shader manager and mesh generation manager.
            _meshGenerationManager = new MeshGenerationManager();

            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Initialize the compute buffers with the specified resolution.
            _meshGenerationManager.InitializeBuffers(resolution);

            // Generate the mesh using the compute shader and the specified resolution.
            vertices = new Vector3[resolution.x * resolution.y];
            triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            _minMax = new float[2];
            // Use the compute shader to generate mesh parameters (vertices and triangles).
            _minMax = _meshGenerationManager.GenerateMeshParameters(noiseShader, resolution, noiseSettings, vertices,
                triangles);

            // Create the mesh using the generated vertices and triangles.
            _meshGenerationManager.CreateMesh(_meshFilter, vertices, triangles);
            
            _meshGenerationManager.ReleaseBuffers();

            return true;
        }

        public void ColourMesh(bool meshGenerated)
        {
            _colourGenerationManager = new ColourGenerationManager();

            if (!meshGenerated) return;
            Vector4[] colourPalette = new Vector4[colour.colorKeys.Length];

            for (int i = 0; i < colour.colorKeys.Length; i++)
            {
                Color c = colour.colorKeys[i].color;
                colourPalette[i] = new Vector4(c.r, c.g, c.b, c.a);
            }

            _colourGenerationManager.ColourMesh(colourShader, _meshFilter, resolution, _minMax, colourPalette);
            
            _colourGenerationManager.ReleaseBuffers();
        }

        // private void OnDestroy()
        // {
        //     // Release the compute buffers when the GameObject is destroyed.
        //     _meshGenerationManager.ReleaseBuffers();
        //     _colourGenerationManager.ReleaseBuffers();
        // }

        #endregion
    }
}