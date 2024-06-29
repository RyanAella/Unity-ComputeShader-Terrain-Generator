# ToDo

- Normals auf GPU berechnen
- Kommentare vereinheitlichen

- FBM implementieren


Method Parameters:

(H) Hurst index - In mathematical literature, classifies the fBm and dictates fractal dimension.

(Lacunarity) - Dictates the gap between successive frequencies.

(Octaves) - Dictates the number of frequencies and scales Level of Detail in the scene.

(Offset) - Offset from the lowest elevation and determines "multifractality".

(Gain) - Controls the amplitude of the frequency.

Iterative application of fbm to the pixel coordinate p to generate a complicated pattern is called ‘Domain Warping’.