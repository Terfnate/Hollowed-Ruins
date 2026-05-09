# HOLLOWED RUINS

**Group 13 Player Manual**

`Hollowed Ruins` is a single-player third-person horror maze game with a chess duel mechanic. The player explores a haunted ruin, collects broken key pieces, survives the Warden's pursuit, and escapes through the final portal.

## Unity Version

`Unity 6000.3.9f1 (Unity 6)`

## Controls

| Input | Action |
|---|---|
| `W / A / S / D` | Move |
| `Mouse` | Look around |
| `Left Shift` | Sprint |
| `Left Click` | Select and move pieces during chess duels |
| `Esc` | Pause / open in-game menu |
| `R` | Restart the current level |
| `Q` | Dash, final-heart mode only |
| `Space` | Jump, final-heart mode only |

## Game Objectives

- Explore the maze and collect all key pieces
- Reveal the exit portal after all key pieces are collected
- Reach the portal before losing all hearts

## Win / Lose Conditions

**Win**
- Collect all key pieces
- Enter the portal

**Lose**
- Lose all 3 hearts
- Hearts are lost by failing chess duels against the Warden

## Special Mechanics

- **Chess Duel System:** If the Warden catches the player, the game shifts into a 4x4 chess challenge
- **Noise Detection:** Sprinting and key pickups create noise that can attract the Warden
- **Last Heart Mode:** On the final heart, the player gains a dash on `Q`, jump on `Space`, and a heartbeat warning overlay
- **Procedural Maze Layout:** Each run uses a dynamically generated maze layout
- **Minimap Fog-of-War:** The minimap gradually reveals explored areas

## System Requirements

- **OS:** Windows 10/11
- **Engine:** Unity 6000.3.9f1
- **Input:** Keyboard + Mouse

## Notes

- Difficulty changes by level through maze size, key count, and Warden speed
- Playable scenes include Tutorial, Easy, Medium, and Hard
