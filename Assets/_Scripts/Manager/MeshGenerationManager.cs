/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: Class for managing mesh generation.
 * License: MIT Licence
 */

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
        /// <param name="name">The name of the mesh.</param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="uvs">An array of Vector2 that defines the UVs of the mesh.</param>
        /// <param name="normals">An array of Vector3 that defines the normals of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        /// <param name="managers">The terrain generation managers.</param>
        /// <param name="shaders">The shaders used.</param>
        /// <param name="settings">The terrain settings.</param>
        /// <param name="meshFilter">The MeshFilter component to apply the mesh to.</param>
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

            // mesh.normals = CalculateNormals(managers, shaders, settings, vertices, triangles);
            
            // Sets the triangles of the mesh.
            mesh.triangles = triangles;

            // Recalculates the normals of the mesh based on the vertices and triangles.
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.mesh = meshFilter.sharedMesh = mesh;
        }

        /// <summary>
        ///     Calculates the normals for the mesh based on the vertices and triangles.
        /// </summary>
        /// <param name="managers">The terrain generation managers.</param>
        /// <param name="shaders">The shaders used.</param>
        /// <param name="settings">The terrain settings.</param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        /// <returns>An array of Vector3 representing the normals of the mesh.</returns>
        private Vector3[] CalculateNormals(TerrainGenerationManagers managers, Shaders shaders,
            TerrainSettings settings, Vector3[] vertices, int[] triangles)
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;

            // Compute normals for each vertex based on triangles.
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

            // Normalize the vertex normals.
            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals;
        }

        /// <summary>
        /// Calculates the surface normal vector for a triangle defined by three vertices.
        /// </summary>
        /// <param name="indexA">The index of the first vertex.</param>
        /// <param name="indexB">The index of the second vertex.</param>
        /// <param name="indexC">The index of the third vertex.</param>
        /// <param name="vertices">Array of vertices representing the mesh geometry.</param>
        /// <returns>The normalized surface normal vector for the triangle.</returns>
        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC, Vector3[] vertices)
        {
            // Get the three points of the triangle from the vertices array.
            Vector3 pointA = vertices[indexA];
            Vector3 pointB = vertices[indexB];
            Vector3 pointC = vertices[indexC];

            // Calculate the two sides of the triangle.
            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            // Calculate and return the normalized surface normal vector.
            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        #endregion
    }
}