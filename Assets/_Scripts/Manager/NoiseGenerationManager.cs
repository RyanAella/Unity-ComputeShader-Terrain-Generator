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
        private static readonly int MinValue = Shader.PropertyToID("min_value");
        private static readonly int NormalsBuffer = Shader.PropertyToID("_Normals_Buffer");
        private static readonly int TriangleCount = Shader.PropertyToID("triangle_count");

        /// <summary>
        /// Releases the compute buffers when they are no longer needed.
        /// </summary>
        /// <remarks>
        /// This method is used to release the memory allocated for the compute buffers.
        /// It should be called when the buffers are no longer in use to prevent memory leaks.
        /// </remarks>
        public static void ReleaseBuffers()
        {
            // Release the vertices buffer
            ComputeBufferManager.Instance.VerticesBuffer?.Release();

            //Release the uv buffer
            ComputeBufferManager.Instance.UVBuffer?.Release();

            //Release the normals buffer
            ComputeBufferManager.Instance.NormalsBuffer?.Release();

            // Release the triangles buffer
            ComputeBufferManager.Instance.TrianglesBuffer?.Release();

            // Release the noise layer buffer
            ComputeBufferManager.Instance.NoiseLayerBuffer?.Release();

            // Release the noise layer offset vectors buffer
            ComputeBufferManager.Instance.NoiseLayerOffsetVectors?.Release();

            // Release the domain warping offset buffer
            ComputeBufferManager.Instance.DomainWarpingOffsetBuffer?.Release();
        }

        /// <summary>
        /// Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="shaders"></param>
        /// <param name="terrainSettings"></param>
        /// <param name="noiseSettings">The noise settings for generating the mesh.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="uv"></param>
        /// <param name="normals"></param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        /// <param name="isWater"></param>
        /// <returns>An array containing the minimum and maximum height values of the generated mesh.</returns>
        public static void GenerateNoiseParameters(Shaders shaders, TerrainSettings terrainSettings,
            NoiseSettings noiseSettings, Vector3[] vertices, Vector2[] uv, Vector3[] normals, int[] triangles, bool isWater)
        {
            GenerateNoise(shaders, noiseSettings, terrainSettings.GeneralSettings, isWater);

            // Compare the height values of the generated vertices and return the minimum and maximum values.
            GeneratorFunctions.CompareHeightValues(terrainSettings.GeneralSettings.resolution, shaders,
                ComputeBufferManager.Instance.VerticesBuffer);

            // NOTE: For single chunks, if an island is desired
            // if (isGround)
            // {
            //     managers.FalloffMapManager.ApplyFalloffMap(generalSettings, shaders, ComputeBufferManager.Instance.VerticesBuffer);
            // }

            // Retrieve the generated vertices and triangles from the compute buffers.
            ComputeBufferManager.Instance.VerticesBuffer.GetData(vertices);
            ComputeBufferManager.Instance.UVBuffer.GetData(uv);
            ComputeBufferManager.Instance.NormalsBuffer.GetData(normals);
            ComputeBufferManager.Instance.TrianglesBuffer.GetData(triangles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shaders"></param>
        /// <param name="noiseSettings"></param>
        /// <param name="generalSettings"></param>
        /// <param name="isWater"></param>
        private static void GenerateNoise(Shaders shaders, NoiseSettings noiseSettings,
            GeneralSettings generalSettings, bool isWater)
        {
            Vector2Int resolution = generalSettings.resolution;

            // Get the noise compute shader
            ComputeShader noiseComputeShader = shaders.noiseGenerationComputeShader;

            // Ensure the noise scale is not too low to avoid a flat mesh
            noiseSettings.noiseScale = Mathf.Max(0.0001f, noiseSettings.noiseScale);

            // Check if a random seed is wanted
            if (noiseSettings.useRandomSeed)
                noiseSettings.SetSeed(Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture));

            // Get the coordinates
            float seedOffsetBase = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;
            Vector2 seedOffset = new Vector2(seedOffsetBase / resolution.x, seedOffsetBase * resolution.y);

            // Find the kernel in the compute shader.
            var noiseKernel = noiseComputeShader.FindKernel("Noise_Generator");

            // Set shader properties
            noiseComputeShader.SetInt(MapWidth, resolution.x);
            noiseComputeShader.SetInt(MapHeight, resolution.y);

            noiseComputeShader.SetVector(SeedOffset, seedOffset);
            noiseComputeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);

            noiseComputeShader.SetFloat(MinValue, generalSettings.minValue);

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

            noiseComputeShader.SetInt(TriangleCount, ((resolution.x - 1) * (resolution.y - 1) * 6) / 3);
            
            noiseComputeShader.SetBool("is_water", isWater);

            int[] noiseLayerIntegers = new int[noiseSettings.noiseLayerSettings.Length];
            for (int i = 0; i < noiseSettings.noiseLayerSettings.Length; i++)
            {
                noiseLayerIntegers[i] = (int)noiseSettings.noiseLayerSettings[i].noiseLayer;
            }

            ComputeBufferManager.Instance.NoiseLayerBuffer.SetData(noiseLayerIntegers);

            // Set the compute buffers
            noiseComputeShader.SetBuffer(noiseKernel, VertexBuffer, ComputeBufferManager.Instance.VerticesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, UVBuffer, ComputeBufferManager.Instance.UVBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, NormalsBuffer, ComputeBufferManager.Instance.NormalsBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, TriangleBuffer, ComputeBufferManager.Instance.TrianglesBuffer);
            noiseComputeShader.SetBuffer(noiseKernel, NoiseLayerBuffer, ComputeBufferManager.Instance.NoiseLayerBuffer);

            ComputeBufferManager.Instance.NoiseLayerOffsetVectors.SetData(noiseSettings.noiseLayerOffsetVectors);
            noiseComputeShader.SetBuffer(noiseKernel, NoiseLayerOffsetVectors,
                ComputeBufferManager.Instance.NoiseLayerOffsetVectors);

            ComputeBufferManager.Instance.DomainWarpingOffsetBuffer.SetData(noiseSettings.offsetVectors);
            noiseComputeShader.SetBuffer(noiseKernel, DomainWarpingOffsetVectors,
                ComputeBufferManager.Instance.DomainWarpingOffsetBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(generalSettings.resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(generalSettings.resolution.y / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            noiseComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        }
    }
}