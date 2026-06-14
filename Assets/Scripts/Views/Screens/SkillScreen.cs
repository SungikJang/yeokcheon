// Assets/Scripts/Views/Screens/SkillScreen.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YeokCheonEngine.ElementSystem.ViewSystem;
using YeokCheonDomain.Skill;

namespace Views.Screens
{
    public sealed class SkillScreen : View
    {
        // 단전 슬롯 버튼 캐시 (3개 고정)
        private Button[] _slotButtons;
        private int      _selectedSlotIndex = -1; // 현재 선택된 슬롯
        
        private const int SLOT_COUNT = 3;

        public override void OnSpawn()
        {
            base.OnSpawn();
            SubscribeToState<SkillState>(OnSkillChanged);
        }

        public override void OnEnableElement()
        {
            base.OnEnableElement();

            // 단전 슬롯 버튼 연결
            _slotButtons = new Button[SLOT_COUNT];
            for (int i = 0; i < SLOT_COUNT; i++)
            {
                var idx = i; // 클로저 캡처용
                _slotButtons[i] = QueryComponent<Button>($"Btn_Slot_{i}");
                if (_slotButtons[i] != null)
                    _slotButtons[i].onClick.AddListener(() => OnSlotClicked(idx));
            }
        }

        public override void OnDisableElement()
        {
            base.OnDisableElement();

            for (int i = 0; i < SLOT_COUNT; i++)
                if (_slotButtons?[i] != null)
                    _slotButtons[i].onClick.RemoveAllListeners();

            _selectedSlotIndex = -1;
        }

        public override void OnDespawn()
        {
            _slotButtons = null;
            base.OnDespawn();
        }

        // ── 상태 반영 ────────────────────────────────────────────────────

        private void OnSkillChanged(SkillState state)
        {
            for (int i = 0; i < SLOT_COUNT; i++)
            {
                var slotText = QueryComponent<TMP_Text>($"Text_Slot_{i}");
                if (slotText == null) continue;

                var equippedId = state.DantianSlots[i];
                slotText.text = equippedId == -1 ? "비어있음" : $"무공 #{equippedId}";
            }

            var countText = QueryComponent<TMP_Text>("Text_SkillCount");
            if (countText != null)
                countText.text = $"보유 무공: {state.OwnedSkillIds.Count}";

            var stonesText = QueryComponent<TMP_Text>("Text_Stones");
            if (stonesText != null)
                stonesText.text = $"무공석: {state.SkillStones:N0}";
        }

        // ── 슬롯 클릭: 장착/해제 토글 ────────────────────────────────────

        private void OnSlotClicked(int slotIndex)
        {
            var state = GlobalEngine.GetState<SkillState>();
            var equippedId = state.DantianSlots[slotIndex];

            if (equippedId != -1)
                GlobalEngine.Dispatch<SkillState>(
                    new UnequipSkillTrigger { SlotIndex = slotIndex });
            else
                _selectedSlotIndex = slotIndex;
        }
    }
}