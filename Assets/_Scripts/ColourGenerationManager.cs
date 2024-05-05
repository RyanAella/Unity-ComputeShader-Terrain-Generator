/*
 * Author: Rebecca Biebl
 * Creation Date: 01-05-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


namespace _Scripts
{
    using UnityEngine;

    public class ColourGenerationManager
    {
        private ComputeBuffer _vertexBuffer;
        private ComputeBuffer _colorBuffer;
        
        private static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
        private static readonly int ColourBuffer = Shader.PropertyToID("ColourBuffer");
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");
        private static readonly int MinHeight = Shader.PropertyToID("min_height");
        private static readonly int MaxHeight = Shader.PropertyToID("max_height");
        private static readonly int VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length");

        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution, float[] minMax)
        {
            // Get the Mesh from the MeshFilter
            Mesh mesh = meshFilter.mesh;

            // Get the vertices from the Mesh
            Vector3[] vertices = mesh.vertices;

            InitializeBuffers(vertices);

            // Set the data for the vertex buffer
            _vertexBuffer.SetData(vertices);

            // Find the kernel and set the buffers
            int kernel = computeShader.FindKernel("ColourMesh");
            computeShader.SetBuffer(kernel, VertexBuffer, _vertexBuffer);
            computeShader.SetBuffer(kernel, ColourBuffer, _colorBuffer);

            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
            
            // Set the height parameters for the gradient
            computeShader.SetFloat(MinHeight, minMax[0]);
            computeShader.SetFloat(MaxHeight, minMax[1]);
            
            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);

            // Dispatch the Compute Shader
            computeShader.Dispatch(kernel, vertices.Length / 8, 1, 1);

            // Get the colors from the Compute Shader
            Color[] colors = new Color[vertices.Length];
            _colorBuffer.GetData(colors);

            // Apply the colors to the Mesh
            mesh.colors = colors;
            
            meshFilter.mesh = meshFilter.sharedMesh = mesh;
        }

        private void InitializeBuffers(Vector3[] vertices)
        {
            // Create a ComputeBuffer for vertices and another for colors
            _vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
            _colorBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 4); // RGBA
        }

        public void ReleaseBuffers()
        {
            // Clean up the Compute Buffers
            _vertexBuffer.Release();
            _colorBuffer.Release();
        }
    }

}