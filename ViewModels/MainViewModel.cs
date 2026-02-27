using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Services;
using System.Text.RegularExpressions;

using System.Windows.Media; // FontFamily 사용을 위해 필요
using System.Collections.Generic;
using System.Linq;

namespace NotionDeadlineFairy.ViewModels
{
    public class MainViewModel : BaseViewModel, IWidget
    {
        private readonly NotionService _notionService;

        private int _count = 0;
        public int Count
        {
            get => _count;
            set
            {
                if (this._count != value)
                {
                    this._count = value;
                    OnPropertyChanged();
                }
            }
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
        public string BackgroundColor { get => _backgroundColor; set { if (_backgroundColor == value) return;  _backgroundColor = value; OnPropertyChanged(); EnabledSave = ValidateInput(); } }
        public string ForegroundColor { get => _foregroundColor; set { if (_foregroundColor == value) return;  _foregroundColor = value; OnPropertyChanged(); EnabledSave = ValidateInput(); } }
        private bool _enabledSave = false;
        public bool EnabledSave { get => _enabledSave; set { if (_enabledSave == value) return; _enabledSave = value; OnPropertyChanged(); } }

        public RelayCommand IncrementCommand { get; }
        public RelayCommand DecrementCommand { get; }

        public IReadOnlyList<System.Windows.Media.FontFamily> SystemFonts { get; } = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
        private double _fontSize = 12;
        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); EnabledSave = ValidateInput(); }
        }
        private System.Windows.Media.FontFamily _selectedFontFamily = new System.Windows.Media.FontFamily("Segoe UI");
        public System.Windows.Media.FontFamily SelectedFontFamily
        {
            get => _selectedFontFamily;
            set { _selectedFontFamily = value; OnPropertyChanged(); EnabledSave = ValidateInput(); }
        }

        public MainViewModel()
        {
            this._notionService = NotionService.Instance;

            IncrementCommand = new RelayCommand((arg) =>
            {
                Count++;
            });

            DecrementCommand = new RelayCommand((arg) =>
            {
                Count--;
            });

            Refresh();

            ServiceLocator.Instance.Register<IWidget>(this);

            var settings = SettingService.Instance.Current;
            this.Width = settings.WindowWidth;
            this.Height = settings.WindowHeight;
            this.Left = settings.WindowLeft;
            this.Top = settings.WindowTop;
            this.BackgroundColor = settings.BackgroundColor;
            this.ForegroundColor = settings.ForegroundColor;
            this.FontSize = settings.FontSize;
            this.SelectedFontFamily = new System.Windows.Media.FontFamily(settings.FontFamily);
        }

        public void Refresh()
        {
            // TODO: 코드 구현
            // throw new NotImplementedException();
            _ = InitializeAsync();
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

        private bool ValidateFontSize(double size)
        {
            return size >= 8 && size <= 100;
        }

        private bool ValidateFontFamily(System.Windows.Media.FontFamily family)
        {
            return family != null && !string.IsNullOrWhiteSpace(family.Source);
        }

        private bool ValidateInput()
        {
            return ValidateColor(BackgroundColor) && ValidateColor(ForegroundColor) && ValidateFontSize(FontSize) && ValidateFontFamily(SelectedFontFamily);
        }


        public void SaveThemeSettings()
        {
            var settings = SettingService.Instance.Current;

            settings.BackgroundColor = this.BackgroundColor;
            settings.ForegroundColor = this.ForegroundColor;
            settings.FontSize = this.FontSize;
            settings.FontFamily = this.SelectedFontFamily.Source;

            SettingService.Instance.Save();

            var views = ServiceLocator.Instance.GetService<IWidget>();
            if (views == null)
                return;
            foreach(var view in views)
            {
                try
                {
                    view.ReDraw();
                }
                catch(Exception ex)
                {
                    // ignore
                }
            }
        }

        public void ReDraw()
        {
            // nothing
        }
    }
}
