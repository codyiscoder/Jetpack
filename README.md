# Jetpack Mod

A simple jetpack mod for Gorilla Tag that lets you boost yourself upward using your controllers. The mod works by applying force based on your head direction, allowing you to move around more freely and add a bit of extra movement to your gameplay.

## Installation

1. Make sure you have **BepInEx** installed for Gorilla Tag.
2. Download the latest Jetpack mod release.
3. Place the Jetpack plugin file into your:

```
Gorilla Tag/BepInEx/plugins
```

folder.

4. Launch Gorilla Tag.

The mod will automatically load when the game starts.

## How to Use

1. Join a **MODDED** room.
2. Hold the trigger input to activate the jetpack. (if you hold both simultaneously)
3. Use your head movement to control the direction of the boost.
4. Hold both controllers at the same time for a stronger boost.

The jetpack only works while the mod is enabled and you are in a compatible modded lobby.

## Controls
Left or right controller input to activate jetpack 
Both controllers = Increased thrust
Head movement controls boost direction

## Configuration

The mod creates a configuration file where you can adjust settings such as:

- **Force** - Controls how strong the jetpack boost is.
- **Volume** - Controls the jetpack sound volume.

The config file can be found in:

```
BepInEx/config
```

Edit the values, save the file, and restart Gorilla Tag for changes to apply.

## Notes

- The jetpack is intended for use in modded rooms only.
- The mod automatically disables itself in non-modded rooms.
- If the mod is disabled or removed while playing, it will clean up any active effects and reset your rotation.
