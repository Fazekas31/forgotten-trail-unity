# Ash Creek Asset Store integration

The local Ash Creek scene can use these packages from Unity's `Asset Store > My Assets`:

- `Unity Terrain - URP Demo Scene` — distant terrain, SpeedTree prototypes and terrain materials.
- `EasyRoads3D Free v3` — road material, road shaders and road authoring tools.

The packages are intentionally not committed: the terrain package is several gigabytes and is
already available in the project's Unity Asset Store cache. The runtime checks for these assets
and falls back to the authored Blender architecture and procedural road if a package is not
imported on another machine.

The main architectural landmarks remain in `Resources/Environment/AshCreek_Architecture.glb`.
Asset Store content is used for environmental dressing and surface language, not as a replacement
for the story locations (saloon, church, station and barn route).
