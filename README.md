# HOLLOWED RUINS

Haunted-maze survival game built in Unity 6 for `CMPS434 - Game Development Course Project`.

## Player Manual

### Game Overview

You are trapped inside a haunted ruin maze. Explore the labyrinth, collect every broken key piece, reveal the exit portal, and escape before the ghost catches you.

If the ghost reaches you, the game shifts into a 4x4 chess duel. Winning the duel buys time and forces the ghost away. Losing the duel costs a heart. Lose all hearts and the run ends.

### Unity Version

`Unity 6000.3.9f1 (Unity 6)`

### Platform

- Windows 10/11
- Keyboard + mouse supported
- Input System package also includes gamepad bindings

### Controls

- `W / A / S / D` - Move
- `Mouse` - Look around
- `Left Shift` - Sprint
- `Left Click` - Select / move chess pieces during duels
- `Q` - Dash, only when on the final heart
- `Space` - Jump, only when on the final heart

### Objective

- Collect all key pieces hidden in the maze
- Reveal the exit portal once all key pieces are collected
- Reach the portal before losing all hearts

### Win Condition

- Collect all key pieces
- Enter the exit portal

### Lose Condition

- Lose all 3 hearts
- Hearts are lost only by failing chess duels

### HUD

- `Hearts` - top-left life display
- `Key Counter` - shows progress such as `2 / 3`
- `Portal Prompt` - changes to `Find the portal!` when all keys are collected
- `Ghost Warning` - pulses while the ghost is actively chasing
- `Minimap` - top-right tactical view with a cyan player marker
- `Last Heart Overlay` - red heartbeat screen effect on final life
- `Dash Indicator` - cooldown display for the emergency dash ability

### The Ghost

- Patrols the maze using Unity NavMesh
- Detects the player through line of sight
- Investigates sound events emitted by movement and pickups
- Sprinting creates larger noise than walking
- Picking up a key piece emits a very large noise pulse, often dragging the ghost toward the pickup location

### Chess Duel

When the ghost catches the player, exploration pauses and a chess challenge begins on a 4x4 board.

Possible objectives include:

- Survive for a number of turns
- Protect a specified piece for a number of turns
- Avoid losing any white piece for a number of turns
- Capture a specified enemy piece before turns run out

Results:

- Win: the ghost screams, vanishes temporarily, and respawns elsewhere in the maze
- Lose: you lose 1 heart, the ghost is briefly stunned, and exploration resumes

### Last Heart Mode

When only one heart remains:

- `Q` enables a forward dash with a 30-second cooldown
- `Space` enables jumping
- Sprinting gains a speed multiplier
- A heartbeat overlay appears on the HUD

### Level Differences

Each playable scene changes the pressure curve through:

- Maze size
- Number of key pieces required
- Ghost patrol speed
- Ghost chase speed

Current level configs:


| Level    |  Maze | Keys | Patrol Speed | Chase Speed |
| -------- | ----: | ---: | -----------: | ----------: |
| Tutorial |   7x7 |    2 |            3 |           6 |
| Easy     | 12x12 |    3 |            4 |           8 |
| Medium   | 15x15 |    4 |            5 |           9 |
| Hard     | 17x17 |    5 |            6 |          10 |

## Project Overview

### High Concept

`Hollowed Ruins` combines third-person maze exploration with short-form tactical chess encounters. The main tension comes from alternating between spatial survival and turn-based problem solving.

### Core Loop

1. Spawn into a procedurally generated maze.
2. Explore, collect key pieces, and manage ghost pressure.
3. Enter a chess duel if the ghost catches the player.
4. Resume exploration after duel resolution.
5. Escape through the revealed portal.

### Build Scenes

Configured build scenes in `ProjectSettings/EditorBuildSettings.asset`:

- `Assets/_Scenes/MainMenu.unity`
- `Assets/_Scenes/LevelTutorial.unity`
- `Assets/_Scenes/LevelEasy.unity`
- `Assets/_Scenes/LevelMedium.unity`
- `Assets/_Scenes/LevelHard.unity`

## Opening The Project

1. Open Unity Hub.
2. Add the folder `Hollowed Ruins`.
3. Open the project with `Unity 6000.3.9f1`.
4. Load `Assets/_Scenes/MainMenu.unity` to start from the intended front end.

Primary packages in use:

