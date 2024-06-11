/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


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
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        /// <param name="meshFilter"></param>
        public void CreateMesh(MeshFilter meshFilter, string name, Vector3[] vertices, Vector2[] uvs, int[] triangles)
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
            mesh.SetVertices(vertices);
                        
            // mesh.SetUVs(0, uvs);

            mesh.triangles = triangles;
            // Sets the triangles of the mesh.
            // mesh.SetTriangles(triangles, 0);

            // Recalculates the normals of the mesh based on the vertices and triangles.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;
        }

        #endregion
    }
}