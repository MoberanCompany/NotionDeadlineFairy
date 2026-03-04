using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Threading;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.ViewModels;

namespace NotionDeadlineFairy.ViewModels
{
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

        // 1. 타이머 글자색
        private System.Windows.Media.Brush _timerColor = System.Windows.Media.Brushes.Black;
        public System.Windows.Media.Brush TimerColor
        {
            get => _timerColor;
            set { _timerColor = value; OnPropertyChanged(); }
        }

        // 2. 카드 배경색
        private System.Windows.Media.Brush _cardBackground = System.Windows.Media.Brushes.White;
        public System.Windows.Media.Brush CardBackground
        {
            get => _cardBackground;
            set { _cardBackground = value; OnPropertyChanged(); }
        }

        private double _timerFontScale = 1.0;
        public double TimerFontScale
        {
            get => _timerFontScale;
            set { 
                _timerFontScale = value;
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TimerFontSize)); 
            }
        }

        public double TimerFontSize
        {
            get => SettingService.Instance.Current.FontSize * _timerFontScale;
        }

        public TaskItemViewModel(NotionPageData data)
        {
            _data = data;
            if (HasDeadline)
            {
                StartTimer();
            }
        }

        public void ReDraw()
        {
            OnPropertyChanged(nameof(TimerFontSize));
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

            UpdateStyles(diff);
        }

        private void UpdateStyles(TimeSpan diff)
        {
            var settings = SettingService.Instance.Current;

            System.Windows.Media.Brush userDefaultForeground = System.Windows.Media.Brushes.Black;
            System.Windows.Media.Brush userDefaultBackground = System.Windows.Media.Brushes.White;

            try
            {
                // 글자색 변환
                if (!string.IsNullOrEmpty(settings.ForegroundColor))
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.ForegroundColor);
                    userDefaultForeground = new System.Windows.Media.SolidColorBrush(color);
                }

                // 배경색 변환
                if (!string.IsNullOrEmpty(settings.BackgroundColor))
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.BackgroundColor);
                    userDefaultBackground = new System.Windows.Media.SolidColorBrush(color);
                }
            }
            catch { }

           
            if (diff.Ticks <= 0) // 기한 지남
            {
                TimerColor = System.Windows.Media.Brushes.White;
                TimerFontScale = 1.0;
            }
            else if (diff.TotalHours < 1) // 1시간 미만
            {
                TimerColor = System.Windows.Media.Brushes.White;
                TimerFontScale = 1.4;
            }
            else if (diff.TotalDays < 1) // 하루 미만
            {
                TimerColor = System.Windows.Media.Brushes.Orange;
                TimerFontScale = 1.0;
            }
            else // 평상시
            {
                TimerColor = userDefaultForeground;
                TimerFontScale = 1.0;
            }
            CardBackground = userDefaultBackground;

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
}