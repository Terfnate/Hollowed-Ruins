# Hollowed Ruins — Game Design Reference

Course: CMPS434 | Section: L01 | Instructor: Dr. Osama Halabi
Team: Majid Marzouq, Oluwadamilola Olajide, Abdullah Hamed, Mohamad Allouh

---

## Overview

Single-player, third-person exploration game.
The player explores a haunted 7x7 maze, hunted by a ghost.
Collect 5 hidden key pieces and escape through the exit to win.
When the ghost catches you, you face it in a chess duel.
Lose the duel — lose a heart. Win the duel — the ghost is stunned and you run.

---

## Genre & Platform

- Genre: Puzzle Adventure + Strategy
- Platform: Windows PC (keyboard/mouse, optional controller)
- Player count: Single player
- Target audience: Ages 13–30, Explorers and Strategists

---

## Core Loop

```
Spawn → Explore 7x7 maze → Collect 5 key pieces → Reach exit = WIN
                                   |
                          Ghost catches player
                                   |
                     Chess mini-challenge (4x4 board)
                      /                         \
                    WIN                         LOSE
                     |                            |
          Ghost screams + stunned (~3s)      Lose 1 heart (out of 3)
          Player escapes and runs                  |
                                          0 hearts = GAME OVER
```

---

## Gameplay Layers

### Layer 1 — Real-Time Exploration
- Third-person perspective
- Player navigates a randomly generated 7x7 grid maze each run
- The maze SIZE is fixed (always 7x7), the LAYOUT changes every run
- 5 key pieces are hidden in corridor cells across the maze
- Collecting all 5 pieces unlocks the exit
- Ghost patrols first, then chases on detection
- Ghost detects via line of sight OR noise sensitivity
- No health drain from proximity — only danger is being caught

### Layer 2 — Chess Mini-Challenge (Duel)
- Triggered when the ghost catches the player
- Exploration pauses, a custom 4x4 chess board appears
- Player plays one side, ghost AI plays the other
- Each duel has a specific objective (not a full chess match)
- No time limit per move
- Win: Ghost screams, stunned for ~3 seconds, player resumes exploring
- Lose: Player loses 1 heart, resumes exploring, ghost immediately hunts again

---

## Win / Lose Conditions

| Condition | Result |
|---|---|
| Collect all 5 pieces + reach exit | WIN |
| Lose all 3 hearts | GAME OVER |

---

## Health System

- Player has 3 hearts
- Hearts are lost only by losing a chess duel
- No passive health drain
- Hearts are not recoverable (no healing items in MVP)

---

## Ghost Behavior

| State | Description |
|---|---|
| Patrol | Ghost wanders maze corridors on a path |
| Chase | Ghost actively pursues player via NavMesh |
| Stun | Ghost is frozen ~3 seconds after losing a chess duel |

Detection triggers:
- Line of sight: ghost sees player directly in its view cone
- Noise sensitivity: loud player movement or interacting with objects alerts the ghost even without line of sight

---

## Chess Mini-Challenge

- Board size: 4x4
- Ghost plays as the opponent (AI-controlled)
- Each scenario is a hand-designed board state with specific pieces
- Objectives vary per encounter (to be brainstormed in detail later)
- Examples:
  - "Don't lose any piece in 3 turns"
  - "Protect the bishop for 2 turns"
  - "Capture the opponent's piece in 2 moves"
  - "Keep your king safe from check for N turns"
- Every scenario is always winnable — never unfair
- No time limit

---

## Level Design

- Map: 7x7 grid maze
- Size is FIXED at 7x7 — does not change
- Layout is RANDOMLY GENERATED every run using Recursive Backtracking (depth-first search)
  - Guarantees all cells are reachable
  - Produces exactly one solution path between any two points
- Each cell is either a wall or a corridor
- After generation, the following are placed in corridor cells:
  - Player spawn point
  - 5 key pieces (random corridor cells)
  - Exit point (revealed only after all 5 pieces are collected)
  - Ghost spawn point
- NavMesh baked at runtime for ghost pathfinding

---

## Key Systems

| System | Description |
|---|---|
| GameStateManager | Switches between Exploring and ChessDuel states |
| MazeGenerator | Generates 7x7 maze using recursive backtracking, places objects |
| HealthSystem | Tracks 3 hearts, loses one on chess duel loss, triggers game over |
| GhostAI | NavMesh patrol/chase, line-of-sight detection, noise sensitivity, stun state |
| PieceCollectionSystem | Tracks collected pieces (0-5), reveals exit on completion |
| ChessSystem | 4x4 board, ghost AI opponent, objective validation, win/lose outcome |
| NoiseSystem | Detects player actions, broadcasts alerts to ghost |
| AudioManager | Ambient sounds, ghost detection alarm, chess duel music, stun scream |

---

## Design Goals

- Player feels constant tension while exploring
- Chess duels feel intense but fair (always winnable)
- Losing a heart feels punishing but not frustrating (3 chances)
- Clear HUD: pieces collected, hearts remaining, current objective
- Strong atmosphere through sound and lighting

---

## MVP Scope

- 7x7 randomly generated maze (fixed size, random layout)
- One ghost with patrol, chase, stun behavior
- Line-of-sight and noise detection
- 5 collectible key pieces
- 3-heart health system
- Chess mini-challenge with 4 pre-designed 4x4 scenarios
- Basic HUD (hearts, piece counter)
- Basic audio (ambient, alert, chess duel, stun scream)

---

## Stretch Goals

- More ghost types with different patrol patterns
- More chess duel scenarios
- Environmental traps or hazards
- Story elements (journal entries, lore pickups)
- Difficulty settings (faster ghost, fewer hearts)

---

## Out of Scope

- Multiplayer
- Multiple levels
- Full chess engine
- RPG systems
- Open world

---

## Team Responsibilities

| Member | Role | Deliverables |
|---|---|---|
| Majid Marzouq | Lead Designer | Chess scenario design, ghost balancing, piece placement rules |
| Oluwadamilola Olajide | Programmer | Maze generator, player movement, ghost AI, chess system, health system, noise system |
| Abdullah Hamed | Sound & Music | Ambient ruins audio, ghost alert sound, chess duel music, stun scream SFX |
| Mohamad Allouh | 3D Artist & UI | Ruin environment, ghost model, key piece model, chess board UI, HUD, menus |

---

## Comparable Games

- The Ouroboros King: strategic depth with roguelike progression
- Phantom Abyss: dungeon exploration with a persistent threat hunter

Our differentiation: the ghost is both a real-time hunter AND a chess opponent. Escaping it requires both physical evasion and strategic thinking.

---

_Last updated: 2026-04-06_
