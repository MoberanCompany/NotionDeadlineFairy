using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Abstractions
{
    public interface INavigation
    {
        void OpenDatabaseEdit();
        void Quit();
    }
}
