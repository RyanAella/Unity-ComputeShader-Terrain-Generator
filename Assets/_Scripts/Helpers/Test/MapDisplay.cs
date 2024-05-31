/*
 * Author: Rebecca Biebl
 * Creation Date: 27-05-2024
 * Description: This script is responsible for displaying and updating a map using a Texture2D.
 * License: Licence
 */

using UnityEngine;

namespace _Scripts.Helpers.Test
{
    /*
     * This class displays the map.
     */
    public class MapDisplay
    {
        private static Texture2D _texture; // The texture used to display the map

        // Constructor
        /// <summary>
        /// Initializes a new instance of the MapDisplay class, creating a texture and applying it to a SpriteRenderer.
        /// </summary>
        /// <param name="resolution">Resolution of the texture to be created.</param>
        /// <param name="root">The GameObject to which the texture will be applied.</param>
        public MapDisplay(Vector2Int resolution, GameObject root)
        {
            // Create texture and rect for Sprite
            _texture = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                // FilterMode.Point to get checkerboard pattern
                filterMode = FilterMode.Point
            };

            var rect = new Rect(0, 0, resolution.x, resolution.y);

            // Set all pixels of the texture to magenta
            for (int x = 0; x < resolution.x; x++)
            {
                for (int y = 0; y < resolution.y; y++)
                {
                    _texture.SetPixel(x, y, Color.magenta);
                }
            }

            // Apply color changes to the texture
            _texture.Apply();

            // Create Sprite from the texture
            var sprite = Sprite.Create(_texture, rect, new Vector2(0.5f, 0.5f));

            // Add Sprite to SpriteRenderer on the root GameObject
            var renderer = root.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;

            // If no SpriteRenderer is found, add one
            if (renderer == null)
            {
                renderer = root.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            }

            // If renderer is still null, return
            if (renderer == null) return;

            // Assign the created sprite to the renderer and enable it
            renderer.sprite = sprite;
            renderer.enabled = true;
        }

        /*
         * Update the map
         */
        /// <summary>
        /// Updates the map display with new values from the value map.
        /// </summary>
        /// <param name="valueMap">A 2D array of float values used to update the texture.</param>
        public void UpdateMapDisplay(float[,] valueMap)
        {
            int mapWidth = valueMap.GetLength(0);
            int mapHeight = valueMap.GetLength(1);

            // Check the dimensions of valueMap and Sprite
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    // valueMap contains float values between 0.0 and 1.0
                    float value = Mathf.Clamp(valueMap[x, y], 0.0f, 1.0f); // Ensure values are within the range [0, 1]
                    Color color = Color.Lerp(Color.white, Color.black, value); // Interpolate between white and black based on value
                    _texture.SetPixel(x, y, color);
                }
            }

            // Apply the updated pixel colors to the texture
            _texture.Apply();
        }
    }
}
