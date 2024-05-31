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
    public class WaterGenerator : MonoBehaviour
    {
        #region Variables

        private Vector3[] _vertices;
        private int[] _triangles;

        private List<Vector4> _colourPalette;

        private NoiseGenerationManager _noiseGenerationManager;
        private ColourGenerationManager _colourGenerationManager;

        #endregion

        #region Methods

        public void GenerateWater(NoiseGenerationManager noiseGenerationManager,
            MeshGenerationManager meshGenerationManager, ColourGenerationManager colourGenerationManager, FalloffMapManager falloffMapManager,
            ShaderSettings shaderSettings, GeneralSettings generalSettings, NoiseSettings noiseSettings,
            List<Vector4> colourGradient, MeshFilter meshFilter, float maxHeight)
        {
            _noiseGenerationManager = noiseGenerationManager;
            _colourGenerationManager = colourGenerationManager;

            int resolution = generalSettings.chunkSize;

            bool success = GenerateNoiseAndMesh(_noiseGenerationManager, meshGenerationManager, falloffMapManager, shaderSettings, generalSettings, noiseSettings, meshFilter);

            ColourMesh(success, _colourGenerationManager, resolution, shaderSettings, colourGradient, meshFilter);

            // Put it at the right position
            var pos = transform.position;
            transform.position = new Vector3(pos.x, maxHeight * noiseSettings.waterLevel, pos.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noiseGenerationManager"></param>
        /// <param name="meshGenerationManager"></param>
        /// <param name="falloffMapManager"></param>
        /// <param name="shaderSettings"></param>
        /// <param name="generalSettings"></param>
        /// <param name="noiseSettings"></param>
        /// <param name="meshFilter"></param>
        /// <returns></returns>
        private bool GenerateNoiseAndMesh(NoiseGenerationManager noiseGenerationManager,
            MeshGenerationManager meshGenerationManager, FalloffMapManager falloffMapManager, ShaderSettings shaderSettings, GeneralSettings generalSettings, NoiseSettings noiseSettings, MeshFilter meshFilter)
        {
            int resolution = generalSettings.chunkSize;
            
            // Create arrays to store the vertices and triangles of the mesh.
            // The number of vertices is determined by the chunkSize of the mesh.
            _vertices = new Vector3[resolution * resolution];

            // The number of triangles is determined by the chunkSize of the mesh minus 1.
            // Each quad in the mesh is represented by 2 triangles, so there are 6 indices per quad.
            _triangles = new int[(resolution - 1) * (resolution - 1) * 6];

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The valueClampComputeShader, chunkSize, noiseSettings, vertices, and triangles arrays are passed as arguments.
            noiseGenerationManager.GenerateNoiseParameters(shaderSettings, generalSettings, noiseSettings, _vertices, _triangles, falloffMapManager, false);

            // Creates a new Mesh object.
            // Sets the mesh of the MeshFilter to the newly created mesh.
            meshFilter.mesh = new Mesh
            {
                // Sets the index format of the mesh to UInt32, which is required for large meshes.
                indexFormat = IndexFormat.UInt32,
                name = "Procedural GroundGenerator Mesh GPU"
            };

            // Create the mesh using the generated vertices and triangles.
            // The meshFilter, vertices, and triangles arrays are passed as arguments.
            meshGenerationManager.CreateMesh(meshFilter.sharedMesh, _vertices, _triangles);

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
        public void ColourMesh(bool success, ColourGenerationManager colourGenerationManager, int resolution,
            ShaderSettings shaderSettings, List<Vector4> colourGradient, MeshFilter meshFilter)
        {
            // If the mesh has not been generated, return early.
            if (!success) return;

            int colourCount = colourGradient.Count;

            // Initialize the buffers for the vertices and colors
            _colourGenerationManager.InitializeBuffers(resolution, colourCount);

            // Use the ColourGenerationManager class to colourGradient the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colourGradient palette.
            _colourGenerationManager.ColourMesh(shaderSettings.colourGenerationComputeShader, meshFilter, resolution,
                new[] { 0, 0.3f },
                colourGradient.ToArray(), colourCount);

            _colourGenerationManager.ReleaseBuffers();
        }

        #endregion
    }
}