// Assets/Scripts/Views/Screens/CultivationScreen.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YeokCheonEngine.ElementSystem.ViewSystem;
using YeokCheonEngine.EngineSystem;
using YeokCheonDomain.Cultivation;

namespace Views.Screens
{
    public sealed class CultivationScreen : View
    {
        // 로컬 상태: 돌파 버튼 활성화 여부
        // LocalEngine<bool>: 값이 실제로 바뀔 때만 콜백 호출
        private LocalEngine<bool> _canBreakthrough;

        // ── OnSpawn: 풀에서 꺼낼 때마다 호출 ─────────────────────────────

        public override void OnSpawn()
        {
            base.OnSpawn();

            // 로컬 상태 초기화 — 버튼 상태 바뀔 때 UI 반영
            _canBreakthrough = new LocalEngine<bool>(
                initialState: false,
                onStateChanged: UpdateBreakthroughButton);

            // CultivationState 변경 시 OnCultivationChanged 자동 호출
            // IEquatable 구현된 State면 동일 값일 때 콜백 스킵
            SubscribeToState<CultivationState>(OnCultivationChanged);
        }

        // ── OnEnableElement: 화면이 활성화될 때 ──────────────────────────

        public override void OnEnableElement()
        {
            base.OnEnableElement(); // View.OnEnableElement: IsVisible=true, Effects 실행

            // 버튼 이벤트 등록 — OnDisableElement에서 반드시 제거
            var btn = QueryComponent<Button>("Btn_Breakthrough");
            if (btn != null) btn.onClick.AddListener(OnBreakthroughClicked);
        }

        public override void OnDisableElement()
        {
            base.OnDisableElement();

            var btn = QueryComponent<Button>("Btn_Breakthrough");
            if (btn != null) btn.onClick.RemoveListener(OnBreakthroughClicked);
        }

        // ── 상태 변경 콜백 ────────────────────────────────────────────────

        private void OnCultivationChanged(CultivationState state)
        {
            // 경지명 표시 (예: "삼류입문")
            var realmText = QueryComponent<TextMeshProUGUI>("Text_Realm");
            if (realmText != null) realmText.text = state.RealmName;

            // 경험치 프로그레스 바 (0~1)
            var progressBar = QueryComponent<Slider>("Slider_Progress");
            if (progressBar != null) progressBar.value = state.Progress;

            // 경험치 수치 텍스트 (예: "1234 / 5000")
            var expText = QueryComponent<TextMeshProUGUI>("Text_Exp");
            if (expText != null)
                expText.text = $"{state.CurrentExp:N0} / {state.RequiredExp:N0}";

            // 돌파 가능 조건:
            // 경험치 100% 달성 + 대경지(수동 돌파 필요)일 때만 버튼 활성화
            // 소경지는 AddExp 시 자동 돌파되므로 버튼 불필요
            var isBig = CultivationState.IsBigRealm(state.Tier);
            _canBreakthrough.SetState(state.Progress >= 1f && isBig);
        }

        private void UpdateBreakthroughButton()
        {
            var btn = QueryComponent<Button>("Btn_Breakthrough");
            if (btn != null) btn.interactable = _canBreakthrough.State;
        }

        // ── 버튼 클릭 ─────────────────────────────────────────────────────

        private void OnBreakthroughClicked()
        {
            // Dispatch → CultivationEngine.TryBreakthrough 실행
            // → 성공/실패 후 StateChangedMessage 발행
            // → OnCultivationChanged 자동 재호출
            GlobalEngine.Dispatch<CultivationState>(new BreakthroughTrigger());
        }
    }
}