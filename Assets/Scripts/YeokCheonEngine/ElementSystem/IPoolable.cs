// IPoolable.cs
// 풀링 가능한 오브젝트(Entity, View)의 인터페이스 계층.
// 이 인터페이스 덕분에 Reflection 없이 다형적 Pool 반환이 가능하다.

namespace YeokCheonEngine.ElementSystem
{
    /// <summary>
    /// 풀링 가능한 모든 오브젝트의 마커 인터페이스.
    /// Entity와 View가 공통으로 구현한다.
    /// </summary>
    public interface IPoolable
    {
        // 풀에서 꺼낼 때 호출. 초기화 로직 작성.
        void OnSpawn();

        // 풀에 돌려보낼 때 호출. 정리 로직 작성.
        void OnDespawn();

        /// <summary>
        /// "나를 원래 Pool로 돌려보내는 함수".
        /// Manager가 Spawn 직후 주입한다.
        ///
        /// 사용 예:
        ///   entity.ReturnToPool = () => { SetActive(false); pool.Return(entity); };
        ///
        /// 주의: OnDespawn 안에서 직접 호출하면 안 됨.
        ///        Manager의 Despawn()이 올바른 순서로 호출해준다.
        /// </summary>
        System.Action ReturnToPool { get; set; }
    }


    /// <summary>
    /// 비제네릭 Pool 인터페이스.
    /// Manager가 Dictionary&lt;Type, IPool&gt;로 여러 타입의 Pool을 관리할 수 있게 한다.
    /// 실제 다운캐스팅은 EntityPool&lt;T&gt;, ViewPool&lt;T&gt; 내부에서만 발생한다.
    /// </summary>
    public interface IPool
    {
        // 사용 완료된 오브젝트를 Pool로 반환.
        // 내부 구현(EntityPool<T>)에서 (T)obj 한 번만 다운캐스팅.
        void Return(IPoolable obj);

        // N개를 미리 생성해 스택에 쌓아둠.
        // 첫 Spawn 시 Instantiate 지연 없애려면 게임 시작 시 WarmUp 호출.
        void WarmUp(int count);
    }
}