/*
 * Author: Rebecca Biebl
 * Creation Date: 05-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Helpers
{
    public static class GeneratorFunctions
    {
        #region Variables

        // Declare shader property identifiers (IDs) for mesh properties
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
            GlobalMinMaxBuffer = Shader.PropertyToID("_Global_Min_Max_Buffer"); // Global Min/Max buffer property ID

        #endregion

        #region Methods

        /// <summary>
        /// Releases the compute buffers used for local min-max computation and global min-max computation, as well as the clamped vertices buffer.
        /// </summary>
        public static void ReleaseBuffers()
        {
            // Release the local min-max compute buffer
            ComputeBufferManager.Instance.LocalMinMaxBuffer?.Release();

            // Release the global min-max compute buffer
            ComputeBufferManager.Instance.GlobalMinMaxBuffer?.Release();
        }

        /// <summary>
        /// Computes the global minimum and maximum height values from the given vertices using a compute shader.
        /// </summary>
        /// <param name="resolution">The chunkSize of the compute shader.</param>
        /// <param name="shaders"></param>
        /// <param name="verticesBuffer">The buffer containing the vertices.</param>
        /// <returns>The array of global minimum and maximum height values.</returns>
        public static void CompareHeightValues(Vector2Int resolution, Shaders shaders,
            ComputeBuffer verticesBuffer)
        {
            ComputeShader computeShader = shaders.valueClampComputeShader;
            
            // Compute local min-max values once
            ComputeLocalMinMax(resolution, computeShader, verticesBuffer);

            // Compute global min-max values
            ComputeGlobalMinMax(resolution, computeShader);

            // Clamp height values using the computed global min-max values
            ClampHeightValues(computeShader, verticesBuffer, resolution);
            
            // Compute local min-max values once
            ComputeLocalMinMax(resolution, computeShader, verticesBuffer);
            
            // Compute global min-max values
            ComputeGlobalMinMax(resolution, computeShader);
        }

        /// <summary>
        /// Computes the local minimum and maximum values from the given vertices using a compute shader.
        /// </summary>
        /// <param name="resolution">The chunkSize of the compute shader.</param>
        /// <param name="computeShader">The compute shader to use for the computation.</param>
        /// <param name="verticesBuffer">The buffer containing the vertices.</param>
        /// <returns>The array of local minimum and maximum values.</returns>
        private static void ComputeLocalMinMax(Vector2Int resolution, ComputeShader computeShader,
            ComputeBuffer verticesBuffer)
        {
            // Find the kernel for the compute shader
            var computeLocal = computeShader.FindKernel("Compute_Local_Min_Max");

            // Set the chunkSize of the vertices buffer
            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);

            // Set the length of the local min-max buffer
            computeShader.SetInt(LocalMinMaxBufferLength, 2);

            // Set the minimum and maximum float values
            computeShader.SetFloat(FloatMinValue, float.MinValue);
            computeShader.SetFloat(FloatMaxValue, float.MaxValue);

            // Set the vertices buffer data
            computeShader.SetBuffer(computeLocal, VertexBuffer, verticesBuffer);

            // Set the local min-max buffer
            computeShader.SetBuffer(computeLocal, LocalMinMaxBuffer, ComputeBufferManager.Instance.LocalMinMaxBuffer);

            // Calculate the dispatch dimensions
            var dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader
            computeShader.Dispatch(computeLocal, dispatchX, dispatchY, 1);
        }

        /// <summary>
        /// Computes the global minimum and maximum values from the given local minimum and maximum values using a compute shader.
        /// </summary>
        /// <param name="resolution">The chunkSize of the compute shader.</param>
        /// <param name="computeShader">The compute shader to use for the computation.</param>
        /// <returns>The array of global minimum and maximum values.</returns>
        private static void ComputeGlobalMinMax(Vector2Int resolution, ComputeShader computeShader)
        {
            // Find the compute shader kernel for computing global min-max
            var computeGlobal = computeShader.FindKernel("Compute_Global_Min_Max");

            // Set the buffer length based on chunkSize
            computeShader.SetInt(LocalMinMaxBufferLength, resolution.x * resolution.y);
            
            computeShader.SetBuffer(computeGlobal, LocalMinMaxBuffer, ComputeBufferManager.Instance.LocalMinMaxBuffer);
            computeShader.SetBuffer(computeGlobal, GlobalMinMaxBuffer, ComputeBufferManager.Instance.GlobalMinMaxBuffer);

            // Dispatch the compute shader
            computeShader.Dispatch(computeGlobal, 1, 1, 1);
        }

        /// <summary>
        /// Clamps the height values of a terrain using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for clamping the height values.</param>
        /// <param name="verticesBuffer">The buffer containing the vertices of the terrain.</param>
        /// <param name="resolution">The chunkSize of the terrain.</param>
        private static void ClampHeightValues(ComputeShader computeShader, ComputeBuffer verticesBuffer,
            Vector2Int resolution)
        {
            // Find the kernel in the compute shader for clamping height values.
            int clampKernel = computeShader.FindKernel("Clamp_Height_Values");

            // Set the map width and height in the compute shader.
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
            
            computeShader.SetBuffer(clampKernel, GlobalMinMaxBuffer, ComputeBufferManager.Instance.GlobalMinMaxBuffer);

            computeShader.SetBuffer(clampKernel, VertexBuffer, verticesBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader to clamp the height values.
            computeShader.Dispatch(clampKernel, dispatchX, dispatchY, 1);
        }
        
        // public static void MakeHeight(ComputeShader computeShader, ComputeBuffer verticesBuffer,
        //     Vector2Int chunkSize, Vector3[] vertices, float maxTerrainHeight)
        // {
        //     Debug.Log("MakeHeight");
        //     // Find the kernel in the compute shader for clamping height values.
        //     int clampKernel = computeShader.FindKernel("Make_Height");
        //
        //     // Set the map width and height in the compute shader.
        //     computeShader.SetInt(MapWidth, chunkSize.x);
        //     computeShader.SetInt(MapHeight, chunkSize.y);
        //     
        //     computeShader.SetFloat("max_terrain_height", maxTerrainHeight);
        //
        //     // Set the vertices buffer in the compute shader.
        //     verticesBuffer.SetData(vertices);
        //     computeShader.SetBuffer(clampKernel, VertexBuffer, verticesBuffer);
        //
        //     // Calculate the number of thread groups to dispatch.
        //     var dispatchX = Mathf.CeilToInt(chunkSize.x / 16f);
        //     var dispatchY = Mathf.CeilToInt(chunkSize.y / 16f);
        //
        //     // Dispatch the compute shader to clamp the height values.
        //     computeShader.Dispatch(clampKernel, dispatchX, dispatchY, 1);
        //
        //     // Get the clamped vertices from the compute shader.
        //     verticesBuffer.GetData(vertices);
        //
        //     foreach (var vector3 in vertices)
        //     {
        //         Debug.Log("After Height: " + vector3);
        //     }
        // }

        #endregion
    }
}