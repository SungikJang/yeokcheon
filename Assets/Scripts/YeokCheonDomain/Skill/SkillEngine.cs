using UnityEngine;
using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Skill
{
    public sealed class SkillEngine : GlobalSubEngine<SkillState>
    {
        protected override void UpdateState(SkillState state, ITrigger trigger)
        {
            switch (trigger)
            {
                case AcquireSkillTrigger t:
                    if (!state.OwnedSkillIds.Contains(t.SkillId))
                        state.OwnedSkillIds.Add(t.SkillId);
                    break;

                case EquipSkillTrigger t:
                    Equip(state, t.SkillId, t.SlotIndex);
                    break;

                case UnequipSkillTrigger t:
                    if (t.SlotIndex is >= 0 and < 3)
                        state.DantianSlots[t.SlotIndex] = -1;
                    break;

                case RestoreSkillTrigger t:
                    state.OwnedSkillIds.Clear();
                    if (t.OwnedSkillIds != null)
                        state.OwnedSkillIds.AddRange(t.OwnedSkillIds);
                    state.DantianSlots = t.DantianSlotIds ?? new int[3] { -1, -1, -1 };
                    state.SkillStones  = t.SkillStones;
                    break;
                default:
                    Debug.LogWarning($"[SkillEngine] 알 수 없는 트리거: {trigger.GetType().Name}");
                    break;
            }
        }
        
        private static void Equip(SkillState state, int skillId, int slotIndex)
        {
            if (!state.OwnedSkillIds.Contains(skillId))
            {
                Debug.LogWarning($"[SkillEngine] 보유하지 않은 무공: {skillId}");
                return;
            }

            Unequip(state, slotIndex);

            // 다른 슬롯에 이미 장착돼 있으면 해제
            for (int i = 0; i < state.DantianSlots.Length; i++)
                if (state.DantianSlots[i] == skillId)
                    state.DantianSlots[i] = -1;

            state.DantianSlots[slotIndex] = skillId;
        }

        private static void Unequip(SkillState state, int slotIndex)
        {
            state.DantianSlots[slotIndex] = -1;
        }
    }
}