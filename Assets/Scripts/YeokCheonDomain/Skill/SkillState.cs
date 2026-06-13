using System.Collections.Generic;
using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Skill
{
    public sealed class SkillState : GlobalState
    {
        public List<int> OwnedSkillIds  = new();   // 보유 스킬 ID 목록
        public int[]     DantianSlots   = new int[3] { -1, -1, -1 }; // -1 = 빈 슬롯
        public int       SkillStones    = 0;        // 스킬 강화 재료
    }
}