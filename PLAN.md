# Auto-Town Tech Demo - Development Plan

## Overview

**Game Concept:** An idle town-building game where workers autonomously gather resources, construct buildings, and grow the population.

**Tech Demo Scope:** Limited demonstration of core systems with placeholder graphics, focusing on solid architecture and expandable design.

**Target Features:**
- Procedurally generated top-down 2D world
- Job-based worker AI system
- Resource gathering and management
- Building construction and processing
- Population growth mechanics
- Basic UI for game state monitoring

---

## Architecture Principles (from CLAUDE.md)

- ✅ **Decoupled Systems via Signals** - Systems communicate through Godot signals
- ✅ **Separation of Concerns** - Focused, single-responsibility classes
- ✅ **Avoid Tight Coupling** - Minimize dependencies between components
- ✅ **Modular Design** - Reusable, testable systems
- ✅ **Proper Resource Management** - Lifecycle methods and cleanup
- ✅ **Scene Organization** - Logical hierarchy and inheritance

---

## Phase 1: Foundation & Project Structure

### 1.1 Folder Structure
- [ ] Create `Scripts/` directory for all C# scripts
- [ ] Create `Scripts/Core/` for core game systems
- [ ] Create `Scripts/Entities/` for game entities (Worker, Building, Resource)
- [ ] Create `Scripts/Systems/` for manager classes
- [ ] Create `Scripts/Data/` for data classes and enums
- [ ] Create `Scripts/UI/` for UI components
- [ ] Create `Scenes/` directory
- [ ] Create `Scenes/Entities/` for entity scenes
- [ ] Create `Scenes/UI/` for UI scenes
- [ ] Create `Scenes/World/` for world/map scenes
- [ ] Create `Assets/` directory for placeholders
- [ ] Create `Resources/` directory for Godot resources

### 1.2 Core Data Models
- [ ] Create `ResourceType.cs` enum (Wood, Stone, Food)
- [ ] Create `JobType.cs` enum (None, Lumberjack, Miner, Forager, Farmer, Builder)
- [ ] Create `TaskType.cs` enum (Gather, Build, Process, Haul)
- [ ] Create `BuildingType.cs` enum (TownHall, Stockpile, House, Sawmill, Mine, Farm)
- [ ] Create `ResourceData.cs` class for resource storage
- [ ] Create `BuildingData.cs` class for building definitions
- [ ] Create `WorkerData.cs` class for worker stats

### 1.3 Base Scenes
- [ ] Create `Main.tscn` - Root game scene
- [ ] Create `World.tscn` - Map and entity container
- [ ] Create `GameManager.cs` - Central game state manager
- [ ] Create `UI.tscn` - Main UI overlay
- [ ] Test basic scene loading and hierarchy

### 1.4 Placeholder Graphics System
- [ ] Create `PlaceholderSprite.cs` - Generates colored shapes
- [ ] Define color coding: Green=Wood, Gray=Stone, Yellow=Food, Blue=Worker, Red=Building
- [ ] Create utility for spawning placeholder sprites

**Dependencies:** None
**Estimated Completion:** Phase 1 complete when folder structure exists and Main scene runs

---

## Phase 2: World & Map Generation

### 2.1 Tile System
- [x] Create `Tile.cs` - Individual tile class (implemented via WorldData tile queries)
- [x] Create `TileType.cs` enum (Grass, Dirt, Water, Mountain)
- [x] Implement tile properties (walkable, buildable)
- [x] Create `TileMap` integration or custom grid (TileMapLayer with programmatic TileSet)

