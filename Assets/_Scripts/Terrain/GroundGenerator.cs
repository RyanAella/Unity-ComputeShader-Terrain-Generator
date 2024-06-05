/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class GroundGenerator : MonoBehaviour
    {
        #region Variables

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private int[] _triangles;

        private List<Vector4> _colourPalette;

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noiseGenerationManager"></param>
        /// <param name="meshGenerationManager"></param>
        /// <param name="colourGenerationManager"></param>
        /// <param name="falloffMapManagerGenerationManager"></param>
        /// <param name="generalSettings"></param>
        /// <param name="shaderSettings"></param>
        /// <param name="noiseSettings"></param>
        /// <param name="colourGradient"></param>
        /// <param name="meshFilter"></param>
        public void GenerateGround(NoiseGenerationManager noiseGenerationManager,
            MeshGenerationManager meshGenerationManager, ColourGenerationManager colourGenerationManager, FalloffMapManager falloffMapManagerGenerationManager, GeneralSettings generalSettings,
            ShaderSettings shaderSettings, NoiseSettings noiseSettings,
            List<Vector4> colourGradient, MeshFilter meshFilter)
        {
            bool success = GenerateNoiseAndMesh(noiseGenerationManager, meshGenerationManager, falloffMapManagerGenerationManager, generalSettings, shaderSettings, noiseSettings, meshFilter);

            // // Create a list of colour palette vectors based on the color keys in the colour gradient.
            // // Each vector represents a color with components for red, green, blue, and alpha.
            // _colourPalette = colourGradient.colorKeys
            //     .Select(colourKey =>
            //         new Vector4(colourKey.color.r, colourKey.color.g, colourKey.color.b, colourKey.color.a))
            //     .ToList();

            ColourMesh(success, colourGenerationManager, GeneralSettings.chunkSize,
                shaderSettings, colourGradient, meshFilter);

            AdjustMeshHeight(success, noiseSettings, meshFilter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noiseGenerationManager"></param>
        /// <param name="meshGenerationManager"></param>
        /// <param name="falloffMapManager"></param>
        /// <param name="generalSettings"></param>
        /// <param name="shaderSettings"></param>
        /// <param name="noiseSettings"></param>
        /// <param name="meshFilter"></param>
        /// <returns></returns>
        private bool GenerateNoiseAndMesh(NoiseGenerationManager noiseGenerationManager,
            MeshGenerationManager meshGenerationManager, FalloffMapManager falloffMapManager, GeneralSettings generalSettings, ShaderSettings shaderSettings,
            NoiseSettings noiseSettings, MeshFilter meshFilter)
        {
            // Get the chunkSize of the mesh.
            Vector2Int resolution = GeneralSettings.chunkSize;

            // Create arrays to store the vertices and triangles of the mesh.
            // The number of vertices is determined by the chunkSize of the mesh.
            _vertices = new Vector3[resolution.x * resolution.y];

            _uv = new Vector2[resolution.x * resolution.y];

            // The number of triangles is determined by the chunkSize of the mesh minus 1.
            // Each quad in the mesh is represented by 2 triangles, so there are 6 indices per quad.
            _triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The valueClampComputeShader, chunkSize, noiseSettings, vertices, and triangles arrays are passed as arguments.
            noiseGenerationManager.GenerateNoiseParameters(shaderSettings, noiseSettings, _vertices, _uv, _triangles, falloffMapManager, true);

            // Creates a new Mesh object.
            // Sets the mesh of the MeshFilter to the newly created mesh.
            meshFilter.mesh = new Mesh
            {
                // Sets the index format of the mesh to UInt32, which is required for large meshes.
                // indexFormat = IndexFormat.UInt32,
                name = "Procedural GroundGenerator Mesh GPU"
            };

            // Create the mesh using the generated vertices and triangles.
            // The _meshFilter, vertices, and triangles arrays are passed as arguments.
            meshGenerationManager.CreateMesh(meshFilter.sharedMesh, _vertices, _uv, _triangles);

            return true;
        }

        /// <summary>
        /// Colours the mesh based on the generated mesh and the specified colourGradient palette.
        /// </summary>
        /// <param name="success">Indicates whether the mesh has been generated or not.</param>
        /// <param name="colourGenerationManager"></param>
        /// <param name="resolution"></param>
        /// <param name="shaderSettings"></param>
        /// <param name="colourGradient"></param>
        /// <param name="meshFilter"></param>
        public void ColourMesh(bool success, ColourGenerationManager colourGenerationManager, Vector2Int resolution,
            ShaderSettings shaderSettings, List<Vector4> colourGradient, MeshFilter meshFilter)
        {
            // If the mesh has not been generated, return early.
            if (!success) return;

            int colourCount = colourGradient.Count;

            // ToDo: 
            // Initialize the buffers for the vertices and colors
            colourGenerationManager.InitializeBuffers(resolution, colourCount);

            // Use the ColourGenerationManager class to colourGradient the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colourGradient palette.
            colourGenerationManager.ColourMesh(shaderSettings.colourGenerationComputeShader, meshFilter, resolution,
                new[] { 0.3f, 1 },
                colourGradient.ToArray(), colourCount);

            // ToDo:
            colourGenerationManager.ReleaseBuffers();
        }

        /// <summary>
        /// Adjusts the height of the mesh based on the maximum terrain height specified in the noise settings.
        /// </summary>
        /// <param name="success">Indicates whether the mesh has been generated or not.</param>
        /// <param name="noiseSettings"></param>
        /// <param name="meshFilter"></param>
        private void AdjustMeshHeight(bool success, NoiseSettings noiseSettings, MeshFilter meshFilter)
        {
            // If the mesh has not been generated, return early.
            if (!success) return;


            // Get the mesh from the MeshFilter.
            Mesh mesh = meshFilter.sharedMesh;

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