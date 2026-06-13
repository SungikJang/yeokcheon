// LocalEngine.cs
// View 내부의 로컬 상태 관리.
// React의 useState와 동일한 개념.
// 값이 실제로 바뀔 때만 onStateChanged 콜백을 호출한다.

using System;

namespace YeokCheonEngine.EngineSystem
{
    public sealed class LocalEngine<T>
    {
        private T             _state;
        private readonly Action _onStateChanged;
        // _onStateChanged: 상태 변경 시 호출할 콜백.
        // 보통 View.RunLifeCycle을 넘겨서 상태 변경 → 자동 재렌더링.

        public LocalEngine(T initialState, Action onStateChanged)
        {
            _state          = initialState;
            _onStateChanged = onStateChanged;
        }

        // 현재 상태. 읽기 전용.
        public T State => _state;

        // 새 값으로 상태 변경.
        // 동일 값이면 콜백 호출 안 함 → 불필요한 재렌더링 방지.
        public void SetState(T newValue)
        {
            if (IsEqual(_state, newValue)) return; // 같은 값이면 무시.
            _state = newValue;
            _onStateChanged?.Invoke();             // 달라졌을 때만 콜백.
        }

        // 함수형 업데이트: 현재 값을 기반으로 새 값 계산.
        // 예) engine.SetState(prev => prev + 1)
        public void SetState(Func<T, T> updater) => SetState(updater(_state));

        // 값 동등 비교.
        // IEquatable 구현 타입이면 그 메서드 사용 (빠름, 박싱 없음).
        // 아니면 Object.Equals 사용.
        private static bool IsEqual(T a, T b)
        {
            if (a is IEquatable<T> eq) return eq.Equals(b);
            return Equals(a, b);
        }
    }
}