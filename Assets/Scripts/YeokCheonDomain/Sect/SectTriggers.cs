using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Sect
{
    public struct JoinSectTrigger : ITrigger
    {
        public Faction Faction;
        public int     SectId;
    }
    public struct LeaveSectTrigger : ITrigger { }
    public struct RestoreSectTrigger : ITrigger
    {
        public int SectId; public int SectGrade; public long JoinedAt;
    }
}