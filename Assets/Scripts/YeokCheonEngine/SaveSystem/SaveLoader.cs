// SaveLoader.cs
// 앱 시작 시 MemoryRepository에서 데이터를 읽어
// 각 도메인 Engine에 RestoreTrigger를 Dispatch해서 상태 복원.

using Cysharp.Threading.Tasks;
using VContainer.Unity;
using YeokCheonDomain.Cultivation;
using YeokCheonDomain.Sect;
using YeokCheonDomain.Session;
using YeokCheonDomain.Skill;
using YeokCheonEngine.EngineSystem;

namespace YeokCheonEngine.SaveSystem
{
    public sealed class SaveLoader : IInitializable
    {
        private readonly MemoryRepository _memory;
        private readonly GlobalEngine     _engine;

        public SaveLoader(MemoryRepository memory, GlobalEngine engine)
        {
            _memory = memory;
            _engine = engine;
        }

        // VContainer: Initialize 순서는 GameSaveSystem 다음이어야 함.
        // (GameSaveSystem.Initialize()가 먼저 LoadAsync 해야 캐시에 데이터가 있음)
        public void Initialize()
        {
            _ = RestoreAsync();
        }

        private async UniTask RestoreAsync()
        {
            var data = await _memory.LoadAsync();

            _engine.Dispatch<CultivationState>(new RestoreCultivationTrigger
            {
                RealmTier  = data.Cultivation.RealmTier,
                CurrentExp = data.Cultivation.CurrentExp,
                LastTickAt = data.Cultivation.LastTickAt,
            });

            _engine.Dispatch<SkillState>(new RestoreSkillTrigger
            {
                OwnedSkillIds = data.Skill.OwnedSkillIds,
                DantianSlotIds = data.Skill.DantianSlotIds,
                SkillStones   = data.Skill.SkillStones,
            });

            _engine.Dispatch<SectState>(new RestoreSectTrigger
            {
                SectId    = data.Sect.SectId,
                SectGrade = data.Sect.SectGrade,
                JoinedAt  = data.Sect.JoinedAt,
            });

            _engine.Dispatch<SessionState>(new RestoreSessionTrigger
            {
                PlayerId   = data.Session.PlayerId,
                PlayerName = data.Session.PlayerName,
                CreatedAt  = data.Session.CreatedAt,
            });
        }

        // 각 도메인 복원 메서드. 도메인 코드 작성 후 구현.
        private void RestoreCultivation(CultivationSave save) { /* TODO */ }
        private void RestoreSkill(SkillSave save)             { /* TODO */ }
        private void RestoreSect(SectSave save)               { /* TODO */ }
        private void RestoreSession(SessionSave save)         { /* TODO */ }
    }
}