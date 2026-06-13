// Entity.cs
// 비-UI 게임오브젝트의 기반 클래스.
// 사운드 플레이어, 파티클, 적 유닛 등이 이를 상속.

using VContainer;
using YeokCheonEngine.ElementSystem;

namespace YeokCheonEngine.ElementSystem.EntitySystem
{
    public abstract class Entity : Element
    {
        // EntityManager 주입. 자식 클래스에서 Spawn/Despawn 호출 시 사용.
        [Inject] protected EntityManager EntityManager;
    }
}