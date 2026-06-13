using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Skill
{
    public struct AcquireSkillTrigger : ITrigger { public int SkillId; }
    public struct EquipSkillTrigger   : ITrigger { public int SkillId; public int SlotIndex; }
    public struct UnequipSkillTrigger : ITrigger { public int SlotIndex; }

    public struct RestoreSkillTrigger : ITrigger
    {
        public int[] OwnedSkillIds;
        public int[] DantianSlotIds;
        public int   SkillStones;
    }
}