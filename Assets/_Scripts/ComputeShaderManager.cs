/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System;
using UnityEngine;

namespace _Scripts
{
    /// <summary>
    /// Manages the initialization and release of compute buffers for mesh generation using a compute shader.
    /// </summary>
    [Serializable]
    public class ComputeShaderManager
    {
        #region Variables

        // Compute buffers for storing vertex and triangle data.
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _trianglesBuffer;

        // Shader property IDs for map dimensions and buffers.
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");
        private static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
        private static readonly int TriangleBuffer = Shader.PropertyToID("TriangleBuffer");

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the compute buffers based on the given resolution.
        /// </summary>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        public void InitializeBuffers(Vector2Int resolution)
        {
            // Allocate memory for the noise map buffer.
            _verticesBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
            // Allocate memory for the triangles buffer.
            _trianglesBuffer = new ComputeBuffer((resolution.x - 1) * (resolution.y - 1) * 6, sizeof(int));
        }

        /// <summary>
        /// Releases the compute buffers when they are no longer needed.
        /// </summary>
        public void ReleaseBuffers()
        {
            _verticesBuffer.Release();
            _trianglesBuffer.Release();
        }

        /// <summary>
        /// Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for generating the mesh.</param>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        public void GenerateMeshParameters(ComputeShader computeShader, Vector2Int resolution, Vector3[] vertices, int[] triangles)
        {
            // Find the kernel in the compute shader.
            var noiseKernel = computeShader.FindKernel("NoiseGenerator");

            // Set shader properties for map dimensions.
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);

            // Set the compute buffers for the vertices and triangles.
            computeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            computeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);

            // Calculate the number of thread groups to dispatch.
            int dispatchX = Mathf.CeilToInt(resolution.x);
            int dispatchY = Mathf.CeilToInt(resolution.y);

            // Dispatch the compute shader to generate the mesh parameters.
            computeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);

            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _trianglesBuffer.GetData(triangles);
        }

        #endregion
    }
}