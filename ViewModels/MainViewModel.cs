using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Services;
using System.Threading.Tasks;

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

            Refresh();

            ServiceLocator.Instance.Register<IWidget>(this);
        }

        public void Refresh()
        {
            // TODO: 内靛 备泅
            // throw new NotImplementedException();
            _ = InitializeAsync();
        }

        public void SetEditMode(bool enabled)
        {
            // TODO: 内靛 备泅
            // throw new NotImplementedException();
        }

        public void SetClickThrough(bool enabled)
        {
            // TODO: 内靛 备泅
            // throw new NotImplementedException();
        }

        private async Task InitializeAsync()
        {
            var list = await this._notionService.GetAllDatabaseItemsAsync();
        }
    }
}
