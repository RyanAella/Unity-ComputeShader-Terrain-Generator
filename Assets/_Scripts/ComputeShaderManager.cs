/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts
{
    /// <summary>
    ///     Manages the initialization and release of compute buffers for mesh generation using a compute shader.
    /// </summary>
    [Serializable]
    public class ComputeShaderManager
    {
        #region Variables

        // Compute buffers for storing vertex and triangle data.
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _trianglesBuffer;

        private ComputeBuffer _localMinMaxBuffer;
        private ComputeBuffer _globalMinMaxBuffer;

        // Shader property IDs
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");
        private static readonly int NoiseScale = Shader.PropertyToID("noise_scale");
        private static readonly int NoiseHeight = Shader.PropertyToID("noise_height");
        private static readonly int Octaves = Shader.PropertyToID("octaves");
        private static readonly int Lacunarity = Shader.PropertyToID("lacunarity");
        private static readonly int Persistence = Shader.PropertyToID("persistence");
        private static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
        private static readonly int TriangleBuffer = Shader.PropertyToID("TriangleBuffer");
        private static readonly int FloatMinValue = Shader.PropertyToID("float_min_value");
        private static readonly int FloatMaxValue = Shader.PropertyToID("float_max_value");
        private static readonly int VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length");
        private static readonly int LocalMinMaxBufferLength = Shader.PropertyToID("local_min_max_buffer_length");

        private static readonly int LocalMinMaxBuffer = Shader.PropertyToID("LocalMinMaxBuffer");
        private static readonly int GlobalMinMaxBuffer = Shader.PropertyToID("GlobalMinMaxBuffer");

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

            _localMinMaxBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float));
            _globalMinMaxBuffer = new ComputeBuffer(2, sizeof(float));
        }

        /// <summary>
        ///     Releases the compute buffers when they are no longer needed.
        /// </summary>
        public void ReleaseBuffers()
        {
            _verticesBuffer.Release();
            _trianglesBuffer.Release();
            
            _localMinMaxBuffer.Release();
            _globalMinMaxBuffer.Release();
        }

        /// <summary>
        ///     Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for generating the mesh.</param>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        /// <param name="noiseSettings"></param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        public void GenerateMeshParameters(ComputeShader computeShader, Vector2Int resolution,
            NoiseSettings noiseSettings, Vector3[] vertices, int[] triangles)
        {
            if (noiseSettings.noiseScale <= 0) noiseSettings.noiseScale = 0.0001f;
            
            // Find the kernel in the compute shader.
            var noiseKernel = computeShader.FindKernel("NoiseGenerator");

            // Set shader properties for map dimensions.
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);

            computeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);
            computeShader.SetFloat(NoiseHeight, noiseSettings.noiseHeight);

            computeShader.SetInt(Octaves, noiseSettings.octaves);
            computeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);
            computeShader.SetFloat(Persistence, noiseSettings.persistence);

            // Set the compute buffers for the vertices and triangles.
            computeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            computeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution.x);
            var dispatchY = Mathf.CeilToInt(resolution.y);

            // Dispatch the compute shader to generate the mesh parameters.
            computeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);

            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _trianglesBuffer.GetData(triangles);

            Compare(resolution, computeShader, vertices);
        }

        void Compare(Vector2Int resolution, ComputeShader computeShader, Vector3[] vertices)
        {
            var computeLocal = computeShader.FindKernel("ComputeLocalMinMax");

            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);
            computeShader.SetInt(LocalMinMaxBufferLength, 2);

            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            _verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(computeLocal, VertexBuffer, _verticesBuffer);

            computeShader.SetBuffer(computeLocal, LocalMinMaxBuffer, _localMinMaxBuffer);

            int dispatchX = Mathf.Max(1, Mathf.CeilToInt((float)resolution.x / 8));
            int dispatchY = Mathf.Max(1, Mathf.CeilToInt((float)resolution.y / 8));

            computeShader.Dispatch(computeLocal, dispatchX, dispatchY, 1);

            float[] localMinMax = new float[2];
            _localMinMaxBuffer.GetData(localMinMax);

            

            var computeGlobal = computeShader.FindKernel("ComputeGlobalMinMax");

            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            computeShader.SetInt(LocalMinMaxBufferLength, resolution.x * resolution.y);

            _localMinMaxBuffer.SetData(localMinMax);
            computeShader.SetBuffer(computeGlobal, LocalMinMaxBuffer, _localMinMaxBuffer);
            computeShader.SetBuffer(computeGlobal, GlobalMinMaxBuffer, _globalMinMaxBuffer);

            computeShader.Dispatch(computeGlobal, 1, 1, 1);

            float[] globalMinMax = new float[2];
            _globalMinMaxBuffer.GetData(globalMinMax);

            foreach (var minMax in globalMinMax)
            {
                Debug.Log("MinMax: " + minMax);
            }
        }

        #endregion
    }
}