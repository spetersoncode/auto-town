using Godot;
using autotown.Core;
using autotown.Entities;

namespace autotown.Data;

/// <summary>
/// Task for constructing a building at a construction site.
/// Generated after all resources have been delivered to the site.
/// </summary>
public partial class BuildTask : Task
{
    /// <summary>
    /// The construction site where building work is performed.
    /// </summary>
    public ConstructionSite Site { get; private set; }

    /// <summary>
    /// Time required to complete construction (in seconds).
    /// </summary>
    private float _buildTime;

    /// <summary>
    /// Progress of construction (0.0 to 1.0).
    /// </summary>
    public float Progress { get; private set; } = 0f;

    /// <summary>
    /// Time elapsed working on construction.
    /// </summary>
    private float _elapsedTime = 0f;

    /// <summary>
    /// Builders can perform build tasks.
    /// </summary>
    public override JobType[] ValidJobTypes => new[] { JobType.Builder };

    /// <summary>
    /// Estimated duration is the build time from building data.
    /// </summary>
    public override float EstimatedDuration => _buildTime;

    /// <summary>
    /// Creates a new build task for a construction site.
    /// </summary>
    /// <param name="position">Position of the construction site</param>
    /// <param name="buildTime">Time required to complete construction</param>
    /// <param name="site">The construction site being worked on</param>
    public BuildTask(Vector2 position, float buildTime, ConstructionSite site)
    {
        Type = TaskType.Build;
        Position = position;
        _buildTime = buildTime;
        Site = site;

        // Build tasks have higher priority than gathering and hauling
        Priority = 2;
    }

    /// <summary>
    /// Checks if this task is still valid (construction site exists and is not complete).
    /// </summary>
    public bool IsValid()
    {
        return Site != null
            && IsInstanceValid(Site)
            && !Site.IsConstructionComplete
            && Site.AreResourcesFullyDelivered;
    }

    public override void OnStart()
    {
        if (!IsValid())
        {
            LogManager.Log(LogManager.DEBUG_BUILD_TASK, $"[BuildTask] Cannot start - construction site is invalid or not ready");
            Cancel();
            return;
        }

        // Mark construction as in progress
        Site.IsConstructionInProgress = true;

        LogManager.Log(LogManager.DEBUG_BUILD_TASK, $"[BuildTask] Started construction of {Site.Data.Name} at {Position}");
    }

    public override void OnUpdate(double delta)
    {
        if (!IsValid())
        {
            Cancel();
            return;
        }

        // Update construction progress
        _elapsedTime += (float)delta;
        Progress = Mathf.Clamp(_elapsedTime / _buildTime, 0f, 1f);

        // Log progress at 25% intervals
        int progressPercent = Mathf.FloorToInt(Progress * 100);
        if (progressPercent > 0 && progressPercent % 25 == 0)
        {
            // Only log once per 25% milestone
            int lastPercent = Mathf.FloorToInt((_elapsedTime - (float)delta) / _buildTime * 100);
            if (lastPercent < progressPercent && progressPercent % 25 == 0)
            {
                LogManager.Log(LogManager.DEBUG_BUILD_TASK, $"[BuildTask] Construction progress: {progressPercent}%");
            }
        }

        // Check if construction is complete
        if (Progress >= 1.0f)
        {
            Complete();
        }
    }

    public override void OnComplete()
    {
        if (!IsValid())
        {
            LogManager.Warning("[BuildTask] Construction site became invalid before completion");
            return;
        }

        LogManager.Log(LogManager.DEBUG_BUILD_TASK, $"[BuildTask] Completed construction of {Site.Data.Name}");

        // Notify the construction site that building is complete
        Site.CompleteConstruction();
    }

    /// <summary>
    /// Cancels the construction task.
    /// </summary>
    public override void Cancel()
    {
        if (Site != null && IsInstanceValid(Site))
        {
            Site.IsConstructionInProgress = false;
        }

        LogManager.Log(LogManager.DEBUG_BUILD_TASK, $"[BuildTask] Construction cancelled");
        base.Cancel();
    }

    /// <summary>
    /// Gets the remaining construction time in seconds.
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, _buildTime - _elapsedTime);
    }
}
