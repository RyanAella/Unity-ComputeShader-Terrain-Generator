/*
 * Author: Rebecca Biebl
 * Creation Date: 01-05-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


using System.Collections.Generic;

namespace _Scripts
{
    using UnityEngine;

    public class ColourGenerationManager
    {
        private ComputeBuffer _vertexBuffer;
        private ComputeBuffer _colorBuffer;
        
        private ComputeBuffer _colourPaletteBuffer;
        
        private static readonly int VertexBuffer = Shader.PropertyToID("Vertex_Buffer");
        private static readonly int ColourBuffer = Shader.PropertyToID("Colour_Buffer");
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");
        private static readonly int MinHeight = Shader.PropertyToID("min_height");
        private static readonly int MaxHeight = Shader.PropertyToID("max_height");
        private static readonly int VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length");
        private static readonly int ColourPaletteBuffer = Shader.PropertyToID("Colour_Palette_Buffer");
        private static readonly int ColourCount = Shader.PropertyToID("colour_count");

        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution, float[] minMax, Vector4[] colourPalette)
        {
            // Get the Mesh from the MeshFilter
            Mesh mesh = meshFilter.sharedMesh;

            // Get the vertices from the Mesh
            Vector3[] vertices = mesh.vertices;

            int colourCount = colourPalette.Length;

            InitializeBuffers(vertices, colourCount);

            // Set the data for the vertex buffer
            _vertexBuffer.SetData(vertices);

            // for (int i = 0; i < vertices.Length; i++)
            // {
            //     Debug.Log("Height: " + vertices[i].y);
            // }

            // Find the kernel and set the buffers
            int kernel = computeShader.FindKernel("Colour_Mesh");
            computeShader.SetBuffer(kernel, VertexBuffer, _vertexBuffer);
            computeShader.SetBuffer(kernel, ColourBuffer, _colorBuffer);
            
            _colourPaletteBuffer.SetData(colourPalette);
            computeShader.SetBuffer(kernel, ColourPaletteBuffer, _colourPaletteBuffer);
            computeShader.SetInt(ColourCount, colourCount);

            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
            
            // Set the height parameters for the gradient
            
            computeShader.SetFloat(MinHeight, minMax[0]);
            computeShader.SetFloat(MaxHeight, minMax[1]);
            
            computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);

            // Dispatch the Compute Shader
            computeShader.Dispatch(kernel, resolution.x / 8, resolution.y / 8, 1);

            // Get the colors from the Compute Shader
            Color[] colors = new Color[vertices.Length];
            _colorBuffer.GetData(colors);

            // Apply the colors to the Mesh
            mesh.colors = colors;
            
            meshFilter.mesh = meshFilter.sharedMesh = mesh;
        }

        private void InitializeBuffers(IReadOnlyCollection<Vector3> vertices, int colourCount)
        {
            // Create a ComputeBuffer for vertices and another for colors
            _vertexBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3);
            _colorBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 4); // RGBA

            _colourPaletteBuffer = new ComputeBuffer(colourCount, sizeof(float) * 4);
        }

        public void ReleaseBuffers()
        {
            // Clean up the Compute Buffers
            _vertexBuffer.Release();
            _colorBuffer.Release();
        }
    }

}