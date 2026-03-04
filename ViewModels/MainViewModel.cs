using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.Generic;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.Utils;
using FontFamily = System.Windows.Media.FontFamily;

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

        public double Width { get => _width; set { _width = value; OnPropertyChanged(); } }
        public double Height { get => _height; set { _height = value; OnPropertyChanged(); } }
        public double Left { get => _left; set { _left = value; OnPropertyChanged(); } }
        public double Top { get => _top; set { _top = value; OnPropertyChanged(); } }

        private string _backgroundColor = "#FFFFFFFF";
        private string _foregroundColor = "#000000FF";
        public string BackgroundColor { get => _backgroundColor; set { if (_backgroundColor == value) return; _backgroundColor = value; OnPropertyChanged(); EnabledSave = ValidateInput(); } }
        public string ForegroundColor { get => _foregroundColor; set { if (_foregroundColor == value) return; _foregroundColor = value; OnPropertyChanged(); EnabledSave = ValidateInput(); } }

        public SolidColorBrush ForegroundBrush
        {
            get => new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ForegroundColor));
        }

        public SolidColorBrush BackgroundBrush
        {
            get => new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(BackgroundColor));
        }

        public double DefaultFontSize
        {
            get => FontSize;
        }

        public double DetailFontSize
        {
            get => FontSize * 0.8;
        }
        public double DatabaseTitleFontSize
        {
            get => FontSize * 0.6;
        }
        public FontFamily DefaultFontFamily
        {
            get => SelectedFontFamily;
        }

        private bool _enabledSave = false;
        public bool EnabledSave { get => _enabledSave; set { if (_enabledSave == value) return; _enabledSave = value; OnPropertyChanged(); } }

        public IReadOnlyList<FontFamily> SystemFonts { get; } = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();

        private double _fontSize = 12;
        public double FontSize { get => _fontSize; set { _fontSize = value; OnPropertyChanged(); EnabledSave = ValidateInput(); } }

        private FontFamily _selectedFontFamily = new FontFamily("Segoe UI");
        public FontFamily SelectedFontFamily { get => _selectedFontFamily; set { _selectedFontFamily = value; OnPropertyChanged(); EnabledSave = ValidateInput(); } }
        public DateType[] DateTypeOptions { get; } =
            Enum.GetValues(typeof(DateType)).Cast<DateType>().ToArray();

        private DateType _dateType = DateType.NONE;
        public DateType DateType
        {
            get => _dateType;
            set
            {
                if (_dateType != value)
                {
                    _dateType = value;
                    OnPropertyChanged();
                    Refresh();
                }
            }
        }

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
        }

        //  날짜 필터 메서드
        private bool PassDate(NotionPageData t, DateTime? upper)
        {
            if (t.EndAt == null) return false;
            if (upper == null) return true;
            return t.EndAt.Value <= upper.Value;
        }

        public void Refresh()
        {
            var settings = SettingService.Instance.Current;

            if (settings == null || settings.DatabaseConfigs.Count == 0)
            {
                TaskList.Clear();
                return;
            }

            Task.Run(async () =>
            {
                var rawData = await _notionService.GetAllDatabaseItemsAsync();

                DispatcherHelper.BeginInvoke(() =>
                {
                    TaskList.Clear();
                    if (rawData != null && rawData.Any())
                    {
                        var upper = DateUtil.getTime(DateType);
                        var filtered = rawData.Where(t => PassDate(t, upper)).ToList();

                        var vms = filtered.Select(d => new TaskItemViewModel(d));
                        foreach (var vm in vms)
                        {
                            TaskList.Add(vm);
                        }
                    }
                });
            });
        }

        public void SetEditMode(bool enabled)
        {
            this.IsEditMode = enabled;
            if (this.IsEditMode == false)
                this.SaveCurrentPosition();
        }

        public void SetClickThrough(bool enabled) { }

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

        private bool ValidateFontSize(double size) => size >= 8 && size <= 100;

        private bool ValidateFontFamily(FontFamily family)
            => family != null && !string.IsNullOrWhiteSpace(family.Source);

        private bool ValidateInput()
            => ValidateColor(BackgroundColor) && ValidateColor(ForegroundColor)
            && ValidateFontSize(FontSize) && ValidateFontFamily(SelectedFontFamily);

        public void SaveThemeSettings()
        {
            var settings = SettingService.Instance.Current;
            settings.BackgroundColor = this.BackgroundColor;
            settings.ForegroundColor = this.ForegroundColor;
            settings.FontSize = this.FontSize;
            settings.FontFamily = this.SelectedFontFamily.Source;
            SettingService.Instance.Save();

            OnPropertyChanged(nameof(DefaultFontFamily));
            OnPropertyChanged(nameof(DefaultFontSize));
            OnPropertyChanged(nameof(DetailFontSize));
            OnPropertyChanged(nameof(DatabaseTitleFontSize));
            OnPropertyChanged(nameof(ForegroundBrush));
            OnPropertyChanged(nameof(BackgroundBrush));
            OnPropertyChanged(nameof(BackgroundBrush));

            var views = ServiceLocator.Instance.GetService<IWidget>();
            if (views == null) return;
            foreach (var view in views)
            {
                try { view.ReDraw(); }
                catch (Exception) { }
            }
        }

        public void ReDraw() 
        {
            foreach (var item in TaskList)
            {
                item.ReDraw();
            }
        }
    }
}