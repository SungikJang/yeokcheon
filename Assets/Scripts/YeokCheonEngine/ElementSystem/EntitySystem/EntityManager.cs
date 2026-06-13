// EntityManager.cs
// Entity 오브젝트의 생성(Spawn) / 반환(Despawn) / 워밍업(WarmUp) 관리.
// 내부적으로 타입별 EntityPool<T>를 보유한다.

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using YeokCheonEngine.ElementSystem;

namespace YeokCheonEngine.ElementSystem.EntitySystem
{
    public sealed class EntityManager
    {
        private readonly IObjectResolver _container;   // VContainer DI 컨테이너.
        private readonly AssetRegistry   _assetRegistry;

        // 타입(Type) → Pool 매핑.
        // string 키 아닌 Type 키 → 호출마다 문자열 생성 없음.
        private readonly Dictionary<Type, IPool> _pools = new();

        // 모든 풀링 오브젝트를 씬 계층에서 숨기는 루트 오브젝트.
        // DontDestroyOnLoad로 씬 전환에도 유지.
        private Transform _poolRoot;

        public EntityManager(IObjectResolver container, AssetRegistry assetRegistry)
        {
            _container     = container;
            _assetRegistry = assetRegistry;
        }


        // ── Spawn: 풀에서 Entity 꺼내기 ──────────────────────────────────

        public T Spawn<T>(Transform parent = null) where T : Entity
        {
            var pool   = GetOrCreatePool<T>();
            var entity = (T)((EntityPool<T>)pool).Rent();

            // 부모 설정. parent가 null이면 PoolRoot 아래 배치.
            entity.transform.SetParent(parent != null ? parent : GetPoolRoot(), false);
            entity.gameObject.SetActive(true);

            // ★ 핵심: ReturnToPool 람다 주입.
            // entity는 어디서든 ReturnToPool()만 호출하면 자동으로 이 Pool로 반환됨.
            // pool 참조가 클로저에 캡처되어 있으므로 EntityManager 참조 불필요.
            entity.ReturnToPool = () =>
            {
                entity.gameObject.SetActive(false);
                entity.transform.SetParent(GetPoolRoot(), false);
                pool.Return(entity);
            };

            entity.OnSpawn();
            return entity;
        }


        // ── Despawn: Entity 반환 ──────────────────────────────────────────

        public void Despawn<T>(T entity) where T : Entity
        {
            DespawnChildren(entity.transform); // 자식 Entity 먼저 재귀 처리.
            entity.OnDespawn();                // 정리 콜백 (구독 해제 등).
            entity.ReturnToPool();             // Pool에 반환.
        }


        // ── WarmUp: 미리 N개 생성 ─────────────────────────────────────────

        public void WarmUp<T>(int count) where T : Entity
        {
            GetOrCreatePool<T>().WarmUp(count);
        }


        // ── 내부: 자식 Entity 재귀 Despawn ───────────────────────────────

        private void DespawnChildren(Transform parent)
        {
            // 역순 순회: Despawn 중 자식이 제거돼도 인덱스 오류 없음.
            for (var i = parent.childCount - 1; i >= 0; i--)
                TryDespawnChild(parent.GetChild(i));
        }

        private void TryDespawnChild(Transform t)
        {
            // 재귀: 더 깊은 자식부터 처리.
            for (var i = t.childCount - 1; i >= 0; i--)
                TryDespawnChild(t.GetChild(i));

            // "(Entity)" 접두사 오브젝트만 처리. 일반 자식 오브젝트는 건드리지 않음.
            if (!t.name.StartsWith("(Entity)")) return;

            // Reflection 없음: IPoolable 인터페이스로 다형성 호출.
            var poolable = t.GetComponent<IPoolable>();
            if (poolable == null) return;

            poolable.OnDespawn();
            poolable.ReturnToPool?.Invoke();
        }


        // ── Pool 조회/생성 ────────────────────────────────────────────────

        private EntityPool<T> GetOrCreatePool<T>() where T : Entity
        {
            if (_pools.TryGetValue(typeof(T), out var existing))
                return (EntityPool<T>)existing;

            var prefab = _assetRegistry.GetEntityPrefab<T>(); // null이어도 OK (코드 전용 Entity).
            var pool   = new EntityPool<T>(prefab, GetPoolRoot(), _container);
            _pools[typeof(T)] = pool;
            return pool;
        }

        private Transform GetPoolRoot()
        {
            if (_poolRoot != null) return _poolRoot;

            var go = new GameObject("[EntityPool]");
            UnityEngine.Object.DontDestroyOnLoad(go); // 씬 전환에도 유지.
            _poolRoot = go.transform;
            return _poolRoot;
        }
    }


    // ── EntityPool<T>: 타입 T 전용 풀 ────────────────────────────────────

    internal sealed class EntityPool<T> : IPool where T : Entity
    {
        private readonly GameObject      _prefab;
        private readonly Transform       _root;
        private readonly IObjectResolver _container;
        private readonly Stack<T>        _stack = new();
        // Stack: LIFO. 가장 최근에 반환된 것부터 재사용 → 캐시 친화적.

        public EntityPool(GameObject prefab, Transform root, IObjectResolver container)
        {
            _prefab    = prefab;
            _root      = root;
            _container = container;
        }

        public T Rent()
        {
            return _stack.Count > 0 ? _stack.Pop() : CreateNew();
        }

        // IPool.Return 구현.
        // 여기서만 (T)obj 다운캐스팅 발생 — 딱 한 번, 안전하게.
        public void Return(IPoolable obj)
        {
            _stack.Push((T)obj);
        }

        public void WarmUp(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var e = CreateNew();
                e.gameObject.SetActive(false);
                _stack.Push(e);
            }
        }

        private T CreateNew()
        {
            GameObject go;

            if (_prefab != null)
            {
                go = UnityEngine.Object.Instantiate(_prefab, _root);
            }
            else
            {
                // 프리팹 없음 → 빈 오브젝트 생성 (코드로만 구성하는 Entity).
                go = new GameObject(typeof(T).Name);
                go.transform.SetParent(_root, false);
            }

            // "(Entity)" 접두사: DespawnChildren에서 경계선으로 사용.
            go.name = $"(Entity){typeof(T).Name}";
            go.SetActive(false);

            // VContainer: [Inject] 필드 전부 자동 주입.
            // Element의 GlobalEngine, Entity의 EntityManager 등.
            _container.InjectGameObject(go);

            return go.GetComponent<T>();
        }
    }
}