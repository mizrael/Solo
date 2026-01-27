namespace Solo.Services;

public class GamePauseManager
{
    private GamePauseManager() { }

    private static Lazy<GamePauseManager> _instance = new(() => new GamePauseManager());
    public static GamePauseManager Instance => _instance.Value;

    private readonly HashSet<object> _pauseRequesters = new();

    public bool IsPaused => _pauseRequesters.Count > 0;

    public event Action<bool>? OnPauseChanged;

    public void RequestPause(object requester)
    {
        if (_pauseRequesters.Add(requester) && _pauseRequesters.Count == 1)
            OnPauseChanged?.Invoke(true);
    }

    public void ReleasePause(object requester)
    {
        if (_pauseRequesters.Remove(requester) && _pauseRequesters.Count == 0)
            OnPauseChanged?.Invoke(false);
    }
}
