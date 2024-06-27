/*
 * Author: Rebecca Biebl
 * Creation Date: 26-06-2024
 * Description: A brief description of the script.
 * License: MIT License
 */

using _Scripts.Helpers;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.TerrainGenerators
{
    /// <summary>  
    /// Base class for all terrain generators.
    /// </summary>
    public class BaseGenerator : MonoBehaviour
    {
        #region Variables

        private Vector3[] _vertices; // Vertices of the mesh
        private Vector2[] _uv; // UV coordinates of the mesh
        private Vector3[] _normals; // Normals of the mesh
        private int[] _triangles; // Triangles of the mesh
            
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Generates the noise and mesh.
        /// </summary>
        /// <param name="managers">The terrain generation managers.</param>
        /// <param name="terrainSettings">The terrain settings.</param>
        /// <param name="shaders">The shader settings.</param>
        /// <param name="meshFilter">The mesh filter.</param>
        /// <param name="isWater">Flag indicating if it's water.</param>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        protected bool GenerateNoiseAndMesh(TerrainGenerationManagers managers, TerrainSettings terrainSettings,
            Shaders shaders, MeshFilter meshFilter, bool isWater)
        {
            Vector2Int resolution = terrainSettings.GeneralSettings.resolution;

            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;

            if (_vertices == null || _vertices.Length != verticesPerLineX * verticesPerLineZ)
            {
                _vertices = new Vector3[verticesPerLineX * verticesPerLineZ];
                _uv = new Vector2[verticesPerLineX * verticesPerLineZ];
                _normals = new Vector3[verticesPerLineX * verticesPerLineZ];
                _triangles = new int[(verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6];
            }

            NoiseGenerationManager.GenerateNoiseParameters(shaders, terrainSettings, terrainSettings.NoiseSettings,
                _vertices, _uv, _normals, _triangles, isWater);

            string meshName = isWater ? "Water" : "Ground";

            managers.MeshGenerationManager.CreateMesh(managers, shaders, terrainSettings, meshFilter, meshName,
                _vertices, _uv, _normals, _triangles);

            return true;
        }
        
        /// <summary>
        /// Adjusts the height of the mesh vertices based on the specified height multiplier.
        /// </summary>
        /// <param name="meshFilter">The MeshFilter containing the mesh to adjust.</param>
        /// <param name="generalSettings">The general settings including the maximum terrain height.</param>
        protected static void AdjustMeshHeight(MeshFilter meshFilter, GeneralSettings generalSettings)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] meshVertices = mesh.vertices;
            float heightMultiplier = generalSettings.maxTerrainHeight;

            // Adjust each vertex's height based on the multiplier
            for (int i = 0; i < meshVertices.Length; i++)
            {
                meshVertices[i].y *= heightMultiplier;
            }

            // Update the mesh vertices and recalculate normals for proper lighting
            mesh.vertices = meshVertices;
            mesh.RecalculateNormals();
        }
            
        #endregion
            

    }
}