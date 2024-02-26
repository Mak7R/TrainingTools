namespace SimpleAuthorizer;

public class AutoClearedSessionContainer : SessionContainer
{
    private bool _isClearing = true;
    private Task? _clearingTask;

    private TimeSpan _cleaningChecksFrequency = new TimeSpan(0, 0, 5, 0); // 5 min
    private TimeSpan _sessionTime = new TimeSpan(0, 1, 0, 0); // 1 hour
    
    public TimeSpan CleaningChecksFrequency
    {
        get => _cleaningChecksFrequency;
        set
        {
            if (_isClearing)
            {
                StopClearing();
                _cleaningChecksFrequency = value;
                RunClearing();
            }
            else
            {
                _cleaningChecksFrequency = value;
            }
        }
    } 

    public TimeSpan SessionTime
    {
        get => _sessionTime;
        set
        {
            if (_isClearing)
            {
                StopClearing();
                _sessionTime = value;
                RunClearing();
            }
            else
            {
                _sessionTime = value;
            }
        } 
    } 
    public bool IsClearing { get; private set; }
    
    public AutoClearedSessionContainer()
    {
        RunClearing();
    }
    
    private async Task ClearingAsync()
    {
        while (_isClearing)
        {
            await Task.Delay(CleaningChecksFrequency);
            Clear();
        }
    }
    
    private void Clear()
    {
        var currentTime = DateTime.Now;
        Authorizations.RemoveAll(s => (currentTime - s.AuthDateTime) > SessionTime);
    }

    public void RunClearing()
    {
        if (IsClearing) return;
        if (_clearingTask != null) return;
        
        _clearingTask = ClearingAsync();
        _isClearing = true;
        IsClearing = true;
    }
    
    public async void StopClearing()
    {
        if (!IsClearing) return;

        _isClearing = false;
        if (_clearingTask != null)
        {
            await _clearingTask;
            _clearingTask = null;
        }
        IsClearing = false;
    }
}