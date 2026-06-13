// CultivationTriggers.cs
// 수련 도메인에서 발생할 수 있는 모든 트리거.
// ITrigger 구현 → GlobalEngine.Dispatch 가능.

using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Cultivation
{
    // 경험치 획득 (틱마다, 오프라인 보상 등).
    public struct AddExpTrigger : ITrigger
    {
        public double Amount;
    }

    // 수동 돌파 시도 (충분한 경험치 있을 때).
    public struct BreakthroughTrigger : ITrigger { }

    // 게임 시작 시 저장 데이터 복원.
    public struct RestoreCultivationTrigger : ITrigger
    {
        public int    RealmTier;
        public double CurrentExp;
        public long   LastTickAt;
        public bool   IsInCultivation;
        public double ExpPerTick;
        public long   LastSeenServerTimestamp;
    }
}