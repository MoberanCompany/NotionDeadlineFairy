using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.ViewModels
{
    public class NotionDatabaseSettingItemViewModel: BaseViewModel
    {

        private string _name = "";
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _apiKey = "";
        public string ApiKey {
            get
            {
                return this._apiKey;
            }
            set
            {
                if(this._apiKey != value)
                {
                    this._apiKey = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _databaseUrl = "";
        public string DatabaseUrl
        {
            get
            {
                return this._databaseUrl;
            }
            set
            {
                if (this._databaseUrl != value)
                {
                    this._databaseUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get
            {
                return this._isConnected;
            }
            set
            {
                if (this._isConnected != value)
                {
                    this._isConnected = value;
                    OnPropertyChanged();
                }
            }
        }


        // CSV 형식 사용
        private string _showingProperties = "";
        public string ShowingProperties
        {
            get
            {
                return this._showingProperties;
            }
            set
            {
                if (this._showingProperties != value)
                {
                    this._showingProperties = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _endDatePropertyName = "";
        public string EndDatePropertyName
        {
            get
            {
                return this._endDatePropertyName;
            }
            set
            {
                if (this._endDatePropertyName != value)
                {
                    this._endDatePropertyName = value;
                    OnPropertyChanged();
                }
            }
        }



        private string _resultMessage = "";
        public string ResultMessage
        {
            get
            {
                return this._resultMessage;
            }
            set
            {
                if (this._resultMessage != value)
                {
                    this._resultMessage = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
