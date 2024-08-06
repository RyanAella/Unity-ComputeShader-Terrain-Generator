/*
 * Author: Rebecca Biebl
 * Creation Date: 26-06-2024
 * Description: A brief description of the script.
 * License: MIT License
 */

using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Generators
{
    /// <summary>  
    /// Base class for all terrain generators.
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        #region Variables

        private Vector3[] _vertices; // Vertices of the mesh
        private Vector2[] _uvs; // UV coordinates of the mesh
        private Vector3[] _normals; // Normals of the mesh
        private int[] _triangles; // Triangles of the mesh
            
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Generates the terrain mesh and applies colour to it.
        /// </summary>
        /// <param name="managers">The terrain generation managers for noise, mesh, and colour generation.</param>
        /// <param name="terrainSettings">The settings for terrain generation.</param>
        /// <param name="shaders">The shader settings for colour generation.</param>
        /// <param name="colourGradient">The colour gradient palette for mesh colouring.</param>
        /// <param name="colourHeights">The heights at which colours should transition.</param>
        /// <param name="meshFilter">The mesh filter to apply the generated mesh.</param>
        /// <param name="colourCount">The number of colours in the palette.</param>
        /// <param name="isWater">Flag indicating if it's water terrain.</param>
        /// <param name="maxHeight">The maximum height of the terrain.</param>
        public void GenerateTerrain(TerrainGenerationManagers managers, TerrainSettings terrainSettings, Shaders shaders,
            List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter, int colourCount, bool isWater, float maxHeight)
        {
            // Generate the noise and mesh.
            if (!GenerateNoiseAndMesh(managers, terrainSettings, shaders, meshFilter, isWater))
            {
                return; // Stop further processing if mesh generation fails.
            }

            if (isWater)
            {
                SetWaterPosition(terrainSettings, maxHeight);
            }
            else
            {
                ApplyColourToMesh(managers, terrainSettings, shaders, colourGradient, colourHeights, meshFilter, colourCount);

                // Adjust the mesh height.
                managers.MeshGenerationManager.AdjustMeshHeight(meshFilter, terrainSettings.GeneralSettings);
            }
        }

        /// <summary>
        /// Generates the noise and mesh.
        /// </summary>
        /// <param name="managers">The terrain generation managers.</param>
        /// <param name="terrainSettings">The terrain settings.</param>
        /// <param name="shaders">The shader settings.</param>
        /// <param name="meshFilter">The mesh filter.</param>
        /// <param name="isWater">Flag indicating if it's water.</param>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateNoiseAndMesh(TerrainGenerationManagers managers, TerrainSettings terrainSettings,
            Shaders shaders, MeshFilter meshFilter, bool isWater)
        {
            Vector2Int resolution = terrainSettings.GeneralSettings.resolution;

            // int verticesPerLineX = resolution.x * 2 + 1;
            // int verticesPerLineZ = resolution.y * 2 + 1;
            int verticesPerLineX = resolution.x + 1;
            int verticesPerLineZ = resolution.y + 1;

            if (_vertices == null || _vertices.Length != verticesPerLineX * verticesPerLineZ)
            {
                _vertices = new Vector3[verticesPerLineX * verticesPerLineZ];
                _uvs = new Vector2[verticesPerLineX * verticesPerLineZ];
                _normals = new Vector3[verticesPerLineX * verticesPerLineZ];
                _triangles = new int[(verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6];
            }

            managers.NoiseGenerationManager.GenerateNoiseParameters(managers, shaders, terrainSettings, terrainSettings.NoiseSettings,
                _vertices, _uvs, _normals, _triangles, isWater);

            string meshName = isWater ? "Water" : "Ground";

            managers.MeshGenerationManager.CreateMesh(meshFilter, meshName, _vertices, _uvs, _normals, _triangles);

            return true;
        }
        
        private void SetWaterPosition(TerrainSettings terrainSettings, float maxHeight)
        {
            // Set the water position.
            var pos = transform.position;
            transform.position = new Vector3(pos.x, maxHeight * terrainSettings.GeneralSettings.waterLevel, pos.z);
        }
        
        private static void ApplyColourToMesh(TerrainGenerationManagers managers, TerrainSettings terrainSettings,
            Shaders shaders, List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter, int colourCount)
        {
            // Use the ColourGenerationManager class to colour the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colour gradient palette.
            managers.ColourGenerationManager.ColourMesh(shaders.colourGenerationComputeShader, meshFilter,
                terrainSettings.GeneralSettings.resolution, new[] { 0.0f, 1f },
                colourGradient.ToArray(), colourHeights, colourCount, terrainSettings.GeneralSettings);
        }
            
        #endregion
            

    }
}