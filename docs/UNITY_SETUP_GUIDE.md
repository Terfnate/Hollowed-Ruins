# Hollowed Ruins — Unity Scene Setup Guide

Follow this guide after cloning the repo to wire up the scene in Unity.
Do this in order — some steps depend on earlier ones.

---

## 1. Rename the Scene

In the Project window:
- `Assets/Scenes/SampleScene` → rename to `Level01`
- Open it: double-click `Level01`

---

## 2. Create the `_Managers` GameObject

This holds all singleton manager scripts.

1. In the Hierarchy, right-click → `Create Empty` → name it `_Managers`
2. Add the following components to it:
   - `GameStateManager`
   - `HealthSystem`
   - `NoiseSystem`
   - `PieceCollectionSystem`
   - `ChessDuelManager`
3. On `ChessDuelManager`, assign `Scenarios` (see Step 8 for how to create them)

---

## 3. Create the Maze Generator GameObject

1. Right-click Hierarchy → `Create Empty` → name it `MazeGenerator`
2. Add component: `MazeGenerator`
3. Assign in Inspector:
   | Field | What to assign |
   |---|---|
   | Floor Prefab | Your floor tile prefab (Step 6) |
   | Wall Prefab | Your wall prefab (Step 6) |
   | Key Piece Prefab | Your key piece prefab (Step 6) |
   | Exit Prefab | Your exit prefab (Step 6) |
   | Width / Height | Leave at 7 |
   | Cell Size | Leave at 4 |
   | Key Piece Count | Leave at 5 |

---

## 4. Create the Player GameObject

1. Right-click Hierarchy → `3D Object > Capsule` → rename to `Player`
2. Tag it as `Player` (Inspector top → Tag dropdown → Add Tag → `Player`)
3. Add components:
   - `PlayerController`
   - `PlayerInput` (set Actions asset to `InputSystem_Actions`)
   - `CharacterController`
4. Create an empty child object → name it `CameraTarget`
   - Position it at about `(0, 1.5, 0)` relative to the player
5. Assign `CameraTarget` to the `Camera Target` field on `PlayerController`

### Camera Setup
1. Install **Cinemachine** from Package Manager if not already installed
2. In Hierarchy → right-click → `Cinemachine > Cinemachine Camera`
3. Set the `Follow` and `Look At` targets to `CameraTarget`
4. Set Cinemachine Body to `3rd Person Follow`:
   - Camera Distance: 5
   - Shoulder Offset Y: 1.5

---

## 5. Create the Ghost GameObject

1. Right-click Hierarchy → `Create Empty` → name it `Ghost`
2. Add components:
   - `NavMeshAgent`
   - `GhostAI`
   - `GhostAnimator`
   - A `Sphere Collider` set to **Is Trigger = true**, Radius ~0.8
3. Set `GhostAI` Inspector fields:
   | Field | Value |
   |---|---|
   | Sight Range | 8 |
   | Sight Angle | 90 |
   | Sight Block Mask | Select the `Wall` layer |
   | Patrol Speed | 2.5 |
   | Chase Speed | 5 |
   | Stun Duration | 3 |
4. Add a child 3D model (placeholder: Capsule) for the ghost's visual + Animator

> **Note:** After maze generation at runtime, the ghost must be moved to `MazeGenerator.GetGhostSpawnWorld()`. Add a simple `GhostSpawner` script or handle this in a game initializer.

---

## 6. Create Prefabs

Create these in `Assets/Prefabs/`. Make a 3D object, configure it, then drag it to the Prefabs folder.

### Floor Prefab
- `3D Object > Plane` or `Cube` (scale to match cell size, e.g. `4 x 0.1 x 4`)
- Layer: `Floor`
- No collider needed on top (CharacterController handles gravity)

### Wall Prefab
- `3D Object > Cube` → scale `(4, 3, 4)`
- Add `Box Collider`
- Layer: `Wall` ← ghost's `Sight Block Mask` must include this layer

### Key Piece Prefab
- `3D Object > Sphere` (small, ~0.4 scale)
- Add `Sphere Collider` → set **Is Trigger = true**
- Add component: `KeyPiece`

### Exit Prefab
- `3D Object > Cube` or a portal mesh
- Add `Box Collider` → set **Is Trigger = true**
- Add component: `ExitTrigger`
- Give it a distinct material (glowing green, etc.)

---

## 7. Set Up NavMesh

The ghost uses NavMesh to navigate. Because the maze is generated at runtime, you need a **runtime NavMesh bake**.

1. Install package: `AI Navigation` (already in your `manifest.json`)
2. Add component `NavMeshSurface` to the `MazeGenerator` GameObject
3. Set `Collect Objects` to `Children`
4. In `MazeGenerator.cs`, add this after `BuildMesh()` in `Start()`:

```csharp
GetComponent<NavMeshSurface>().BuildNavMesh();
```

5. Make sure your Floor prefab is on a layer included in the NavMesh bake

---

## 8. Create Chess Scenarios (Designer Task — Majid)

Each scenario is a ScriptableObject. Create at least 4 for MVP.

**How to create:**
`Right-click in Project > Create > Hollowed Ruins > Chess Scenario`

