using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Sect
{
    public enum Faction { None = -1, 정파 = 0, 사파 = 1, 마교 = 2 }

    public sealed class SectState : GlobalState
    {
        public Faction Faction   = Faction.None; // 영구 고정
        public int     SectId    = -1;           // 가문 ID
        public int     SectGrade = 0;
        public long    JoinedAt  = 0;
        public bool    IsInSect  => SectId >= 0;
    }
}