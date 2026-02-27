using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media; 

namespace NotionDeadlineFairy.Converters
{
    public class DeadlineToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is DateTime endAt)
            {
                // 1. 마감 기한이 없는 경우
                if (endAt == DateTime.MaxValue)
                {
                    return parameter?.ToString() == "Size" ? 13.0 : System.Windows.Media.Brushes.Black;
                }

                TimeSpan remaining = endAt - DateTime.Now;

                // 2. 색상 결정 
                if (parameter?.ToString() == "Color")
                {
                    if (remaining.TotalSeconds <= 0) return System.Windows.Media.Brushes.DimGray;
                    if (remaining.TotalHours < 1) return System.Windows.Media.Brushes.Red;
                    if (remaining.TotalDays < 1) return System.Windows.Media.Brushes.Orange;

                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#222222");
                }

                // 3. 폰트 크기 결정 (FontSize)
                if (parameter?.ToString() == "Size")
                {
                    if (remaining.TotalHours < 1) return 18.0;
                    return 13.0;
                }
            }

            return parameter?.ToString() == "Size" ? 13.0 : System.Windows.Media.Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}