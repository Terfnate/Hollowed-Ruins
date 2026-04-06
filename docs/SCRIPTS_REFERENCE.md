# Hollowed Ruins — Scripts Reference

A breakdown of every script in the project, organized by phase.
Use this as a reference when wiring up the scene in Unity.

---

## Phase 1 — Player Foundation

### `Scripts/Core/GameStateManager.cs`
**Singleton. The brain of the game — everything listens to it.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour, Singleton, DontDestroyOnLoad |
| Attach to | A persistent `_Managers` GameObject in the scene |

**States:**
| State | When |
|---|---|
| `Exploring` | Normal gameplay, player moves freely |
| `ChessDuel` | Ghost caught the player, chess board is active |
| `GameOver` | Player lost all 3 hearts |
| `Win` | Player collected all pieces and reached the exit |

**Key methods:**
- `SetState(GameState)` — transitions to a new state, pauses/resumes time
- `IsExploring()` — returns true if currently in exploration
- `IsInChessDuel()` — returns true if chess duel is active

**Events:**
- `OnStateChanged(GameState)` — fired whenever state changes; UI and systems subscribe to this

---

### `Scripts/Core/HealthSystem.cs`
**Tracks the player's 3 hearts. Only loses hearts from chess duels.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour, Singleton |
| Attach to | `_Managers` GameObject |
| Inspector | `maxHearts` (default 3) |

**Key methods:**
- `LoseHeart()` — removes one heart; triggers `GameOver` state if 0 remain

**Events:**
- `OnHeartsChanged(int)` — passes current heart count; HUDController listens
- `OnGameOver()` — fired when hearts hit 0

---

### `Scripts/Core/NoiseSystem.cs`
**Event bus for noise. Player emits noise, ghost listens.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour, Singleton |
| Attach to | `_Managers` GameObject |

**Key methods:**
- `EmitNoise(Vector3 position, float radius)` — broadcasts a noise event

**Events:**
- `OnNoiseEmitted(Vector3, float)` — GhostAI subscribes to this on Start

---

### `Scripts/Player/PlayerController.cs`
**Third-person movement, camera control, and noise emission.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Player GameObject |
| Requires | `CharacterController`, `PlayerInput` |

**Inspector fields:**
| Field | Description |
|---|---|
| `walkSpeed` | Movement speed while walking (default 4) |
| `runSpeed` | Movement speed while running (default 7) |
| `gravity` | Downward force (default -20) |
| `rotationSpeed` | How fast player turns toward movement direction |
| `cameraTarget` | Empty child Transform the Cinemachine camera follows |
| `cameraDistance` | How far back the camera sits |
| `cameraSensitivity` | Mouse/stick look sensitivity |
| `cameraMinY / cameraMaxY` | Vertical camera clamp angles |
| `walkNoiseRadius` | Noise radius while walking (default 5) |
| `runNoiseRadius` | Noise radius while running (default 12) |

**Key methods:**
- `EmitLoudNoise(float radius)` — call this when player interacts with objects

---

## Phase 2 — Maze

### `Scripts/Maze/MazeGenerator.cs`
**Generates the 7x7 maze at runtime and places all objects in it.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour, Singleton |
| Attach to | `MazeGenerator` GameObject in the scene |

**Inspector fields:**
| Field | Description |
|---|---|
| `width / height` | Maze size (both 7) |
| `cellSize` | World-space size of each cell in units (default 4) |
| `floorPrefab` | Prefab for walkable corridor cells |
| `wallPrefab` | Prefab for wall cells |
| `keyPiecePrefab` | Prefab for the 5 collectible key pieces |
| `exitPrefab` | Prefab for the exit point |
| `keyPieceCount` | Number of pieces to place (default 5) |

**How it works:**
1. Runs Recursive Backtracking from (0,0) to carve corridors
2. Instantiates floor/wall prefabs for every cell
3. Places player spawn, ghost spawn, 5 key pieces, and exit in random corridor cells
4. Exit starts hidden — revealed via `RevealExit()` when all pieces are collected

**Key methods:**
- `GetPlayerSpawnWorld()` — returns world position of player spawn
- `GetGhostSpawnWorld()` — returns world position of ghost spawn
- `RevealExit()` — activates the exit GameObject (called by PieceCollectionSystem)

---

### `Scripts/Maze/PieceCollectionSystem.cs`
**Tracks how many key pieces the player has collected (0–5).**

| Item | Detail |
|---|---|
| Type | MonoBehaviour, Singleton |
| Attach to | `_Managers` GameObject |
| Inspector | `totalPieces` (default 5) |

**Key methods:**
- `CollectPiece()` — increments count; calls `RevealExit()` when all collected
- `AllCollected()` — returns true if all 5 collected

**Events:**
- `OnPieceCollected(int collected, int total)` — HUDController listens for counter
- `OnAllPiecesCollected()` — fired when count reaches 5

---

### `Scripts/Maze/KeyPiece.cs`
**Attach to the key piece prefab. Handles collection on player contact.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Key piece prefab |
| Requires | Trigger Collider on the prefab |

