/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


using System;
using System.Globalization;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Manager
{
    /// <summary>
    ///     Class for managing mesh generation.
    /// </summary>
    [Serializable]
    public class MeshGenerationManager
    {
        #region Variables

        // Compute buffers for storing vertex and triangle data.
        private ComputeBuffer _verticesBuffer; // Compute buffer for vertices
        private ComputeBuffer _trianglesBuffer; // Compute buffer for triangles

        // Shader property IDs used for communication with compute shaders.
        private static readonly int MapWidth = Shader.PropertyToID("map_width"); // ID for map width
        private static readonly int MapHeight = Shader.PropertyToID("map_height"); // ID for map height
        private static readonly int SeedOffset = Shader.PropertyToID("seed_offset"); // ID for seed offset
        private static readonly int NoiseScale = Shader.PropertyToID("noise_scale"); // ID for noise scale
        private static readonly int NoiseHeight = Shader.PropertyToID("noise_height"); // ID for noise height
        private static readonly int Octaves = Shader.PropertyToID("octaves"); // ID for number of octaves
        private static readonly int Lacunarity = Shader.PropertyToID("lacunarity"); // ID for lacunarity
        private static readonly int Persistence = Shader.PropertyToID("persistence"); // ID for persistence

        private static readonly int
            MaxTerrainHeight = Shader.PropertyToID("max_terrain_height"); // ID for max terrain height

        private static readonly int IslandRadius = Shader.PropertyToID("island_radius"); // ID for island radius
        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
        private static readonly int TriangleBuffer = Shader.PropertyToID("_Triangle_Buffer"); // ID for triangle buffer

        // Private Mesh object used for storing generated mesh data.
        private Mesh _mesh; // Mesh object for storing mesh data

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the compute buffers based on the given resolution.
        /// </summary>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        public void InitializeBuffers(Vector2Int resolution)
        {
            // Allocate memory for the noise map buffer.
            _verticesBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
            // Allocate memory for the triangles buffer.
            _trianglesBuffer = new ComputeBuffer((resolution.x - 1) * (resolution.y - 1) * 6, sizeof(int));
        }

        /// <summary>
        /// Releases the compute buffers when they are no longer needed.
        /// </summary>
        /// <remarks>
        /// This method is used to release the memory allocated for the compute buffers.
        /// It should be called when the buffers are no longer in use to prevent memory leaks.
        /// </remarks>
        public void ReleaseBuffers()
        {
            // Release the vertices buffer
            _verticesBuffer.Release();

            // Release the triangles buffer
            _trianglesBuffer.Release();
        }

        /// <summary>
        /// Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for generating the mesh.</param>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        /// <param name="noiseSettings">The noise settings for generating the mesh.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        /// <param name="islandRadius">The radius of the island.</param>
        /// <returns>An array containing the minimum and maximum height values of the generated mesh.</returns>
        public float[] GenerateMeshParameters(ComputeShader computeShader, Vector2Int resolution,
            NoiseSettings noiseSettings, Vector3[] vertices, int[] triangles, float islandRadius)
        {
            // Ensure the noise scale is not too low to avoid a flat mesh
            noiseSettings.noiseScale = Mathf.Max(0.0001f, noiseSettings.noiseScale);

            // Check if a random seed is wanted
            if (noiseSettings.useRandomSeed)
                noiseSettings.SetSeed(Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture));

            // Get the coordinates
            float seedOffset = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;

            // Find the kernel in the compute shader.
            int noiseKernel = computeShader.FindKernel("Noise_Generator");

            // Set shader properties
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);

            computeShader.SetFloat(SeedOffset, seedOffset);
            computeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);
            computeShader.SetFloat(NoiseHeight, noiseSettings.noiseHeight);

            computeShader.SetInt(Octaves, noiseSettings.octaves);
            computeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);
            computeShader.SetFloat(Persistence, noiseSettings.persistence);

            computeShader.SetFloat(MaxTerrainHeight, noiseSettings.maxTerrainHeight);

            computeShader.SetFloat(IslandRadius, islandRadius);

            // Set the compute buffers for the vertices and triangles.
            computeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            computeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);

            // Calculate the number of thread groups to dispatch.
            int dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            int dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            computeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);

            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _trianglesBuffer.GetData(triangles);

            // Compare the height values of the generated vertices and return the minimum and maximum values.
            float[] minMax =
                GeneratorFunctions.CompareHeightValues(resolution, computeShader, _verticesBuffer, vertices);

            return minMax;
        }


        /// <summary>
        ///     Creates a new mesh based on the given vertices and triangles.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        public void CreateMesh(Mesh mesh, Vector3[] vertices, int[] triangles)
        {
            // Clears all previous data in the mesh.
            mesh.Clear();

            // Sets the vertices of the mesh.
            mesh.SetVertices(vertices);

            // Sets the triangles of the mesh.
            mesh.SetTriangles(triangles, 0);

            // Recalculates the normals of the mesh based on the vertices and triangles.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
        }

        #endregion
    }
}