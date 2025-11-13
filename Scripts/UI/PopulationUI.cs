using Godot;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI component that displays current population vs housing capacity.
/// Shows format: 👥 Current/Capacity with color coding based on availability.
/// </summary>
public partial class PopulationUI : Label
{
    private PopulationManager _populationManager;

    private const float CAPACITY_WARNING_THRESHOLD = 0.8f; // Yellow at 80% capacity
    private readonly Color COLOR_NORMAL = new Color(1, 1, 1); // White
    private readonly Color COLOR_WARNING = new Color(1, 0.84f, 0); // Yellow
    private readonly Color COLOR_FULL = new Color(1, 0.3f, 0.3f); // Red

    public override void _Ready()
    {
        // Get PopulationManager reference
        _populationManager = GetNode<PopulationManager>("/root/PopulationManager");

        // Subscribe to population signals
        _populationManager.PopulationChanged += OnPopulationChanged;
        _populationManager.HousingCapacityChanged += OnHousingCapacityChanged;

        // Initial update
        UpdateDisplay();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (_populationManager != null)
        {
            _populationManager.PopulationChanged -= OnPopulationChanged;
            _populationManager.HousingCapacityChanged -= OnHousingCapacityChanged;
        }
    }

    private void OnPopulationChanged(int newPopulation)
    {
        UpdateDisplay();
    }

    private void OnHousingCapacityChanged(int newCapacity, int occupied, int available)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int current = _populationManager.CurrentPopulation;
        int capacity = _populationManager.TotalHousingCapacity;

        // Update text
        Text = $"👥 {current}/{capacity}";

        // Update color based on capacity usage
        if (capacity == 0)
        {
            // No housing yet
            Modulate = COLOR_WARNING;
        }
        else if (current >= capacity)
        {
            // At full capacity
            Modulate = COLOR_FULL;
        }
        else
        {
            float usageRatio = (float)current / capacity;
            if (usageRatio >= CAPACITY_WARNING_THRESHOLD)
            {
                // Near capacity
                Modulate = COLOR_WARNING;
            }
            else
            {
                // Plenty of space
                Modulate = COLOR_NORMAL;
            }
        }
    }
}
