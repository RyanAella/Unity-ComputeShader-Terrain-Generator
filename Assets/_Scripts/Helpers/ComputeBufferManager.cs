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
        public ComputeBuffer DomainWarpingOffsetBuffer { get; private set; }
        public ComputeBuffer FalloffMapBuffer { get; private set; }
        public ComputeBuffer LocalMinMaxBuffer { get; private set; }
        public ComputeBuffer GlobalMinMaxBuffer { get; private set; }
        public ComputeBuffer ColourBuffer { get; private set; }
        public ComputeBuffer ColourPaletteBuffer { get; private set; }
        public ComputeBuffer ColourPaletteHeightsBuffer { get; private set; }

        // Private Constructor to prevent instantiation
        private ComputeBufferManager() { }

        /// <summary>
        /// Gets the instance of the Compute Buffer Manager.
        /// </summary>
        public static ComputeBufferManager Instance => _instance ??= new ComputeBufferManager();

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
            // int verticesPerLineX = resolution.x * 2 + 1;
            // int verticesPerLineZ = resolution.y * 2 + 1;
            int verticesPerLineX = resolution.x + 1;
            int verticesPerLineZ = resolution.y + 1;

            VerticesBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 3);
            UVBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 2);
            NormalsBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 3);
            TrianglesBuffer = new ComputeBuffer((verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6, sizeof(int));
            NoiseLayerBuffer = new ComputeBuffer(noiseSettings.octaves, sizeof(int));
            DomainWarpingOffsetBuffer = new ComputeBuffer(Mathf.Max(noiseSettings.offsetVectors.Length, 1), sizeof(float) * 2);
            FalloffMapBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float));
            LocalMinMaxBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float));
            GlobalMinMaxBuffer = new ComputeBuffer(2, sizeof(float));
            ColourBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 4);
            ColourPaletteBuffer = new ComputeBuffer(colourCount, sizeof(float) * 4);
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
            VerticesBuffer?.Release();
            UVBuffer?.Release();
            NormalsBuffer?.Release();
            TrianglesBuffer?.Release();
            NoiseLayerBuffer?.Release();
            DomainWarpingOffsetBuffer?.Release();
            FalloffMapBuffer?.Release();
            LocalMinMaxBuffer?.Release();
            GlobalMinMaxBuffer?.Release();
            ColourBuffer?.Release();
            ColourPaletteBuffer?.Release();
            ColourPaletteHeightsBuffer?.Release();
        }
    }
}