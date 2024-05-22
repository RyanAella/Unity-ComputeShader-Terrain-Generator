/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */


using System;
using UnityEngine;

namespace _Scripts.Manager
{
    /// <summary>
    ///     Class for managing mesh generation.
    /// </summary>
    [Serializable]
    public class MeshGenerationManager
    {
        #region Variables

        // Compute buffers for storing vertex and triangle data.

        // Shader property IDs used for communication with compute shaders.

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a new mesh based on the given vertices and triangles.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        public void CreateMesh(Mesh mesh, Vector3[] vertices, int[] triangles)
        {
            // Clears all previous data in the mesh.
            mesh.Clear();

            // Sets the vertices of the mesh.
            mesh.SetVertices(vertices);

            // Sets the triangles of the mesh.
            mesh.SetTriangles(triangles, 0);

            // Recalculates the normals of the mesh based on the vertices and triangles.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
        }

        #endregion
    }
}