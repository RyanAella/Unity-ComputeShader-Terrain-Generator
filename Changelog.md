# Changelog Unity Compute Shader Terrain Generator

## [0.19.0] - 2024-06-27
### Added
- Introduced `BaseGenerator` class, serving as a base class for both `GroundGenerator` and `WaterGenerator`.

### Changed
- `ReleaseBuffers()` method is now exclusively within `ComputeBufferManager`.

### Removed
- Removed `WaterNoiseSettings` from the project.

## [0.18.0] - 2024-06-15
### Added
- Implemented `ComputeBufferManager` to optimize buffer usage by avoiding unnecessary recreations.
  This enhancement improves performance by reusing buffers for frequently accessed data.
- Enhanced `ColourGeneration` with a `smooth` option for smoother color transitions across terrain features.
- Introduced `ColourGradient` as a `ScriptableObject` to facilitate flexible and dynamic color management.
- Added `Water.shadergraph` to enhance water shader functionality for realistic water rendering.

### Fixed
- Addressed UV generation issue, ensuring accurate texture mapping now processed on the CPU.

### Code Cleanup
- Improved code efficiency through refactoring for enhanced maintainability and performance optimizations.

## [0.17.0] - 2024-06-11
### Added
- Implemented UV generation.
- Introduced `Billow Noise` and `Ridge Noise` functionalities.
- Made `Domain Warping` and `FBM` dynamic.

### Changed
- Aligned water mesh noise settings with ground mesh.
- Consolidated all settings into `TerrainSettings`.
- Restructured all managers under `TerrainGenerationManagers`.
- Replaced `colourGradient` with `TerrainColour` array containing `Height` and `Colour`, allowing for the use of an arbitrary number of colors.

### Fixed
- Rectified incorrect calculation of domain warping offset vectors.
- Addressed issues with colour assignment.
- Conducted general code cleanups and minor bug fixes to improve stability and performance.

- Tested various colour variations to achieve a more realistic appearance.

## [0.16.0] - 2024-06-03
### Added
- Implemented the ability to use a `falloffMap` within the Compute Shader, enhancing terrain edge smoothness and realism.

### Changed
- `Parameter Optimization`: Reduced unnecessary parameter passing in various methods to streamline the code and improve performance.
- `Method Refinement`: Shortened and optimized multiple methods for better readability and maintainability.
- `Noise Generation Overhaul`: Refined the noise generation process to improve terrain detail and variety, ensuring a more visually appealing and diverse landscape.

### Fixed
- General code cleanups and minor bug fixes to enhance stability and performance.

## [0.15.0] - 2024-05-29
### Added
- `GeneralSettings`: Introduced new settings including `resolution` and `colourGradient`.
- `ShaderSettings`: Added settings for all shaders used in the project.
- `ColourGenerationManager`: Moved the `GetColourPalette` method from `GameManager` to `ColourGenerationManager`.

### Changed
- `GameManager`: Refactored code for better organization and clarity.
- `GroundGenerator` and `WaterGenerator`: Code refactorings and cleanups for improved performance and readability.

### Fixed
- Various minor bug fixes and stability improvements across the scripts.

## [0.14.0] - 2024-05-24
### Changed
- Various files relocated for better organization and documentation purposes.
- `NoiseGeneration.compute`: Currently undergoing testing of noise functions.
- `ColourGeneration.compute`: Alpha values are now handled separately.
- `ValueClamp.compute`: Renamed, unnecessary code deleted.
- `GridGenerator`: Newly added, generates grid for documentation purposes.
- `ColourGenerationManager`: Cleanup and optimization.
- `GameManager`: Added enum `NoiseType`. Introduced separate `NoiseSettings` for water and ground. Separate `ColourGradients` for water and ground. Alpha keys are now handled separately.
- `NoiseGenerationManager`: Newly added.
- `GroundGenerator` / `WaterGenerator`: Refactorings and optimizations.

## [0.13.0] - 2024-05-22
### Added
- Created prefabs for ground and water objects in the scene.
- Updated the terrain mesh generation to ensure compatibility with the new prefabs.
- Added a new material for water: `WaterMaterial.mat`.
- Implemented water generation functionality.

### Changed
- Renamed materials from `MapMaterial.mat` to `GroundMaterial.mat` for clarity.
- Relocated some assets for better organization and documentation.
- Adjusted the compute shaders for noise generation (`NoiseGeneration.compute`) to improve performance and stability.

## [0.12.0] - 2024-05-19
### Changed
- Revised and cleaned up all scripts to improve efficiency and clarity.

### Added
- Added comments to the code for better readability and maintainability.

### Fixed
- Color generation for the terrain now runs with the Compute Shader again.

## [0.11.0] - 2024-05-17
### Added
- `AdjustMeshHeight` method to ensure the mesh displays visible height differences.

### Changed
- Moved some parameters from `GameManager` to `NoiseSettings`.

### Fixed
- Resolved an issue where some parts of the mesh were not coloured due to y-values falling outside the 0-1 range.

## [0.10.1] - 2024-05-13
### Changed
- Added code comments for improved readability and maintainability.
- Conducted code cleanup and organization for better clarity.
- Introduced a new function to ensure that the y-values are within the correct range for colours.
- Relocated the mesh colouring process to the CPU to facilitate debugging.

## [0.9.0] - 2024-05-10
- Testing

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
