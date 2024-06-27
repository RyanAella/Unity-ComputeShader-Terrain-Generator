/*
 * Author: Rebecca Biebl
 * Creation Date: 01-05-2024
 * Description: Class for managing colour generation.
 * License: MIT Licence
 */


using System.Collections.Generic;
using _Scripts.Helpers;
using UnityEngine;

namespace _Scripts.Manager
{
    /// <summary>
    /// Class for managing colour generation.
    /// </summary>
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

        private static readonly int ColourPaletteBuffer =
                Shader.PropertyToID("Colour_Palette_Buffer"), // ID for color palette buffer
            ColourPaletteHeightsBuffer =
                Shader.PropertyToID("Colour_Palette_Heights_Buffer"); // ID for color palette buffer
        
        private static readonly int ColourCount = Shader.PropertyToID("colour_count"); // ID for color count

        private static readonly int Time = Shader.PropertyToID("time"); // ID for time
        private static readonly int IsWater = Shader.PropertyToID("is_water"); // ID for is water

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
        /// <param name="colourPaletteHeights">The heights corresponding to each color in the palette.</param>
        /// <param name="colourCount">The number of colors in the palette.</param>
        /// <param name="isWater">Boolean flag indicating if the mesh is water.</param>
        /// <param name="material">The material to apply to the mesh.</param>
        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution,
            float[] minMax, Vector4[] colourPalette, float[] colourPaletteHeights, int colourCount, bool isWater,
            Material material)
        {
            if (!meshFilter || !meshFilter.sharedMesh)
            {
                Debug.LogError("MeshFilter or mesh is not assigned.");
                return;
            }

            // Calculate vertices per line
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
            computeShader.SetBuffer(kernel, ColourPaletteHeightsBuffer,
                ComputeBufferManager.Instance.ColourPaletteHeightsBuffer);
            computeShader.SetInt(ColourCount, colourCount);

            // Set the map width and height in the compute shader
            computeShader.SetInt(MapWidth, verticesPerLineX);
            computeShader.SetInt(MapHeight, verticesPerLineZ);

            // Set the height parameters for the gradient
            computeShader.SetFloat(MinHeight, minMax[0]);
            computeShader.SetFloat(MaxHeight, minMax[1]);

            // Calculate the length of the vertices buffer
            computeShader.SetInt(VerticesBufferLength, verticesPerLineX * verticesPerLineZ);

            computeShader.SetFloat(Time, UnityEngine.Time.time);

            computeShader.SetBool(IsWater, isWater);

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

        /// <summary>
        /// Interpolates a color based on the colors of neighboring vertices.
        /// </summary>
        /// <param name="vertices">The array of vertices in the mesh.</param>
        /// <param name="colors">The array of colors assigned to each vertex.</param>
        /// <param name="neighborIndices">The list of indices of neighboring vertices.</param>
        /// <returns>The interpolated color calculated as a simple average of neighboring colors.</returns>
        Color InterpolateColor(Vector3[] vertices, Color[] colors, List<int> neighborIndices)
        {
            // Initialize the accumulated color to black
            Color accumulatedColor = Color.black;
            // Initialize the count of neighboring vertices
            int neighborCount = 0;

            // Iterate over each neighbor index
            foreach (int neighborIndex in neighborIndices)
            {
                // Add the color of the neighbor to the accumulated color
                accumulatedColor += colors[neighborIndex];
                // Increment the count of neighboring vertices
                neighborCount++;
            }

            // Calculate the interpolated color as the average of neighboring colors
            Color interpolatedColor = accumulatedColor / neighborCount;

            // Return the interpolated color
            return interpolatedColor;
        }

        /// <summary>
        /// Gets a color palette from an array of TerrainColour objects.
        /// </summary>
        /// <param name="colours">The array of TerrainColour objects to extract colors from.</param>
        /// <param name="length">The number of colors in the resulting palette.</param>
        /// <param name="heights">The heights corresponding to each color in the palette.</param>
        /// <returns>The list of Vector4 colors extracted from the TerrainColour objects.</returns>
        public static List<Vector4> GetColorPalette(TerrainColour[] colours, out int length, out List<float> heights)
        {
            // Initialize the resulting color palette and heights list
            List<Vector4> palette = new List<Vector4>();
            heights = new List<float>();

            // Iterate over all TerrainColour objects
            for (int i = 0; i < colours.Length; i++)
            {
                // Check if the height value is within the desired range
                if (colours[i].height >= 0 && colours[i].height <= 1)
                {
                    // Extract the RGB values and alpha
                    var rgb = colours[i].colour;
                    var alpha = colours[i].colour.a; // Default alpha value if not defined otherwise

                    // Add the new Vector4 element to the palette
                    palette.Add(new Vector4(rgb.r, rgb.g, rgb.b, alpha));
                    heights.Add(colours[i].height);

                    // Optional debugging statement
                    // Debug.Log($"Added colour: {rgb} with height: {colours[i].Height}");
                }
            }

            // Set the length of the palette
            length = palette.Count;

            return palette;
        }

        #endregion
    }
}