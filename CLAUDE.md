# Golf Tycoon - Unity Mobile Game

## Project Overview
A mobile golf base-building/progression game inspired by Clash of Clans, Golf Inc, and arcade golf games. Players own a golf property, grow it from 1 hole to 18, earn money from AI and real players, and compete via asynchronous course visits.

## Tech Stack
- **Engine**: Unity 6 (6000.4.1f1) with URP (Universal Render Pipeline)
- **Language**: C#
- **Target**: iOS and Android (landscape orientation)
- **Packages**: URP 17.4.0, Input System 1.19.0, Cinemachine 2.10.6, TextMeshPro

## Project Structure
```
Assets/Scripts/
├── Core/           # GameManager, ServiceLocator, EventBus, SaveSystem, SceneLoader, TimeManager
├── Data/           # PlayerSaveData, CourseSaveData, GameConstants (all tuning values)
├── Golf/
│   ├── ShotMechanic/  # PowerBar, AccuracyBar, ShotController (state machine), ShotResolver, ShotArc
│   ├── Ball/          # GolfBall (state: OnTee/InFlight/OnGreen/InBunker/Holed), BallPhysics, BallCamera
│   ├── Club/          # ClubData (ScriptableObject), ClubInventory, ClubSelector, ClubUpgrade
│   ├── Course/        # HoleData, CourseData, HoleInstance, CourseLoader, HazardZone, GreenPhysics, CostTable
│   ├── Scoring/       # ScorecardManager, ScoreCalculator, RoundResult
│   └── Camera/        # GolfCameraController (states: Tee/Flight/Approach/Putt/Celebration)
├── Building/       # PropertyManager, HoleSlot, HoleDesigner, ConstructionTimer, PropertyExpansion, DrivingRange
├── Economy/        # CurrencyManager, IncomeCalculator, AIGolferSimulator, PricingManager, OfflineProgressCalculator
├── Progression/    # ReputationSystem, UnlockManager, AchievementData, AchievementTracker
├── Social/         # LeaderboardManager, CourseVisitManager, CourseRatingSystem, AsyncMultiplayerManager
├── UI/
│   ├── Screens/    # MainMenuScreen, HUDScreen, ScorecardScreen, BuilderScreen, ShopScreen, LeaderboardScreen
│   └── Components/ # PowerBarUI, AccuracyBarUI, TimerDisplay, CurrencyDisplay, ProgressBar
├── Utility/        # MathHelpers, TouchInputHandler, ObjectPool
└── Editor/         # ProjectSetup.cs (auto-setup script: Golf Tycoon > Setup Entire Project)
```

## Scenes (loaded additively from Boot)
- **Boot.unity** - Persistent scene with GameManager singleton, never unloaded
- **MainMenu.unity** - Title screen with Play/Build/Visit/DrivingRange/Shop buttons
- **GolfGameplay.unity** - Golf gameplay with ball, shot mechanic, course, HUD, scorecard
- **CourseBuilder.unity** - Top-down property view for building/upgrading holes
- **DrivingRange.unity** - Passive income mini-scene (placeholder)

## Architecture
- **ServiceLocator pattern** for dependency injection (not singletons)
- **EventBus** for decoupled cross-system communication (typed events)
- **ScriptableObjects** for all game data (clubs, holes, courses, costs)
- **Additive scene loading** - Boot stays loaded, other scenes load/unload on top
- **UTC timestamps** for all timers and offline progress calculations
- **JSON save system** to Application.persistentDataPath

## Core Game Loop
1. Player starts with 1 hole + driving range
2. AI golfers generate passive income every 5 seconds
3. Player earns money, builds/upgrades holes, upgrades driving range
4. Course expands: 1 → 3 → 9 → 18 holes (fixed tiers, gated by reputation + currency)
5. Construction takes real time ("ground under repair")
6. Other players can visit your course (async multiplayer) for boosted income
7. Leaderboards per course, ratings, reputation/prestige system

