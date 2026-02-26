using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace NotionDeadlineFairy.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// PropertyChanged에 등록된 메서드들을 Invoke 해준다.
        /// 프로퍼티 Setter 에서는 이름을 넣지 않고 OnPropertyChanged() 호출만 해도 해당 프로퍼티 이름으로 처리된다
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
