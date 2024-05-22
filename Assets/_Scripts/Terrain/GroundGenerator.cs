/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GroundGenerator : MonoBehaviour
    {
        #region Variables

        // Reference to the MeshFilter component attached to the GameObject.
        private MeshFilter _meshFilter; // Reference to the MeshFilter component

        private Vector3[] _vertices;
        private int[] _triangles;

        // Array to store minimum and maximum values.
        private float[] _minMaxValues; // Array to store minimum and maximum values

        private List<Vector4> _colourPalette;

        private NoiseGenerationManager _noiseGenerationManager;
        private ColourGenerationManager _colourGenerationManager;

        #endregion

        #region Methods

        public void GenerateGround(NoiseGenerationManager noiseGenerationManager,
            MeshGenerationManager meshGenerationManager, ColourGenerationManager colourGenerationManager, ComputeShader noiseGenerationComputeShader,
            ComputeShader valueClampComputeShader, ComputeShader colourGenerationComputeShader, Vector2Int resolution, NoiseSettings noiseSettings,
            List<Vector4> colourGradient, float islandRadius, NoiseType noiseType)
        {
            _noiseGenerationManager = noiseGenerationManager;
            _colourGenerationManager = colourGenerationManager;
            
            bool success = GenerateNoiseAndMesh(_noiseGenerationManager, meshGenerationManager, noiseGenerationComputeShader,
                valueClampComputeShader, resolution, noiseSettings, islandRadius, noiseType);
            
            // // Create a list of colour palette vectors based on the color keys in the colour gradient.
            // // Each vector represents a color with components for red, green, blue, and alpha.
            // _colourPalette = colourGradient.colorKeys
            //     .Select(colourKey =>
            //         new Vector4(colourKey.color.r, colourKey.color.g, colourKey.color.b, colourKey.color.a))
            //     .ToList();

            ColourMesh(success, _colourGenerationManager, resolution, colourGenerationComputeShader, colourGradient);

            AdjustMeshHeight(success, noiseSettings);
        }

        private bool GenerateNoiseAndMesh(NoiseGenerationManager noiseGenerationManager,
            MeshGenerationManager meshGenerationManager,
            ComputeShader noiseGenerationComputeShader, ComputeShader valueClampComputeShader, Vector2Int resolution,
            NoiseSettings noiseSettings, float islandRadius, NoiseType noiseType)
        {
            // Create arrays to store the vertices and triangles of the mesh.
            // The number of vertices is determined by the resolution of the mesh.
            _vertices = new Vector3[resolution.x * resolution.y];

            // The number of triangles is determined by the resolution of the mesh minus 1.
            // Each quad in the mesh is represented by 2 triangles, so there are 6 indices per quad.
            _triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Get the MeshFilter component attached to the GameObject.
            _meshFilter = GetComponent<MeshFilter>();

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The valueClampComputeShader, resolution, noiseSettings, vertices, and triangles arrays are passed as arguments.
            _minMaxValues = noiseGenerationManager.GenerateNoiseParameters(noiseGenerationComputeShader,
                valueClampComputeShader, resolution,
                noiseSettings, _vertices, _triangles, islandRadius, noiseType);

            // Creates a new Mesh object.
            // Sets the mesh of the MeshFilter to the newly created mesh.
            _meshFilter.mesh = new Mesh
            {
                // Sets the index format of the mesh to UInt32, which is required for large meshes.
                indexFormat = IndexFormat.UInt32,
                name = "Procedural GroundGenerator Mesh GPU"
            };

            // Create the mesh using the generated vertices and triangles.
            // The _meshFilter, vertices, and triangles arrays are passed as arguments.
            meshGenerationManager.CreateMesh(_meshFilter.mesh, _vertices, _triangles);

            return true;
        }

        /// <summary>
        /// Colours the mesh based on the generated mesh and the specified colourGradient palette.
        /// </summary>
        /// <param name="success">Indicates whether the mesh has been generated or not.</param>
        /// <param name="colourGenerationManager"></param>
        /// <param name="resolution"></param>
        /// <param name="colourGenerationComputeShader"></param>
        /// <param name="colourGradient"></param>
        public void ColourMesh(bool success, ColourGenerationManager colourGenerationManager, Vector2Int resolution,
            ComputeShader colourGenerationComputeShader, List<Vector4> colourGradient)
        {
            // If the mesh has not been generated, return early.
            if (!success) return;

            int colourCount = colourGradient.Count;

            // Initialize the buffers for the vertices and colors
            _colourGenerationManager.InitializeBuffers(resolution, colourCount);

            // Use the ColourGenerationManager class to colourGradient the mesh using the specified compute shader, mesh filter, resolution, min/max values, and colourGradient palette.
            _colourGenerationManager.ColourMesh(colourGenerationComputeShader, _meshFilter, resolution, new float[] {0.3f, 1},
                colourGradient.ToArray(), colourCount);
            
            _colourGenerationManager.ReleaseBuffers();
        }

        /// <summary>
        /// Adjusts the height of the mesh vertices based on the specified noise settings.
        /// </summary>
        private void AdjustMeshHeight(bool success, NoiseSettings noiseSettings)
        {
            // If the mesh has not been generated, return early.
            if (!success) return;


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
        
        /// <summary>
        /// This method is called when the script is destroyed. It releases the compute buffers used by the mesh generation manager.
        /// It also releases the compute buffers used by the ColourGenerationManager class.
        /// </summary>
        private void OnDestroy()
        {
            _noiseGenerationManager?.ReleaseBuffers();

            // Check if the colour generation manager is not null before trying to release the buffers.
            // Call the ReleaseBuffers method of the colour generation manager to release the compute buffers.
            // This is done to free up memory and prevent memory leaks.
            _colourGenerationManager?.ReleaseBuffers();
        }

        #endregion
    }
}