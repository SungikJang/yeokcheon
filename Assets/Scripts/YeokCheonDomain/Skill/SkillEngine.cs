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
                    if (t.SlotIndex is >= 0 and < 3
                        && state.OwnedSkillIds.Contains(t.SkillId))
                        state.DantianSlots[t.SlotIndex] = t.SkillId;
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
            }
        }
    }
}