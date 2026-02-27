using System.Windows.Threading;

namespace NotionDeadlineFairy.Utils
{
    /// <summary>
    /// UI 스레드의 Dispatcher를 캡슐화하여 어디서든 UI 작업을 수행할 수 있게 하는 헬퍼
    /// </summary>
    public static class DispatcherHelper
    {
        private static Dispatcher? _dispatcher;

        /// <summary>
        /// UI 스레드의 Dispatcher를 초기화합니다. App 시작 시 한 번 호출되어야 합니다.
        /// </summary>
        /// <param name="dispatcher">Application의 Dispatcher</param>
        public static void Initialize(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// UI 스레드에서 동기적으로 작업을 실행합니다.
        /// </summary>
        /// <param name="action">실행할 작업</param>
        public static void Invoke(Action action)
        {
            if (_dispatcher is null)
                throw new InvalidOperationException("DispatcherHelper가 초기화되지 않았습니다.");

            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.Invoke(action);
        }

        /// <summary>
        /// UI 스레드에서 비동기적으로 작업을 실행합니다.
        /// </summary>
        /// <param name="action">실행할 작업</param>
        /// <param name="priority">작업 우선순위 (기본값: Normal)</param>
        public static void BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (_dispatcher is null)
                throw new InvalidOperationException("DispatcherHelper가 초기화되지 않았습니다.");

            _dispatcher.BeginInvoke(action, priority);
        }

        /// <summary>
        /// UI 스레드에서 비동기적으로 작업을 실행하고 Task를 반환합니다.
        /// </summary>
        /// <param name="action">실행할 작업</param>
        /// <param name="priority">작업 우선순위 (기본값: Normal)</param>
        public static Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (_dispatcher is null)
                throw new InvalidOperationException("DispatcherHelper가 초기화되지 않았습니다.");

            return _dispatcher.InvokeAsync(action, priority).Task;
        }

        /// <summary>
        /// 현재 스레드가 UI 스레드인지 확인합니다.
        /// </summary>
        /// <returns>UI 스레드이면 true</returns>
        public static bool CheckAccess()
        {
            return _dispatcher?.CheckAccess() ?? false;
        }
    }
}
