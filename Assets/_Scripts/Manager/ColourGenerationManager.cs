/*
 * Author: Rebecca Biebl
 * Creation Date: 01-05-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Manager
{
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
        
        private int _verticesBufferLength; // Length of the vertices buffer
        
        #endregion

        #region Methods

        /// <summary>
        /// Colours a mesh using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for colouring the mesh.</param>
        /// <param name="meshFilter">The MeshFilter that holds the mesh to be colored.</param>
        /// <param name="resolution">The chunkSize of the mesh.</param>
        /// <param name="minMax">The array containing the minimum and maximum height values for the gradient.</param>
        /// <param name="colourPalette">The array of colors to use for coloring the mesh.</param>
        /// <param name="colourCount"></param>
        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, int resolution,
            float[] minMax, Vector4[] colourPalette, int colourCount)
        {
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError("MeshFilter or mesh is not assigned.");
                return;
            }
            
            // Get the Mesh from the MeshFilter
            Mesh mesh = meshFilter.sharedMesh;
        
            // Get the vertices from the Mesh
            Vector3[] vertices = mesh.vertices;
        
            // Set the data for the vertex buffer
            _vertexBuffer.SetData(vertices);
        
            // Find the kernel and set the buffers
            int kernel = computeShader.FindKernel("Colour_Mesh");
            
            computeShader.SetBuffer(kernel, VertexBuffer, _vertexBuffer);
            computeShader.SetBuffer(kernel, ColourBuffer, _colorBuffer);
        
            // Set the color palette buffer
            _colourPaletteBuffer.SetData(colourPalette);
            computeShader.SetBuffer(kernel, ColourPaletteBuffer, _colourPaletteBuffer);
            computeShader.SetInt(ColourCount, colourCount);
        
            // Set the map width and height in the compute shader
            computeShader.SetInt(MapWidth, resolution);
            computeShader.SetInt(MapHeight, resolution);
        
            // Set the height parameters for the gradient
            computeShader.SetFloat(MinHeight, minMax[0]);
            computeShader.SetFloat(MaxHeight, minMax[1]);
        
            // Calculate the length of the vertices buffer
            computeShader.SetInt(VerticesBufferLength, _verticesBufferLength);
        
            // Calculate the number of thread groups to dispatch.
            int dispatchX = Mathf.CeilToInt(resolution / 8f);
            int dispatchY = Mathf.CeilToInt(resolution / 8f);
        
            // Dispatch the Compute Shader
            computeShader.Dispatch(kernel, dispatchX, dispatchY, 1);
        
            // Get the colors from the Compute Shader
            Color[] colors = new Color[vertices.Length];
            _colorBuffer.GetData(colors);
        
            // Apply the colors to the Mesh
            mesh.colors = colors;
        }

        /// <summary>
        /// Initializes the compute buffers used for coloring a mesh.
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="colourCount">The number of colors in the palette.</param>
        public void InitializeBuffers(int resolution, int colourCount)
        {
            int vertexCount = _verticesBufferLength = resolution * resolution;
            
            // Create a ComputeBuffer for storing the vertices of the mesh.
            // The buffer size is determined by the number of vertices in the mesh.
            // Each vertex is represented by a Vector3, so the buffer size is 3 times the number of vertices.
            _vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);

            // Create a ComputeBuffer for storing the colors of the mesh.
            // The buffer size is determined by the number of vertices in the mesh.
            // Each color is represented by a Vector4 (RGBA), so the buffer size is 4 times the number of vertices.
            _colorBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 4);

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
            _vertexBuffer?.Release();

            // Release the Compute Buffer for colors
            _colorBuffer?.Release();
            
            // Release the Compute Buffer for color palette
            _colourPaletteBuffer?.Release();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gradient"></param>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        /// <returns></returns>
        public static List<Vector4> GetColorPalette(Gradient gradient, float minRange, float maxRange)
        {
            List<Vector4> palette = new List<Vector4>();

            foreach (var colorKey in gradient.colorKeys)
            {
                if (colorKey.time >= minRange && colorKey.time <= maxRange)
                {
                    // Find the corresponding alpha key
                    float alpha = 1.0f; // Default alpha
                    foreach (var alphaKey in gradient.alphaKeys)
                    {
                        if (Mathf.Approximately(alphaKey.time, colorKey.time))
                        {
                            alpha = alphaKey.alpha;
                            break;
                        }
                    }

                    palette.Add(new Vector4(colorKey.color.r, colorKey.color.g, colorKey.color.b, alpha));
                }
            }

            return palette;
        }

        #endregion
    }
}