**Fill in:**
- `Pieces`: list of pieces with type, color, and cell (x: 0-3, y: 0-3)
- `Objective`:
  - `Type`: pick from DontLosePiece / ProtectPiece / CaptureTarget / SurviveNTurns
  - `Turns Allowed`: how many turns the player has
  - `Target Piece Type`: used for ProtectPiece and CaptureTarget
  - `Description`: text shown on HUD (e.g. "Don't lose any piece in 3 turns!")

**Example scenario — "Protect the Bishop":**
```
Pieces:
  White King   (2, 0)
  White Bishop (1, 1)
  Black Rook   (0, 3)
  Black Knight (3, 3)

Objective:
  Type: ProtectPiece
  Turns Allowed: 3
  Target Piece Type: Bishop
  Description: "Protect the Bishop for 3 turns!"
```

Once created, assign all scenario assets to the `Scenarios` list on `ChessDuelManager`.

---

## 9. Set Up the Canvas and UI

1. Right-click Hierarchy → `UI > Canvas`
2. Set Canvas Scaler to `Scale With Screen Size` (1920x1080 reference)
3. Add `UIManager` component to the Canvas
4. Create 4 child panels inside the Canvas:

### HUD Panel (shown during Exploring)
- Add `HUDController` component
- Inside, create:
  - 3 `UI > Image` objects for hearts → assign to `Heart Icons` array
  - `UI > Text - TextMeshPro` for piece counter → assign to `Piece Counter Text`
  - Empty GameObject for ghost warning icon → assign to `Ghost Warning Icon`
- Assign `heartFullSprite` and `heartEmptySprite` (create simple colored sprites)

### Chess Duel Panel (shown during ChessDuel)
- Add `ChessBoardUI` component
- Inside, create:
  - A `Grid Layout Group` panel → 4 columns, square cells → assign to `Board Grid`
  - `UI > Text - TextMeshPro` for objective → assign to `Objective Text`
  - `UI > Text - TextMeshPro` for turns → assign to `Turns Text`
- Create the **Cell Prefab** (see below)

### Game Over Panel
- Add `GameScreensUI` component
- Add "Restart" Button → wire `OnClick` to `GameScreensUI.OnRestartClicked()`
- Add "Quit" Button → wire `OnClick` to `GameScreensUI.OnQuitClicked()`

### Win Panel
- Add `GameScreensUI` component
- Same buttons as Game Over

5. Assign all 4 panels to the `UIManager` Inspector fields

---

## 10. Create the Chess Cell Prefab

This is used by `ChessBoardUI` to build the 4x4 grid.

1. `UI > Image` → rename to `ChessCell`
2. Add component: `ChessBoardCell`
3. Add component: `Button`
4. Inside it, create 3 child Images:
   ```
   ChessCell (ChessBoardCell + Button)
   ├── Background (Image)  ← backgroundImage field
   ├── Highlight  (Image)  ← highlightImage field  (set Alpha to 0 by default)
   └── Piece      (Image)  ← pieceImage field
   ```
5. Wire the Button `OnClick()` event to `ChessBoardCell.OnClick()`
6. Save as a prefab in `Assets/Prefabs/UI/`
7. Assign to `Cell Prefab` field on `ChessBoardUI`

---

## 11. Assign Piece Sprites to ChessBoardUI (Artist Task — Mohamad)

Create or import 12 sprites (6 white, 6 black) for chess pieces.

Assign them in order on `ChessBoardUI`:
```
Index 0 = King
Index 1 = Queen
Index 2 = Rook
Index 3 = Bishop
Index 4 = Knight
Index 5 = Pawn
```

Until real sprites are ready, use colored placeholder squares.

---

## 12. Final Checklist Before Pressing Play

- [ ] `_Managers` has: GameStateManager, HealthSystem, NoiseSystem, PieceCollectionSystem, ChessDuelManager
- [ ] `MazeGenerator` has: MazeGenerator + NavMeshSurface, all 4 prefabs assigned
- [ ] `Player` tagged as `Player`, has PlayerController + PlayerInput + CharacterController
- [ ] `Ghost` has: NavMeshAgent + GhostAI + GhostAnimator + Trigger Collider
- [ ] Canvas has: UIManager with all 4 panels assigned
- [ ] At least 1 ChessScenario asset assigned to ChessDuelManager
- [ ] Wall prefab is on the `Wall` layer, assigned in GhostAI's `Sight Block Mask`
- [ ] Floor prefab is on a NavMesh-bakeable layer

---

## Common Errors & Fixes

| Error | Fix |
|---|---|
| `NullReferenceException: PieceCollectionSystem.Instance` | Add `PieceCollectionSystem` component to `_Managers` GameObject |
| Ghost doesn't move | NavMesh not baked — add `NavMeshSurface` and call `BuildNavMesh()` at runtime |
| Player falls through floor | Floor prefab missing a Collider |
| Chess board doesn't appear | `chessDuelPanel` not assigned in UIManager, or no scenario in ChessDuelManager |
| Key pieces not collected | Key piece prefab collider not set to Is Trigger = true, or Player tag missing |

---

_Last updated: 2026-04-06_
