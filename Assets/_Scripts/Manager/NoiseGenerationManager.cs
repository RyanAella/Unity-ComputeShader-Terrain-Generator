/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */

using System.Globalization;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Manager
{
    public class NoiseGenerationManager
    {
        private ComputeBuffer _verticesBuffer; // Compute buffer for vertices
        private ComputeBuffer _trianglesBuffer; // Compute buffer for triangles
        private ComputeBuffer _baseNoiseBuffer; // Compute buffer for vertices

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
        private static readonly int BaseNoiseBuffer = Shader.PropertyToID("_Base_Noise_Buffer");
        private static readonly int NoiseType1 = Shader.PropertyToID("noise_type");

        private Mesh _mesh; // Mesh object for storing mesh data


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
            _baseNoiseBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
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
        /// <param name="noiseComputeShader">The compute shader to use for generating the mesh.</param>
        /// <param name="valueClampComputeShader"></param>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        /// <param name="noiseSettings">The noise settings for generating the mesh.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        /// <param name="islandRadius">The radius of the island.</param>
        /// <param name="generateMultipleLayers"></param>
        /// <returns>An array containing the minimum and maximum height values of the generated mesh.</returns>
        public float[] GenerateNoiseParameters(ComputeShader noiseComputeShader, ComputeShader valueClampComputeShader,
            Vector2Int resolution, NoiseSettings noiseSettings, Vector3[] vertices, int[] triangles, float islandRadius,
            bool generateMultipleLayers)
        {
            float seedOffset = 0.0f;
            int noiseKernel = 0;
            int dispatchX = 0;
            int dispatchY = 0;

            // if (!generateMultipleLayers)
            // {
            // Ensure the noise scale is not too low to avoid a flat mesh
            noiseSettings.noiseScale = Mathf.Max(0.0001f, noiseSettings.noiseScale);

            // Check if a random seed is wanted
            if (noiseSettings.useRandomSeed)
                noiseSettings.SetSeed(Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture));

            // Get the coordinates
            seedOffset = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;

            // Find the kernel in the compute shader.
            noiseKernel = noiseComputeShader.FindKernel("Noise_Generator");

            // Set shader properties
            noiseComputeShader.SetInt(MapWidth, resolution.x);
            noiseComputeShader.SetInt(MapHeight, resolution.y);

            noiseComputeShader.SetFloat(SeedOffset, seedOffset);
            noiseComputeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);
            noiseComputeShader.SetFloat(NoiseHeight, noiseSettings.noiseHeight);

            noiseComputeShader.SetInt(Octaves, noiseSettings.octaves);
            noiseComputeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);
            noiseComputeShader.SetFloat(Persistence, noiseSettings.persistence);

            noiseComputeShader.SetFloat(MaxTerrainHeight, noiseSettings.maxTerrainHeight);

            noiseComputeShader.SetFloat(IslandRadius, islandRadius);
            noiseComputeShader.SetInt(NoiseType1, (int)noiseSettings.noiseType);

            // Set the compute buffers for the vertices and triangles.
            noiseComputeShader.SetBuffer(noiseKernel, BaseNoiseBuffer, _baseNoiseBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);

            // Calculate the number of thread groups to dispatch.
            dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);

            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _trianglesBuffer.GetData(triangles);
            // }
            // else
            // {
            //     // Ensure the noise scale is not too low to avoid a flat mesh
            //     noiseSettings.noiseScale = Mathf.Max(0.0001f, noiseSettings.noiseScale);
            //
            //     // Check if a random seed is wanted
            //     if (noiseSettings.useRandomSeed)
            //         noiseSettings.SetSeed(Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture));
            //
            //     // Get the coordinates
            //     seedOffset = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;
            //
            //     // Find the kernel in the compute shader.
            //     noiseKernel = noiseComputeShader.FindKernel("Noise_Generator");
            //
            //     // Set shader properties
            //     noiseComputeShader.SetInt(MapWidth, resolution.x);
            //     noiseComputeShader.SetInt(MapHeight, resolution.y);
            //
            //     noiseComputeShader.SetFloat(SeedOffset, seedOffset);
            //     noiseComputeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);
            //     noiseComputeShader.SetFloat(NoiseHeight, noiseSettings.noiseHeight);
            //
            //     noiseComputeShader.SetInt(Octaves, noiseSettings.octaves);
            //     noiseComputeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);
            //     noiseComputeShader.SetFloat(Persistence, noiseSettings.persistence);
            //
            //     noiseComputeShader.SetFloat(MaxTerrainHeight, noiseSettings.maxTerrainHeight);
            //
            //     noiseComputeShader.SetFloat(IslandRadius, islandRadius);
            //     noiseComputeShader.SetInt(NoiseType1, (int)NoiseType.FractionalBrownianMotion);
            //
            //     // Set the compute buffers for the vertices and triangles.
            //     _verticesBuffer.SetData(vertices);
            //     _trianglesBuffer.SetData(triangles);
            //     noiseComputeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            //     noiseComputeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);
            //
            //     // Calculate the number of thread groups to dispatch.
            //     dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            //     dispatchY = Mathf.CeilToInt(resolution.y / 16f);
            //
            //     // Dispatch the compute shader to generate the mesh parameters.
            //     noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
            //
            //     // Retrieve the generated vertices and triangles from the compute buffers.
            //     _verticesBuffer.GetData(vertices);
            //
            //     var vertSave = vertices;
            //
            //     // Use the generated vertices for the second pass
            //     _baseNoiseBuffer.SetData(vertices);
            //     noiseComputeShader.SetBuffer(noiseKernel, BaseNoiseBuffer, _baseNoiseBuffer);
            //     noiseComputeShader.SetInt(NoiseType1, (int)NoiseType.DomainWarping); // Second pass: Apply domain warping
            //
            //     // Dispatch the compute shader to apply domain warping (second pass).
            //     noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
            //
            //     Debug.Log("Retrieving vertices data...");
            //     
            //     // Retrieve the final vertices and triangles from the compute buffers.
            //     _verticesBuffer.GetData(vertices);
            //     _trianglesBuffer.GetData(triangles);
            //     
            //     for (int i = 0; i < vertices.Length; i++)
            //     {
            //         Debug.Log("Before Domain Warping: " + vertSave[i].x + ", " + vertSave[i].y + ", " + vertSave[i].z);
            //         Debug.Log("After Domain Warping: " + vertices[i].x + " " + vertices[i].y + " " + vertices[i].z);
            //     }
            // }

            // Compare the height values of the generated vertices and return the minimum and maximum values.
            float[] minMax =
                GeneratorFunctions.CompareHeightValues(resolution, valueClampComputeShader, _verticesBuffer, vertices);

            return minMax;
        }
    }
}