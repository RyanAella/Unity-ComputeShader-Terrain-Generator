/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


using System;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Manager
{
    /// <summary>
    ///     Class for managing mesh generation.
    /// </summary>
    public class MeshGenerationManager
    {
        #region Methods

        /// <summary>
        ///     Creates a new mesh based on the given vertices and triangles.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="uvs"></param>
        /// <param name="normals"></param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        /// <param name="managers"></param>
        /// <param name="shaders"></param>
        /// <param name="settings"></param>
        /// <param name="meshFilter"></param>
        public void CreateMesh(TerrainGenerationManagers managers, Shaders shaders, TerrainSettings settings,
            MeshFilter meshFilter, string name, Vector3[] vertices, Vector2[] uvs, Vector3[] normals, int[] triangles)
        {
            // Creates a new Mesh object.
            // Sets the mesh of the MeshFilter to the newly created mesh.
            Mesh mesh = new Mesh
            {
                name = name,
                indexFormat = IndexFormat.UInt32,
            };

            // Clears all previous data in the mesh.
            mesh.Clear();

            // Sets the vertices of the mesh.
            mesh.vertices = vertices;

            // Sets the uvs of the mesh.
            mesh.uv = uvs;

            // Sets the normals of the mesh.
            // mesh.normals = normals;

            mesh.normals = CalculateNormals(managers, shaders, settings, vertices, triangles);

            // Sets the triangles of the mesh.
            mesh.triangles = triangles;

            // Recalculates the normals of the mesh based on the vertices and triangles.
            mesh.RecalculateBounds();

            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="shaders"></param>
        /// <param name="settings"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private Vector3[] CalculateNormals(TerrainGenerationManagers managers, Shaders shaders,
            TerrainSettings settings, Vector3[] vertices, int[] triangles)
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;

            // Vector2Int resolution = settings.GeneralSettings.resolution;
            //
            // ComputeShader computeShader = shaders.meshGenerationComputeShader;
            //
            // computeShader.FindKernel("Mesh_Generation");

            // computeShader.SetInt("triangle_count",triangleCount);
            //
            // ComputeBuffer _trianglesBuffer = new ComputeBuffer((resolution.x - 1) * (resolution.y - 1) * 6, sizeof(int))
            // computeShader.SetBuffer("_Triangle_Buffer", );

            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;

                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC, vertices);

                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals;
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC, Vector3[] vertices)
        {
            Vector3 pointA = vertices[indexA];
            Vector3 pointB = vertices[indexB];
            Vector3 pointC = vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        #endregion
    }
}