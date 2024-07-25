/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: A script for managing mesh generation in Unity.
 * License: MIT Licence
 */

using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Manager
{
    /// <summary>
    /// Class responsible for managing noise generation for mesh generation in Unity.
    /// </summary>
    public class NoiseGenerationManager
    {
        private static readonly int ChunkSize = Shader.PropertyToID("chunk_size"); // ID for chunk size
        private static readonly int MapSize = Shader.PropertyToID("map_size"); // ID for map size
        private static readonly int SeedOffset = Shader.PropertyToID("seed_offset"); // ID for seed offset
        private static readonly int NoiseScale = Shader.PropertyToID("noise_scale"); // ID for noise scale
        private static readonly int Octaves = Shader.PropertyToID("octaves"); // ID for number of octaves
        private static readonly int Lacunarity = Shader.PropertyToID("lacunarity"); // ID for lacunarity
        private static readonly int Persistence = Shader.PropertyToID("persistence"); // ID for persistence

        private static readonly int
            MaxTerrainHeight = Shader.PropertyToID("max_terrain_height"); // ID for max terrain height

        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
        private static readonly int UVBuffer = Shader.PropertyToID("_UV_Buffer"); // ID for uv buffer
        private static readonly int NormalsBuffer = Shader.PropertyToID("_Normals_Buffer"); // ID for normals buffer
        private static readonly int TriangleBuffer = Shader.PropertyToID("_Triangle_Buffer"); // ID for triangle buffer
        private static readonly int NoiseLayerBuffer = Shader.PropertyToID("_Noise_Layer_Buffer"); // ID for noise layer buffer

        private Mesh _mesh; // Mesh object for storing mesh data
        private static readonly int Offset = Shader.PropertyToID("offset"); // ID for offset
        private static readonly int Amplitude = Shader.PropertyToID("amplitude"); // ID for amplitude
        private static readonly int Frequency = Shader.PropertyToID("frequency"); // ID for frequency
        private static readonly int WarpSteps = Shader.PropertyToID("warp_steps"); // ID for warp steps
        private static readonly int DomainWarpingMultiplicative = Shader.PropertyToID("domain_warping_mul"); // ID for domain warping
        private static readonly int DomainWarpingOffsetVectors = Shader.PropertyToID("_DW_Offsets"); // ID for domain warping
        private static readonly int TriangleCount = Shader.PropertyToID("triangle_count"); // ID for triangle count
        private static readonly int IsWater = Shader.PropertyToID("is_water"); // ID for is water

        /// <summary>
        /// Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="managers">The terrain generation managers.</param>
        /// <param name="shaders">The shaders used for rendering.</param>
        /// <param name="terrainSettings">The settings related to the terrain.</param>
        /// <param name="noiseSettings">The noise settings for generating the mesh.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="uv">Array to store the UV coordinates.</param>
        /// <param name="normals">Array to store the normals of the vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        /// <param name="isWater">Flag indicating if the mesh represents water.</param>
        /// <returns>An array containing the minimum and maximum height values of the generated mesh.</returns>
        public void GenerateNoiseParameters(TerrainGenerationManagers managers, Shaders shaders, TerrainSettings terrainSettings,
            NoiseSettings noiseSettings, Vector3[] vertices, Vector2[] uv, Vector3[] normals, int[] triangles,
            bool isWater)
        {
            // Generate the noise for the mesh.
            GenerateNoise(shaders, noiseSettings, terrainSettings.GeneralSettings, isWater);

            // Compare the height values of the generated vertices.
            managers.ValueClampManager.CompareHeightValues(terrainSettings.GeneralSettings.resolution, shaders,
                ComputeBufferManager.Instance.VerticesBuffer);

            // NOTE: For single chunks, if an island is desired
            // if (isGround)
            // {
            //     managers.FalloffMapManager.ApplyFalloffMap(generalSettings, shaders, ComputeBufferManager.Instance.VerticesBuffer);
            // }
            
            // Retrieve the generated vertices, UVs, normals, and triangles from the compute buffers.
            ComputeBufferManager.Instance.VerticesBuffer.GetData(vertices);
            ComputeBufferManager.Instance.UVBuffer.GetData(uv);
            ComputeBufferManager.Instance.NormalsBuffer.GetData(normals);
            ComputeBufferManager.Instance.TrianglesBuffer.GetData(triangles);
        }

        /// <summary>
        /// Generates noise for mesh generation based on specified settings.
        /// </summary>
        /// <param name="shaders">The shader settings.</param>
        /// <param name="noiseSettings">The noise generation settings.</param>
        /// <param name="generalSettings">The general settings.</param>
        /// <param name="isWater">Flag indicating if the noise is for water.</param>
        private static void GenerateNoise(Shaders shaders, NoiseSettings noiseSettings,
            GeneralSettings generalSettings, bool isWater)
        {
            // Get the resolution
            Vector2Int resolution = generalSettings.resolution;
            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;

            // Get the noise compute shader
            ComputeShader noiseComputeShader = shaders.noiseGenerationComputeShader;

            // Calculate seed offset
            float seedOffsetBase = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;
            Vector2 seedOffset = new Vector2(seedOffsetBase / verticesPerLineX, seedOffsetBase * verticesPerLineZ);

            // Find the kernel in the compute shader.
            var noiseKernel = noiseComputeShader.FindKernel("Noise_Generator");
            Vector2 mapSize = new Vector2(verticesPerLineX, verticesPerLineZ);
            
            // Shader property setup
            noiseComputeShader.SetVector(ChunkSize, generalSettings.chunkSize);
            noiseComputeShader.SetVector(MapSize, mapSize);
            noiseComputeShader.SetVector(SeedOffset, seedOffset);
            noiseComputeShader.SetFloat(NoiseScale, noiseSettings.noiseScale / 100f); // div by magic number in order to decrease noise_scale
            
            // Set up FBM parameters
            noiseComputeShader.SetInt(Octaves, noiseSettings.octaves);
            noiseComputeShader.SetFloat(Amplitude, noiseSettings.amplitude);
            noiseComputeShader.SetFloat(Frequency, noiseSettings.frequency);
            noiseComputeShader.SetFloat(Persistence, noiseSettings.persistence);
            noiseComputeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);

            // Set up Domain Warping parameters
            noiseComputeShader.SetInt(WarpSteps, noiseSettings.warpSteps);
            noiseComputeShader.SetFloat(DomainWarpingMultiplicative, noiseSettings.domainWarpingMultiplicative);

            // Calculate triangle count
            noiseComputeShader.SetInt(TriangleCount, ((verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6) / 3);

            // Set water flag
            noiseComputeShader.SetBool(IsWater, isWater);

            // Set noise layer integers
            int[] noiseLayerIntegers = new int[noiseSettings.noiseLayerSettings.Length];
            for (int i = 0; i < noiseSettings.noiseLayerSettings.Length; i++)
            {
                noiseLayerIntegers[i] = (int)noiseSettings.noiseLayerSettings[i].noiseLayer;
            }
            ComputeBufferManager.Instance.NoiseLayerBuffer.SetData(noiseLayerIntegers);

            // Set compute buffers
            noiseComputeShader.SetBuffer(noiseKernel, VertexBuffer, ComputeBufferManager.Instance.VerticesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, UVBuffer, ComputeBufferManager.Instance.UVBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, NormalsBuffer, ComputeBufferManager.Instance.NormalsBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, TriangleBuffer, ComputeBufferManager.Instance.TrianglesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, NoiseLayerBuffer, ComputeBufferManager.Instance.NoiseLayerBuffer);

            ComputeBufferManager.Instance.DomainWarpingOffsetBuffer.SetData(noiseSettings.offsetVectors);
            noiseComputeShader.SetBuffer(noiseKernel, DomainWarpingOffsetVectors,
                ComputeBufferManager.Instance.DomainWarpingOffsetBuffer);

            // Calculate the number of thread groups to dispatch
            var dispatchX = Mathf.CeilToInt(verticesPerLineX / 16f);
            var dispatchY = Mathf.CeilToInt(verticesPerLineZ / 16f);

            // Dispatch the compute shader to generate the mesh parameters
            noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        }
    }
}