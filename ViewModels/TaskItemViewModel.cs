using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.ViewModels;
using System.Windows.Threading;

public class TaskItemViewModel : BaseViewModel
{
    private NotionPageData _data;
    private DispatcherTimer? _internalTimer;

    public string Title => _data.Title;
    public string DatabaseTitle => _data.DatabaseTitle;
    public string Url => _data.Url;
    public DateTime? EndAt => _data.EndAt;

    public Dictionary<string, NotionField> Values => _data.Values;
    public bool HasDeadline => _data.EndAt.HasValue;

    private string _remainingTimeText = string.Empty;
    public string RemainingTimeText
    {
        get => _remainingTimeText;
        set { _remainingTimeText = value; OnPropertyChanged(); }
    }

    public TaskItemViewModel(NotionPageData data)
    {
        _data = data;
        if (HasDeadline)
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        _internalTimer = new DispatcherTimer();
        _internalTimer.Interval = TimeSpan.FromSeconds(1);
        _internalTimer.Tick += (s, e) => UpdateRemainingTime();
        _internalTimer.Start();
        UpdateRemainingTime();
    }

    private void UpdateRemainingTime()
    {
        if (!EndAt.HasValue)
        {
            RemainingTimeText = "";
            return;
        }

        TimeSpan diff = EndAt.Value - DateTime.Now;

        if (diff.Ticks <= 0)
        {
            RemainingTimeText = "기한지남";
        }
        else if (diff.TotalDays >= 1)
        {
            RemainingTimeText = $"{(int)diff.TotalDays}일 {diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
        }
        else
        {
            RemainingTimeText = $"{diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
        }

        OnPropertyChanged(nameof(EndAt));
    }

    public void Update(NotionPageData newData)
    {
        _internalTimer?.Stop();
        _internalTimer = null;

        _data = newData;
        OnPropertyChanged(string.Empty);
        UpdateRemainingTime();
    }
}