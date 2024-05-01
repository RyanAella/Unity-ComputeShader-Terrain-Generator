# Changelog Unity Compute Shader Terrain Generator


## [0.4.0] - 2024-05-01

## Added

- Compare vertices and get min and max height value (On GPU)


## [0.3.0] - 2024-04-30

## Added
- NoiseSettings.cs
  - noise_scale
  - octaves
  - Persistence
  - Lacunarity


## [0.2.0] - 2024-04-29

## Added
- Code Comments

## Changed
- Vertex and Triangle generation now runs on the GPU
- Code Refactoring and Cleanup


## [0.1.0] - 2024-04-26

## Added
- OpenSimplexNoise.cs from https://gist.github.com/digitalshadow/134a3a02b67cecd72181
- GenerationManagerComputeShader.cs - mesh generation with Compute Shader
- ComputeShaderManager.cs
- MeshGenerator.cs
- MeshGenerator.compute
- URP Integration

## Changed
- GenerationManager.cs -> GenerationManagerCPU.cs


## [0.0.0] - 2024-04-17

## Added
- Git repository
- .gitignore
- README.md
- Gantt diagram
- Changelog.md
