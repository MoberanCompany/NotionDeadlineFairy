using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Services
{

    internal class ThemeService
    {
        private static readonly Lazy<ThemeService> _instance = new(() => new ThemeService());
        public static ThemeService Instance => _instance.Value;

        public event Action<string, string>? ThemeChanged;

        public void ApplyTheme(string backgroundColor, string foregroundColor)
        {
            ThemeChanged?.Invoke(backgroundColor, foregroundColor);
        }
    }
}
