using System;
using System.Collections.ObjectModel;
using System.Linq;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.Models;
using Microsoft.VisualBasic.Logging;

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

        public MainViewModel()
        {
            _notionService = NotionService.Instance;
            TaskList = new ObservableCollection<TaskItemViewModel>();

            Refresh();
            
            ServiceLocator.Instance.Register<IWidget>(this);
        }

        public void Refresh()
        {
            var settings = SettingService.Instance.Current;

            if (settings == null || settings.DatabaseConfigs.Count == 0)
            {
                TaskList = new ObservableCollection<TaskItemViewModel>();
                return;
            }

            var rawData = _notionService.GetAllDatabaseItems();

            if (rawData != null && rawData.Any())
            {
                var vms = rawData.Select(d => new TaskItemViewModel(d));
                TaskList = new ObservableCollection<TaskItemViewModel>(vms);
            }
            else
            {
                TaskList = new ObservableCollection<TaskItemViewModel>();
            }
        }

        public void SetClickThrough(bool enabled)
        {
        }

        public void SetEditMode(bool enabled)
        {
        }
    }
}