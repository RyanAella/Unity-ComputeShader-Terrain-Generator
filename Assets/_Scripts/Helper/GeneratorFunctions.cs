/*
 * Author: Rebecca Biebl
 * Creation Date: 05-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using UnityEngine;

namespace _Scripts.Helper
{
    public static class GeneratorFunctions
    {
        #region Variables

        private static ComputeBuffer _localMinMaxBuffer;
        private static ComputeBuffer _globalMinMaxBuffer;

        private static ComputeBuffer _clampedVerticesBuffer;

        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");

        private static readonly int FloatMinValue = Shader.PropertyToID("float_min_value");
        private static readonly int FloatMaxValue = Shader.PropertyToID("float_max_value");
        private static readonly int VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length");
        private static readonly int LocalMinMaxBufferLength = Shader.PropertyToID("local_min_max_buffer_length");

        private static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
        private static readonly int LocalMinMaxBuffer = Shader.PropertyToID("LocalMinMaxBuffer");
        private static readonly int GlobalMinMaxBuffer = Shader.PropertyToID("GlobalMinMaxBuffer");
        private static readonly int ClampedVertexBuffer = Shader.PropertyToID("ClampedBuffer");

        #endregion

        #region Methods

        private static void InitializeBuffers(Vector2Int resolution)
        {
            _localMinMaxBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float));
            _globalMinMaxBuffer = new ComputeBuffer(2, sizeof(float));
            _clampedVerticesBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
        }

        private static void ReleaseBuffers()
        {
            _localMinMaxBuffer.Release();
            _globalMinMaxBuffer.Release();
            _clampedVerticesBuffer.Release();
        }

        public static float[] CompareHeightValues(Vector2Int resolution, ComputeShader computeShader,
            ComputeBuffer verticesBuffer, Vector3[] vertices)
        {
            InitializeBuffers(resolution);

            var localMinMax = ComputeLocalMinMax(resolution, computeShader, verticesBuffer, vertices);

            var globalMinMax = ComputeGlobalMinMax(resolution, computeShader, localMinMax);

            ReleaseBuffers();

            return globalMinMax;
        }

        private static float[] ComputeLocalMinMax(Vector2Int resolution, ComputeShader computeShader,
            ComputeBuffer verticesBuffer, Vector3[] vertices)
        {
            var computeLocal = computeShader.FindKernel("ComputeLocalMinMax");

            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);
            computeShader.SetInt(LocalMinMaxBufferLength, 2);

            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(computeLocal, VertexBuffer, verticesBuffer);

            computeShader.SetBuffer(computeLocal, LocalMinMaxBuffer, _localMinMaxBuffer);

            var dispatchX = Mathf.Max(1, Mathf.CeilToInt((float)resolution.x / 8));
            var dispatchY = Mathf.Max(1, Mathf.CeilToInt((float)resolution.y / 8));

            computeShader.Dispatch(computeLocal, dispatchX, dispatchY, 1);

            var localMinMax = new float[2];
            _localMinMaxBuffer.GetData(localMinMax);

            return localMinMax;
        }

        private static float[] ComputeGlobalMinMax(Vector2Int resolution, ComputeShader computeShader,
            float[] localMinMax)
        {
            var computeGlobal = computeShader.FindKernel("ComputeGlobalMinMax");

            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            computeShader.SetInt(LocalMinMaxBufferLength, resolution.x * resolution.y);

            _localMinMaxBuffer.SetData(localMinMax);
            computeShader.SetBuffer(computeGlobal, LocalMinMaxBuffer, _localMinMaxBuffer);
            computeShader.SetBuffer(computeGlobal, GlobalMinMaxBuffer, _globalMinMaxBuffer);

            computeShader.Dispatch(computeGlobal, 1, 1, 1);

            var globalMinMax = new float[2];
            _globalMinMaxBuffer.GetData(globalMinMax);

            return globalMinMax;
        }

        public static void ClampHeightValues(ComputeShader computeShader, ComputeBuffer verticesBuffer,
            Vector2Int resolution, Vector3[] vertices,
            float[] minMax)
        {
            var clampKernel = computeShader.FindKernel("ClampHeightValues");

            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);

            _globalMinMaxBuffer.SetData(minMax);
            computeShader.SetBuffer(clampKernel, GlobalMinMaxBuffer, _globalMinMaxBuffer);

            verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(clampKernel, VertexBuffer, verticesBuffer);

            computeShader.SetBuffer(clampKernel, ClampedVertexBuffer, _clampedVerticesBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution.x);
            var dispatchY = Mathf.CeilToInt(resolution.y);

            // Dispatch the compute shader to generate the mesh parameters.
            computeShader.Dispatch(clampKernel, dispatchX, dispatchY, 1);

            // foreach (var vector3 in vertices)
            // {
            //     Debug.Log("Before Clamp: " + vector3);
            // }

            _clampedVerticesBuffer.GetData(vertices);

            // foreach (var vector3 in vertices)
            // {
            //     Debug.Log("After Clamp: " + vector3);
            // }
        }

        #endregion
    }
}