- `com.unity.inputsystem`
- `com.unity.cinemachine`
- `com.unity.ai.navigation`
- `com.unity.render-pipelines.universal`
- `com.unity.ugui`
- `com.unity.textmeshpro`

## Directory Guide

### High-Value Folders

- `Hollowed Ruins/Assets/_Scenes` - main menu and gameplay scenes
- `Hollowed Ruins/Assets/_Scenes/Levels Config` - per-difficulty `LevelConfig` assets
- `Hollowed Ruins/Assets/Scripts` - gameplay code
- `Hollowed Ruins/Assets/Audio` - ambient, music, and ghost SFX
- `Hollowed Ruins/Assets/Art` - character art, animation controllers, materials, and textures
- `Hollowed Ruins/Assets/UI` - UI sprites and fonts
- `Hollowed Ruins/Assets/DungeonModularPack` - modular environment art used to build the maze
- `docs` - design and script reference documents

### Core Runtime Script Layout

- `Scripts/Core` - game state, health, noise, level tuning
- `Scripts/Player` - player movement and spawn logic
- `Scripts/Maze` - procedural generation, key placement, exit flow
- `Scripts/Ghost` - AI, animation bridge, ghost spawn support
- `Scripts/Chess` - duel logic, board model, scenarios, evaluator, AI
- `Scripts/UI` - HUD, minimap, menus, duel board, end screens

## Gameplay Systems Reference

This section is written in the style of a production handoff: what each system owns, what must be assigned in the Inspector, and why it exists.

### 1. State Management

#### `GameStateManager`

Role:
Central authority for game flow. All major systems query or react to `Exploring`, `ChessDuel`, `GameOver`, and `Win`.

Key responsibilities:

- Stores the active `LevelConfig`
- Broadcasts state changes through `OnStateChanged`
- Freezes and resumes world time with `Time.timeScale`
- Persists across scene reloads with `DontDestroyOnLoad`

#### `HealthSystem`

Role:
Owns the player's heart economy.

Key responsibilities:

- Initializes and resets hearts
- Emits `OnHeartsChanged`
- Triggers `GameOver` when hearts reach zero

#### `NoiseSystem`

Role:
Simple event bus for sound-driven gameplay.

Key responsibilities:

- Emits noise events with `position` and `radius`
- Lets the player and pickup systems influence ghost behavior without hard references

#### `LevelConfig`

Role:
ScriptableObject that defines difficulty tuning per level.

Fields:

- `mazeWidth`
- `mazeHeight`
- `keyPieceCount`
- `ghostPatrolSpeed`
- `ghostChaseSpeed`

### 2. Player Layer

#### `PlayerController`

Role:
Primary exploration controller.

Required components:

- `CharacterController`
- `PlayerInput`

Serialized dependencies:

- `cameraTarget`
- `animator`
- `footstepSource`

Major features:

- Walk and sprint locomotion
- Mouse-look orbit target control
- Gravity handling through `CharacterController`
- Noise emission tied to movement state
- Footstep loop playback
- Last-heart sprint boost
- Last-heart dash and jump

Notable implementation details:

- `DashCooldownRemaining` and `DashReady` expose player status to HUD systems cleanly
- `EmitLoudNoise(float radius)` provides an explicit hook for future interactables
- Debug keys currently exist in `Update()`: `P` forces a chess duel and `L` removes a heart

#### `PlayerSpawner`

Role:
Legacy utility for spawning a prefab at a `PlayerSpawn` marker.

Current status:
The procedural maze flow already positions the player through `MazeGenerator.PositionActors()`. This spawner is useful only if the project returns to marker-based spawning.

### 3. Maze Generation And Objective Layer

#### `MazeGenerator`

Role:
Builds the maze at runtime and places mission-critical actors and objectives.

Required dependencies:

- `NavMeshSurface` on the same GameObject for runtime bake
- Assigned `floorPrefab`, `wallPrefab`, `keyPiecePrefab`, and `exitPrefab`
- References to the player transform and ghost actor

Major features:

- Reads width, height, and key count from `GameStateManager.levelConfig`
- Uses recursive backtracking to carve a solvable maze
- Forces a central 2x2 hall for reliable spawn space
- Adds extra gaps to create loops and reduce single-solution predictability
- Builds floor, ceiling, and wall geometry
- Bakes a runtime NavMesh for ghost navigation
- Places the player, ghost, exit, and key pieces
- Notifies `MinimapFog` once the layout is ready

Production value:
This script is effectively the level bootstrapper. It owns both layout generation and mission placement, which makes it the most important scene setup script in the project.

