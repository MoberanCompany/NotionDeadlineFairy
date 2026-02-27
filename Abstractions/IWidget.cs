using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Abstractions
{
    /// <summary>
    /// 위젯의 UI 상태 제어 기능 (View 책임)
    /// </summary>
    public interface IWidget
    {
        void SetClickThrough(bool enabled);
        void Refresh();
        void SetEditMode(bool enabled);
    }
}
