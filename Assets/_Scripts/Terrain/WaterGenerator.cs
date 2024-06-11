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
        private Vector2[] _uv;
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
        public void GenerateWater(TerrainGenerationManagers managers, TerrainSettings terrainSettings, Shaders shaders, List<Vector4> colourGradient, float[] colourHeights,
            MeshFilter meshFilter, float maxHeight, int colourCount)
        {
            bool success = GenerateNoiseAndMesh(managers, terrainSettings, shaders, meshFilter);

            ColourMesh(managers, terrainSettings, success, shaders, colourGradient, colourHeights, meshFilter, colourCount);

            // Put it at the right position
            var pos = transform.position;
            transform.position = new Vector3(pos.x, maxHeight * 0.25f/*terrainSettings.GeneralSettings.waterLevel*/, pos.z);
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
            Vector2Int resolution = terrainSettings.GeneralSettings.resolution;

            // Create arrays to store the vertices and triangles of the mesh.
            // The number of vertices is determined by the chunkSize of the mesh.
            _vertices = new Vector3[resolution.x * resolution.y];

            _uv = new Vector2[resolution.x * resolution.y];

            // The number of triangles is determined by the chunkSize of the mesh minus 1.
            // Each quad in the mesh is represented by 2 triangles, so there are 6 indices per quad.
            _triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The valueClampComputeShader, chunkSize, noiseSettings, vertices, and triangles arrays are passed as arguments.
            managers.NoiseGenerationManager.GenerateNoiseParameters(shaders, terrainSettings, terrainSettings.WaterNoiseSettings, _vertices,
                _uv, _triangles, managers, false);

            for (int i = 0; i < _vertices.Length; i++)
            {
                var vector3 = _vertices[i];
                vector3.y = Mathf.Clamp(vector3.y, 0.0f, terrainSettings.GeneralSettings.waterLevel);
            
                _vertices[i] = vector3;
            }

            string meshName = "Water";

            // Create the mesh using the generated vertices and triangles.
            // The meshFilter, vertices, and triangles arrays are passed as arguments.
            managers.MeshGenerationManager.CreateMesh(meshFilter, meshName, _vertices, _uv, _triangles);

            return true;
        }

        /// <summary>
        /// Colours the mesh based on the generated mesh and the specified colourGradient palette.
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="terrainSettings"></param>
        /// <param name="success">Indicates whether the mesh has been generated or not.</param>
        /// <param name="shaders"></param>
        /// <param name="colourGradient"></param>
        /// <param name="colourHeights"></param>
        /// <param name="meshFilter"></param>
        /// <param name="colourCount"></param>
        public void ColourMesh(TerrainGenerationManagers managers, TerrainSettings terrainSettings, bool success, Shaders shaders,
            List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter, int colourCount)
        {
            // If the mesh has not been generated, return early.
            if (!success) return;

            // Use the ColourGenerationManager class to colourGradient the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colourGradient palette.
            managers.WaterColourGenerationManager.ColourMesh(shaders.colourGenerationComputeShader, meshFilter,
                terrainSettings.GeneralSettings.resolution, new[] { 0, terrainSettings.GeneralSettings.waterLevel}, colourGradient.ToArray(), colourHeights, colourCount);
        }

        #endregion
    }
}