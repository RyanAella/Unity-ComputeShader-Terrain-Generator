/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: This script generates a ground mesh using noise and falloff maps, applies colour based on a gradient palette,
 *              and adjusts the mesh height. It integrates with several manager classes to handle the different aspects of mesh
 *              generation and colour application.
 * License: MIT License
 */

using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Terrain
{
    /// <summary>
    /// This class is responsible for generating the ground mesh and applying colour to it.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class GroundGenerator : MonoBehaviour
    {
        #region Methods

        /// <summary>
        /// Generates the ground mesh and applies colour to it.
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="terrainSettings"></param>
        /// <param name="shaders">The shader settings.</param>
        /// <param name="colourGradient">The colour gradient palette.</param>
        /// <param name="colourHeights"></param>
        /// <param name="meshFilter">The mesh filter.</param>
        /// <param name="groundColourCount">The number of colours in the palette.</param>
        public void GenerateGround(TerrainGenerationManagers managers, TerrainSettings terrainSettings,
            Shaders shaders, List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter,
            int groundColourCount)
        {
            // Generate the noise and mesh.
            if (!GenerateNoiseAndMesh(managers, shaders, terrainSettings, meshFilter))
            {
                return; // Stop further processing if mesh generation fails.
            }

            // Colour the mesh.
            ColourMesh(managers, terrainSettings.GeneralSettings.resolution, shaders, colourGradient, colourHeights,
                meshFilter, groundColourCount);

            // Adjust the mesh height.
            AdjustMeshHeight(meshFilter, terrainSettings.GeneralSettings);
        }

        /// <summary>
        /// Generates a mesh using the provided noise generation manager, mesh generation manager, falloff map manager, shader settings, noise settings, and mesh filter.
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="shaders">The shader settings.</param>
        /// <param name="terrainSettings"></param>
        /// <param name="meshFilter">The mesh filter.</param>
        /// <returns>True if the mesh generation was successful, false otherwise.</returns>
        private bool GenerateNoiseAndMesh(TerrainGenerationManagers managers, Shaders shaders,
            TerrainSettings terrainSettings, MeshFilter meshFilter)
        {
            // Get the chunkSize of the mesh.
            Vector2Int resolution = terrainSettings.GeneralSettings.resolution;

            // Create arrays to store the vertices and triangles of the mesh.
            // The number of vertices is determined by the chunkSize of the mesh.
            Vector3[] vertices = new Vector3[resolution.x * resolution.y];

            Vector2[] uv = new Vector2[resolution.x * resolution.y];

            // The number of triangles is determined by the chunkSize of the mesh minus 1.
            // Each quad in the mesh is represented by 2 triangles, so there are 6 indices per quad.
            int[] triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];

            // Use the compute shader to generate mesh parameters (vertices and triangles).
            // The valueClampComputeShader, chunkSize, noiseSettings, vertices, and triangles arrays are passed as arguments.
            managers.NoiseGenerationManager.GenerateNoiseParameters(shaders, terrainSettings,
                terrainSettings.GroundNoiseSettings, vertices,
                uv, triangles, managers, true);

            string meshName = "Ground";

            // Create the mesh using the generated vertices and triangles.
            // The _meshFilter, vertices, and triangles arrays are passed as arguments.
            managers.MeshGenerationManager.CreateMesh(meshFilter, meshName, vertices, uv, triangles);

            return true;
        }

        /// <summary>
        /// Colours the mesh based on the generated mesh and the specified colourGradient palette.
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="resolution">The resolution of the mesh.</param>
        /// <param name="shaders">The settings for the shader.</param>
        /// <param name="colourGradient">The list of colours for the gradient.</param>
        /// <param name="colourHeights"></param>
        /// <param name="meshFilter">The MeshFilter component to apply the colours to.</param>
        /// <param name="colourCount"></param>
        public void ColourMesh(TerrainGenerationManagers managers, Vector2Int resolution,
            Shaders shaders, List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter,
            int colourCount)
        {
            // Use the ColourGenerationManager class to colour the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colour gradient palette.
            managers.GroundColourGenerationManager.ColourMesh(shaders.colourGenerationComputeShader, meshFilter,
                resolution,
                new[] { 0.0f, 1 }, colourGradient.ToArray(), colourHeights, colourCount);
        }

        /// <summary>
        /// Adjusts the height of the mesh based on the maximum terrain height specified in the noise settings.
        /// </summary>
        /// <param name="meshFilter">The MeshFilter containing the mesh to adjust.</param>
        /// <param name="generalSettings">The general settings containing the maximum terrain height.</param>
        private void AdjustMeshHeight(MeshFilter meshFilter, GeneralSettings generalSettings)
        {
            // Get the mesh from the MeshFilter.
            Mesh mesh = meshFilter.sharedMesh;

            // Get the vertices of the mesh.
            Vector3[] meshVertices = mesh.vertices;

            // Calculate the height multiplier based on the maximum terrain height specified in the noise settings.
            float heightMultiplier = generalSettings.maxTerrainHeight;

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