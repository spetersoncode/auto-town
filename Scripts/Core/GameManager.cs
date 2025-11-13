using Godot;

namespace autotown.Core;

/// <summary>
/// Defines the current state of the game.
/// </summary>
public enum GameState
{
    /// <summary>Game is actively running</summary>
    Playing,

    /// <summary>Game is paused</summary>
    Paused,

    /// <summary>Game has ended (win or loss)</summary>
    GameOver
}

/// <summary>
/// Central game manager that coordinates all game systems.
/// Configured as an autoload singleton in project settings.
/// </summary>
public partial class GameManager : Node
{
    // Signals for game state changes
    [Signal]
    public delegate void GameStateChangedEventHandler(GameState newState);

    [Signal]
    public delegate void GameInitializedEventHandler();

    [Signal]
    public delegate void GameSpeedChangedEventHandler(float speed);

    // Properties
    private GameState _currentState = GameState.Playing;
    private float _gameSpeed = GameConfig.GAME_SPEED_NORMAL;

    /// <summary>Gets the current game state</summary>
    public GameState CurrentState => _currentState;

    /// <summary>Gets or sets the game speed multiplier</summary>
    public float GameSpeed
    {
        get => _gameSpeed;
        set
        {
            _gameSpeed = Mathf.Clamp(value, GameConfig.MIN_GAME_SPEED, GameConfig.MAX_GAME_SPEED);
            Engine.TimeScale = _gameSpeed;
            GD.Print($"GameManager: Game speed set to {_gameSpeed}x");
            EmitSignal(SignalName.GameSpeedChanged, _gameSpeed);
        }
    }

    /// <summary>
    /// Called when the node enters the scene tree.
    /// Initializes the game manager.
    /// </summary>
    public override void _Ready()
    {
        // Allow GameManager to process even when game is paused
        ProcessMode = ProcessModeEnum.Always;

        GD.Print("GameManager: Initializing...");
        Initialize();
    }

    /// <summary>
    /// Initializes the game systems.
    /// </summary>
    private void Initialize()
    {
        _currentState = GameState.Playing;
        _gameSpeed = GameConfig.GAME_SPEED_NORMAL;
        Engine.TimeScale = _gameSpeed;

        // Manager systems (autoload singletons initialize themselves):
        // - ResourceManager (✓ implemented)
        // Future managers:
        // - WorkerManager
        // - TaskManager
        // - BuildingManager
        // - PopulationManager

        EmitSignal(SignalName.GameInitialized);
        GD.Print("GameManager: Initialization complete");
    }

    /// <summary>
    /// Changes the current game state.
    /// </summary>
    /// <param name="newState">The new game state</param>
    public void ChangeState(GameState newState)
    {
        if (_currentState == newState)
            return;

        var oldState = _currentState;
        _currentState = newState;

        GD.Print($"GameManager: State changed from {oldState} to {newState}");
        EmitSignal(SignalName.GameStateChanged, (int)newState);

        // Handle state-specific logic
        switch (newState)
        {
            case GameState.Paused:
                GetTree().Paused = true;
                break;
            case GameState.Playing:
                GetTree().Paused = false;
                break;
            case GameState.GameOver:
                GetTree().Paused = true;
                break;
        }
    }

    /// <summary>
    /// Toggles between playing and paused states.
    /// </summary>
    public void TogglePause()
    {
        if (_currentState == GameState.Playing)
            ChangeState(GameState.Paused);
        else if (_currentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    public void NewGame()
    {
        GD.Print("GameManager: Starting new game...");
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void QuitGame()
    {
        GD.Print("GameManager: Quitting game...");
        GetTree().Quit();
    }

    /// <summary>
    /// Handles input for game-level controls (pause, speed, etc.).
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        // Escape key toggles pause
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePause();
        }

        // Number keys for game speed (1-3)
        if (_currentState == GameState.Playing)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Key1:
                        GameSpeed = GameConfig.GAME_SPEED_NORMAL;
                        break;
                    case Key.Key2:
                        GameSpeed = GameConfig.GAME_SPEED_FAST;
                        break;
                    case Key.Key3:
                        GameSpeed = GameConfig.GAME_SPEED_FASTER;
                        break;
                }
            }
        }
    }
}
