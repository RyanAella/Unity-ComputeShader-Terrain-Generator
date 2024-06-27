/*
 * Author: Rebecca Biebl
 * Creation Date: 13-06-2024
 * Description: This script manages the creation of compute buffers.
 * License: MIT Licence
 */


using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Helpers
{
    /// <summary>
    /// Manages the creation and release of compute buffers.
    /// </summary>
    public class ComputeBufferManager
    {
        private static ComputeBufferManager _instance;

        // Getter for the buffers
        public ComputeBuffer VerticesBuffer { get; private set; }
        public ComputeBuffer UVBuffer { get; private set; }
        public ComputeBuffer NormalsBuffer { get; private set; }
        public ComputeBuffer TrianglesBuffer { get; private set; }
        public ComputeBuffer NoiseLayerBuffer { get; private set; }
        public ComputeBuffer NoiseLayerOffsetVectors { get; private set; }
        public ComputeBuffer DomainWarpingOffsetBuffer { get; private set; }
        public ComputeBuffer FalloffMapBuffer { get; private set; }
        public ComputeBuffer LocalMinMaxBuffer { get; private set; }
        public ComputeBuffer GlobalMinMaxBuffer { get; private set; }
        public ComputeBuffer ColourBuffer { get; private set; }
        public ComputeBuffer ColourPaletteBuffer { get; private set; }
        public ComputeBuffer ColourPaletteHeightsBuffer { get; private set; }

        #region Methods

        // Private Constructor to prevent instantiation
        private ComputeBufferManager()
        {
        }

        /// <summary>
        /// Gets the instance of the Compute Buffer Manager.
        /// </summary>
        public static ComputeBufferManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComputeBufferManager();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Initializes the compute buffers.
        /// </summary>
        public static void InitializeBuffers(Vector2Int resolution, TerrainSettings terrainSettings, int colourCount)
        {
            var instance = new ComputeBufferManager();
            instance.InitBuffers(resolution, terrainSettings.NoiseSettings, colourCount);
            _instance = instance;
        }

        /// <summary>
        /// Initializes the compute buffers with given settings.
        /// </summary>
        /// <param name="resolution">The resolution of the terrain.</param>
        /// <param name="noiseSettings">The noise settings used for terrain generation.</param>
        /// <param name="colourCount">The number of colors in the color palette.</param>
        private void InitBuffers(Vector2Int resolution, NoiseSettings noiseSettings, int colourCount)
        {
            // Calculate vertices per line
            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;

            // Allocate memory for the vertices buffer.
            VerticesBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 3);
            
            // Allocate memory for the UV buffer.
            UVBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 2);
            
            // Allocate memory for the normals buffer.
            NormalsBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 3);
            
            // Allocate memory for the triangles buffer.
            TrianglesBuffer = new ComputeBuffer((verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6, sizeof(int));

            // Allocate memory for the noise layer buffer.
            NoiseLayerBuffer = new ComputeBuffer(noiseSettings.octaves, sizeof(int));
            
            // Allocate memory for the noise layer offset vectors buffer.
            NoiseLayerOffsetVectors = new ComputeBuffer(noiseSettings.noiseLayerOffsetVectors.Length, sizeof(float) * 2);
            
            // Allocate memory for the domain warping offset buffer.
            DomainWarpingOffsetBuffer = new ComputeBuffer((noiseSettings.offsetVectors.Length != 0) ? noiseSettings.offsetVectors.Length : 1, sizeof(float) * 2);

            // Allocate memory for the falloff map buffer.
            FalloffMapBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float));

            // Allocate memory for the local min-max values buffer.
            LocalMinMaxBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float));

            // Allocate memory for the global min-max values buffer.
            GlobalMinMaxBuffer = new ComputeBuffer(2, sizeof(float));

            // Allocate memory for the color buffer.
            ColourBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 4);

            // Allocate memory for the color palette buffer.
            ColourPaletteBuffer = new ComputeBuffer(colourCount, sizeof(float) * 4);
            
            // Allocate memory for the color palette heights buffer.
            ColourPaletteHeightsBuffer = new ComputeBuffer(colourCount, sizeof(float));
        }

        /// <summary>
        /// Releases the compute buffers when they are no longer needed.
        /// </summary>
        /// <remarks>
        /// This method is used to release the memory allocated for the compute buffers.
        /// It should be called when the buffers are no longer in use to prevent memory leaks.
        /// </remarks>
        public void ReleaseBuffers()
        {
            // Release the vertices buffer
            VerticesBuffer?.Release();

            //Release the uv buffer
            UVBuffer?.Release();

            //Release the normals buffer
            NormalsBuffer?.Release();

            // Release the triangles buffer
            TrianglesBuffer?.Release();

            // Release the noise layer buffer
            NoiseLayerBuffer?.Release();

            // Release the noise layer offset vectors buffer
            NoiseLayerOffsetVectors?.Release();

            // Release the domain warping offset buffer
            DomainWarpingOffsetBuffer?.Release();

            // Release the falloff map buffer
            FalloffMapBuffer?.Release();

            // Release the Compute Buffer for vertices
            VerticesBuffer?.Release();

            // Release the Compute Buffer for colors
            ColourBuffer?.Release();

            // Release the Compute Buffer for color palette
            ColourPaletteBuffer?.Release();
            ColourPaletteHeightsBuffer?.Release();
            
            // Release the local min-max compute buffer
            LocalMinMaxBuffer?.Release();

            // Release the global min-max compute buffer
            GlobalMinMaxBuffer?.Release();
        }

        #endregion
    }
}