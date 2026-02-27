using System;
using System.Collections.ObjectModel;
using System.Linq;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Services;
using System.Text.RegularExpressions;
using NotionDeadlineFairy.Models;
using Microsoft.VisualBasic.Logging;
using NotionDeadlineFairy.Utils;

namespace NotionDeadlineFairy.ViewModels
{
    public class MainViewModel : BaseViewModel, IWidget
    {
        private readonly NotionService _notionService;

        private ObservableCollection<TaskItemViewModel> _taskList;
        public ObservableCollection<TaskItemViewModel> TaskList
        {
            get => _taskList;
            set { _taskList = value; OnPropertyChanged(); }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isSettingsVisible = false;
        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set { _isSettingsVisible = value; OnPropertyChanged(); }
        }
        private double _width = 350;
        private double _height = 500;
        private double _left = 300;
        private double _top = 300;

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }
        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }
        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(); }
        }
        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(); }
        }


        private string _backgroundColor = "#FFFFFFFF";
        private string _foregroundColor = "#000000FF";
        public string BackgroundColor { get => _backgroundColor; set { if (_backgroundColor == value) return;  _backgroundColor = value; OnPropertyChanged(); EnabledSave = ValidateColor(value) && ValidateColor(ForegroundColor); } }
        public string ForegroundColor { get => _foregroundColor; set { if (_foregroundColor == value) return;  _foregroundColor = value; OnPropertyChanged(); EnabledSave = ValidateColor(value) && ValidateColor(BackgroundColor); } }
        private bool _enabledSave = false;
        public bool EnabledSave { get => _enabledSave; set { if (_enabledSave == value) return; _enabledSave = value; OnPropertyChanged(); } }

        public RelayCommand IncrementCommand { get; }
        public RelayCommand DecrementCommand { get; }

        public MainViewModel()
        {
            _notionService = NotionService.Instance;
            TaskList = new ObservableCollection<TaskItemViewModel>();

            Refresh();
            
            ServiceLocator.Instance.Register<IWidget>(this);

            var settings = SettingService.Instance.Current;
            this.Width = settings.WindowWidth;
            this.Height = settings.WindowHeight;
            this.Left = settings.WindowLeft;
            this.Top = settings.WindowTop;
            this.BackgroundColor = settings.BackgroundColor;
            this.ForegroundColor = settings.ForegroundColor;
        }

        public void Refresh()
        {
            var settings = SettingService.Instance.Current;

            if (settings == null || settings.DatabaseConfigs.Count == 0)
            {
                TaskList = new ObservableCollection<TaskItemViewModel>();
                return;
            }

            Task.Run(async () =>
            {
                var rawData = await _notionService.GetAllDatabaseItemsAsync();

                DispatcherHelper.BeginInvoke(() =>
                {
                    if (rawData != null && rawData.Any())
                    {
                        var vms = rawData.Select(d => new TaskItemViewModel(d));
                        TaskList = new ObservableCollection<TaskItemViewModel>(vms);
                    }
                    else
                    {
                        TaskList = new ObservableCollection<TaskItemViewModel>();
                    }
                });
            });
        }

        public void SetEditMode(bool enabled)
        {
            this.IsEditMode = enabled;
            if(this.IsEditMode == false)
            {
                this.SaveCurrentPosition();
            }
        }

        public void SetClickThrough(bool enabled)
        {
            // TODO: 코드 구현
            // throw new NotImplementedException();
        }

        private async Task InitializeAsync()
        {
            var list = await this._notionService.GetAllDatabaseItemsAsync();
        }

        public void SaveCurrentPosition()
        {
            var settings = SettingService.Instance.Current;
            
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;
            settings.WindowLeft = this.Left;
            settings.WindowTop = this.Top;

            SettingService.Instance.Save();
        }

        private bool ValidateColor(string hex)
        {
            string hexPattern = @"^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$";
            return Regex.IsMatch(hex, hexPattern);
        }


        public void SaveCurrentColor()
        {
            var settings = SettingService.Instance.Current;

            settings.BackgroundColor = this.BackgroundColor;
            settings.ForegroundColor = this.ForegroundColor;

            SettingService.Instance.Save();
        }
    }
}