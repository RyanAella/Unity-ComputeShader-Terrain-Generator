/*
 * Author: Rebecca Biebl
 * Creation Date: 22-05-2024
 * Description: Generates a grid of points and connects them with lines based on the specified parameters.
 * License: Licence
 */

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Scripts.Helpers.Documentation
{
    public class GridGenerator : MonoBehaviour
    {
        public int rows = 5; // Number of rows
        public int columns = 5; // Number of columns
        public float spacing = 1.0f; // Spacing between points
        public GameObject pointPrefab; // Prefab for points (optional)
        public LineRenderer linePrefab; // Prefab for lines
        public TMP_Text textPrefab; // Prefab for labels (TextMeshPro)
        public Canvas worldCanvas; // Reference to the World Space Canvas

        private void Start()
        {
            GenerateGrid(); // Call method to generate grid
        }

        private void GenerateGrid()
        {
            rows += 1; // Increment rows
            columns += 1; // Increment columns
            
            int pointIndex = 0; // Initialize point index
            List<Vector3> points = new List<Vector3>(); // List to store points

            // Generate points and labels
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    // Calculate position for each point
                    Vector3 position = new Vector3(x * spacing, y * spacing, 0);
                    points.Add(position); // Add position to list

                    // Instantiate point prefab if provided
                    if (pointPrefab != null)
                    {
                        Instantiate(pointPrefab, position, Quaternion.identity, transform);
                    }

                    // Add label using TextMeshPro if prefab and canvas are provided
                    if (textPrefab != null && worldCanvas != null)
                    {
                        TMP_Text number = Instantiate(textPrefab, worldCanvas.transform);
                        number.text = pointIndex.ToString();
                        number.alignment = TextAlignmentOptions.Center;
                        Vector3 numberPos = new Vector3(position.x, position.y, position.z - 1f);
                        number.rectTransform.localPosition = numberPos;
                        number.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    }

                    pointIndex++; // Increment point index
                }
            }

            // Connect points with lines
            for (int i = 0; i < points.Count; i++)
            {
                if (i + 1 < points.Count && (i + 1) % columns != 0)
                {
                    DrawLine(points[i], points[i + 1]); // Draw horizontal line
                }

                if (i + columns < points.Count)
                {
                    DrawLine(points[i], points[i + columns]); // Draw vertical line
                }
            }
        }

        void DrawLine(Vector3 start, Vector3 end)
        {
            if (linePrefab == null) return;
            
            LineRenderer line = Instantiate(linePrefab, transform);
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
    }
}
