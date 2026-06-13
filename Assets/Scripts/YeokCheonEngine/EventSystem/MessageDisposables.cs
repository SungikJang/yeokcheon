// MessageDisposables.cs
// IDisposable 구독 객체들을 모아두는 컨테이너.
// View나 Entity의 OnDespawn에서 DisposeAll() 한 번으로 전체 구독 해제.

using System;
using System.Collections.Generic;

namespace YeokCheonEngine.EventSystem
{
    public sealed class MessageDisposables
    {
        // 등록된 모든 구독 객체를 보관하는 리스트.
        // 초기 용량 8 — 대부분의 View는 구독이 8개 이하다.
        private readonly List<IDisposable> _list = new(8);

        // 구독 객체를 컨테이너에 등록.
        public void Add(IDisposable disposable) => _list.Add(disposable);

        // 등록된 모든 구독을 한꺼번에 해제.
        // try-catch: 하나가 실패해도 나머지는 계속 해제.
        public void DisposeAll()
        {
            foreach (var d in _list)
                try { d.Dispose(); } catch { }
            _list.Clear();
        }
    }

    // 확장 메서드: AddTo 체이닝 문법 지원.
    public static class MessageDisposablesExtensions
    {
        // 사용법:
        // MessageBus.Subscribe<XxxMessage>(handler).AddTo(Subs);
        // — 구독하자마자 컨테이너에 바로 등록. 임시 변수 불필요.
        public static MessageDisposables AddTo(
            this IDisposable disposable, MessageDisposables container)
        {
            container.Add(disposable);
            return container; // 체이닝 가능하도록 container 반환.
        }
    }
}