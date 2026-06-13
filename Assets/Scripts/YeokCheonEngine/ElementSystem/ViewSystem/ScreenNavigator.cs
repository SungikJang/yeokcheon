// ScreenNavigator.cs
// 전체화면 이동을 스택으로 관리.
// UniTask로 Enter/Exit 애니메이션 순서 보장.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public sealed class ScreenNavigator
    {
        private readonly ViewManager _viewManager;
        private readonly Transform   _screenRoot; // 화면들이 붙는 Canvas Transform

        // 화면 스택. Peek() = 현재 최상위 화면.
        private readonly Stack<View> _stack = new();

        // 화면 전환 중 중복 입력 방지.
        private bool _isTransitioning;

        public ScreenNavigator(ViewManager viewManager, Transform screenRoot)
        {
            _viewManager = viewManager;
            _screenRoot  = screenRoot;
        }

        public View CurrentScreen => _stack.Count > 0 ? _stack.Peek() : null;


        // ── Push: 새 화면 위에 쌓기 ──────────────────────────────────────

        public async UniTask PushAsync<T>() where T : View
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            try
            {
                // 현재 화면 비활성화 (Despawn 아님, 스택에는 유지).
                var prev = CurrentScreen;
                if (prev != null)
                    prev.gameObject.SetActive(false);

                // 새 화면 Spawn.
                var next = _viewManager.Spawn<T>(_screenRoot);
                _stack.Push(next);

                // Enter 애니메이션 재생.
                await PlayEnterAnimation(next);
            }
            finally
            {
                _isTransitioning = false;
            }
        }


        // ── Pop: 현재 화면 닫기 ───────────────────────────────────────────

        public async UniTask PopAsync()
        {
            if (_isTransitioning || _stack.Count <= 1) return;
            _isTransitioning = true;

            try
            {
                var current = _stack.Pop();

                // Exit 애니메이션 재생 후 Despawn.
                await PlayExitAnimation(current);
                _viewManager.Despawn(current);

                // 이전 화면 다시 활성화.
                var prev = CurrentScreen;
                if (prev != null)
                    prev.gameObject.SetActive(true);
            }
            finally
            {
                _isTransitioning = false;
            }
        }


        // ── Replace: 현재 화면을 다른 화면으로 교체 ──────────────────────

        public async UniTask ReplaceAsync<T>() where T : View
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            try
            {
                // 현재 화면 Exit.
                if (_stack.Count > 0)
                {
                    var current = _stack.Pop();
                    await PlayExitAnimation(current);
                    _viewManager.Despawn(current);
                }

                // 새 화면 Push.
                var next = _viewManager.Spawn<T>(_screenRoot);
                _stack.Push(next);
                await PlayEnterAnimation(next);
            }
            finally
            {
                _isTransitioning = false;
            }
        }


        // ── 루트까지 전부 정리 ────────────────────────────────────────────

        public void PopAll()
        {
            while (_stack.Count > 0)
            {
                var v = _stack.Pop();
                _viewManager.Despawn(v);
            }
        }


        // ── 애니메이션 헬퍼 ───────────────────────────────────────────────

        // View에 ViewAnimator가 붙어있으면 애니메이션 재생, 없으면 즉시 완료.
        private async UniTask PlayEnterAnimation(View view)
        {
            var animator = view.GetComponent<ViewAnimator>();
            if (animator != null)
                await animator.PlayEnter();
        }

        private async UniTask PlayExitAnimation(View view)
        {
            var animator = view.GetComponent<ViewAnimator>();
            if (animator != null)
                await animator.PlayExit();
        }
    }
}