**Inspector fields:**
| Field | Description |
|---|---|
| `bobSpeed` | Speed of floating animation (default 2) |
| `bobHeight` | Height of float (default 0.3) |
| `rotateSpeed` | Degrees per second spin (default 90) |

- Calls `PieceCollectionSystem.CollectPiece()` and destroys itself on player trigger

---

### `Scripts/Maze/ExitTrigger.cs`
**Attach to the exit prefab. Triggers Win state when player enters with all pieces.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Exit prefab |
| Requires | Trigger Collider on the prefab |

- Checks `PieceCollectionSystem.AllCollected()` before allowing win
- Calls `GameStateManager.SetState(GameState.Win)`

---

## Phase 3 — Ghost AI

### `Scripts/Ghost/GhostAI.cs`
**The ghost's full behaviour: patrol, chase, stun. NavMesh-driven.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Ghost GameObject |
| Requires | `NavMeshAgent` on Ghost |

**States:**
| State | Behaviour |
|---|---|
| `Patrol` | Wanders to random NavMesh positions |
| `Chase` | Moves directly toward the player |
| `Stun` | Frozen in place for `stunDuration` seconds |

**Inspector fields:**
| Field | Description |
|---|---|
| `sightRange` | Max distance to detect player (default 8) |
| `sightAngle` | Half-angle of view cone in degrees (default 90) |
| `sightBlockMask` | LayerMask for walls that block line of sight |
| `patrolSpeed` | NavMesh speed while patrolling (default 2.5) |
| `chaseSpeed` | NavMesh speed while chasing (default 5) |
| `patrolWaitTime` | Seconds to pause at each patrol waypoint (default 1.5) |
| `patrolRadius` | Radius for random patrol target selection (default 10) |
| `stunDuration` | Seconds frozen after losing chess duel (default 3) |

**Key methods:**
- `Stun()` — freezes the ghost; called by ChessDuelManager on player win

**Detection:**
- Line of sight: checks angle + raycast through walls using `sightBlockMask`
- Noise: subscribes to `NoiseSystem.OnNoiseEmitted`, moves to investigate if in range

**Catch:**
- Uses `OnTriggerEnter` — when ghost collider touches the Player, fires `ChessDuel` state

---

### `Scripts/Ghost/GhostAnimator.cs`
**Drives the ghost Animator from GhostAI state.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Ghost GameObject (alongside GhostAI) |
| Requires | `Animator` on ghost model child |

**Animator parameters it sets:**
| Parameter | Type | When true |
|---|---|---|
| `IsMoving` | Bool | Patrol or Chase |
| `IsChasing` | Bool | Chase only |
| `IsStunned` | Bool | Stun only |
| `Scream` | Trigger | Called after losing chess duel |

**Key methods:**
- `TriggerScream()` — fires the Scream trigger; called by ChessDuelManager

---

## Phase 4 — Chess System

### `Scripts/Chess/ChessData.cs`
**All shared enums and data classes. No MonoBehaviour.**

| Item | Description |
|---|---|
| `PieceType` | King, Queen, Rook, Bishop, Knight, Pawn |
| `PieceColor` | White (player), Black (ghost) |
| `ObjectiveType` | DontLosePiece, ProtectPiece, CaptureTarget, SurviveNTurns |
| `ChessPiece` | Holds type, color, board position, captured state |
| `PiecePlacement` | Serializable struct used in ChessScenario to define initial layout |
| `ChessObjective` | Holds objective type, turn count, target piece, and HUD description text |

---

### `Scripts/Chess/ChessScenario.cs`
**ScriptableObject. Designers create scenarios in the Unity Editor — no code needed.**

**How to create a scenario:**
`Assets > Create > Hollowed Ruins > Chess Scenario`

| Field | Description |
|---|---|
| `pieces` | List of PiecePlacements — type, color, cell (0–3, 0–3) |
| `objective` | The win/lose condition and description text |

---

### `Scripts/Chess/ChessBoard.cs`
**Pure C# class (no MonoBehaviour). Holds all board logic.**

| Method | Description |
|---|---|
| `LoadScenario(ChessScenario)` | Sets up pieces from a scenario |
| `GetAt(Vector2Int)` | Returns the piece at a cell, or null |
| `GetPiecesOf(PieceColor)` | Returns all living pieces of a color |
| `ExecuteMove(ChessPiece, Vector2Int)` | Moves a piece, captures if occupied, returns captured piece |
| `GetLegalMoves(ChessPiece)` | Returns all legal target cells for a piece |

**Supported piece movement:**
- Rook: horizontal/vertical slides
- Bishop: diagonal slides
- Queen: Rook + Bishop combined
- King: one square any direction
- Knight: L-shape jumps
- Pawn: forward one square, diagonal captures

---

### `Scripts/Chess/GhostChessAI.cs`
**Pure C# class. Picks the ghost's move each turn.**

- First priority: capture a white piece if possible
- Fallback: random legal move

---

### `Scripts/Chess/ChessObjectiveEvaluator.cs`
**Pure C# class. Checks win/lose after every round.**

