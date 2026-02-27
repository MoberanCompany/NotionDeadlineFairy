using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NotionDeadlineFairy.ViewModels
{
    public class NotionDatabaseSettingViewModel: BaseViewModel
    {
        public ObservableCollection<NotionDatabaseSettingItemViewModel> SettingList { get; set; }


        public NotionDatabaseSettingViewModel()
        {
            SettingList = new();
        }

    }
}
