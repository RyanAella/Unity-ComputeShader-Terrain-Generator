/*
 * Author: Rebecca Biebl
 * Creation Date: 01-05-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


using System.Collections.Generic;
using _Scripts.Helpers;
using UnityEngine;

namespace _Scripts.Manager
{
    public class ColourGenerationManager
    {
        #region Variables

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
            ColourPaletteBuffer = Shader.PropertyToID("Colour_Palette_Buffer"), // ID for color palette buffer
            ColourPaletteHeightsBuffer = Shader.PropertyToID("Colour_Palette_Heights_Buffer"); // ID for color palette buffer

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
        /// <param name="colourPaletteHeights"></param>
        /// <param name="colourCount"></param>
        /// <param name="isWater"></param>
        /// <param name="material"></param>
        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution,
            float[] minMax, Vector4[] colourPalette, float[] colourPaletteHeights, int colourCount, bool isWater, Material material)
        {
            if (!meshFilter || !meshFilter.sharedMesh)
            {
                Debug.LogError("MeshFilter or mesh is not assigned.");
                return;
            }
            
            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;

            // Get the Mesh from the MeshFilter
            Mesh mesh = meshFilter.sharedMesh;
            
            // Get the vertices from the Mesh
            Vector3[] vertices = mesh.vertices;
            
            // Set the data for the vertex buffer
            ComputeBufferManager.Instance.VerticesBuffer.SetData(vertices);
            
            // Find the kernel and set the buffers
            int kernel = computeShader.FindKernel("Colour_Mesh");
            
            computeShader.SetBuffer(kernel, VertexBuffer, ComputeBufferManager.Instance.VerticesBuffer);
            computeShader.SetBuffer(kernel, ColourBuffer, ComputeBufferManager.Instance.ColourBuffer);
            
            // Set the color palette buffer
            ComputeBufferManager.Instance.ColourPaletteBuffer.SetData(colourPalette);
            computeShader.SetBuffer(kernel, ColourPaletteBuffer, ComputeBufferManager.Instance.ColourPaletteBuffer);
            
            ComputeBufferManager.Instance.ColourPaletteHeightsBuffer.SetData(colourPaletteHeights);
            computeShader.SetBuffer(kernel, ColourPaletteHeightsBuffer, ComputeBufferManager.Instance.ColourPaletteHeightsBuffer);
            computeShader.SetInt(ColourCount, colourCount);
            
            // Set the map width and height in the compute shader
            computeShader.SetInt(MapWidth, verticesPerLineX);
            computeShader.SetInt(MapHeight, verticesPerLineZ);
            
            // Set the height parameters for the gradient
            computeShader.SetFloat(MinHeight, minMax[0]);
            computeShader.SetFloat(MaxHeight, minMax[1]);
            
            // Calculate the length of the vertices buffer
            computeShader.SetInt(VerticesBufferLength, verticesPerLineX * verticesPerLineZ);
            
            computeShader.SetFloat("time", Time.time);
            
            computeShader.SetBool("is_water", isWater);
            
            // Calculate the number of thread groups to dispatch.
            int dispatchX = Mathf.CeilToInt(verticesPerLineX / 8f);
            int dispatchY = Mathf.CeilToInt(verticesPerLineZ / 8f);
            
            // Dispatch the Compute Shader
            computeShader.Dispatch(kernel, dispatchX, dispatchY, 1);

            // Get the colors from the Compute Shader
            Color[] colours = new Color[vertices.Length];
            ComputeBufferManager.Instance.ColourBuffer.GetData(colours);

            // Apply the colors to the Mesh
            mesh.colors = colours;
            
            // List<int>[] neighborIndices = new List<int>[vertices.Length];
            //
            // // Set up example neighbor indices (you should have your own logic to populate this)
            // // For demonstration purposes, we assume each vertex has 3 neighbors
            // for (int i = 0; i < vertices.Length; i++)
            // {
            //     neighborIndices[i] = new List<int>();
            //     neighborIndices[i].Add((i + 1) % vertices.Length); // Example: Connect to next vertex
            //     neighborIndices[i].Add((i + 2) % vertices.Length); // Example: Connect to vertex skipping one
            //     neighborIndices[i].Add((i + 3) % vertices.Length); // Example: Connect to vertex skipping two
            // }
            //
            // // Interpolate colors based on neighbors
            // for (int i = 0; i < vertices.Length; i++)
            // {
            //     Color interpolatedColor = InterpolateColor(vertices, colours, neighborIndices[i]);
            //     colours[i] = interpolatedColor;
            // }
            //
            // // Apply interpolated colors to mesh
            // mesh.colors = colours;
        }

        Color InterpolateColor(Vector3[] vertices, Color[] colors, List<int> neighborIndices)
        {
            Color accumulatedColor = Color.black;
            int neighborCount = 0;

            foreach (int neighborIndex in neighborIndices)
            {
                accumulatedColor += colors[neighborIndex];
                neighborCount++;
            }

            // Interpolate color (simple average)
            Color interpolatedColor = accumulatedColor / neighborCount;

            return interpolatedColor;
        }
        

        /// <summary>
        /// Releases the compute buffers used for storing vertices, colors, and color palettes.
        /// </summary>
        public void ReleaseBuffers()
        {
            // Release the Compute Buffer for vertices
            ComputeBufferManager.Instance.VerticesBuffer?.Release();

            // Release the Compute Buffer for colors
            ComputeBufferManager.Instance.ColourBuffer?.Release();

            // Release the Compute Buffer for color palette
            ComputeBufferManager.Instance.ColourPaletteBuffer?.Release();
            ComputeBufferManager.Instance.ColourPaletteHeightsBuffer?.Release();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colours"></param>
        /// <param name="length"></param>
        /// <param name="heights"></param>
        /// <returns></returns>
        public static List<Vector4> GetColorPalette(TerrainColour[] colours, out int length, out List<float> heights)
        {
            List<Vector4> palette = new List<Vector4>();
            heights = new List<float>();

            // Durchlaufen aller TerrainColour-Objekte
            for (int i = 0; i < colours.Length; i++)
            {
                // Überprüfen, ob der Höhenwert innerhalb des gewünschten Bereichs liegt
                if (colours[i].height >= 0 && colours[i].height <= 1)
                {
                    // Extrahieren des RGB-Werts und des Alphas
                    var rgb = colours[i].colour;
                    var alpha = colours[i].colour.a; // Standard-Alpha-Wert, falls nicht anders definiert
            
                    // Hinzufügen des neuen Vektor4-Elements zur Palette
                    palette.Add(new Vector4(rgb.r, rgb.g, rgb.b, alpha));
                    heights.Add(colours[i].height);

                    // Optionaler Debugging-Ausdruck
                    // Debug.Log($"Added colour: {rgb} with height: {colours[i].Height}");
                }
            }

            length = palette.Count;

            return palette;
        }

        #endregion
    }
}