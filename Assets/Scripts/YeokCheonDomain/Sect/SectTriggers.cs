using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Sect
{
    public struct JoinSectTrigger  : ITrigger { public int SectId; public long JoinedAt; }
    public struct LeaveSectTrigger : ITrigger { }
    public struct RestoreSectTrigger : ITrigger
    {
        public int SectId; public int SectGrade; public long JoinedAt;
    }
}