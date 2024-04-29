/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A script for managing mesh generation in Unity.
 * License: Licence
 */

using System;
using UnityEngine;

namespace _Scripts
{
    /// <summary>
    /// Class for managing mesh generation.
    /// </summary>
    [Serializable]
    public class MeshGenerationManager
    {
        #region Variables

        // Private Mesh object used for storing generated mesh data.
        private Mesh _mesh;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new mesh based on the given vertices and triangles.
        /// </summary>
        /// <param name="filter">The MeshFilter that uses the mesh.</param>
        /// <param name="vertices">An array of Vector3 that defines the vertices of the mesh.</param>
        /// <param name="triangles">An array of int that defines the indices of the vertices forming the triangles of the mesh.</param>
        public void CreateMesh(MeshFilter filter, Vector3[] vertices, int[] triangles)
        {
            // Creates a new Mesh object.
            _mesh = new Mesh();
            // Sets the mesh of the MeshFilter to the newly created mesh.
            filter.sharedMesh = _mesh;
            // Sets the index format of the mesh to UInt32, which is required for large meshes.
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            // Clears all previous data in the mesh.
            _mesh.Clear();
            // Sets the vertices of the mesh.
            _mesh.vertices = vertices;
            // Sets the triangles of the mesh.
            _mesh.triangles = triangles;

            // Recalculates the normals of the mesh based on the vertices and triangles.
            _mesh.RecalculateNormals();
        }

        #endregion
    }
}