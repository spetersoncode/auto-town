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
- [ ] Create `Worker.cs` - Main worker class (extends CharacterBody2D or Area2D)
- [ ] Implement worker state machine: Idle, Moving, Working, Hauling
- [ ] Create `WorkerState.cs` enum
- [ ] Add worker movement logic
- [ ] Create `Worker.tscn` scene with placeholder sprite
- [ ] Add worker spawn position logic

### 4.2 Job Role System
- [ ] Create `JobRole.cs` - Defines worker's assigned job
- [ ] Implement job assignment logic
- [ ] Create job skill system (optional efficiency modifiers)
- [ ] Add job change functionality
- [ ] Emit signals: WorkerJobChanged, WorkerStateChanged

### 4.3 Pathfinding
- [ ] Choose pathfinding approach (Godot NavigationAgent2D or A*)
- [ ] If using NavigationAgent2D: Setup navigation mesh
- [ ] If using A*: Implement grid-based pathfinding
- [ ] Integrate pathfinding with Worker movement
- [ ] Add path visualization for debugging (toggle)
- [ ] Handle dynamic obstacles (buildings, other workers)

### 4.4 Worker Manager
- [ ] Create `WorkerManager.cs` - Manages all workers
- [ ] Track active workers list
- [ ] Track idle workers
- [ ] Implement worker spawning
- [ ] Implement worker assignment to jobs
- [ ] Emit signals: WorkerSpawned, WorkerRemoved

**Dependencies:** Phase 1, Phase 2, Phase 3
**Estimated Completion:** Workers can move around map and be assigned jobs

---

## Phase 5: Task & Job Management

### 5.1 Task System
- [ ] Create `Task.cs` - Base task class
- [ ] Implement `GatherTask.cs` - Harvest resources
- [ ] Implement `BuildTask.cs` - Construct buildings
- [ ] Implement `HaulTask.cs` - Transport resources
- [ ] Implement `ProcessTask.cs` - Convert resources (sawmill, etc.)
- [ ] Add task properties (location, required resources, duration)
- [ ] Add task state tracking (Pending, InProgress, Completed, Failed)

### 5.2 Task Queue
- [ ] Create `TaskManager.cs` - Global task queue system
- [ ] Implement task creation and queuing
- [ ] Implement task priority system (urgent, normal, low)
- [ ] Add task filtering by job type
- [ ] Implement task completion callback
- [ ] Emit signals: TaskAdded, TaskCompleted, TaskFailed

### 5.3 Job Assignment Logic
- [ ] Implement worker-to-task matching algorithm
- [ ] Match workers to tasks based on JobType
- [ ] Prioritize tasks by distance and priority
- [ ] Handle multiple workers competing for same task
- [ ] Implement idle worker task scanning
- [ ] Add logic for workers to switch tasks if needed

### 5.4 Task Execution
- [ ] Workers navigate to task location
- [ ] Workers execute task action (gather, build, etc.)
- [ ] Display progress indicator on worker
- [ ] Complete task and update game state
- [ ] Workers return to idle or pick next task
- [ ] Handle task interruption or cancellation

**Dependencies:** Phase 3, Phase 4
**Estimated Completion:** Workers autonomously find and complete tasks based on job roles

---

## Phase 6: Building System

### 6.1 Building Definitions
- [ ] Create `Building.cs` - Base building class
- [ ] Define building properties (type, cost, build time, function)
- [ ] Create `BuildingDefinitions.cs` - Static building data
- [ ] Implement `TownHall.cs` - Starting building
- [ ] Implement `House.cs` - Housing for population growth
- [ ] Implement `Sawmill.cs` - Processes wood (if needed)
- [ ] Implement `Mine.cs` - Processes stone (if needed)
- [ ] Implement `Farm.cs` - Produces food

### 6.2 Building Placement
- [ ] Create `BuildingPlacer.cs` - Handles placement logic
- [ ] Implement placement validation (terrain, space, overlap)
- [ ] Add placement preview (ghost building)
- [ ] Handle placement confirmation
- [ ] Update world grid with building footprint
- [ ] Emit signal: BuildingPlacementRequested

### 6.3 Construction System
- [ ] Create construction site entity
- [ ] Generate BuildTask when building placed
- [ ] Workers assigned as Builders take construction tasks
- [ ] Track construction progress
- [ ] Consume resources during construction
- [ ] Complete building when construction finishes
- [ ] Emit signals: ConstructionStarted, ConstructionCompleted

### 6.4 Building Functionality
- [ ] Implement building activation/operation
- [ ] Processing buildings generate ProcessTasks
- [ ] Housing tracks occupancy
- [ ] Buildings consume resources if needed (optional)
- [ ] Add building state (UnderConstruction, Active, Inactive)
- [ ] Create scenes for each building type

**Dependencies:** Phase 3, Phase 5
**Estimated Completion:** Buildings can be placed, constructed by workers, and function

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
- [ ] Create `ResourcePanel.cs` - Shows resource counts
- [ ] Display Wood, Stone, Food quantities
- [ ] Update in real-time via ResourceChanged signal
- [ ] Add icons/labels for each resource type
- [ ] Create `ResourcePanel.tscn`

### 8.2 Worker Status Panel
- [ ] Create `WorkerPanel.cs`
- [ ] Display total worker count
- [ ] Display workers by job type (Lumberjack: 3, Miner: 2, etc.)
- [ ] Display idle worker count
- [ ] Update via WorkerManager signals
- [ ] Create `WorkerPanel.tscn`

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
- [ ] Phase 4: Worker System
- [ ] Phase 5: Task & Job Management
- [ ] Phase 6: Building System
- [ ] Phase 7: Population Growth
- [ ] Phase 8: UI & Game Loop
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

**Last Updated:** 2025-01-12
**Version:** 1.2 - Phase 3 Complete