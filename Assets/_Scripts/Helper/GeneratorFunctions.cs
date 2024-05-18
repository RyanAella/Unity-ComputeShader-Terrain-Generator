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

        // Declare static ComputeBuffer variables for storing data
        private static ComputeBuffer _localMinMaxBuffer; // Buffer for local Min/Max values
        private static ComputeBuffer _globalMinMaxBuffer; // Buffer for global Min/Max values
        private static ComputeBuffer _clampedVerticesBuffer; // Buffer for clamped vertex data
        private static ComputeBuffer _vertexBuffer; // Buffer for storing vertices of the mesh
        private static ComputeBuffer _outputBuffer; // Buffer for storing output data

        // Declare shader property identifiers (IDs) for shader properties
        private static readonly int
            MapWidth = Shader.PropertyToID("map_width"), // Map width property ID
            MapHeight = Shader.PropertyToID("map_height"), // Map height property ID
            FloatMinValue = Shader.PropertyToID("float_min_value"), // Minimum float value property ID
            FloatMaxValue = Shader.PropertyToID("float_max_value"), // Maximum float value property ID
            VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length"), // Vertices buffer length property ID
            LocalMinMaxBufferLength =
                Shader.PropertyToID("local_min_max_buffer_length"); // Local Min/Max buffer length property ID

        // Declare shader property identifiers (IDs) for ComputeBuffer variables
        private static readonly int
            VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"), // Vertex buffer property ID
            LocalMinMaxBuffer = Shader.PropertyToID("_Local_Min_Max_Buffer"), // Local Min/Max buffer property ID
            GlobalMinMaxBuffer = Shader.PropertyToID("_Global_Min_Max_Buffer"), // Global Min/Max buffer property ID
            ClampedVertexBuffer = Shader.PropertyToID("_Clamped_Buffer"); // Clamped vertex buffer property ID

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the compute buffers used for local min-max computation and global min-max computation, as well as the clamped vertices buffer.
        /// </summary>
        /// <param name="resolution">The resolution of the buffers.</param>
        private static void InitializeBuffers(Vector2Int resolution)
        {
            // Create a new compute buffer for the local min-max values with a size determined by the resolution.
            _localMinMaxBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float));

            // Create a new compute buffer for the global min-max values with a size of 2.
            _globalMinMaxBuffer = new ComputeBuffer(2, sizeof(float));

            // Create a new compute buffer for the clamped vertices with a size determined by the resolution.
            _clampedVerticesBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
        }

        /// <summary>
        /// Releases the compute buffers used for local min-max computation and global min-max computation, as well as the clamped vertices buffer.
        /// </summary>
        private static void ReleaseBuffers()
        {
            _localMinMaxBuffer.Release();
            _globalMinMaxBuffer.Release();
            _clampedVerticesBuffer.Release();
        }

        /// <summary>
        /// Computes the global minimum and maximum height values from the given vertices using a compute shader.
        /// </summary>
        /// <param name="resolution">The resolution of the compute shader.</param>
        /// <param name="computeShader">The compute shader to use for the computation.</param>
        /// <param name="verticesBuffer">The buffer containing the vertices.</param>
        /// <param name="vertices">The array containing the vertices.</param>
        /// <returns>The array of global minimum and maximum height values.</returns>
        public static float[] CompareHeightValues(Vector2Int resolution, ComputeShader computeShader,
            ComputeBuffer verticesBuffer, Vector3[] vertices)
        {
            InitializeBuffers(resolution);

            var localMinMax = ComputeLocalMinMax(resolution, computeShader, verticesBuffer, vertices);

            var globalMinMax = ComputeGlobalMinMax(resolution, computeShader, localMinMax);

            ClampHeightValues(computeShader, verticesBuffer, resolution, vertices, globalMinMax);
            
            // MakeHeight(computeShader, verticesBuffer, resolution, vertices, maxTerrainHeight);
            
            // for (int i = 0; i < globalMinMax.Length; i++)
            // {
            //     Debug.Log("Before: " + globalMinMax[i]);
            // }
            
            localMinMax = ComputeLocalMinMax(resolution, computeShader, verticesBuffer, vertices);
            
            globalMinMax = ComputeGlobalMinMax(resolution, computeShader, localMinMax);

            ReleaseBuffers();

            // for (int i = 0; i < globalMinMax.Length; i++)
            // {
            //     Debug.Log("After: " + globalMinMax[i]);
            // }

            return globalMinMax;
        }

        /// <summary>
        /// Computes the local minimum and maximum values from the given vertices using a compute shader.
        /// </summary>
        /// <param name="resolution">The resolution of the compute shader.</param>
        /// <param name="computeShader">The compute shader to use for the computation.</param>
        /// <param name="verticesBuffer">The buffer containing the vertices.</param>
        /// <param name="vertices">The array containing the vertices.</param>
        /// <returns>The array of local minimum and maximum values.</returns>
        private static float[] ComputeLocalMinMax(Vector2Int resolution, ComputeShader computeShader,
            ComputeBuffer verticesBuffer, Vector3[] vertices)
        {
            // Find the kernel for the compute shader
            var computeLocal = computeShader.FindKernel("Compute_Local_Min_Max");

            // Set the resolution of the vertices buffer
            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);

            // Set the length of the local min-max buffer
            computeShader.SetInt(LocalMinMaxBufferLength, 2);

            // Set the minimum and maximum float values
            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            // Set the vertices buffer data
            verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(computeLocal, VertexBuffer, verticesBuffer);

            // Set the local min-max buffer
            computeShader.SetBuffer(computeLocal, LocalMinMaxBuffer, _localMinMaxBuffer);

            // Calculate the dispatch dimensions
            var dispatchX = Mathf.CeilToInt((float)resolution.x / 16);
            var dispatchY = Mathf.CeilToInt((float)resolution.y / 16);

            // Dispatch the compute shader
            computeShader.Dispatch(computeLocal, dispatchX, dispatchY, 1);

            // Get the data from the local min-max buffer
            var localMinMax = new float[2];
            _localMinMaxBuffer.GetData(localMinMax);

            return localMinMax;
        }

        /// <summary>
        /// Computes the global minimum and maximum values from the given local minimum and maximum values using a compute shader.
        /// </summary>
        /// <param name="resolution">The resolution of the compute shader.</param>
        /// <param name="computeShader">The compute shader to use for the computation.</param>
        /// <param name="localMinMax">The array of local minimum and maximum values.</param>
        /// <returns>The array of global minimum and maximum values.</returns>
        private static float[] ComputeGlobalMinMax(Vector2Int resolution, ComputeShader computeShader,
            float[] localMinMax)
        {
            // Find the compute shader kernel for computing global min-max
            var computeGlobal = computeShader.FindKernel("Compute_Global_Min_Max");

            // Set the minimum and maximum float values in the compute shader
            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            // Set the buffer length based on resolution
            computeShader.SetInt(LocalMinMaxBufferLength, resolution.x * resolution.y);

            // Set the data of local min-max buffer
            _localMinMaxBuffer.SetData(localMinMax);
            computeShader.SetBuffer(computeGlobal, LocalMinMaxBuffer, _localMinMaxBuffer);
            computeShader.SetBuffer(computeGlobal, GlobalMinMaxBuffer, _globalMinMaxBuffer);

            // Dispatch the compute shader
            computeShader.Dispatch(computeGlobal, 1, 1, 1);

            // Get the global min-max values
            var globalMinMax = new float[2];
            _globalMinMaxBuffer.GetData(globalMinMax);

            return globalMinMax;
        }

        /// <summary>
        /// Clamps the height values of a terrain using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for clamping the height values.</param>
        /// <param name="verticesBuffer">The buffer containing the vertices of the terrain.</param>
        /// <param name="resolution">The resolution of the terrain.</param>
        /// <param name="vertices">The array containing the vertices of the terrain.</param>
        /// <param name="minMax">The array containing the minimum and maximum height values.</param>
        public static void ClampHeightValues(ComputeShader computeShader, ComputeBuffer verticesBuffer,
            Vector2Int resolution, Vector3[] vertices,
            float[] minMax)
        {
            // Find the kernel in the compute shader for clamping height values.
            int clampKernel = computeShader.FindKernel("Clamp_Height_Values");

            // Set the map width and height in the compute shader.
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);

            // Set the global min and max height values in the compute shader.
            _globalMinMaxBuffer.SetData(minMax);
            computeShader.SetBuffer(clampKernel, GlobalMinMaxBuffer, _globalMinMaxBuffer);

            // Set the vertices buffer in the compute shader.
            verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(clampKernel, VertexBuffer, verticesBuffer);

            // Set the clamped vertices buffer in the compute shader.
            computeShader.SetBuffer(clampKernel, ClampedVertexBuffer, _clampedVerticesBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader to clamp the height values.
            computeShader.Dispatch(clampKernel, dispatchX, dispatchY, 1);

            // foreach (var vector3 in vertices)
            // {
            //     Debug.Log("Before Clamp: " + vector3);
            // }

            // Get the clamped vertices from the compute shader.
            _clampedVerticesBuffer.GetData(vertices);

            // foreach (var vector3 in vertices)
            // {
            //     Debug.Log("After Clamp: " + vector3);
            // }
        }
        
        public static void MakeHeight(ComputeShader computeShader, ComputeBuffer verticesBuffer,
            Vector2Int resolution, Vector3[] vertices, float maxTerrainHeight)
        {
            Debug.Log("MakeHeight");
            // Find the kernel in the compute shader for clamping height values.
            int clampKernel = computeShader.FindKernel("Make_Height");

            // Set the map width and height in the compute shader.
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
            
            computeShader.SetFloat("max_terrain_height", maxTerrainHeight);

            // Set the vertices buffer in the compute shader.
            verticesBuffer.SetData(vertices);
            computeShader.SetBuffer(clampKernel, VertexBuffer, verticesBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader to clamp the height values.
            computeShader.Dispatch(clampKernel, dispatchX, dispatchY, 1);

            // Get the clamped vertices from the compute shader.
            verticesBuffer.GetData(vertices);

            foreach (var vector3 in vertices)
            {
                Debug.Log("After Height: " + vector3);
            }
        }

        #endregion
    }
}