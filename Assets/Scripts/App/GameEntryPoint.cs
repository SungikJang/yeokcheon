// Assets/Scripts/App/GameEntryPoint.cs

using UnityEngine;
using VContainer.Unity;
using Views.Screens;
using YeokCheonEngine.ElementSystem.EntitySystem;
using YeokCheonEngine.ElementSystem.ViewSystem;

namespace App
{
    public sealed class GameEntryPoint : IStartable
    {
        private readonly EntityManager _entityManager;
        private readonly ViewManager   _viewManager;
        private ScreenNavigator        _navigator;

        // 생성자 = VContainer가 자동으로 의존성 주입
        public GameEntryPoint(EntityManager entityManager, ViewManager viewManager)
        {
            _entityManager = entityManager;
            _viewManager   = viewManager;
        }

        public void Start()
        {
            // 화면 설정
            Screen.orientation = ScreenOrientation.Portrait;
            Application.targetFrameRate = 60;

            // ScreenNavigator 초기화
            // 씬에 있는 루트 Canvas를 찾아 연결
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                _navigator = new ScreenNavigator(_viewManager, canvas.transform);

                // 첫 화면 Push — CultivationScreen은 다음 단계에서 만든다
                _ = _navigator.PushAsync<CultivationScreen>();
            }

            Debug.Log("[YeokCheonEngine] 게임 시작");
        }
    }
}