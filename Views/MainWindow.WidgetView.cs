using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NotionDeadlineFairy.Views
{
    public partial class MainWindow : IWidget
    {
        public void SetEditMode(bool enabled)
        {
            ResizeMode = enabled ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
        }

    }
}
