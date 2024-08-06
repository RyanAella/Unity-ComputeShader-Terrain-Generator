// /*
//  * Author: Rebecca Biebl
//  * Creation Date: 31-05-2024
//  * Description: Manages the generation of a falloff map using a compute shader.
//  * License: MIT Licence
//  */
//
//
// using _Scripts.Helpers;
// using _Scripts.ScriptableObjects;
// using UnityEngine;
//
// namespace _Scripts.Manager
// {
//     /// <summary>
//     /// Manages the generation of a falloff map using a compute shader.
//     /// </summary>
//     public class FalloffMapManager
//     {
//         #region Variables
//
//         private static readonly int MapWidth = Shader.PropertyToID("map_width"); // ID for map width
//         private static readonly int MapHeight = Shader.PropertyToID("map_height"); // ID for map height
//         
//         private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer"); // ID for vertex buffer
//         private static readonly int FalloffMapBuffer = Shader.PropertyToID("_Falloff_Map_Buffer"); // ID for falloff map buffer
//
//         #endregion
//
//         #region Methods
//         
//         /// <summary>
//         /// Applies the falloff map generation using the provided shaders and general settings.
//         /// </summary>
//         /// <param name="shaders">The shader settings to use.</param>
//         /// <param name="generalSettings">The general settings for the falloff map.</param>
//         public void ApplyFalloffMap(Shaders shaders, GeneralSettings generalSettings)
//         {
//             // Get resolution from general settings
//             Vector2Int resolution = generalSettings.resolution;
//             
//             // Calculate vertices per line
//             int verticesPerLineX = resolution.x * 2 + 1;
//             int verticesPerLineZ = resolution.y * 2 + 1;
//             
//             ComputeShader falloffComputeShader = shaders.falloffComputeShader;
//             
//             // Find the kernel in the compute shader.
//             var noiseKernel = falloffComputeShader.FindKernel("Falloff_Map");
//             
//             // Set shader properties
//             falloffComputeShader.SetInt(MapWidth, verticesPerLineX);
//             falloffComputeShader.SetInt(MapHeight, verticesPerLineZ);
//             
//             falloffComputeShader.SetBuffer(noiseKernel, VertexBuffer, ComputeBufferManager.Instance.VerticesBuffer);
//             falloffComputeShader.SetBuffer(noiseKernel, FalloffMapBuffer, ComputeBufferManager.Instance.FalloffMapBuffer);
//
//             // Calculate the number of thread groups to dispatch.
//             var dispatchX = Mathf.CeilToInt(verticesPerLineX / 16f);
//             var dispatchY = Mathf.CeilToInt(verticesPerLineZ / 16f);
//
//             // Dispatch the compute shader to generate the mesh parameters.
//             falloffComputeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
//         }
//
//
//         #endregion
//     }
// }