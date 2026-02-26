using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Services;

namespace NotionDeadlineFairy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly NotionService _notionService;

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
            this._notionService = NotionService.Instance;

            IncrementCommand = new RelayCommand((arg) =>
            {
                Count++;
            });

            DecrementCommand = new RelayCommand((arg) =>
            {
                Count--;
            });

            var list = this._notionService.GetAllDatabaseItems();
        }
    }
}
