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
        private ComputeBuffer _uvBuffer;
        private ComputeBuffer _trianglesBuffer; // Compute buffer for triangles
        private ComputeBuffer _domainWarpingOffsetBuffer;
        private ComputeBuffer _noiseLayerBuffer;
        private ComputeBuffer _noiseLayerStrengthBuffer;
        private ComputeBuffer _noiseLayerOffsetVectors;

        private static readonly int MapWidth = Shader.PropertyToID("map_width"); // ID for map width
        private static readonly int MapHeight = Shader.PropertyToID("map_height"); // ID for map height
        private static readonly int SeedOffset = Shader.PropertyToID("seed_offset"); // ID for seed offset
        private static readonly int NoiseScale = Shader.PropertyToID("noise_scale"); // ID for noise scale
        private static readonly int Octaves = Shader.PropertyToID("octaves"); // ID for number of octaves
        private static readonly int Lacunarity = Shader.PropertyToID("lacunarity"); // ID for lacunarity
        private static readonly int Persistence = Shader.PropertyToID("persistence"); // ID for persistence

        private static readonly int
            MaxTerrainHeight = Shader.PropertyToID("max_terrain_height"); // ID for max terrain height

        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
        private static readonly int UVBuffer = Shader.PropertyToID("_UV_Buffer");
        private static readonly int TriangleBuffer = Shader.PropertyToID("_Triangle_Buffer"); // ID for triangle buffer

        private Mesh _mesh; // Mesh object for storing mesh data
        private static readonly int Offset = Shader.PropertyToID("offset");
        private static readonly int Amplitude = Shader.PropertyToID("amplitude");
        private static readonly int Frequency = Shader.PropertyToID("frequency");
        private static readonly int WarpSteps = Shader.PropertyToID("warp_steps");
        private static readonly int DomainWarpingMultiplicative = Shader.PropertyToID("domain_warping_multiplicative");
        private static readonly int DomainWarpingOffsetVectors = Shader.PropertyToID("_Domain_Warping_Offset_Vectors");
        private static readonly int NoiseLayerBuffer = Shader.PropertyToID("_Noise_Layer_Buffer");
        private static readonly int NoiseLayerOffsetVectors = Shader.PropertyToID("_Noise_Layer_Offset_Vectors");
        private static readonly int NoiseLayerStrengthBuffer = Shader.PropertyToID("Noise_Layer_Strength_Buffer");

        /// <summary>
        ///     Initializes the compute buffers based on the given chunkSize.
        /// </summary>
        /// <param name="resolution">The chunkSize of the mesh to be generated.</param>
        /// <param name="noiseSettings"></param>
        public void InitializeBuffers(Vector2Int resolution, NoiseSettings noiseSettings)
        {
            // Allocate memory for the noise map buffer.
            _verticesBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
            _uvBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 2);
            // Allocate memory for the triangles buffer.
            _trianglesBuffer = new ComputeBuffer((resolution.x - 1) * (resolution.y - 1) * 6, sizeof(int));

            _noiseLayerBuffer = new ComputeBuffer(noiseSettings.octaves, sizeof(int));
            _noiseLayerStrengthBuffer = new ComputeBuffer(noiseSettings.octaves, sizeof(int));

            _noiseLayerOffsetVectors =
                new ComputeBuffer(noiseSettings.noiseLayerOffsetVectors.Length, sizeof(float) * 2);

            _domainWarpingOffsetBuffer =
                new ComputeBuffer((noiseSettings.offsetVectors.Length != 0) ? noiseSettings.offsetVectors.Length : 1,
                    sizeof(float) * 2);
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

            //Release the uv buffer
            _uvBuffer?.Release();

            // Release the triangles buffer
            _trianglesBuffer?.Release();

            // Release the noise layer buffer
            _noiseLayerBuffer?.Release();

            // Release the noise layer offset vectors buffer
            _noiseLayerOffsetVectors?.Release();

            // Release the domain warping offset buffer
            _domainWarpingOffsetBuffer?.Release();
        }

        /// <summary>
        /// Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="shaders"></param>
        /// <param name="terrainSettings"></param>
        /// <param name="noiseSettings">The noise settings for generating the mesh.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="uv"></param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        /// <param name="managers"></param>
        /// <param name="isGround"></param>
        /// <returns>An array containing the minimum and maximum height values of the generated mesh.</returns>
        public void GenerateNoiseParameters(Shaders shaders, TerrainSettings terrainSettings,
            NoiseSettings noiseSettings,  Vector3[] vertices, Vector2[] uv,
            int[] triangles, TerrainGenerationManagers managers, bool isGround)
        {
            GenerateNoise(shaders, noiseSettings, terrainSettings.GeneralSettings);

            // Compare the height values of the generated vertices and return the minimum and maximum values.
            GeneratorFunctions.CompareHeightValues(terrainSettings.GeneralSettings.resolution, shaders, _verticesBuffer);

            // NOTE: For single chunks, if an island is desired
            // if (isGround)
            // {
            //     managers.FalloffMapManager.ApplyFalloffMap(generalSettings, shaders, _verticesBuffer);
            // }

            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _uvBuffer.GetData(uv);
            _trianglesBuffer.GetData(triangles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shaders"></param>
        /// <param name="noiseSettings"></param>
        /// <param name="generalSettings"></param>
        private void GenerateNoise(Shaders shaders, NoiseSettings noiseSettings,
            GeneralSettings generalSettings)
        {
            // Get the noise compute shader
            ComputeShader noiseComputeShader = shaders.noiseGenerationComputeShader;

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
            noiseComputeShader.SetInt(MapWidth, generalSettings.resolution.x);
            noiseComputeShader.SetInt(MapHeight, generalSettings.resolution.y);

            noiseComputeShader.SetFloat(SeedOffset, seedOffset);
            noiseComputeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);

            noiseComputeShader.SetFloat("min_value", generalSettings.minValue);

            // FBM
            noiseComputeShader.SetInt(Octaves, noiseSettings.octaves);
            noiseComputeShader.SetFloat(Amplitude, noiseSettings.amplitude);
            noiseComputeShader.SetFloat(Frequency, noiseSettings.frequency);
            noiseComputeShader.SetFloat(Persistence, noiseSettings.persistence);
            noiseComputeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);

            // Domain Warping
            noiseComputeShader.SetInt(WarpSteps, noiseSettings.warpSteps);
            noiseComputeShader.SetFloat(DomainWarpingMultiplicative, noiseSettings.domainWarpingMultiplicative);

            noiseComputeShader.SetVector(Offset, noiseSettings.offset);

            noiseComputeShader.SetFloat(MaxTerrainHeight, generalSettings.maxTerrainHeight);

            int[] noiseLayerIntegers = new int[noiseSettings.noiseLayerSettings.Length];
            for (int i = 0; i < noiseSettings.noiseLayerSettings.Length; i++)
            {
                noiseLayerIntegers[i] = (int)noiseSettings.noiseLayerSettings[i].noiseLayer;
            }

            int[] noiseLayerStrengths = new int[noiseSettings.noiseLayerSettings.Length];
            for (int i = 0; i < noiseSettings.noiseLayerSettings.Length; i++)
            {
                noiseLayerStrengths[i] = (int)noiseSettings.noiseLayerSettings[i].strength;
            }

            _noiseLayerBuffer.SetData(noiseLayerIntegers);
            _noiseLayerStrengthBuffer.SetData(noiseLayerStrengths);

            // Set the compute buffers
            noiseComputeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, UVBuffer, _uvBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, NoiseLayerBuffer, _noiseLayerBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, NoiseLayerStrengthBuffer, _noiseLayerStrengthBuffer);

            _noiseLayerOffsetVectors.SetData(noiseSettings.noiseLayerOffsetVectors);
            noiseComputeShader.SetBuffer(noiseKernel, NoiseLayerOffsetVectors, _noiseLayerOffsetVectors);

            _domainWarpingOffsetBuffer.SetData(noiseSettings.offsetVectors);
            noiseComputeShader.SetBuffer(noiseKernel, DomainWarpingOffsetVectors, _domainWarpingOffsetBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(generalSettings.resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(generalSettings.resolution.y / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        }
    }
}