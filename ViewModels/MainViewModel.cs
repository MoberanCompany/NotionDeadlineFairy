using NotionDeadlineFairy.Commands;

namespace NotionDeadlineFairy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private int _count = 0;
        public int Count 
        {
            get => _count;
            set
            {
                if(this._count != value)
                {
                    this._count = value;
                    OnPropertyChanged();
                }
            }
        }


        public RelayCommand IncrementCommand { get; }
        public RelayCommand DecrementCommand { get; }

        public MainViewModel()
        {
            IncrementCommand = new RelayCommand((arg) =>
            {
                Count++;
            });

            DecrementCommand = new RelayCommand((arg) =>
            {
                Count--;
            });
        }
    }
}