| Objective | Win condition | Lose condition |
|---|---|---|
| `DontLosePiece` | Survive N turns without losing any piece | Any white piece is captured |
| `ProtectPiece` | Target piece survives N turns | Target piece is captured |
| `CaptureTarget` | Player captures the target piece | Turns run out before capture |
| `SurviveNTurns` | Survive N turns | Turns run out (fail) |

---

### `Scripts/Chess/ChessDuelManager.cs`
**Orchestrates the full chess duel. Connects all chess systems together.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour, Singleton |
| Attach to | `_Managers` GameObject |

**Inspector fields:**
| Field | Description |
|---|---|
| `scenarios` | List of ChessScenario assets — one is picked at random per duel |
| `ghostMoveDelay` | Seconds the ghost "thinks" before moving (default 1.2) |

**Flow:**
1. `GameStateManager` switches to `ChessDuel` → duel starts automatically
2. Random scenario is loaded onto the board
3. Player clicks cells to select piece and move
4. Ghost moves after `ghostMoveDelay` seconds
5. `ChessObjectiveEvaluator` checks result after each round
6. **Win** → ghost screams + stuns, game returns to Exploring
7. **Lose** → `HealthSystem.LoseHeart()` called, game returns to Exploring (or GameOver)

**Events:**
- `OnDuelStarted(ChessBoard)` — ChessBoardUI subscribes to build the grid
- `OnPlayerMoved(ChessPiece, Vector2Int)` — UI refreshes
- `OnGhostMoved(ChessPiece, Vector2Int)` — UI refreshes
- `OnTurnsRemainingChanged(int)` — UI updates turns counter
- `OnPlayerWon` — UI can play win effect
- `OnPlayerLost` — UI can play lose effect

---

## Phase 5 — UI

### `Scripts/UI/UIManager.cs`
**Shows the correct panel based on game state.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Canvas root GameObject |

**Inspector fields:** Assign `hudPanel`, `chessDuelPanel`, `gameOverPanel`, `winPanel`

---

### `Scripts/UI/HUDController.cs`
**Exploration HUD — hearts, piece counter, ghost warning.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | HUD Panel |

**Inspector fields:**
| Field | Description |
|---|---|
| `heartIcons` | Array of 3 Image components |
| `heartFullSprite` | Sprite for full heart |
| `heartEmptySprite` | Sprite for empty heart |
| `pieceCounterText` | TextMeshPro showing "X / 5" |
| `ghostWarningIcon` | GameObject that pulses when ghost is chasing |
| `warningPulseSpeed` | Alpha pulse speed (default 3) |

---

### `Scripts/UI/ChessBoardUI.cs`
**Renders the 4x4 chess board and handles player input.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Chess Duel Panel |

**Inspector fields:**
| Field | Description |
|---|---|
| `boardGrid` | GridLayoutGroup set to 4 columns |
| `cellPrefab` | Prefab with ChessBoardCell component |
| `lightCellColor / darkCellColor` | Checkerboard colors |
| `selectedColor` | Highlight color for selected piece |
| `validMoveColor` | Highlight color for valid move targets |
| `whitePieceSprites` | 6 sprites in order: King, Queen, Rook, Bishop, Knight, Pawn |
| `blackPieceSprites` | Same order for black pieces |
| `objectiveText` | TextMeshPro showing the objective description |
| `turnsText` | TextMeshPro showing "Turns left: X" |

---

### `Scripts/UI/ChessBoardCell.cs`
**One cell on the chess board. Attach to the cell prefab.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Cell prefab used by ChessBoardUI |

**Prefab structure needed:**
```
Cell (ChessBoardCell)
├── Background (Image)   ← backgroundImage
├── Highlight  (Image)   ← highlightImage
└── Piece      (Image)   ← pieceImage
```

- `OnClick()` must be wired to the Button component's OnClick event

---

### `Scripts/UI/GameScreensUI.cs`
**Restart and quit buttons for Game Over and Win screens.**

| Item | Detail |
|---|---|
| Type | MonoBehaviour |
| Attach to | Game Over Panel or Win Panel |

**Methods to wire to buttons:**
- `OnRestartClicked()` — reloads the current scene
- `OnQuitClicked()` — quits the application

---

## Script Dependency Map

```
GameStateManager ◄── All systems read state from here
       │
       ├── HealthSystem ◄── ChessDuelManager (LoseHeart)
       │        └── HUDController (OnHeartsChanged)
       │
       ├── NoiseSystem ◄── PlayerController (EmitNoise)
       │        └── GhostAI (OnNoiseHeard)
       │
       ├── MazeGenerator
       │        └── PieceCollectionSystem ──► MazeGenerator.RevealExit()
       │                  ├── KeyPiece (CollectPiece)
       │                  └── HUDController (OnPieceCollected)
       │
       ├── GhostAI ──► GameStateManager.SetState(ChessDuel)
       │        └── GhostAnimator
       │
       ├── ChessDuelManager
       │        ├── ChessBoard (pure logic)
       │        ├── GhostChessAI (pure logic)
       │        ├── ChessObjectiveEvaluator (pure logic)
       │        └── ChessBoardUI
       │
       └── UIManager ──► shows/hides panels
```

---

_Last updated: 2026-04-06_
