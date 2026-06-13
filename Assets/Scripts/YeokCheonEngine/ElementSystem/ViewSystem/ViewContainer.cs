// ViewContainer.cs
// View 안에서 자식 View들을 관리하는 컨테이너.
// Single 모드: 한 번에 한 View만 표시.
// Multiple 모드: 여러 View 동시 표시.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public enum ContainerMode
    {
        Single,   // 탭, 페이지 전환 등 (하나만 활성)
        Multiple  // 팝업, 오버레이 등 (여러 개 동시 활성)
    }

    public sealed class ViewContainer : MonoBehaviour
    {
        [SerializeField] private ContainerMode _mode = ContainerMode.Single;

        public ContainerMode Mode => _mode;

        // 현재 이 컨테이너에서 활성화된 View들.
        // Single 모드에서는 최대 1개.
        private readonly List<View> _activeViews = new(4);

        public IReadOnlyList<View> ActiveViews => _activeViews;

        public void AddView(View view)
        {
            _activeViews.Add(view);
        }

        public void RemoveView(View view)
        {
            _activeViews.Remove(view);
        }

        public bool HasView<T>() where T : View
        {
            foreach (var v in _activeViews)
                if (v is T) return true;
            return false;
        }
    }
}