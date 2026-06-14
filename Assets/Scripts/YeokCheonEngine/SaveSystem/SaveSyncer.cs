// Assets/Scripts/YeokCheonEngine/SaveSystem/SaveSyncer.cs
// GlobalEngine에서 현재 상태를 읽어 SaveData를 만든다.
// GameSaveSystem.TakeSnapshot()이 저장하기 전에 호출.

using YeokCheonEngine.EngineSystem;
using YeokCheonDomain.Cultivation;
using YeokCheonDomain.Sect;
using YeokCheonDomain.Session;
using YeokCheonDomain.Skill;

namespace YeokCheonEngine.SaveSystem
{
    public sealed class SaveSyncer
    {
        private readonly GlobalEngine      _engine;
        private readonly MemoryRepository  _memory;

        public SaveSyncer(GlobalEngine engine, MemoryRepository memory)
        {
            _engine = engine;
            _memory = memory;
        }

        // 현재 GlobalEngine 상태 → SaveData 갱신
        public void Sync()
        {
            var data = _memory.GetCached() ?? new SaveData();

            var cult = _engine.GetState<CultivationState>();
            data.Cultivation.RealmTier                = (int)cult.Tier;
            data.Cultivation.CurrentExp               = cult.CurrentExp;
            data.Cultivation.LastTickAt               = cult.LastTickAt;
            data.Cultivation.IsInCultivation          = cult.IsInCultivation;
            data.Cultivation.ExpPerTick               = cult.ExpPerTick;
            data.Cultivation.LastSeenServerTimestamp  = cult.LastSeenServerTimestamp;

            var sect = _engine.GetState<SectState>();
            data.Sect.SectId    = sect.SectId;
            data.Sect.SectGrade = sect.SectGrade;
            data.Sect.JoinedAt  = sect.JoinedAt;

            var skill = _engine.GetState<SkillState>();
            data.Skill.SkillStones    = skill.SkillStones;
            data.Skill.OwnedSkillIds  = skill.OwnedSkillIds.ToArray();
            data.Skill.DantianSlotIds = (int[])skill.DantianSlots.Clone();

            var session = _engine.GetState<SessionState>();
            data.Session.PlayerId   = session.PlayerId;
            data.Session.PlayerName = session.PlayerName;

            _memory.SetCache(data);
        }
    }
}