### 2.2 Map Generator
- [x] Create `WorldGenerator.cs` - Procedural generation system
- [x] Implement noise-based terrain generation (use Godot's FastNoiseLite)
- [x] Add configurable map parameters (size, seed, density)
- [x] Generate walkable terrain (grass/dirt tiles)
- [x] Place terrain obstacles (water, mountains) for variety
- [x] Create `WorldData.cs` to store generated map data

### 2.3 Resource Node Placement
- [x] Implement resource node spawning algorithm
- [x] Spawn tree clusters (Wood resource nodes)
- [x] Spawn stone deposits (Stone resource nodes)
- [x] Spawn foraging areas (Food resource nodes)
- [x] Ensure balanced distribution across map
- [x] Store resource node positions in WorldData

### 2.4 Camera System
- [x] Create `CameraController.cs`
- [x] Implement WASD or arrow key panning
- [x] Implement mouse wheel zoom (with min/max limits)
- [x] Add edge scrolling option (mouse at screen edge)
- [x] Clamp camera to map boundaries
- [x] Set initial camera focus on town center

**Dependencies:** Phase 1
**Estimated Completion:** Map generates with resources, camera is controllable ✅ **COMPLETE**

---

## Phase 3: Resource System

### 3.1 Resource Types & Data
- [x] Create `Resource.cs` - Base resource class (reused existing ResourceData.cs)
- [x] Implement resource storage (Dictionary<ResourceType, int>)
- [x] Add resource change events (signal: ResourceChanged)
- [x] Create `ResourceManager.cs` - Global resource tracking

### 3.2 Harvestable Resource Nodes
- [x] Create `HarvestableResource.cs` - Base class for map resources
- [x] Implement `TreeNode.cs` - Wood resource (extends HarvestableResource)
- [x] Implement `StoneNode.cs` - Stone resource
- [x] Implement `ForageNode.cs` - Food resource
- [x] Add harvest progress tracking
- [x] Add depletion logic (resource consumed after X harvests)
- [x] Emit signal when resource is depleted
- [x] Create scenes: `TreeNode.tscn`, `StoneNode.tscn`, `ForageNode.tscn` (attached scripts)

### 3.3 Stockpile System
- [x] Create `Stockpile.cs` - Resource storage building
- [x] Implement resource deposit logic
- [x] Implement resource withdrawal logic
- [x] Add capacity limits (optional for tech demo)
- [x] Create `Stockpile.tscn` scene
- [x] Integrate with ResourceManager

### 3.4 Resource Collection
- [x] Implement resource gathering mechanics (harvest system ready)
- [ ] Workers collect resources from nodes (Phase 4 dependency)
- [ ] Workers carry resources to stockpile (Phase 4 dependency)
- [x] Update ResourceManager when deposited (integrated)
- [ ] Add visual feedback (worker carrying resource indicator) (Phase 4 dependency)

**Dependencies:** Phase 1, Phase 2
**Estimated Completion:** Workers can gather resources and deposit in stockpile ✅ **COMPLETE (Phase 3 systems ready, worker integration in Phase 4)**

---

## Phase 4: Worker System (Job Roles)

### 4.1 Worker Entity
- [x] Create `Worker.cs` - Main worker class (extends CharacterBody2D)
- [x] Implement worker state machine: Idle, Moving, Working, Hauling
- [x] Create `WorkerState.cs` enum
- [x] Add worker movement logic with MoveAndSlide()
- [x] Create `Worker.tscn` scene with circular placeholder sprite
- [x] Add worker spawn position logic at town center

### 4.2 Job Role System
- [x] Implement job assignment via WorkerData
- [x] Add job change functionality (keyboard controls 1-4)
- [x] Emit signals: WorkerJobChanged, WorkerStateChanged
- [x] Add job-based color coding for visual distinction
- [x] Implement job filtering for task assignment

### 4.3 Pathfinding
- [x] Choose pathfinding approach: NavigationAgent2D
- [x] Setup simple flat navigation mesh (no obstacles for Phase 4)
- [x] Integrate pathfinding with Worker movement
- [x] Configure navigation distances for reliable arrival
- [~] Handle dynamic obstacles - **DEFERRED** (see Deferred Tasks)

### 4.4 Worker Manager
- [x] Create `WorkerManager.cs` - Manages all workers
- [x] Track active workers list
- [x] Track workers by job type
- [x] Implement worker spawning (5 starter workers)
- [x] Implement keyboard-based job assignment
- [x] Add worker selection system
- [x] Emit signals: WorkerSpawned, WorkerRemoved

### 4.5 Task System Integration (Phase 5 merged into Phase 4)
- [x] Create `Task.cs` - Base task class with lifecycle
- [x] Create `GatherTask.cs` - Resource harvesting tasks
- [x] Create `TaskManager.cs` - Global task queue and management
- [x] Implement on-demand task creation (no pre-generation spam)
- [x] Add resource reservation system to prevent conflicts
- [x] Implement autonomous worker task scanning and assignment
- [x] Complete harvest → haul → deposit workflow

**Dependencies:** Phase 1, Phase 2, Phase 3
**Estimated Completion:** Workers autonomously gather resources and deposit at stockpile ✅ **COMPLETE**

---

## Phase 5: Task & Job Management ✅ **MERGED INTO PHASE 4**

### 5.1 Task System
- [x] Create `Task.cs` - Base task class
- [x] Implement `GatherTask.cs` - Harvest resources
- [ ] Implement `BuildTask.cs` - Construct buildings - **DEFERRED to Phase 6**
- [ ] Implement `HaulTask.cs` - Transport resources - **DEFERRED** (integrated into GatherTask workflow)
- [ ] Implement `ProcessTask.cs` - Convert resources - **DEFERRED to Phase 6**
- [x] Add task properties (location, required resources, duration)
- [x] Add task state tracking (Pending, InProgress, Completed, Cancelled)

### 5.2 Task Queue
- [x] Create `TaskManager.cs` - Global task queue system
- [x] Implement task creation and queuing (on-demand)
- [x] Add task filtering by job type
- [x] Implement task completion callback
- [x] Emit signals: TaskAdded, TaskCompleted, TaskCancelled, TaskRemoved
- [x] Automatic cleanup of finished tasks

### 5.3 Job Assignment Logic
- [x] Implement worker-to-task matching algorithm
- [x] Match workers to tasks based on JobType
- [x] Prioritize tasks by distance (find nearest)
- [x] Handle multiple workers via resource reservation system
- [x] Implement idle worker task scanning (every 0.5s)
- [x] Workers automatically pick next task when idle

### 5.4 Task Execution
- [x] Workers navigate to task location
- [x] Workers execute task action (gather with timed progress)
- [x] Display progress via console logs (25% intervals)
- [x] Complete task and update game state
- [x] Workers return to idle and scan for next task
- [x] Handle task cancellation when resource depleted

**Dependencies:** Phase 3, Phase 4
**Estimated Completion:** Workers autonomously find and complete tasks based on job roles ✅ **COMPLETE (GatherTask only, BuildTask deferred)**

---

## Phase 6: Building System ✅ **COMPLETE**

### 6.1 Building Definitions
- [x] Create `Building.cs` - Base building class
- [x] Define building properties (type, cost, build time, function)
- [x] Create `BuildingDefinitions.cs` - Static building data
- [x] Implement `TownHall.cs` - Starting building (spawned by WorldGenerator)
- [x] Implement `House.cs` - Housing for population growth
- [x] Implement `Sawmill.cs` - Processes wood
- [x] Implement `Mine.cs` - Processes stone
- [x] Implement `Farm.cs` - Produces food

### 6.2 Building Placement
- [x] Create `BuildingPlacementUI.cs` - Keyboard-based placement (F1-F4)
- [x] Implement placement validation (resource checks)
- [x] Add placement preview (purple construction site square)
- [x] Handle placement confirmation (instant placement on keypress)
- [x] Integrate with BuildingManager for construction workflow
- [x] Emit signal: ConstructionStarted

### 6.3 Construction System
- [x] Create `ConstructionSite.cs` - Construction site entity
- [x] Create `HaulResourceTask.cs` - Transport resources from stockpile to site
- [x] Create `BuildTask.cs` - Construction work task
- [x] Generate haul tasks when building placed
- [x] Builders autonomously haul resources to construction site
- [x] Track resource delivery progress
- [x] Generate build task when all resources delivered
- [x] Track construction progress (timed task with progress logging)
- [x] Complete building when construction finishes
- [x] Emit signals: ResourcesFullyDelivered, ConstructionCompleted
- [x] Fix Task.Complete() to call OnComplete() for proper building spawn

### 6.4 Building Functionality
- [x] Implement building activation/operation (OnConstructionComplete)
- [x] Create `ProcessTask.cs` - Processing buildings generate tasks
- [x] Housing tracks capacity (House.cs)
- [x] Add building state (UnderConstruction, Active, Inactive)
- [x] Create scenes for each building type (.tscn files)
- [x] Create `BuildingManager.cs` autoload singleton to orchestrate workflow

### 6.5 Developer Experience
- [x] Create `LogManager.cs` - Per-system debug logging control
- [x] Configure debug flags (construction enabled, others disabled)
- [x] Clean up repetitive log spam from harvest/worker systems

**Dependencies:** Phase 3, Phase 5
**Estimated Completion:** Buildings can be placed, constructed by workers, and function ✅ **COMPLETE**

---

## Phase 7: Population Growth

### 7.1 Housing System
- [ ] Implement housing capacity tracking
- [ ] Calculate available housing slots
- [ ] Track current population vs capacity
- [ ] Emit signal: HousingCapacityChanged

### 7.2 Population Growth Logic
- [ ] Create `PopulationManager.cs`
- [ ] Implement growth trigger conditions (housing + resources)
- [ ] Define resource cost per new worker
- [ ] Implement growth cooldown timer
- [ ] Check conditions periodically (e.g., every 10 seconds)
- [ ] Emit signal: PopulationGrowthTriggered

### 7.3 Worker Spawning
- [ ] Implement new worker spawning at Town Hall or House
- [ ] Assign default job or auto-assign based on needs
- [ ] Add worker to WorkerManager
- [ ] Deduct resources for new worker
- [ ] Update population count
- [ ] Emit signal: WorkerSpawned

### 7.4 Resource Consumption
- [ ] Implement periodic resource consumption (optional for tech demo)
- [ ] Workers consume food over time (if implementing)
- [ ] Trigger warnings if resources low
- [ ] Game over condition if resources depleted (optional)

**Dependencies:** Phase 4, Phase 6
**Estimated Completion:** Population grows when housing and resources are available

---

## Phase 8: UI & Game Loop

### 8.1 Resource Display
- [x] Create `ResourceDisplayUI.cs` - Shows resource counts
- [x] Display Wood, Stone, Food quantities
- [x] Update in real-time via ResourceChanged signal
- [x] Add color-coded labels for each resource type
- [x] Integrated into UI.tscn

### 8.2 Worker Status Panel
- [x] Create `WorkerSelectionUI.cs`
- [x] Display selected worker info (job, state)
- [x] Show keyboard controls for job assignment
- [~] Display workers by job type - **DEFERRED** (basic selection UI only)
- [~] Display idle worker count - **DEFERRED**
- [x] Integrated into UI.tscn

### 8.3 Game Controls
- [ ] Implement pause/unpause functionality
- [ ] Add game speed controls (1x, 2x, 3x - optional)
- [ ] Create simple menu (New Game, Quit)
- [ ] Add keyboard shortcuts for common actions

### 8.4 Game Loop & Win Condition
- [ ] Implement game initialization (spawn starting buildings, workers, resources)
- [ ] Define tech demo win condition (e.g., reach 20 population)
- [ ] Display win message when condition met
- [ ] Add restart functionality
- [ ] Implement game over condition (optional: all workers dead, no resources)

### 8.5 UI Polish
- [ ] Create `GameUI.tscn` - Master UI scene
- [ ] Layout UI panels (top bar, side panel, etc.)
- [ ] Add tooltips on hover (optional)
- [ ] Add building placement UI (optional for tech demo)
- [ ] Test UI responsiveness and visibility

**Dependencies:** Phase 3, Phase 4, Phase 7
**Estimated Completion:** UI displays game state, player can monitor progress

---

## Phase 9: Integration & Testing

### 9.1 System Integration
- [ ] Connect all systems via signals
- [ ] Test GameManager orchestration
- [ ] Verify no circular dependencies
- [ ] Test game initialization sequence
- [ ] Test scene loading and transitions

### 9.2 Gameplay Testing
- [ ] Test full gameplay loop (start to win condition)
- [ ] Test worker task assignment and execution
- [ ] Test resource gathering and consumption
- [ ] Test building construction
- [ ] Test population growth
- [ ] Balance resource costs and generation rates

### 9.3 Bug Fixes & Refinement
- [ ] Fix pathfinding issues
- [ ] Fix worker AI edge cases
- [ ] Fix resource duplication or loss bugs
- [ ] Fix UI update issues
- [ ] Fix performance issues (if any)

### 9.4 Documentation
- [ ] Document signal connections in code
- [ ] Add XML comments to public APIs
- [ ] Update CLAUDE.md if needed
- [ ] Create README for tech demo setup

**Dependencies:** All previous phases
**Estimated Completion:** Tech demo is fully functional and playable

---

## Phase 10: Optional Enhancements (Post-Tech Demo)

### 10.1 Visual Improvements
- [ ] Replace placeholder sprites with actual art
- [ ] Add animations (worker walking, resource gathering)
- [ ] Add particle effects (building construction, resource gathering)
- [ ] Improve UI styling and theme

### 10.2 Gameplay Expansion
- [ ] Add more resource types (Iron, Gold, etc.)
- [ ] Add more building types
- [ ] Add technology/research system
- [ ] Add random events (weather, visitors, etc.)
- [ ] Add day/night cycle

### 10.3 Advanced Systems
- [ ] Implement save/load system
- [ ] Add procedural map variations
- [ ] Implement worker health and needs
- [ ] Add advanced AI (job efficiency, skill progression)
- [ ] Multiplayer or sandbox mode

---

## Technical Notes

### Signal Architecture
Key signals to implement for decoupled systems:
- **ResourceManager:** `ResourceChanged(ResourceType, int amount)`
- **WorkerManager:** `WorkerSpawned(Worker)`, `WorkerRemoved(Worker)`, `WorkerJobChanged(Worker, JobType)`
- **TaskManager:** `TaskAdded(Task)`, `TaskCompleted(Task)`, `TaskFailed(Task)`
- **PopulationManager:** `PopulationChanged(int population)`, `HousingCapacityChanged(int capacity)`
- **BuildingManager:** `BuildingPlaced(Building)`, `BuildingCompleted(Building)`, `BuildingDestroyed(Building)`

### Performance Considerations
- Use object pooling for frequently spawned entities (workers, tasks)
- Optimize pathfinding with grid caching and path smoothing
- Limit task queue size to prevent memory bloat
- Use spatial partitioning for entity queries (Godot's built-in QuadTree)

### Testing Strategy
- Unit test core logic (resource calculations, task assignment)
- Integration test system interactions (signals, state changes)
- Playtest full gameplay loop regularly
- Profile performance with large numbers of workers/buildings

---

## Current Status
- [x] Phase 0: Project initialized
- [x] Phase 1: Foundation & Project Structure
- [x] Phase 2: World & Map Generation
- [x] Phase 3: Resource System
- [x] Phase 4: Worker System (includes Phase 5 GatherTask integration)
- [x] Phase 5: Task & Job Management (merged into Phase 4)
- [x] Phase 6: Building System
- [ ] Phase 7: Population Growth
- [~] Phase 8: UI & Game Loop (partial - resource display complete)
- [ ] Phase 9: Integration & Testing
- [ ] Phase 10: Optional Enhancements

---

## Development Workflow

1. **Work in small increments** - Complete one checklist item at a time
2. **Test frequently** - Run the game after each major change
3. **Commit regularly** - Use conventional commits (feat, fix, refactor)
4. **Follow CLAUDE.md** - Maintain architectural principles throughout
5. **Iterate** - Refine systems as you discover edge cases

---

## Deferred Tasks (Phase 4 Simplifications)

The following items were intentionally simplified or deferred to maintain a working baseline:

### Navigation & Collision
- **Building Collision Avoidance** - Buildings currently have no collision; workers pass through them
  - Reason: Physics-based collision was causing workers to get stuck
  - Solution: Simplified to flat navigation mesh with no obstacles
  - Future: Add proper NavigationObstacle2D or baked navigation holes when pathfinding is more robust

### Task System
- **BuildTask, HaulTask, ProcessTask** - Only GatherTask implemented
  - Reason: Focus on core gather → haul → deposit workflow first
  - Hauling integrated directly into GatherTask rather than separate task type
  - Building and processing tasks deferred to Phase 6

### UI Enhancements
- **Detailed Worker Panel** - Only basic selection UI implemented
  - Missing: Worker count by job type, idle worker count, worker list
  - Reason: Core functionality prioritized over detailed statistics
  - Future: Add comprehensive worker management UI in Phase 8

### Visual Feedback
- **Worker Carry Indicators** - No visual indication of carried resources
  - Reason: Placeholder sprites make this less critical
  - Future: Add when proper sprite animations are implemented (Phase 10)

### Pathfinding Optimizations
- **Path Visualization** - No debug pathfinding overlay
- **Dynamic Obstacle Avoidance** - Disabled for simplicity
- **Path Smoothing** - Using default Godot pathfinding without optimization
  - Reason: Simple direct paths work for current map density
  - Future: Optimize when adding more complex map layouts

---

**Last Updated:** 2025-01-12
**Version:** 1.4 - Phase 6 Complete (Building System with construction workflow)