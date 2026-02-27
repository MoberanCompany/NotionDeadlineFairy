using System;
using System.Collections.Generic;
using System.Text;
using NotionDeadlineFairy.Models;

namespace NotionDeadlineFairy.Abstractions
{
    /// <summary>
    /// 위젯의 UI 상태 제어 기능 (View 책임)
    /// Default Interface Methods를 이용하여 필요한 함수만 구현하 도록 설계
    /// </summary>
    public interface IWidget
    {
        void SetEditMode(bool enabled) { }
        void SetClickThrough(bool enabled) { }
        void SetWindowMode(WindowMode mode) { }
        void Refresh() { }
        void ReDraw();
    }
}
