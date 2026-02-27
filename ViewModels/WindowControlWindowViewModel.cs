using NotionDeadlineFairy.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.ViewModels
{
    internal class WindowControlWindowViewModel : BaseViewModel
    {
        private bool _isSettingsVisible = false;
        private string _backgroundColor = "#FFFFFFFF";
        public string BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; OnPropertyChanged(); } }
        private string _foregroundColor = "#000000FF";
        public string ForegroundColor { get => _foregroundColor; set { _foregroundColor = value; OnPropertyChanged(); } }

        public bool IsSettingsVisible  { 
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

        private bool _isEditMode = true;
        public bool IsEditMode
        { get => _isEditMode; set {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();
                    if (_isEditMode == false)
                    {
                        SaveCurrentSettings();
                    }
                }
            }
        }
    

        public WindowControlWindowViewModel()
        {
            var settings = getCurrentSettings();
            IsEditMode = settings.IsEditMode;
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
            Top = settings.WindowTop;
            Left = settings.WindowLeft;
        }

        private AppSetting getCurrentSettings()
        {
            return SettingService.Instance.Current;
        }
        private void SaveCurrentSettings()
        {
            ThemeService.Instance.ApplyTheme(this.BackgroundColor, this.ForegroundColor);

            var settings = getCurrentSettings();
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;
            settings.WindowLeft = this.Left;
            settings.WindowTop = this.Top;

            SettingService.Instance.Save();

        } }
}
