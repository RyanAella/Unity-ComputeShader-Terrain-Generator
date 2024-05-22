/*
 * Author: Rebecca Biebl
 * Creation Date: 22-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Scripts.Helpers.Documentation
{
    public class GridGenerator : MonoBehaviour
    {
        public int rows = 5; // Anzahl der Zeilen
        public int columns = 5; // Anzahl der Spalten
        public float spacing = 1.0f; // Abstand zwischen den Punkten
        public GameObject pointPrefab; // Prefab für die Punkte (optional, z.B. eine kleine Kugel oder ein leeres GameObject)
        public LineRenderer linePrefab; // Prefab für die Linien
        public TMP_Text textPrefab; // Prefab für die Beschriftung (TextMeshPro)
        public Canvas worldCanvas; // Reference to the World Space Canvas

        private void Start()
        {
            GenerateGrid();
        }

        void GenerateGrid()
        {
            rows += 1;
            columns += 1;
            
            int pointIndex = 0;
            List<Vector3> points = new List<Vector3>();

            // Erzeugen der Punkte und Nummerierung
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Vector3 position = new Vector3(x * spacing, y * spacing, 0);
                    points.Add(position);

                    // Punkt erzeugen
                    if (pointPrefab != null)
                    {
                        Instantiate(pointPrefab, position, Quaternion.identity, transform);
                    }

                    // Nummerierung hinzufügen
                    if (textPrefab != null && worldCanvas != null)
                    {
                        TMP_Text number = Instantiate(textPrefab, worldCanvas.transform);
                        number.text = pointIndex.ToString();
                        number.alignment = TextAlignmentOptions.Center;
                        // number.fontSize = 0.5f; // Stellen Sie sicher, dass die Schriftgröße angemessen ist
                        Vector3 numberPos = new Vector3(position.x, position.y, position.z - 1f);
                        number.rectTransform.localPosition = numberPos; // Setzen Sie die Position des Textes
                        number.rectTransform.localScale = new Vector3(1f, 1f, 1f); // Skalieren Sie den Text
                    }

                    pointIndex++;
                }
            }

            // Verbinden der Punkte mit Linien
            for (int i = 0; i < points.Count; i++)
            {
                if (i + 1 < points.Count && (i + 1) % columns != 0)
                {
                    DrawLine(points[i], points[i + 1]);
                }

                if (i + columns < points.Count)
                {
                    DrawLine(points[i], points[i + columns]);
                }
            }
        }

        void DrawLine(Vector3 start, Vector3 end)
        {
            if (linePrefab != null)
            {
                LineRenderer line = Instantiate(linePrefab, transform);
                line.positionCount = 2;
                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        }
    }
}
