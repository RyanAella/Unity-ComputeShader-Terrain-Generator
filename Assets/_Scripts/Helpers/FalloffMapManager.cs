/*
 * Author: Rebecca Biebl
 * Creation Date: 31-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Helpers
{
    public class FalloffMapManager
    {
        #region Variables
        
        private ComputeBuffer _falloffMapBuffer; // Compute buffer for vertices

        private static readonly int MapWidth = Shader.PropertyToID("map_width"); // ID for map width
        private static readonly int MapHeight = Shader.PropertyToID("map_height"); // ID for map height
        
        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
        private static readonly int FalloffMapBuffer = Shader.PropertyToID("_Falloff_Map_Buffer");

        #endregion

        #region Methods

        public void InitializeBuffers(int resolution)
        {
            _falloffMapBuffer = new ComputeBuffer(resolution * resolution, sizeof(float));
        }
        
        public void ReleaseBuffers()
        {
            _falloffMapBuffer?.Release();
        }

        public void ApplyFalloffMap(GeneralSettings generalSettings, ShaderSettings shaderSettings, ComputeBuffer vertexBuffer)
        {
            int resolution = generalSettings.chunkSize;
            ComputeShader falloffComputeShader = shaderSettings.falloffComputeShader;
            
            // Find the kernel in the compute shader.
            var noiseKernel = falloffComputeShader.FindKernel("Falloff_Map");

            // Set shader properties
            falloffComputeShader.SetInt(MapWidth, resolution);
            falloffComputeShader.SetInt(MapHeight, resolution);
            
            falloffComputeShader.SetBuffer(noiseKernel, VertexBuffer, vertexBuffer);
            falloffComputeShader.SetBuffer(noiseKernel, FalloffMapBuffer, _falloffMapBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution / 16f);
            var dispatchY = Mathf.CeilToInt(resolution / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            falloffComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        }

        #endregion
    }
}