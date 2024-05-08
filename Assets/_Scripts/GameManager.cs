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

        // Manager for handling compute shader operations and mesh generation.
        // [SerializeField] private ComputeShaderManager computeShaderManager;

        [SerializeField] private MeshGenerationManager meshGenerationManager;

        [SerializeField] private Vector4 colorStart;
        [SerializeField] private Vector4 colorSecond;
        [SerializeField] private Vector4 colorThird;
        [SerializeField] private Vector4 colorEnd;

        [SerializeField] private Gradient colour;

        // Public arrays for mesh data.
        public Vector3[] vertices;
        public int[] triangles;

        // Reference to the MeshFilter component attached to the GameObject.
        private MeshFilter _meshFilter;

        private ColourGenerationManager _colourGenerationManager;

        private float[] _minMax;

        #endregion


        #region Unity Methods

        private void Awake()
        {
            // Initialize the compute shader manager and mesh generation manager.
            // computeShaderManager = new ComputeShaderManager();
            meshGenerationManager = new MeshGenerationManager();

            _colourGenerationManager = new ColourGenerationManager();

            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Initialize the compute buffers with the specified resolution.
            meshGenerationManager.InitializeBuffers(resolution);
        }

        private void Start()
        {
            bool meshGenerated = GenerateMesh();

            ColourMesh(meshGenerated);
        }

        public bool GenerateMesh()
        {
            // Initialize the compute shader manager and mesh generation manager.
            // computeShaderManager = new ComputeShaderManager();
            meshGenerationManager = new MeshGenerationManager();

            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Initialize the compute buffers with the specified resolution.
            meshGenerationManager.InitializeBuffers(resolution);

            // Generate the mesh using the compute shader and the specified resolution.
            vertices = new Vector3[resolution.x * resolution.y];
            triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            _minMax = new float[2];
            // Use the compute shader to generate mesh parameters (vertices and triangles).
            _minMax = meshGenerationManager.GenerateMeshParameters(noiseShader, resolution, noiseSettings, vertices,
                triangles);

            // Create the mesh using the generated vertices and triangles.
            meshGenerationManager.CreateMesh(_meshFilter, vertices, triangles);

            return true;
        }

        public void ColourMesh(bool meshGenerated)
        {
            _colourGenerationManager = new ColourGenerationManager();

            if (meshGenerated == true)
            {
                // // Definieren Sie die Farben als Vector4
                // Vector4 colorStart = new Vector4(0.0f, 0.0f, 1.0f, 1.0f); // Blau
                // Vector4 colorSecond = new Vector4(0.0f, 1.0f, 0.0f, 1.0f); // Grün
                // Vector4 colorThird = new Vector4(1.0f, 1.0f, 0.0f, 1.0f); // Gelb
                // Vector4 colorEnd = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);  // Rot

                // Farbpalette als Array erstellen
                // Vector4[] colourPalette = new Vector4[4];
                // colourPalette[0] = colorStart;
                // colourPalette[1] = colorSecond;
                // colourPalette[2] = colorThird;
                // colourPalette[3] = colorEnd;

                Vector4[] colourPalette = new Vector4[colour.colorKeys.Length];

                for (int i = 0; i < colour.colorKeys.Length; i++)
                {
                    Color c = colour.colorKeys[i].color;
                    colourPalette[i] = new Vector4(c.r, c.g, c.b, c.a);
                }

                // Debug.Log(_minMax[0] + " " + _minMax[1]);
                _colourGenerationManager.ColourMesh(colourShader, _meshFilter, resolution, _minMax, colourPalette);
            }
        }

        private void OnDestroy()
        {
            // Release the compute buffers when the GameObject is destroyed.
            meshGenerationManager.ReleaseBuffers();
            _colourGenerationManager.ReleaseBuffers();
        }

        #endregion
    }
}