// ViewManager.cs
// View의 Spawn/Despawn 관리. EntityManager의 View 버전.
// Container 개념이 추가됨 (Single/Multiple 모드 처리).

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public sealed class ViewManager
    {
        private readonly IObjectResolver _container;
        private readonly AssetRegistry   _assetRegistry;

        private readonly Dictionary<Type, IPool> _pools = new();
        private Transform _poolRoot;

        public ViewManager(IObjectResolver container, AssetRegistry assetRegistry)
        {
            _container     = container;
            _assetRegistry = assetRegistry;
        }


        // ── Spawn ─────────────────────────────────────────────────────────

        public T Spawn<T>(Transform parent = null) where T : View
        {
            var pool = GetOrCreatePool<T>();
            var view = (T)((ViewPool<T>)pool).Rent();

            view.transform.SetParent(parent ?? GetPoolRoot(), false);
            view.gameObject.SetActive(true);

            view.ReturnToPool = () =>
            {
                view.gameObject.SetActive(false);
                view.transform.SetParent(GetPoolRoot(), false);
                pool.Return(view);
            };

            view.OnSpawn();
            return view;
        }


        // ── Container 연동 Spawn/Despawn ──────────────────────────────────

        public T SpawnInContainer<T>(ViewContainer viewContainer) where T : View
        {
            // Single 모드면 기존 View를 먼저 정리.
            if (viewContainer.Mode == ContainerMode.Single)
            {
                var active = new List<View>(viewContainer.ActiveViews);
                foreach (var v in active)
                    DespawnInternal(v, viewContainer);
            }

            var view = Spawn<T>(viewContainer.transform);
            viewContainer.AddView(view);
            return view;
        }

        public void DespawnFromContainer<T>(ViewContainer viewContainer) where T : View
        {
            View target = null;
            foreach (var v in viewContainer.ActiveViews)
                if (v is T) { target = v; break; }

            if (target != null)
                DespawnInternal(target, viewContainer);
        }


        // ── Despawn ────────────────────────────────────────────────────────

        public void Despawn<T>(T view) where T : View
            => DespawnInternal(view, null);

        private void DespawnInternal(View view, ViewContainer fromContainer)
        {
            fromContainer?.RemoveView(view);
            view.OnDespawn();
            view.ReturnToPool?.Invoke();
        }


        // ── WarmUp ────────────────────────────────────────────────────────

        public void WarmUp<T>(int count) where T : View
        {
            GetOrCreatePool<T>().WarmUp(count);
        }


        // ── 내부 ──────────────────────────────────────────────────────────

        private ViewPool<T> GetOrCreatePool<T>() where T : View
        {
            if (_pools.TryGetValue(typeof(T), out var existing))
                return (ViewPool<T>)existing;

            var prefab = _assetRegistry.GetViewPrefab<T>();
            var pool   = new ViewPool<T>(prefab, GetPoolRoot(), _container);
            _pools[typeof(T)] = pool;
            return pool;
        }

        private Transform GetPoolRoot()
        {
            if (_poolRoot != null) return _poolRoot;
            var go = new GameObject("[ViewPool]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _poolRoot = go.transform;
            return _poolRoot;
        }
    }


    // ── ViewPool<T> ───────────────────────────────────────────────────────

    internal sealed class ViewPool<T> : IPool where T : View
    {
        private readonly GameObject      _prefab;
        private readonly Transform       _root;
        private readonly IObjectResolver _container;
        private readonly Stack<T>        _stack = new();

        public ViewPool(GameObject prefab, Transform root, IObjectResolver container)
        {
            _prefab    = prefab;
            _root      = root;
            _container = container;
        }

        public T Rent() => _stack.Count > 0 ? _stack.Pop() : CreateNew();

        public void Return(IPoolable obj) => _stack.Push((T)obj);

        public void WarmUp(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var v = CreateNew();
                v.gameObject.SetActive(false);
                _stack.Push(v);
            }
        }

        private T CreateNew()
        {
            GameObject go;
            if (_prefab != null)
                go = UnityEngine.Object.Instantiate(_prefab, _root);
            else
            {
                go = new GameObject(typeof(T).Name);
                go.transform.SetParent(_root, false);
                go.AddComponent<Canvas>();
            }

            go.name = $"(View){typeof(T).Name}";
            go.SetActive(false);
            _container.InjectGameObject(go);
            return go.GetComponent<T>();
        }
    }
}