# Hollowed Ruins — Game Design Reference

Course: CMPS434 | Section: L01 | Instructor: Dr. Osama Halabi
Team: Majid Marzouq, Oluwadamilola Olajide, Abdullah Hamed, Mohamad Allouh

---

## Overview

Single-player, third-person puzzle adventure game.
The player explores a haunted ruin while being hunted by a ghost that slowly possesses them.
When caught, the game shifts into a chess mini-challenge to resist possession.

---

## Genre & Platform

- Genre: Puzzle Adventure + Strategy
- Platform: Windows PC (keyboard/mouse, optional controller)
- Player count: Single player
- Target audience: Ages 13–30, Explorers and Strategists

---

## Core Loop

```
Spawn → Explore ruins → Solve puzzles → Avoid ghost → Escape = WIN
                                              ↓
                                     Ghost catches player
                                              ↓
                                   Chess mini-challenge
                                    /               \
                                  WIN              LOSE
                                   ↓                ↓
                           Resume exploring   Lose health /
                           (meter drops)    possession rises
                                                   ↓
                                         Fully possessed = GAME OVER
```

---

## Gameplay Layers

### Layer 1 — Real-Time Exploration
- Third-person perspective
- Player navigates fixed, hand-crafted ruin corridors
- Puzzles block progression — must be solved to advance
- Ghost AI patrols and actively chases the player
- Possession meter fills while ghost is nearby (acts as health bar)

### Layer 2 — Chess Mini-Challenge
- Triggered when the ghost catches the player
- Exploration pauses, chess board appears
- Player must complete 3 pre-designed objectives (not a full chess match)
- Examples: capture a specific piece, protect a piece for N moves, reach a square
- Win → resume exploration, possession meter decreases
- Lose → possession meter increases, risk of game over

---

## Key Systems

| System | Description |
|---|---|
| GameStateManager | Switches between Exploring and ChessChallenge states |
| PossessionSystem | Tracks possession meter, triggers chess challenge on capture |
| GhostAI | Navigates maze via NavMesh, detects and chases player |
| PuzzleSystem | Manages puzzle triggers and level progression |
| ChessSystem | Manages chess board, objectives, win/lose conditions |
| AudioManager | Ambient sounds, ghost proximity audio, tension music |

---

## Design Goals

- Player feels constant tension while exploring
- Chess challenges feel intense but fair (always solvable)
- Losing feels punishing but not frustrating (meter gives multiple chances)
- Clear UI and readable objectives at all times
- Strong atmosphere through sound and lighting

---

## Level Design

- Map is hand-crafted, fixed layout (NOT procedurally generated)
- One level for MVP
- Level designer (Majid) places corridors, rooms, and puzzle chokepoints manually
- Ghost patrol routes are tuned to the fixed layout
- This decision was made to: control pacing, simplify ghost AI, reduce scope

---

## MVP Scope

- One hand-crafted level
- One ghost with NavMesh AI
- Chess mini-challenge with 3 pre-designed objectives
- Possession/spirit health system
- Basic HUD (possession meter, objective tracker)
- Basic audio (ambient, ghost proximity, tension music)

---

## Stretch Goals

- More levels
- Varied ghost behaviors
- Additional chess challenge scenarios
- Simple story cutscenes

---

## Out of Scope

- Multiplayer
- Open world or procedural generation
- Full chess engine
- RPG systems

---

## Team Responsibilities

| Member | Role | Deliverables |
|---|---|---|
| Majid Marzouq | Lead Designer | Core loop design, puzzle design, level layout, ghost encounter balancing |
| Oluwadamilola Olajide | Programmer | Player movement, ghost AI, chess system, health/possession system |
| Abdullah Hamed | Sound & Music | Ambient sounds, ghost SFX, tension music |
| Mohamad Allouh | 3D Artist & UI | Ruin environment, ghost model, chess board UI, menus |

---

## Comparable Games

- **The Ouroboros King** — strategic depth with roguelike progression
- **Phantom Abyss** — dungeon exploration with persistent threat

Our differentiation: merging turn-based chess combat with real-time haunted exploration in one cohesive experience.

---

_Last updated: 2026-04-04_
