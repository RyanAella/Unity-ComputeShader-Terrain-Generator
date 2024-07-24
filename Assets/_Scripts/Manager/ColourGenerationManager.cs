/*
 * Author: Rebecca Biebl
 * Creation Date: 01-05-2024
 * Description: Class for managing colour generation.
 * License: MIT Licence
 */


using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
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
        private static readonly int MaxTerrainHeight = Shader.PropertyToID("max_terrain_height");

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
        /// <param name="settings"></param>
        public void ColourMesh(ComputeShader computeShader, MeshFilter meshFilter, Vector2Int resolution,
            float[] minMax, Vector4[] colourPalette, float[] colourPaletteHeights, int colourCount,
            GeneralSettings settings)
        {
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                #if DEBUG
                    Debug.LogError("MeshFilter or mesh is not assigned.");
                #endif
                return;
            }

            // Calculate vertices per line
            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;

            // Get the Mesh and vertices
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;

            // Set the data for the vertex buffer
            ComputeBufferManager.Instance.VerticesBuffer.SetData(vertices);

            // Find kernel and set buffers
            int kernel = computeShader.FindKernel("Colour_Mesh");
            computeShader.SetBuffer(kernel, VertexBuffer, ComputeBufferManager.Instance.VerticesBuffer);
            computeShader.SetBuffer(kernel, ColourBuffer, ComputeBufferManager.Instance.ColourBuffer);

            // Set the color palette buffer
            ComputeBufferManager.Instance.ColourPaletteBuffer.SetData(colourPalette);
            computeShader.SetBuffer(kernel, ColourPaletteBuffer, ComputeBufferManager.Instance.ColourPaletteBuffer);

            ComputeBufferManager.Instance.ColourPaletteHeightsBuffer.SetData(colourPaletteHeights);
            computeShader.SetBuffer(kernel, ColourPaletteHeightsBuffer, ComputeBufferManager.Instance.ColourPaletteHeightsBuffer);

            // Set shader parameters
            computeShader.SetInt(ColourCount, colourCount);
            computeShader.SetInt(MapWidth, verticesPerLineX);
            computeShader.SetInt(MapHeight, verticesPerLineZ);
            computeShader.SetFloat(MinHeight, minMax[0]);
            computeShader.SetFloat(MaxHeight, minMax[1]);
            computeShader.SetInt(VerticesBufferLength, verticesPerLineX * verticesPerLineZ);
            computeShader.SetFloat(Time, UnityEngine.Time.time);
            computeShader.SetFloat(MaxTerrainHeight, settings.maxTerrainHeight);

            // Dispatch compute shader
            computeShader.Dispatch(kernel, Mathf.CeilToInt(verticesPerLineX / 16f), Mathf.CeilToInt(verticesPerLineZ / 16f), 1);

            // Get and apply colors
            Color[] colours = new Color[vertices.Length];
            ComputeBufferManager.Instance.ColourBuffer.GetData(colours);
            mesh.colors = colours;
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
                }
            }

            // Set the length of the palette
            length = palette.Count;

            return palette;
        }

        #endregion
    }
}