/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */

using System;
using System.Globalization;
using _Scripts.Helper;
using _Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts
{
    struct MeshElement
    {
        public Vector3 vertex; // Vertex Position
        public Vector3Int triangle; // Triangle Indices
    }
    
    /// <summary>
    ///     Class for managing mesh generation.
    /// </summary>
    [Serializable]
    public class MeshGenerationManager
    {
        #region Variables

        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _trianglesBuffer;

        // Shader property IDs
        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");
        private static readonly int SeedOffset = Shader.PropertyToID("seed_offset");
        private static readonly int NoiseScale = Shader.PropertyToID("noise_scale");
        private static readonly int NoiseHeight = Shader.PropertyToID("noise_height");
        private static readonly int Octaves = Shader.PropertyToID("octaves");
        private static readonly int Lacunarity = Shader.PropertyToID("lacunarity");
        private static readonly int Persistence = Shader.PropertyToID("persistence");
        private static readonly int VertexBuffer = Shader.PropertyToID("_Vertex_Buffer");
        private static readonly int TriangleBuffer = Shader.PropertyToID("_Triangle_Buffer");

        // Private Mesh object used for storing generated mesh data.
        private Mesh _mesh;
        private static readonly int MaxTerrainHeight = Shader.PropertyToID("max_terrain_height");

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the compute buffers based on the given resolution.
        /// </summary>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        public void InitializeBuffers(Vector2Int resolution)
        {
            // Allocate memory for the noise map buffer.
            _verticesBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float) * 3);
            // Allocate memory for the triangles buffer.
            _trianglesBuffer = new ComputeBuffer((resolution.x - 1) * (resolution.y - 1) * 6, sizeof(int));
        }

        /// <summary>
        ///     Releases the compute buffers when they are no longer needed.
        /// </summary>
        public void ReleaseBuffers()
        {
            _verticesBuffer.Release();
            _trianglesBuffer.Release();
        }

        /// <summary>
        ///     Generates mesh parameters (vertices and triangles) using a compute shader.
        /// </summary>
        /// <param name="computeShader">The compute shader to use for generating the mesh.</param>
        /// <param name="resolution">The resolution of the mesh to be generated.</param>
        /// <param name="noiseSettings"></param>
        /// <param name="vertices">Array to store the generated vertices.</param>
        /// <param name="triangles">Array to store the generated triangles.</param>
        public float[] GenerateMeshParameters(ComputeShader computeShader, Vector2Int resolution,
            NoiseSettings noiseSettings, Vector3[] vertices, int[] triangles)
        {
            // Ensure the noise scale is not too low to avoid a flat mesh
            noiseSettings.noiseScale = Mathf.Max(0.1f, noiseSettings.noiseScale);
        
            // Check if a random seed is wanted
            if (noiseSettings.useRandomSeed)
                noiseSettings.SetSeed(Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture));
        
            // Get the coordinates
            float seedOffset = noiseSettings.GetSeed().GetHashCode() / noiseSettings.seedScale;
        
            // Find the kernel in the compute shader.
            int noiseKernel = computeShader.FindKernel("Noise_Generator");
        
            // Set shader properties for map dimensions.
            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);
        
            computeShader.SetFloat(SeedOffset, seedOffset);
            computeShader.SetFloat(NoiseScale, noiseSettings.noiseScale);
            computeShader.SetFloat(NoiseHeight, noiseSettings.noiseHeight);
        
            computeShader.SetInt(Octaves, noiseSettings.octaves);
            computeShader.SetFloat(Lacunarity, noiseSettings.lacunarity);
            computeShader.SetFloat(Persistence, noiseSettings.persistence);
            
            computeShader.SetFloat(MaxTerrainHeight, noiseSettings.maxTerrainHeight);
        
            // Set the compute buffers for the vertices and triangles.
            computeShader.SetBuffer(noiseKernel, VertexBuffer, _verticesBuffer);
            computeShader.SetBuffer(noiseKernel, TriangleBuffer, _trianglesBuffer);
        
            // Calculate the number of thread groups to dispatch.
            int dispatchX = Mathf.CeilToInt(resolution.x / 16f);
            int dispatchY = Mathf.CeilToInt(resolution.y / 16f);
        
            // Dispatch the compute shader to generate the mesh parameters.
            computeShader.Dispatch(noiseKernel, dispatchX, dispatchY, 1);
        
            // Retrieve the generated vertices and triangles from the compute buffers.
            _verticesBuffer.GetData(vertices);
            _trianglesBuffer.GetData(triangles);
        
            float[] minMax = GeneratorFunctions.CompareHeightValues(resolution, computeShader, _verticesBuffer, vertices);
        
            // ClampHeightValues(computeShader, resolution, vertices, minMax);
            //
            // float[] clampedMinMax = CompareHeightValues(resolution, computeShader, vertices);
            
            ReleaseBuffers();
        
            return minMax;
        }


        /// <summary>
        ///     Creates a new mesh based on the given vertices and triangles.
        /// </summary>
        /// <param name="filter">The MeshFilter that uses the mesh.</param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        public Mesh CreateMesh(MeshFilter filter, Vector3[] vertices, int[] triangles)
        {
            // Creates a new Mesh object.
            // Sets the mesh of the MeshFilter to the newly created mesh.
            filter.mesh = _mesh = new Mesh
            {
                // Sets the index format of the mesh to UInt32, which is required for large meshes.
                indexFormat = IndexFormat.UInt32,
                name = "Procedural Mesh GPU"
            };

            filter.sharedMesh = _mesh;

            // Clears all previous data in the mesh.
            _mesh.Clear();
            // Sets the vertices of the mesh.
            _mesh.vertices = vertices;
            // Sets the triangles of the mesh.
            _mesh.triangles = triangles;

            // Recalculates the normals of the mesh based on the vertices and triangles.
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            return _mesh;
        }

        #endregion
    }
}