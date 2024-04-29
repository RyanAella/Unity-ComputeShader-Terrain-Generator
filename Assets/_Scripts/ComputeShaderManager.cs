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

        private ComputeBuffer _noiseBuffer;
        private ComputeBuffer _trianglesBuffer;
        
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");

        private static readonly int NoiseBuffer = Shader.PropertyToID("NoiseBuffer");
        private static readonly int TrianglesBuffer = Shader.PropertyToID("TrianglesBuffer");

        private Vector3[] _noiseMap;
        private int[] _triangles;
        
        private Mesh _mesh;
        
        #endregion

        #region Methods

        public void InitializeBuffer(Vector2Int resolution)
        {
            _noiseBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
            _trianglesBuffer = new ComputeBuffer((resolution.x - 1) * (resolution.y - 1) * 6, sizeof(int));
        }

        public void ReleaseBuffer()
        {
            _noiseBuffer.Release();
        }

        public Vector3[] GenerateNoiseMap(ComputeShader computeShader, Vector2Int resolution, MeshFilter filter)
        {
            _noiseMap = new Vector3[resolution.x * resolution.y];
            _triangles = new int[(resolution.x - 1) * (resolution.y - 1) * 6];
            
            var noiseKernel = computeShader.FindKernel("NoiseGenerator");

            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
            computeShader.SetBuffer(noiseKernel, NoiseBuffer, _noiseBuffer);
            computeShader.SetBuffer(noiseKernel, TrianglesBuffer, _trianglesBuffer);

            int dispatchX = Mathf.CeilToInt(resolution.x);
            int dispatchY = Mathf.CeilToInt(resolution.y);
            computeShader.Dispatch(0, dispatchX, dispatchY, 1);

            _noiseBuffer.GetData(_noiseMap);

            _trianglesBuffer.GetData(_triangles);
            
            _mesh = new Mesh();
            filter.sharedMesh = _mesh;
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            
            _mesh.Clear();
            _mesh.vertices = _noiseMap;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();

            return _noiseMap;
        }

        // public void GenerateMesh(ComputeShader computeShader, Vector2Int resolution)
        // {
        //     // Finden Sie den Index des Kernels "MeshGenerator"
        //     int meshGeneratorKernel = computeShader.FindKernel("MeshGenerator");
        //
        //     // Setzen Sie die erforderlichen Shader-Parameter
        //     computeShader.SetInt(MapWidth, resolution.x);
        //     computeShader.SetInt(MapHeight, resolution.y);
        //     computeShader.SetBuffer(meshGeneratorKernel, NoiseMap, _noiseBuffer);
        //
        //     // Berechnen Sie die Anzahl der Gruppen für die Dispatch-Aufrufe
        //     int dispatchX = Mathf.CeilToInt(resolution.x / 8.0f);
        //     int dispatchY = Mathf.CeilToInt(resolution.y / 8.0f);
        //
        //     // Führen Sie den Kernel "MeshGenerator" aus
        //     computeShader.Dispatch(meshGeneratorKernel, dispatchX, dispatchY, 1);
        // }

        #endregion
    }
}