/*
 * Author: Rebecca Biebl
 * Creation Date: 31-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Manager
{
    public class FalloffMapManager
    {
        #region Variables

        private static readonly int MapWidth = Shader.PropertyToID("map_width"); // ID for map width
        private static readonly int MapHeight = Shader.PropertyToID("map_height"); // ID for map height
        
        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
        private static readonly int FalloffMapBuffer = Shader.PropertyToID("_Falloff_Map_Buffer");

        #endregion

        #region Methods
        
        public static void ReleaseBuffers()
        {
            ComputeBufferManager.Instance.FalloffMapBuffer?.Release();
        }

        public void ApplyFalloffMap(Shaders shaders, GeneralSettings generalSettings)
        {
            Vector2Int resolution = generalSettings.resolution;
            ComputeShader falloffComputeShader = shaders.falloffComputeShader;
            
            // Find the kernel in the compute shader.
            var noiseKernel = falloffComputeShader.FindKernel("Falloff_Map");

            // Set shader properties
            falloffComputeShader.SetInt(MapWidth, resolution.x);
            falloffComputeShader.SetInt(MapHeight, resolution.y);
            
            falloffComputeShader.SetBuffer(noiseKernel, VertexBuffer, ComputeBufferManager.Instance.VerticesBuffer);
            falloffComputeShader.SetBuffer(noiseKernel, FalloffMapBuffer, ComputeBufferManager.Instance.FalloffMapBuffer);

            // Calculate the number of thread groups to dispatch.
            var dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            var dispatchY = Mathf.CeilToInt(resolution.y / 16f);

            // Dispatch the compute shader to generate the mesh parameters.
            falloffComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        }

        #endregion
    }
}