## Golf Gameplay
- **Shot mechanic**: Aim → tap to set Power (oscillating bar 0-1) → tap to set Accuracy (oscillating bar, deviation angle) → ball launches
- **Clubs**: 7 types (Driver, 3-Wood, 5-Iron, 7-Iron, PW, SW, Putter) with 3 upgrade levels each
- **Scoring**: Standard golf (HoleInOne/Albatross/Eagle/Birdie/Par/Bogey/DoubleBogey)
- **Hazards**: Bunkers (reduced distance), Water (1 stroke penalty + re-drop), OB, Trees
- **Green physics**: Slope influence on putting

## Economy
- **Passive income**: AI golfers + driving range (ticks every 5s)
- **Real player visits**: 5x more income than AI
- **Offline earnings**: Calculated on app launch, capped at 8 hours
- **Reputation**: Earned from building, playing, receiving visitors. Unlocks content tiers.
- **Starting currency**: $5,000

## Key Constants (in GameConstants.cs)
- Construction time: 5 min (hole), 10 min (upgrade), 3 min (driving range)
- Expansion costs: $5K (→3), $25K (→9), $100K (→18)
- Reputation thresholds: 500 (3 holes), 2000 (9 holes), 8000 (18 holes)

## Current Status

### What's Built (all 68 C# scripts, ~6000 lines)
- ✅ All game systems written and compiling
- ✅ Auto-setup Editor script creates scenes, prefabs, materials, ScriptableObjects
- ✅ 5 scenes created with UI, cameras, and wired references
- ✅ 7 club ScriptableObjects, 3 hole designs, 1 starter course
- ✅ Prefabs: golf ball, tee marker, flag pin, hole template, hazards, trees
- ✅ Physics materials for fairway, green, rough, bunker, ball
- ✅ Git repo initialized

### Current Issue: UI Not Rendering
The main menu loads (confirmed in Console logs) but UI text/buttons are invisible. The green background shows but no text appears on top of it.

**Likely cause**: TextMeshPro font assignment issue. The setup script was updated to explicitly assign the LiberationSans SDF font, but needs verification that it's working.

**To debug**:
1. In play mode, expand MainMenu > Canvas > MainMenuPanel > TitleText in Hierarchy
2. Check Inspector: does TextMeshPro - Text (UI) have a Font Asset assigned?
3. If font is "None/Missing", the fix is to ensure `tmp.font = GetDefaultFont()` is working in ProjectSetup.cs
4. Alternative: manually drag "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF" onto the Font Asset field

**Non-critical error**: "ServiceLocator: Service TimeManager not registered" — happens briefly during initialization, not blocking.

### What's Next (in priority order)
1. **Fix UI rendering** - Get main menu visible with working buttons
2. **Verify scene transitions** - Play → MainMenu → GolfGameplay flow
3. **Test golf gameplay** - Ball physics, shot mechanic, scoring
4. **Test building system** - Property view, hole construction, timers
5. **Polish shot feel** - Power/accuracy bar tuning, camera follow smoothing
6. **Add visual feedback** - Ball trail particles, landing dust, score popups
7. **Test economy loop** - Income generation, spending, progression
8. **Implement async multiplayer** - Course sharing/visiting via backend
9. **Mobile optimization** - Touch controls, performance profiling
10. **Audio** - Club hit sounds, ambient music, UI sounds

## How to Run
1. Open project in Unity Hub (Unity 6000.4.1f1)
2. If scenes aren't set up: **Golf Tycoon > Setup Entire Project** (top menu)
3. Open **Assets/Scenes/Boot.unity**
4. Press Play

## How to Re-run Setup
The setup script at `Assets/Scripts/Editor/ProjectSetup.cs` can be re-run anytime via **Golf Tycoon > Setup Entire Project**. It recreates all scenes, prefabs, and ScriptableObjects from scratch.
