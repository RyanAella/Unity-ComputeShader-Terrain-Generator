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
        #region Variables

        // Compute buffers for storing vertex positions and vertex colors.
        private ComputeBuffer _vertexBuffer; // Stores vertex positions
        private ComputeBuffer _colorBuffer; // Stores vertex colors

        // Compute buffer for storing the color palette.
        private ComputeBuffer _colourPaletteBuffer; // Stores color palette

        // Shader property IDs for accessing shader variables.
        private static readonly int VertexBuffer = Shader.PropertyToID("Vertex_Buffer"); // ID for vertex buffer
        private static readonly int ColourBuffer = Shader.PropertyToID("Colour_Buffer"); // ID for color buffer
        private static readonly int MapWidth = Shader.PropertyToID("map_width"); // ID for map width
        private static readonly int MapHeight = Shader.PropertyToID("map_height"); // ID for map height
        private static readonly int MinHeight = Shader.PropertyToID("min_height"); // ID for min height
        private static readonly int MaxHeight = Shader.PropertyToID("max_height"); // ID for max height

        private static readonly int
            VerticesBufferLength = Shader.PropertyToID("vertices_buffer_length"); // ID for vertices buffer length

        private static readonly int
            ColourPaletteBuffer = Shader.PropertyToID("Colour_Palette_Buffer"); // ID for color palette buffer

        private static readonly int ColourCount = Shader.PropertyToID("colour_count"); // ID for color count

        #endregion

        #region Methods

        /// <summary>
        /// Colours a mesh using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for colouring the mesh.</param>
        /// <param name="meshFilter">The MeshFilter that holds the mesh to be colored.</param>
        /// <param name="resolution">The resolution of the mesh.</param>
        /// <param name="minMax">The array containing the minimum and maximum height values for the gradient.</param>
        /// <param name="colourPalette">The array of colors to use for coloring the mesh.</param>
        // public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution,
        //     float[] minMax, Vector4[] colourPalette)
        // {
        //     // Get the Mesh from the MeshFilter
        //     Mesh mesh = meshFilter.sharedMesh;
        //
        //     // Get the vertices from the Mesh
        //     Vector3[] vertices = mesh.vertices;
        //
        //     // Get the number of colors in the palette
        //     int colourCount = colourPalette.Length;
        //
        //     // Initialize the buffers for the vertices and colors
        //     InitializeBuffers(vertices, colourCount);
        //
        //     // Set the data for the vertex buffer
        //     _vertexBuffer.SetData(vertices);
        //
        //     // Find the kernel and set the buffers
        //     int kernel = computeShader.FindKernel("Colour_Mesh");
        //     computeShader.SetBuffer(kernel, VertexBuffer, _vertexBuffer);
        //     computeShader.SetBuffer(kernel, ColourBuffer, _colorBuffer);
        //
        //     // Set the color palette buffer
        //     _colourPaletteBuffer.SetData(colourPalette);
        //     computeShader.SetBuffer(kernel, ColourPaletteBuffer, _colourPaletteBuffer);
        //     computeShader.SetInt(ColourCount, colourCount);
        //
        //     // Set the map width and height in the compute shader
        //     computeShader.SetInt(MapWidth, resolution.x);
        //     computeShader.SetInt(MapHeight, resolution.y);
        //
        //     // Set the height parameters for the gradient
        //     computeShader.SetFloat(MinHeight, minMax[0]);
        //     computeShader.SetFloat(MaxHeight, minMax[1]);
        //
        //     // Calculate the length of the vertices buffer
        //     computeShader.SetInt(VerticesBufferLength, resolution.x * resolution.y);
        //
        //     // Calculate the number of thread groups to dispatch.
        //     int dispatchX = Mathf.CeilToInt(resolution.x / 8f);
        //     int dispatchY = Mathf.CeilToInt(resolution.y / 8f);
        //
        //     // Dispatch the Compute Shader
        //     computeShader.Dispatch(kernel, dispatchX, dispatchY, 1);
        //
        //     // Get the colors from the Compute Shader
        //     Color[] colors = new Color[vertices.Length];
        //     _colorBuffer.GetData(colors);
        //
        //     // Apply the colors to the Mesh
        //     mesh.colors = colors;
        //
        //     // Update the sharedMesh of the MeshFilter
        //     meshFilter.mesh = meshFilter.sharedMesh = mesh;
        // }

        /// <summary>
        /// Initializes the compute buffers used for coloring a mesh.
        /// </summary>
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="colourCount">The number of colors in the palette.</param>
        private void InitializeBuffers(IReadOnlyCollection<Vector3> vertices, int colourCount)
        {
            // Create a ComputeBuffer for storing the vertices of the mesh.
            // The buffer size is determined by the number of vertices in the mesh.
            // Each vertex is represented by a Vector3, so the buffer size is 3 times the number of vertices.
            _vertexBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3);

            // Create a ComputeBuffer for storing the colors of the mesh.
            // The buffer size is determined by the number of vertices in the mesh.
            // Each color is represented by a Vector4 (RGBA), so the buffer size is 4 times the number of vertices.
            _colorBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 4);

            // Create a ComputeBuffer for storing the color palette.
            // The buffer size is determined by the number of colors in the palette.
            // Each color is represented by a Vector4 (RGBA), so the buffer size is 4 times the number of colors.
            _colourPaletteBuffer = new ComputeBuffer(colourCount, sizeof(float) * 4);
        }

        /// <summary>
        /// Releases the compute buffers used for storing vertices, colors, and color palettes.
        /// </summary>
        public void ReleaseBuffers()
        {
            // Release the Compute Buffer for vertices
            _vertexBuffer.Release();

            // Release the Compute Buffer for colors
            _colorBuffer.Release();
        }

        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution,
            float[] minMax, Vector4[] colourPalette)
        {
            if (meshFilter == null || meshFilter.mesh == null)
            {
                Debug.LogError("MeshFilter or mesh is not assigned.");
                return;
            }
            
            Vector3[] vertices = meshFilter.mesh.vertices;

            float min_height = minMax[0];
            float max_height = minMax[1];

            int colour_count = colourPalette.Length;

            Color[] Colour_Buffer = new Color[resolution.x * resolution.y];

            for (int i = 0; i < resolution.y; i++)
            {
                for (int j = 0; j < resolution.x; j++)
                {
                    int index = i * resolution.x + j;

                    float height = vertices[index].y;
                    
                    // Debug.Log("min_height: " + min_height + " max_height: " + max_height + " height: " + height);

                    // float normalized_y = 0.0f;
                    // if (max_height!= min_height)
                    // {
                    //     normalized_y = (height - min_height) / (max_height - min_height);
                    // }

                    float normalized_y = height;
                    
                    // Debug.Log("min_height: " + min_height + " max_height: " + max_height + " height: " + height + " normalized_y: " + normalized_y);

                    // Debugging: Print the normalized_y value
                    // Debug.Log($"Normalized Y at ({i}, {j}): {normalized_y}");

                    if (normalized_y >= 0 && normalized_y <= 1)
                    {
                        for (int k = 0; k < colour_count - 1; k++)
                        {
                            float range_start = (float)k / (colour_count - 1);
                            float range_end = (float)(k + 1) / (colour_count - 1);

                            if (normalized_y >= range_start && normalized_y <= range_end)
                            {
                                float t = (normalized_y - range_start) / (range_end - range_start);
                                Colour_Buffer[index] = Color.Lerp(colourPalette[k], colourPalette[k + 1], t);
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Debugging: Print the reason for the else case
                        Debug.LogWarning($"Else case triggered due to normalized_y: {normalized_y}");
                        Colour_Buffer[index] = new Color(0, 0, 0, 1);
                    }
                }
            }

            meshFilter.mesh.colors = meshFilter.sharedMesh.colors = Colour_Buffer;
        }



        #endregion
    }
}