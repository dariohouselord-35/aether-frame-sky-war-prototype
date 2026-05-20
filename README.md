# AETHER FRAME: Sky War Prototype

Original Unity 6 third-person mecha flight combat prototype using only primitive placeholder geometry and built-in Unity systems.

## Setup

1. Open this folder as a Unity 6 project.
2. Unity will auto-generate `Assets/Scenes/SkyArenaPrototype.unity` the first time the editor imports the project.
3. If the scene is not created automatically, run `AETHER FRAME > Build SkyArenaPrototype Scene` from the Unity menu.
4. Open `Assets/Scenes/SkyArenaPrototype.unity` and press Play.

Unity was not discoverable from the local command line during scaffolding, so final compile/play validation should be done inside Unity 6.

## Controls

- `WASD`: Fly forward/back/strafe.
- Mouse: Aim and rotate the mecha.
- `Space`: Ascend.
- `Left Ctrl`: Descend.
- `Left Shift`: Boost while moving. Boost drains Aether Energy, which regenerates when not boosting.
- Left mouse: Fire laser projectiles.
- Right mouse: Fire homing missile.
- `Tab`: Cycle lock-on targets.

## Prototype Loop

Destroy the enemy drones in the arena while avoiding their slow projectiles. The HUD shows player health, Aether Energy, current lock target, remaining drones, and a basic crosshair. When player health reaches zero, the scene restarts.

## Implementation Notes

- All gameplay scripts live under `Assets/Scripts/`.
- The scene builder uses primitive cubes, spheres, capsules, and cylinders for the mecha, drones, arena platforms, towers, projectiles, and muzzle flashes.
- Tuning values are exposed in the Inspector for flight, camera, weapons, homing missiles, enemy AI, health, and HUD references.
- No external assets, music, logos, characters, or third-party mecha designs are used.
