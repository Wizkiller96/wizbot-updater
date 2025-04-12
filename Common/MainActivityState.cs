namespace wizbotupdater;
/// <summary>
/// Defines which states the bot can be in
/// </summary>
public enum MainActivityState
{
    /// <summary>
    /// Bot is running, no action permitted except STOP
    /// </summary>
    Running,

    /// <summary>
    /// Bot is not running and is on the latest version
    /// </summary>
    Runnable,

    /// <summary>
    /// Bot is not running and there's a newer version available
    /// </summary>
    Updatable,

    /// <summary>
    /// Bot is not installed
    /// </summary>
    Downloadable
}