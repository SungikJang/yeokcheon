// MessageBus.cs
// 전역 Pub/Sub 이벤트 버스.
// Publish로 메시지를 보내고, Subscribe로 수신한다.
// 직접 참조 없이 오브젝트 간 통신 가능.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YeokCheonEngine.EventSystem
{
    // ── 공개 API (이것만 외부에서 사용) ─────────────────────────────────────
    public static class MessageBus
    {
        // 씬 로드 전에 실행되는 초기화 메서드.
        // SceneManager 이벤트를 연결해서 씬 언로드 시 Scene 스코프 자동 해제.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneUnloaded += _ => SceneScopeRegistry.ClearAll();
        }

        // 메시지 발행. 기본 스코프는 Global (모든 구독자에게 전달).
        public static void Publish<T>(T message, MessageScope scope = MessageScope.Global)
            where T : struct, IMessage
            => MessageChannel<T>.Publish(message, scope);

        // 구독. IDisposable 반환 — .Dispose()로 구독 해제.
        // priority가 높을수록 먼저 호출됨.
        public static IDisposable Subscribe<T>(
            Action<T> handler,
            MessageScope scope = MessageScope.Global,
            int priority = 0)
            where T : struct, IMessage
            => MessageChannel<T>.Subscribe(handler, scope, priority);

        // 특정 스코프의 구독 전체 해제.
        public static void ClearScope<T>(MessageScope scope)
            where T : struct, IMessage
            => MessageChannel<T>.ClearScope(scope);
    }

    // ── Scene 스코프 자동 해제 레지스트리 ────────────────────────────────────
    // 각 MessageChannel<T>가 씬 언로드 시 호출할 콜백을 여기에 등록.
    internal static class SceneScopeRegistry
    {
        private static readonly List<Action> _callbacks = new(32);
        internal static void Register(Action cb) => _callbacks.Add(cb);
        internal static void ClearAll()
        {
            foreach (var cb in _callbacks) cb?.Invoke();
            _callbacks.Clear();
        }
    }

    // ── 내부 채널: 타입 T 전용 구독자 관리 ───────────────────────────────────
    // T가 다르면 완전히 별개의 static 클래스 → 타입별 독립 채널.
    internal static class MessageChannel<T> where T : struct, IMessage
    {
        // 스코프별 구독자 목록.
        private static readonly SubscriberList _global  = new();
        private static readonly SubscriberList _scene   = new();
        private static readonly SubscriberList _context = new();

        // 순회 안전을 위한 재사용 버퍼 (static → 매 호출 new 할당 없음).
        private static readonly List<Subscriber> _buf = new(16);

        // 정적 생성자: SceneScopeRegistry에 Scene 스코프 해제 콜백 등록.
        static MessageChannel()
        {
            SceneScopeRegistry.Register(() => _scene.Clear());
        }

        // 발행: 스코프에 따라 어느 구독자에게 전달할지 결정.
        internal static void Publish(T msg, MessageScope scope)
        {
            switch (scope)
            {
                case MessageScope.Global:
                    // Global 메시지 → 모든 스코프 구독자에게 전달.
                    Execute(_global, msg);
                    Execute(_scene, msg);
                    Execute(_context, msg);
                    break;
                case MessageScope.Scene:
                    // Scene 메시지 → Scene, Context 구독자에게만.
                    Execute(_scene, msg);
                    Execute(_context, msg);
                    break;
                case MessageScope.Context:
                    // Context 메시지 → Context 구독자에게만.
                    Execute(_context, msg);
                    break;
            }
        }

        // 구독: 구독자를 우선순위에 따라 정렬 삽입.
        internal static IDisposable Subscribe(Action<T> handler, MessageScope scope, int priority)
        {
            var sub = new Subscriber(handler, priority);
            GetList(scope).Add(sub);
            return new Unsubscriber(GetList(scope), sub);
        }

        internal static void ClearScope(MessageScope scope) => GetList(scope).Clear();

        // 구독자 목록 순회 실행.
        // _buf에 복사 후 순회 → 핸들러 안에서 Unsubscribe해도 안전.
        private static void Execute(SubscriberList list, T msg)
        {
            if (list.Count == 0) return;
            _buf.Clear();
            list.CopyTo(_buf);
            foreach (var s in _buf)
                try { s.Handler(msg); }
                catch (Exception e) { Debug.LogError($"[MessageBus] {e}"); }
                // try-catch: 한 핸들러의 예외가 다른 핸들러를 막지 않음.
        }

        private static SubscriberList GetList(MessageScope s) => s switch
        {
            MessageScope.Global  => _global,
            MessageScope.Scene   => _scene,
            MessageScope.Context => _context,
            _                    => _global,
        };

        // ── 구독자 데이터 ────────────────────────────────────────────────────
        internal readonly struct Subscriber
        {
            public readonly Action<T> Handler;
            public readonly int Priority;
            public Subscriber(Action<T> h, int p) { Handler = h; Priority = p; }
        }

        // ── 우선순위 정렬 구독자 목록 ────────────────────────────────────────
        internal sealed class SubscriberList
        {
            private readonly List<Subscriber> _items = new(8);
            public int Count => _items.Count;

            // 우선순위 내림차순 삽입 (높은 priority → 앞쪽).
            public void Add(Subscriber s)
            {
                var idx = _items.Count; // 기본: 맨 뒤.
                for (var i = 0; i < _items.Count; i++)
                    if (s.Priority > _items[i].Priority) { idx = i; break; }
                _items.Insert(idx, s);
            }

            public void Remove(Subscriber s)
            {
                for (var i = 0; i < _items.Count; i++)
                    if (_items[i].Handler == s.Handler) { _items.RemoveAt(i); return; }
            }

            public void CopyTo(List<Subscriber> buf) => buf.AddRange(_items);
            public void Clear() => _items.Clear();
        }

        // ── 구독 해제 핸들러 ─────────────────────────────────────────────────
        private sealed class Unsubscriber : IDisposable
        {
            private readonly SubscriberList _list;
            private readonly Subscriber     _sub;
            private bool _disposed;

            public Unsubscriber(SubscriberList l, Subscriber s) { _list = l; _sub = s; }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _list.Remove(_sub);
            }
        }
    }
}