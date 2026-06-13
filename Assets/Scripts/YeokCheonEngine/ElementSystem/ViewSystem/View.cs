// View.cs
// UI 화면의 기반 클래스. Entity와 달리 상태 구독과 Effects를 지원.
// React의 함수형 컴포넌트 + Hooks를 Unity UI에 매핑한 구조.

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using YeokCheonEngine.EngineSystem;
using YeokCheonEngine.EventSystem;

namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public abstract class View : Element
    {
        // ViewManager 주입.
        [Inject] protected ViewManager ViewManager;

        // ViewContainer: 이 View가 자식 View들을 관리하는 컨테이너.
        // null이면 자식 View 없음.
        public ViewContainer Container { get; private set; }

        // 이 View가 현재 활성화되어 있는지.
        public bool IsVisible { get; private set; }

        // RegisterEffect로 등록된 콜백 목록.
        // OnEnableElement(첫 활성화)에서 한 번만 실행됨.
        private readonly List<Action> _effects = new(4);

        // Props 비교용: 이전 Props 저장.
        // 타입별로 하나씩 저장 (여러 State 구독 지원).
        private readonly Dictionary<Type, object> _prevProps = new(4);


        // ── 생명주기 ─────────────────────────────────────────────────────

        public override void OnSpawn()
        {
            // ViewContainer가 있는 자식 오브젝트 찾아서 초기화.
            var containerGo = QueryComponent<ViewContainer>("(ViewContainer)");
            if (containerGo != null)
                Container = containerGo;
        }

        public override void OnEnableElement()
        {
            IsVisible = true;

            // Effect 실행: OnEnableElement = "처음 화면에 보일 때".
            // useEffect의 [] 의존성 빈 배열과 동일 (마운트 시 한 번).
            foreach (var effect in _effects)
                effect();
        }

        public override void OnDisableElement()
        {
            IsVisible = false;
        }

        public override void OnDespawn()
        {
            _effects.Clear();
            _prevProps.Clear();
            IsVisible = false;
            base.OnDespawn(); // 중요: Subs.DisposeAll() 호출
        }


        // ── SubscribeToState ─────────────────────────────────────────────

        /// <summary>
        /// GlobalEngine의 특정 State가 변경될 때 콜백 실행.
        /// Props(TState)가 IEquatable이면 실제로 변경된 경우에만 콜백 호출.
        /// </summary>
        protected void SubscribeToState<TState>(Action<TState> onChanged)
            where TState : GlobalState  // struct → GlobalState 로 변경
        {
            Subs.Add(
                MessageBus.Subscribe<StateChangedMessage<TState>>(msg =>  // .Global. 제거
                    {
                        if (_prevProps.TryGetValue(typeof(TState), out var prev)
                            && prev is TState prevState
                            && msg.State is IEquatable<TState> eq               // .NewState → .State
                            && eq.Equals(prevState))
                        {
                            return;
                        }

                        _prevProps[typeof(TState)] = msg.State;                 // .NewState → .State
                        onChanged(msg.State);                                   // .NewState → .State
                    },
                    MessageScope.Global)  // 두 번째 파라미터로 스코프 전달
            );

            var current = GlobalEngine.GetState<TState>();
            _prevProps[typeof(TState)] = current;
            onChanged(current);
        }


        // ── RegisterEffect ───────────────────────────────────────────────

        /// <summary>
        /// OnEnableElement(화면이 보이기 시작할 때) 실행할 콜백 등록.
        /// React useEffect(() => {}, []) 와 동일.
        /// </summary>
        protected void RegisterEffect(Action effect)
        {
            _effects.Add(effect);
        }


        // ── 자식 View 관리 헬퍼 ─────────────────────────────────────────

        /// <summary>
        /// 이 View의 Container에서 특정 타입 View 하나를 활성화.
        /// </summary>
        protected T ShowChild<T>() where T : View
        {
            if (Container == null)
            {
                Debug.LogWarning($"[View] {name}: Container가 없습니다.");
                return null;
            }
            return ViewManager.SpawnInContainer<T>(Container);
        }

        protected void HideChild<T>() where T : View
        {
            if (Container == null) return;
            ViewManager.DespawnFromContainer<T>(Container);
        }
    }
}