#### `PieceCollectionSystem`

Role:
Tracks objective progress.

Responsibilities:

- Counts collected key pieces
- Broadcasts progress to UI
- Fires completion events
- Reveals the exit once all keys are found

#### `KeyPiece`

Role:
Collectible actor attached to the key prefab.

Behavior:

- Applies bobbing and spinning for visual readability
- Detects player trigger entry
- Emits a very large noise pulse on pickup
- Reports progress to `PieceCollectionSystem`
- Destroys itself after collection

#### `ExitTrigger`

Role:
Win-condition gate attached to the exit portal.

Behavior:

- Verifies all key pieces are collected
- Pushes game state to `Win`

### 4. Ghost AI Layer

#### `GhostAI`

Role:
Primary enemy behavior controller.

Required components:

- `NavMeshAgent`

Serialized dependencies:

- `sightBlockMask`
- `audioSource`
- `ghostAlertClip`
- `ghostRoarClip`

States:

- `Patrol`
- `Chase`
- `Stun`

Major features:

- Reads movement tuning from `LevelConfig`
- Waits until runtime NavMesh is ready before activating navigation
- Uses forward-angle and raycast checks for visual detection
- Reacts to `NoiseSystem` events for investigation behavior
- Starts chess duels on collision with the player
- Supports `Stun()` and `Vanish(float duration)` as separate recovery outcomes

Important design note:

- Losing a duel stuns the ghost in place
- Winning a duel makes the ghost vanish, relocate, and then resume patrol

#### `GhostAnimator`

Role:
Bridges AI state into animation parameters.

Responsibilities:

- Pushes `IsMoving`
- Pushes `IsChasing`
- Pushes `IsStunned`
- Pushes `Speed`
- Triggers `Scream`

#### `GhostSpawner`

Role:
Additional spawn utility for `GhostSpawn` markers.

Current status:
Not part of the procedural single-ghost loop used by `MazeGenerator`, but valuable if the project expands into multi-ghost scenarios.

### 5. Chess Duel Layer

This subsystem is the identity feature of the project. It is cleanly split between orchestration, board rules, scenario data, objective evaluation, and AI decision-making.

#### `ChessDuelManager`

Role:
Top-level duel orchestrator.

Responsibilities:

- Starts duels when the game state changes to `ChessDuel`
- Loads a random scenario
- Builds the board model
- Handles player clicks and move submission
- Delays and executes ghost turns
- Evaluates win/loss outcome
- Plays duel music
- Resolves post-duel recovery back into exploration

Important runtime behavior:

- Uses `WaitForSecondsRealtime` so chess flow still runs while world time is paused
- Raises UI-facing events such as `OnDuelStarted`, `OnPlayerMoved`, and `OnTurnsRemainingChanged`

#### `ChessBoard`

Role:
Pure board-state model.

Responsibilities:

- Loads a scenario into a 4x4 grid
- Stores live piece positions
- Generates legal moves
- Executes captures
- Supports cloning for AI simulation

Supported piece logic:

- King
- Queen
- Rook
- Bishop
- Knight
- Pawn

#### `ChessData`

Role:
Shared enums and serializable data containers for the duel feature.

Includes:

- `PieceType`
- `PieceColor`
- `ObjectiveType`
- `ChessPiece`
- `PiecePlacement`
- `ChessObjective`

#### `ChessScenario`

Role:
Designer-authored ScriptableObject used to build an encounter.

Contains:

- Initial piece placements
- Objective metadata
- Designer-facing description text for UI

#### `ChessObjectiveEvaluator`

Role:
Evaluates duel success and failure conditions.

Supported objective types:

- `DontLosePiece`
- `ProtectPiece`
- `CaptureTarget`
- `SurviveNTurns`

#### `GhostChessAI`

Role:
Turn-selection logic for the ghost.

Implementation:

- Scores moves using a lightweight minimax search
- Evaluates board advantage by material value
- Plays as black

### 6. UI And Front-End Layer

#### `UIManager`

Role:
Activates the correct screen for each game state.

Panels:

- HUD
- Chess Duel
- Game Over
- Win

#### `HUDController`

Role:
Owns the live exploration HUD.

Dependencies:

- Heart icon images and sprites
- Piece counter `TextMeshProUGUI`
- Ghost warning icon
- Last-heart overlay image
- Dash indicator image and text

Responsibilities:

