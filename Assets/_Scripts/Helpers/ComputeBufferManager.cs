/*
 * Author: Rebecca Biebl
 * Creation Date: 13-06-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Helpers
{
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
        private ComputeBufferManager() {}
        
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

        public static void InitializeBuffers(Vector2Int resolution, TerrainSettings terrainSettings, int colourCount)
        {
            var instance = new ComputeBufferManager();
            instance.InitBuffers(resolution, terrainSettings.GroundNoiseSettings, colourCount);
            _instance = instance;
        }

        private void InitBuffers(Vector2Int resolution, NoiseSettings noiseSettings, int colourCount)
        {
            int verticesPerLineX = resolution.x * 2 + 1;
            int verticesPerLineZ = resolution.y * 2 + 1;
            
            // NOTE: NoiseGenerationManager
            // Allocate memory for the noise map buffer.
            VerticesBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 3);
            UVBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 2);
            NormalsBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 3);
            // Allocate memory for the triangles buffer.
            TrianglesBuffer = new ComputeBuffer((verticesPerLineX - 1) * (verticesPerLineZ - 1) * 6, sizeof(int));

            NoiseLayerBuffer = new ComputeBuffer(noiseSettings.octaves, sizeof(int));

            NoiseLayerOffsetVectors = new ComputeBuffer(noiseSettings.noiseLayerOffsetVectors.Length, sizeof(float) * 2);

            DomainWarpingOffsetBuffer = new ComputeBuffer((noiseSettings.offsetVectors.Length!= 0)? noiseSettings.offsetVectors.Length : 1, sizeof(float) * 2);

            // NOTE: FalloffMapManager
            FalloffMapBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float));
            
            // NOTE: GeneratorFunctions
            // Create a new compute buffer for the local min-max values with a size determined by the chunkSize.
            LocalMinMaxBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float));

            // Create a new compute buffer for the global min-max values with a size of 2.
            GlobalMinMaxBuffer = new ComputeBuffer(2, sizeof(float));
            
            // NOTE: ColourGenerationManager

            // Create a ComputeBuffer for storing the colors of the mesh.
            // The buffer size is determined by the number of vertices in the mesh.
            // Each color is represented by a Vector4 (RGBA), so the buffer size is 4 times the number of vertices.
            ColourBuffer = new ComputeBuffer(verticesPerLineX * verticesPerLineZ, sizeof(float) * 4);
            
            // // Create a ComputeBuffer for storing the color palette.
            // // The buffer size is determined by the number of colors in the palette.
            // // Each color is represented by a Vector4 (RGBA), so the buffer size is 4 times the number of colors.
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
        }
        
        #endregion
    }
}
