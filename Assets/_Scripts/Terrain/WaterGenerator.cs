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

namespace _Scripts.Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class WaterGenerator : MonoBehaviour
    {
        #region Variables

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private Vector3[] _normals;
        private int[] _triangles;

        private List<Vector4> _colourPalette;

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="terrainSettings"></param>
        /// <param name="shaders"></param>
        /// <param name="colourGradient"></param>
        /// <param name="colourHeights"></param>
        /// <param name="meshFilter"></param>
        /// <param name="maxHeight"></param>
        /// <param name="colourCount"></param>
        public void GenerateWater(TerrainGenerationManagers managers, TerrainSettings terrainSettings, Shaders shaders,
            List<Vector4> colourGradient, float[] colourHeights,
            MeshFilter meshFilter, float maxHeight, int colourCount)
        {
            // Generate the noise and mesh.
            if (!GenerateNoiseAndMesh(managers, terrainSettings, shaders, meshFilter))
            {
                return; // Stop further processing if mesh generation fails.
            }
            
            // // Use the ColourGenerationManager class to colourGradient the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colourGradient palette.
            // managers.ColourGenerationManager.ColourMesh(shaders.colourGenerationComputeShader, meshFilter,
            //     terrainSettings.GeneralSettings.resolution,
            //     new[] { 0f, terrainSettings.GeneralSettings.waterLevel }, colourGradient.ToArray(),
            //     colourHeights, colourCount, true, material);

            // meshFilter.sharedMesh.colors = groundMeshFilter.sharedMesh.colors;

            // Put it at the right position
            var pos = transform.position;
            transform.position = new Vector3(pos.x, maxHeight * terrainSettings.GeneralSettings.waterLevel, pos.z);

            MoveWater(shaders, terrainSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="terrainSettings"></param>
        /// <param name="shaders"></param>
        /// <param name="meshFilter"></param>
        /// <returns></returns>
        private bool GenerateNoiseAndMesh(TerrainGenerationManagers managers, TerrainSettings terrainSettings,
            Shaders shaders, MeshFilter meshFilter)
        {
            // Get the chunkSize of the mesh.
            Vector2Int resolution = terrainSettings.GeneralSettings.resolution;

            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;

            // Initialize arrays if not already initialized or if the size has changed.
            if (_vertices == null || _vertices.Length != verticesPerLineX * verticesPerLineZ)
            {
                _vertices = new Vector3[verticesPerLineX * verticesPerLineZ];
                _uv = new Vector2[verticesPerLineX * verticesPerLineZ];
                _normals = new Vector3[verticesPerLineX * verticesPerLineZ];
                _triangles = new int[(verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6];
            }

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The valueClampComputeShader, chunkSize, noiseSettings, vertices, and triangles arrays are passed as arguments.
            NoiseGenerationManager.GenerateNoiseParameters(shaders, terrainSettings, terrainSettings.WaterNoiseSettings,
                _vertices, _uv, _normals, _triangles, true);

            const string meshName = "Water";

            // Create the mesh using the generated vertices and triangles.
            // The meshFilter, vertices, and triangles arrays are passed as arguments.
            managers.MeshGenerationManager.CreateMesh(managers, shaders, terrainSettings, meshFilter, meshName,
                _vertices, _uv, _normals, _triangles);

            return true;
        }

        private void MoveWater(Shaders shaders, TerrainSettings terrainSettings)
        {
            // Vector2Int resolution = terrainSettings.GeneralSettings.resolution;
            //
            // ComputeShader waterMovementShader = shaders.waterMovementComputeShader;
            //
            // var waterKernel = waterMovementShader.FindKernel("Water_Mesh");
            //
            // waterMovementShader.SetInt("map_width", resolution.x);
            // waterMovementShader.SetInt("map_height", resolution.y);
            //
            // waterMovementShader.SetFloat("time", Time.time);
            //
            // waterMovementShader.SetBuffer(waterKernel, "_OutputBuffer", _outputBuffer);
            //
            // waterMovementShader.Dispatch(waterKernel,  resolution.x/ 8, resolution.y / 8, 1);
        }

        #endregion
    }
}