- Reflect heart changes
- Reflect key progress
- Switch prompt to `Find the portal!`
- Pulse ghost-warning alpha during chase
- Animate the heartbeat overlay
- Show dash readiness and cooldown

#### `ChessBoardUI`

Role:
Visual front end for the chess duel.

Dependencies:

- `GridLayoutGroup`
- Cell prefab using `ChessBoardCell`
- White and black piece sprite sets
- Objective and turns labels

Responsibilities:

- Builds the 4x4 board dynamically
- Renders pieces from the pure board model
- Highlights selection and legal moves
- Sends player clicks to `ChessDuelManager`

#### `ChessBoardCell`

Role:
Single reusable board-square prefab script.

Expected child references:

- background image
- highlight image
- piece image

Responsibilities:

- Initializes board coordinates
- Displays piece sprite and tint
- Forwards button clicks back to `ChessBoardUI`

#### `MinimapController`

Role:
Creates and updates the tactical minimap camera.

Responsibilities:

- Configures an orthographic top-down camera
- Positions it over the maze center
- Spawns a cyan cylinder marker for the player
- Hides minimap-only layers from the main camera

#### `MinimapFog`

Role:
Implements simple fog-of-war discovery for the minimap.

Responsibilities:

- Builds per-cell fog quads after the maze is generated
- Reveals the current cell and adjacent cells as the player moves

#### `MainMenuManager`

Role:
Controls the front-end flow from main panel to level select.

Responsibilities:

- Shows and hides menu panels
- Loads the tutorial, easy, medium, and hard scenes
- Quits the game or stops play mode in the editor

#### `GameScreensUI`

Role:
Owns restart and return-to-menu actions from end screens.

Responsibilities:

- Resets game state, hearts, and key progress
- Reloads the current scene
- Returns to `MainMenu`

## Assets And Content Notes

### Environment

- `Assets/DungeonModularPack` provides the modular dungeon geometry and materials used by the maze builder.
- Runtime maze geometry is assembled from assigned floor and wall prefabs rather than from one authored level mesh.

### Character Art

- `Assets/Art/Mixamo` and `Assets/Art/Animations` contain player and ghost animation assets and animator controllers.
- `PlayerAnimator.controller` and `CreepAnimator.controller` are the key animation state machines to verify if characters stop animating correctly.

### Audio

- `Assets/Audio/Ambient/Ambient ruins.mp3`
- `Assets/Audio/Music/Chess_duel.mp3`
- `Assets/Audio/SFX/Ghost_Alert.mp3`
- `Assets/Audio/SFX/Ghost_Roar.mp3`
- `Assets/Audio/SFX/FootSteps_Run.wav`

These clips support the tension curve across exploration, detection, and duel states.

### UI

- `Assets/UI` contains HUD-facing art
- `Assets/TextMesh Pro` contains TMPro dependencies and examples
- `Assets/UI/Sprites/Chess_Pieces` supplies the duel piece icons

## Scene Setup Expectations

For a gameplay scene to function correctly, it should include:

- A persistent managers object with `GameStateManager`, `HealthSystem`, `PieceCollectionSystem`, `NoiseSystem`, and `ChessDuelManager`
- A maze root with `MazeGenerator`, `NavMeshSurface`, and optionally `MinimapFog`
- A player object with `CharacterController`, `PlayerInput`, `PlayerController`, animator, and footstep audio source
- A ghost object with `NavMeshAgent`, `GhostAI`, and `GhostAnimator`
- A UI canvas with `UIManager`, `HUDController`, `ChessBoardUI`, and end-screen buttons
- A minimap camera using `MinimapController`

## Known Architectural Notes

- `GameStateManager` is marked `DontDestroyOnLoad`, so duplicate manager objects across scenes must be avoided.
- `MainMenuManager` stores scene names as strings. If scene names change, those fields must be updated in the Inspector.
- `PlayerSpawner` and `GhostSpawner` appear to be legacy or expansion scripts, while the active procedural flow is driven by `MazeGenerator`.
- `ChessBoardUI` declares a `lastMoveColor` field that is not currently applied in the board refresh path.

## Credits

Course: `CMPS434`

Team:

- Majid Marzouq
- Oluwadamilola Olajide
- Abdullah Hamed
- Mohamad Allouh

Supporting docs:

- `docs/GAME_DESIGN.md`
- `docs/SCRIPTS_REFERENCE.md`
- `cmps434/course_content/lectures`
