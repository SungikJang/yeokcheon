// GlobalEngine.cs
// 게임 상태 관리의 핵심.
// 모든 상태 변경은 GlobalEngine.Dispatch()를 통해서만 이루어진다.

using System;
using System.Collections.Generic;
using UnityEngine;
using YeokCheonEngine.EventSystem;

namespace YeokCheonEngine.EngineSystem
{
    // ══════════════════════════════════════════════════════════════
    // 1. 기반 타입 정의
    // ══════════════════════════════════════════════════════════════

    // 모든 도메인 State의 부모 클래스.
    // CultivationState, SectState 등이 이를 상속.
    public abstract class GlobalState { }

    // 모든 Trigger(명령)의 마커 인터페이스.
    // AddExpTrigger, AttemptBreakthroughTrigger 등이 이를 구현.
    public interface ITrigger { }


    // ══════════════════════════════════════════════════════════════
    // 2. 상태 변경 알림 메시지
    // ══════════════════════════════════════════════════════════════

    // Engine이 상태 변경 후 MessageBus로 발행하는 메시지.
    // View가 이를 구독해서 자동으로 Render()를 다시 호출한다.
    // struct: GC 부하 없음. TState: 어떤 State가 바뀌었는지 타입으로 구분.
    public struct StateChangedMessage<TState> : IMessage
        where TState : GlobalState
    {
        public TState State; // 변경된 상태의 현재 값.
    }


    // ══════════════════════════════════════════════════════════════
    // 3. 미들웨어
    // ══════════════════════════════════════════════════════════════

    // 미들웨어 추상 클래스.
    // Process()를 구현하고 next(trigger)를 호출하면 체인 계속.
    // next()를 호출 안 하면 그 자리에서 Trigger 차단.
    public abstract class EngineMiddleware
    {
        public abstract void Process(
            GlobalState state,       // 현재 상태 (읽기용).
            ITrigger trigger,        // 처리할 명령.
            Action<ITrigger> next);  // 체인의 다음 단계 호출 함수.
    }

    // 로그 찍는 미들웨어.
    // 에디터/개발 빌드에서만 동작 — 출시 빌드에서는 완전히 제거됨.
    public sealed class LoggingMiddleware : EngineMiddleware
    {
        public override void Process(GlobalState state, ITrigger trigger, Action<ITrigger> next)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Engine] {state.GetType().Name} ← {trigger.GetType().Name}");
#endif
            next(trigger); // 반드시 호출. 안 하면 이후 Engine이 실행되지 않음.
        }
    }


    // ══════════════════════════════════════════════════════════════
    // 4. SubEngine 인터페이스와 추상 클래스
    // ══════════════════════════════════════════════════════════════

    // GlobalEngine이 여러 SubEngine을 타입 정보 없이 보관하기 위한 비제네릭 인터페이스.
    // Dictionary<Type, IGlobalSubEngine>으로 보관 가능.
    public interface IGlobalSubEngine
    {
        GlobalState GetStateBase(); // 타입 정보 없이 State를 꺼낼 수 있음.
    }

    // 모든 도메인 Engine의 부모.
    // TState: 이 Engine이 담당하는 State 타입.
    // 예) CultivationEngine : GlobalSubEngine<CultivationState>
    public abstract class GlobalSubEngine<TState> : IGlobalSubEngine
        where TState : GlobalState, new() // new(): 매개변수 없는 생성자 필요.
    {
        // 실제 상태 데이터. protected라 자식 클래스에서 읽을 수 있음.
        // private set: 이 클래스 내부에서만 변경 가능. Dispatch를 통해서만 바뀜.
        protected TState State { get; private set; } = new TState();

        // IGlobalSubEngine 구현 — 타입 정보 없이 State 꺼내기.
        GlobalState IGlobalSubEngine.GetStateBase() => State;

        // 자식 클래스가 반드시 구현해야 하는 상태 전이 로직.
        // switch(trigger)로 각 Trigger별 처리를 작성한다.
        protected abstract void UpdateState(TState state, ITrigger trigger);

        // GlobalEngine이 내부적으로 호출. 외부에서 직접 호출 불가 (internal).
        // 상태 변경 → 알림 발행 순서 보장.
        internal void DispatchInternal(ITrigger trigger)
        {
            UpdateState(State, trigger);
            // 상태 변경 완료. 구독 중인 View들에게 알림.
            MessageBus.Publish(new StateChangedMessage<TState> { State = State });
        }
    }


    // ══════════════════════════════════════════════════════════════
    // 5. GlobalEngine — 관문
    // ══════════════════════════════════════════════════════════════

    public sealed class GlobalEngine
    {
        // State 타입 → SubEngine 매핑.
        // typeof(CultivationState) → CultivationEngine 인스턴스
        private readonly Dictionary<Type, IGlobalSubEngine> _engines    = new();

        // 등록된 미들웨어 목록. 순서대로 실행됨.
        private readonly List<EngineMiddleware>             _middlewares = new();


        // SubEngine 등록.
        // GameLifetimeScope에서 한 번만 호출.
        public void Register<TEngine, TState>(TEngine engine)
            where TEngine : GlobalSubEngine<TState>
            where TState  : GlobalState, new()
        {
            // TryAdd: 이미 있으면 false 반환 (덮어쓰지 않음).
            if (!_engines.TryAdd(typeof(TState), engine))
                Debug.LogWarning($"[GlobalEngine] {typeof(TState).Name} 이미 등록됨.");
        }

        // 미들웨어 추가. 추가 순서가 실행 순서.
        public void AddMiddleware(EngineMiddleware mw) => _middlewares.Add(mw);


        // 현재 상태 읽기. 읽기만 가능, 직접 변경 불가.
        public TState GetState<TState>() where TState : GlobalState
        {
            if (_engines.TryGetValue(typeof(TState), out var e))
                return (TState)e.GetStateBase();

            throw new InvalidOperationException(
                $"[GlobalEngine] {typeof(TState).Name} 미등록. " +
                "GameLifetimeScope에서 Register 했는지 확인.");
        }


        // 상태 변경 명령 전달 — 외부에서 상태를 바꾸는 유일한 방법.
        public void Dispatch<TState>(ITrigger trigger)
            where TState : GlobalState, new()
        {
            if (!_engines.TryGetValue(typeof(TState), out var e))
            {
                Debug.LogError($"[GlobalEngine] {typeof(TState).Name} 미등록.");
                return;
            }
            // 미들웨어 체인 시작 (0번째부터).
            RunMiddleware(0, e.GetStateBase(), trigger, (GlobalSubEngine<TState>)e);
        }


        // 재귀 방식으로 미들웨어 체인 실행.
        // index == _middlewares.Count → 모든 미들웨어 통과 → 실제 Engine 실행.
        private void RunMiddleware<TState>(
            int index,
            GlobalState state,
            ITrigger trigger,
            GlobalSubEngine<TState> engine)
            where TState : GlobalState, new()
        {
            if (index < _middlewares.Count)
            {
                // 아직 미들웨어 남음 → 현재 미들웨어 실행.
                // next 람다: "다음 미들웨어(또는 엔진)로 넘겨"라는 콜백.
                _middlewares[index].Process(
                    state,
                    trigger,
                    next => RunMiddleware(index + 1, state, next, engine));
            }
            else
            {
                // 모든 미들웨어 통과 → Engine 실행.
                engine.DispatchInternal(trigger);
            }
        }
    }
}