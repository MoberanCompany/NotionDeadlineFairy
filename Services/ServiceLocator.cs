using NotionDeadlineFairy.Abstractions;

namespace NotionDeadlineFairy.Services
{
    /// <summary>
    /// 서비스 인스턴스를 타입별로 등록/조회하는 서비스 로케이터
    /// </summary>
    public class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> _instance =
            new Lazy<ServiceLocator>(() => new ServiceLocator());

        public static ServiceLocator Instance => _instance.Value;

        private Dictionary<Type, List<object>> _services = new Dictionary<Type, List<object>>();

        private ServiceLocator() { }

        /// <summary>
        /// 특정 타입의 서비스 인스턴스를 등록합니다. 동일 타입의 여러 인스턴스를 등록할 수 있습니다.
        /// </summary>
        /// <typeparam name="T">등록할 서비스의 타입 (인터페이스 또는 клас스)</typeparam>
        /// <param name="service">등록할 서비스 인스턴스</param>
        public void Register<T>(object service)
        {
            if (_services.ContainsKey(typeof(T)) == false)
            {
                _services[typeof(T)] = new List<object>();
            }
            
            _services[typeof(T)].Add(service);
        }

        /// <summary>
        /// 등록된 서비스 인스턴스를 타입으로 조회합니다. 여러 인스턴스가 등록되어 있을 경우 모두 반환합니다.
        /// </summary>
        /// <typeparam name="T">조회할 서비스의 타입</typeparam>
        /// <returns>등록된 서비스 인스턴스 리스트, 없으면 null</returns>
        public List<T>? GetService<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service.Cast<T>().ToList();
            }

            return default;
        }

        /// <summary>
        /// 특정 타입으로 등록된 서비스 인스턴스를 제거합니다.
        /// </summary>
        /// <typeparam name="T">제거할 서비스의 타입</typeparam>
        /// <param name="service">제거할 서비스 인스턴스</param>
        /// <returns>제거 성공 여부</returns>
        public bool Unregister<T>(object service)
        {
            if ( _services.TryGetValue(typeof(T),out var services))
            {
                return services.Remove(service);
            }

            return false;
        }
    }
}
