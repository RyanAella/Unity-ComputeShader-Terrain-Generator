/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System;
using UnityEngine;

namespace _Scripts
{
    [Serializable]
    public class ComputeShaderManager
    {
        #region Variables
        
        public ComputeBuffer NoiseBuffer => _noiseBuffer;

        private ComputeBuffer _noiseBuffer;
        
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");

        private static readonly int NoiseMap = Shader.PropertyToID("NoiseMap");

        private float[,] _noiseMap;
        
        #endregion

        #region Methods

        public void InitializeBuffer(Vector2Int resolution)
        {
            _noiseBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float));
        }

        public void ReleaseBuffer()
        {
            _noiseBuffer.Release();
        }

        public float[,] GenerateNoiseMap(ComputeShader computeShader, Vector2Int resolution)
        {
            _noiseMap = new float[resolution.x, resolution.y];
            
            var noiseKernel = computeShader.FindKernel("NoiseGenerator");

            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
            computeShader.SetBuffer(0, NoiseMap, _noiseBuffer);

            int dispatchX = Mathf.CeilToInt(resolution.x / 8.0f);
            int dispatchY = Mathf.CeilToInt(resolution.y / 8.0f);
            computeShader.Dispatch(0, dispatchX, dispatchY, 1);

            _noiseBuffer.GetData(_noiseMap);

            return _noiseMap;
        }

        #endregion
    }
}