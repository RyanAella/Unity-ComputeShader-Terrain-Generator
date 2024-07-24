/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: Class for managing mesh generation.
 * License: MIT Licence
 */

using _Scripts.Helpers;
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
        /// <param name="meshFilter">The MeshFilter component to apply the mesh to.</param>
        public void CreateMesh(MeshFilter meshFilter, string name, Vector3[] vertices, Vector2[] uvs, Vector3[] normals,
            int[] triangles)
        {
            // Create and configure a new Mesh.
            Mesh mesh = new Mesh
            {
                name = name, // Set the name of the mesh.
                indexFormat = IndexFormat.UInt32 // Use 32-bit indices to support larger meshes.
            };

            mesh.Clear(); // Clear previous mesh data.

            mesh.vertices = vertices; // Set vertex positions.
            mesh.uv = uvs; // Set UV coordinates.
            mesh.normals = CalculateNormals(vertices, triangles); // Compute normals for proper lighting.
            mesh.triangles = triangles; // Define triangle indices.

            mesh.RecalculateBounds(); // Recalculate the bounds of the mesh.
            mesh.RecalculateTangents(); // Recalculate tangents for better lighting effects.

            meshFilter.mesh = meshFilter.sharedMesh = mesh; // Assign the mesh to the MeshFilter.
        }

        /// <summary>
        ///     Calculates the normals for the mesh based on the vertices and triangles.
        /// </summary>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        /// <returns>An array of Vector3 representing the normals of the mesh.</returns>
        private Vector3[] CalculateNormals(Vector3[] vertices, int[] triangles)
        {
            Vector3[] normals = new Vector3[vertices.Length];

            // Calculate the normals for each triangle.
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int index0 = triangles[i];
                int index1 = triangles[i + 1];
                int index2 = triangles[i + 2];

                // Compute the normal of the triangle using helper method.
                Vector3 normal = SurfaceNormalFromIndices(index0, index1, index2, vertices);

                // Add the normal to the corresponding vertices.
                normals[index0] += normal;
                normals[index1] += normal;
                normals[index2] += normal;
            }

            // Normalize the normals.
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].normalized;
            }

            return normals;
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