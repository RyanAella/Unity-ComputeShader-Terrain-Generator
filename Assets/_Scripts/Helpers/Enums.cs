/*
 * Author: Rebecca Biebl
 * Creation Date: 05-06-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Helpers
{
    public enum NoiseType
    {
        SimplexNoise = 0,
        FractionalBrownianMotion = 1,
        DomainWarping = 2,
        BillowNoise = 3,
        RidgeNoise = 4,
        Noiseless = 5
    }

    public enum NoiseLayer
    {
        Regular = 0,
        Billow = 1,
        Ridge = 2,
    }
}
