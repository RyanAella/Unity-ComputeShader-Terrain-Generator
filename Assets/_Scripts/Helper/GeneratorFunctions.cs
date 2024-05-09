/*
 * Author: Rebecca Biebl
 * Creation Date: 05-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System;
using UnityEngine;

namespace _Scripts.Helper
{
    public static class GeneratorFunctions
    {
        #region Variables

        private static ComputeBuffer _localMinMaxBuffer;
        private static ComputeBuffer _globalMinMaxBuffer;

        private static ComputeBuffer _clampedVerticesBuffer;

        private static ComputeBuffer vertexBuffer;
        private static ComputeBuffer outputBuffer;

        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");

        private static readonly int FloatMinValue = Shader.PropertyToID("float_min_value");
        private static readonly int FloatMaxValue = Shader.PropertyToID("float_max_value");
        private static readonly int VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length");
        private static readonly int LocalMinMaxBufferLength = Shader.PropertyToID("local_min_max_buffer_length");

        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer");
        private static readonly int LocalMinMaxBuffer = Shader.PropertyToID("_Local_Min_Max_Buffer");
        private static readonly int GlobalMinMaxBuffer = Shader.PropertyToID("_Global_Min_Max_Buffer");
        private static readonly int ClampedVertexBuffer = Shader.PropertyToID("_Clamped_Buffer");

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

            // return ComputeMinMax(computeShader, vertices, resolution);

            for (int i = 0; i < globalMinMax.Length; i++)
            {
                Debug.Log(globalMinMax[i]);
            }

            return globalMinMax;
        }

        private static float[] ComputeMinMax(ComputeShader shader, Vector3[] vertices, Vector2Int resolution)
        {
            int kernel = shader.FindKernel("CSMain");

            vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
            outputBuffer = new ComputeBuffer(2, sizeof(float));
            
            vertexBuffer.SetData(vertices);
            
            shader.SetBuffer(kernel, "_Vertex_Buffer", vertexBuffer);
            shader.SetBuffer(kernel, "Output_Buffer", outputBuffer);
            
            shader.SetInt("vertices_buffer_length", vertices.Length);
            shader.SetInt("map_width", resolution.x);

            var dispatchX = Mathf.CeilToInt((float)resolution.x / 16);
            var dispatchY = Mathf.CeilToInt((float)resolution.y / 16);
            
            shader.Dispatch(kernel, dispatchX, dispatchY, 1);

            float[] minMax = new float[2];
            outputBuffer.GetData(minMax);

            for (int i = 0; i < minMax.Length; i++)
            {
                Debug.Log(minMax[i]);
            }

            return minMax;
        }

        private static float[] ComputeLocalMinMax(Vector2Int resolution, ComputeShader computeShader,
            ComputeBuffer verticesBuffer, Vector3[] vertices)
        {
            var computeLocal = computeShader.FindKernel("Compute_Local_Min_Max");

            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);
            computeShader.SetInt(LocalMinMaxBufferLength, 2);

            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(computeLocal, VertexBuffer, verticesBuffer);

            computeShader.SetBuffer(computeLocal, LocalMinMaxBuffer, _localMinMaxBuffer);

            var dispatchX = Mathf.CeilToInt((float)resolution.x / 16);
            var dispatchY = Mathf.CeilToInt((float)resolution.y / 16);

            computeShader.Dispatch(computeLocal, dispatchX, dispatchY, 1);

            var localMinMax = new float[2];
            _localMinMaxBuffer.GetData(localMinMax);

            return localMinMax;
        }

        private static float[] ComputeGlobalMinMax(Vector2Int resolution, ComputeShader computeShader,
            float[] localMinMax)
        {
            var computeGlobal = computeShader.FindKernel("Compute_Global_Min_Max");

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
            var clampKernel = computeShader.FindKernel("Clamp_Height_Values");

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