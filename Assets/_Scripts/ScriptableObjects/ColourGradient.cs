/*
 * Author: Rebecca Biebl
 * Creation Date: 14-06-2024
 * Description: ScriptableObject representing a Colour Gradient for terrain
 * License: MIT Licence
 */

using _Scripts.Helpers;
using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing a Colour Gradient for terrain
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/ColourGradient")]
    public class ColourGradient : ScriptableObject
    {
        #region Variables
        
        [Tooltip("The colours of the mesh.")]
        public TerrainColour[] colours; // Colours of the terrain

        #endregion
    }
}

//Zusammenfassung der Farbverteilung:
//            0%: Schwarz (0, 32, 64) #000000
//       0% - 5%: Tiefes Dunkelblau (0, 32, 64) #002040
//      5% - 10%: Mittleres Dunkelblau (0, 64, 128) #004080
//     10% - 15%: Helles Blau (51, 102, 153) #336699
//     15% - 20%: Sehr Helles Blau (102, 153, 204) #6699CC
//     20% - 25%: Sanfter Übergang von Blau zu Grün (102, 153, 153) #669999
//     25% - 30%: Helleres Grün (128, 179, 128) #80B380
//     30% - 35%: Mittleres Grün (102, 153, 102) #669966
//     35% - 40%: Dunkleres Grün (77, 128, 77) #4D804D
//     40% - 50%: Dunkleres Grünbraun (64, 102, 64) #406640
//     50% - 55%: Helleres Grünbraun (89, 134, 89) #598659
//     55% - 60%: Helleres Grau (153, 153, 153) #999999
//     60% - 65%: Mittleres Grau (102, 102, 102) #666666
//     65% - 70%: Helleres Grau (153, 153, 153) #999999
//     70% - 75%: Mittleres Grau (102, 102, 102) #666666
//     75% - 80%: Dunkleres Grau (51, 51, 51) #333333
//     80% - 85%: Sehr Dunkles Grau (34, 34, 34) #222222
//     85% - 100%: Weiß (255, 255, 255) #FFFFFF
