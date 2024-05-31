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

        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
        private static readonly int TriangleBuffer = Shader.PropertyToID("_Triangle_Buffer"); // ID for triangle buffer
        private static readonly int NoiseType = Shader.PropertyToID("noise_type");

        private Mesh _mesh; // Mesh object for storing mesh data
        private static readonly int Offset = Shader.PropertyToID("offset");


        /// <summary>
        ///     Initializes the compute buffers based on the given chunkSize.
        /// </summary>
        /// <param name="resolution">The chunkSize of the mesh to be generated.</param>
        public void InitializeBuffers(int resolution)
        {
            // Allocate memory for the noise map buffer.
            _verticesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 3);
            // Allocate memory for the triangles buffer.
            _trianglesBuffer = new ComputeBuffer((resolution - 1) * (resolution - 1) * 6, sizeof(int));
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
            _verticesBuffer?.Release();

            // Release the triangles buffer
            _trianglesBuffer?.Release();
        }

        /// <summary>
        /// Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="shaderSettings"></param>
        /// <param name="generalSettings"></param>
        /// <param name="noiseSettings">The noise settings for generating the mesh.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        /// <param name="falloffMapManager"></param>
        /// <param name="isGround"></param>
        /// <returns>An array containing the minimum and maximum height values of the generated mesh.</returns>
        public void GenerateNoiseParameters(ShaderSettings shaderSettings, GeneralSettings generalSettings,
            NoiseSettings noiseSettings, Vector3[] vertices, int[] triangles, FalloffMapManager falloffMapManager,
            bool isGround)
        {
            int resolution = generalSettings.chunkSize;

            GenerateNoise(shaderSettings, noiseSettings, resolution);

            // Compare the height values of the generated vertices and return the minimum and maximum values.
            GeneratorFunctions.CompareHeightValues(resolution, shaderSettings, _verticesBuffer);

            // if (isGround)
            // {
            //     falloffMapManager.ApplyFalloffMap(generalSettings, shaderSettings, _verticesBuffer);
            // }

            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _trianglesBuffer.GetData(triangles);
        }

        private void GenerateNoise(ShaderSettings shaderSettings, NoiseSettings noiseSettings, int resolution)
        {
            // Get the noise compute shader
            ComputeShader noiseComputeShader = shaderSettings.noiseGenerationComputeShader;

            // Ensure the noise scale is not too low to avoid a flat mesh
            noiseSettings.noiseScale = Mathf.Max(0.0001f, noiseSettings.noiseScale);

            // Check if a random seed is wanted
            if (noiseSettings.useRandomSeed)
                noiseSettings.SetSeed(Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture));

            // Get the coordinates
            var seedOffset = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;

            // Find the kernel in the compute shader.
            var noiseKernel = noiseComputeShader.FindKernel("Noise_Generator");

            // Set shader properties
            noiseComputeShader.SetInt(MapWidth, resolution);
            noiseComputeShader.SetInt(MapHeight, resolution);

            // noiseComputeShader.SetFloat(SeedOffset, seedOffset);
            noiseComputeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);
            noiseComputeShader.SetFloat(NoiseHeight, noiseSettings.noiseHeight);

            noiseComputeShader.SetInt(Octaves, noiseSettings.octaves);
            noiseComputeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);
            noiseComputeShader.SetFloat(Persistence, noiseSettings.persistence);
            
            noiseComputeShader.SetVector(Offset, noiseSettings.offset);

            noiseComputeShader.SetFloat(MaxTerrainHeight, noiseSettings.maxTerrainHeight);

            noiseComputeShader.SetInt(NoiseType, (int)noiseSettings.noiseType);

            // Set the compute buffers for the vertices and triangles.
            noiseComputeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution / 16f);
            var dispatchY = Mathf.CeilToInt(resolution / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        }
    }
}