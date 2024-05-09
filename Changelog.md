# Changelog Unity Compute Shader Terrain Generator

## [0.9.0] - 2024-05-09
### Removed
- Editor script for controlling the mesh generation process through the Unity Inspector.

## [0.8.0] - 2024-05-07
### Added
- Editor script for controlling the mesh generation process through the Unity Inspector, 
  allowing updates without needing to enter Play mode.

## [0.7.0] - 2024-05-05
### Added
- New script `GeneratorFunctions.cs` for helper functions

### Changed
- The content of `ComputeShaderManager.cs` has been integrated into `MeshGenerationManager.cs`.
- The functions `CompareHeightValues()` and `ClampHeightValues()` have been moved to `GeneratorFunctions.cs`.
- Several scripts have been renamed for clarity and consistency.
- Colour coding is now dynamic and uses Gradient.

## [0.6.0] - 2024-05-03
### Changed
- Colour coding updated with four distinct colours.
- For more details, check out this [video](https://youtu.be/Qzao8N6YlFs).

## [0.5.0] - 2024-05-02
### Added
- Height clamping (on the GPU) to ensure consistent terrain.
- Colour coding based on height, with linear interpolation between black and white.

## [0.4.0] - 2024-05-01
### Added
- Added a function to compare vertices and determine minimum and maximum height values (on the GPU).

## [0.3.0] - 2024-04-30
### Added
- New script `NoiseSettings.cs` with customizable parameters:
  - `noise_scale`, `octaves`, `persistence`, and `lacunarity`.

## [0.2.0] - 2024-04-29
### Added
- Code comments to improve readability and maintainability.
### Changed
- Vertex and triangle generation now runs on the GPU.
- Refactoring and code cleanup for better performance.

## [0.1.0] - 2024-04-26
### Added
- Added `OpenSimplexNoise.cs` from [source](https://gist.github.com/digitalshadow/134a3a02b67cecd72181).
- New script for mesh generation with Compute Shader.
- Additional scripts, including `GenerationManagerComputeShader`, `ComputeShaderManager.cs`, `MeshGenerator.cs`, and `MeshGenerator.compute`.
- URP integration for better rendering support.

### Changed
- `GenerationManager.cs` renamed to `GenerationManagerCPU.cs` for clarity.

## [0.0.0] - 2024-04-17
### Added
- Git repository setup with initial configurations.
- `.gitignore`, `README.md`, Gantt diagram, and `Changelog.md`.
