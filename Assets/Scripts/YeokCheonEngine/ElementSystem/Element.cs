// Element.cs
// Entity와 View의 공통 추상 기반 클래스.
// MonoBehaviour를 상속하므로 GameObject에 붙일 수 있다.
// IPoolable 구현으로 Pool에서 재사용 가능.

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using YeokCheonEngine.EngineSystem;
using YeokCheonEngine.EventSystem;

namespace YeokCheonEngine.ElementSystem
{
    // 외부에서 Element를 타입 정보 없이 다룰 수 있게 하는 인터페이스.
    public interface IElement
    {
        void OnSpawn();
        void OnEnableElement();
        void OnDisableElement();
        void OnDespawn();
        T QueryComponent<T>(string gameObjectName = null) where T : Component;
    }

    public abstract class Element : MonoBehaviour, IElement, IPoolable
    {
        // VContainer가 자동으로 주입해준다.
        // protected: 자식 클래스(CultivationScreen 등)에서 사용 가능.
        [Inject] protected GlobalEngine GlobalEngine;

        // 첫 Spawn이 완료됐는지 추적하는 플래그.
        // false: 아직 OnSpawn 전 → OnEnableElement 호출 안 함.
        // true:  OnSpawn 완료 → OnEnableElement 정상 호출.
        public bool IsElementReady { get; private set; }

        // 이 Element의 모든 MessageBus 구독을 모아두는 컨테이너.
        // OnDespawn에서 DisposeAll() → 메모리 누수 방지.
        protected readonly MessageDisposables Subs = new();

        // IPoolable 구현. Manager가 Spawn 시 주입.
        // 이 액션을 호출하면 원래 Pool로 자동 반환됨.
        public Action ReturnToPool { get; set; }

        // 컴포넌트 탐색 결과 캐시.
        // 키: "{gameObjectName}::{ComponentTypeName}"
        // 값: 찾은 Component
        private readonly Dictionary<string, Component> _componentCache = new(8);

        // BFS 탐색용 큐. ThreadStatic으로 모든 Element가 하나의 큐를 공유.
        // static이므로 매 탐색마다 new Queue() 할당 없음.
        [ThreadStatic]
        private static Queue<Transform> _bfsQueue;


        // ── Unity MonoBehaviour 생명주기 ─────────────────────────────────

        private void OnEnable()
        {
            // IsElementReady가 false면 OnSpawn 전이므로 호출 안 함.
            if (!IsElementReady) return;
            OnEnableElement();
        }

        private void OnDisable()
        {
            // 첫 번째 OnDisable 호출 시점:
            // Pool 대기 상태에서 SetActive(false)가 처음 불린 것.
            // 이 시점에 IsElementReady를 true로 설정.
            if (!IsElementReady) { IsElementReady = true; return; }
            OnDisableElement();
        }


        // ── IElement 구현 ────────────────────────────────────────────────

        // 자식 클래스에서 override해서 초기화 로직 작성.
        public virtual void OnSpawn()          { }
        public virtual void OnEnableElement()  { }
        public virtual void OnDisableElement() { }

        public virtual void OnDespawn()
        {
            // 자식 클래스에서 override 시 반드시 base.OnDespawn() 호출해야 함.
            // 안 하면 구독이 해제되지 않아 메모리 누수 발생.
            Subs.DisposeAll();        // 모든 MessageBus 구독 해제.
            _componentCache.Clear();  // 캐시 초기화 (다음 Spawn에서 재탐색).
            IsElementReady = false;   // 플래그 초기화.
        }


        // ── QueryComponent: 이름으로 자식 컴포넌트 찾기 (캐싱) ───────────

        public T QueryComponent<T>(string gameObjectName = null) where T : Component
        {
            // 캐시 키 생성.
            var key = $"{gameObjectName}::{typeof(T).Name}";

            // 캐시 히트: 이전에 찾은 결과 즉시 반환 (O(1)).
            if (_componentCache.TryGetValue(key, out var cached))
                return cached as T;

            // 캐시 미스: 실제 탐색 수행.
            T result = gameObjectName == null
                ? GetComponent<T>()           // 자기 자신에서만 탐색.
                : BfsFind<T>(gameObjectName); // 자식 계층에서 이름으로 탐색.

            // 찾으면 캐싱. 다음 호출은 O(1).
            if (result != null)
                _componentCache[key] = result;

            return result;
        }

        // BFS로 특정 이름의 자식 오브젝트에서 컴포넌트 탐색.
        private T BfsFind<T>(string targetName) where T : Component
        {
            _bfsQueue ??= new Queue<Transform>(32); // 없으면 최초 1회 생성.
            _bfsQueue.Clear();
            _bfsQueue.Enqueue(transform); // 자기 자신부터 시작.

            while (_bfsQueue.Count > 0)
            {
                var current = _bfsQueue.Dequeue();
                foreach (Transform child in current)
                {
                    // 이름 일치 → 컴포넌트 탐색.
                    if (child.name == targetName)
                    {
                        var comp = child.GetComponent<T>();
                        if (comp != null) return comp;
                    }

                    // (Entity) / (View) 접두사 오브젝트는 BFS에서 제외.
                    // 다른 Element의 내부까지 침범하지 않도록 경계선 역할.
                    if (!child.name.StartsWith("(Entity)") &&
                        !child.name.StartsWith("(View)"))
                        _bfsQueue.Enqueue(child);
                }
            }
            return null;
        }
    }
}