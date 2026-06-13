// CultivationState.cs
// 수련 도메인 상태. GlobalState 상속 → GlobalEngine에 등록 가능.
// 수련 경지(Realm), 경험치, 마지막 틱 시각을 보유.

using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Cultivation
{
    // 경지 티어. 숫자가 클수록 높은 경지.
    public enum RealmTier
    {
        // 1대경지 — 삼류
        삼류입문 = 0, 삼류중반, 삼류대성,
        // 2대경지 — 이류
        이류입문, 이류중반, 이류대성,
        // 3대경지 — 일류
        일류입문, 일류중반, 일류대성,
        // 4대경지 — 절정
        절정입문, 절정중반, 절정대성,
        // 5대경지 — 초절정
        초절정입문, 초절정중반, 초절정대성,
        // 6대경지 — 화경
        화경입문, 화경중반, 화경대성,
        // 7대경지 — 현경
        현경입문, 현경중반, 현경대성,
        // 8대경지 — 생사경
        생사경입문, 생사경중반, 생사경대성,
        // 9대경지 — 공경
        공경입문, 공경중반, 공경대성,
    }

    // 소경지(자동) / 대경지(수동) 구분
    public static bool IsBigRealm(RealmTier tier)
        => (int)tier % 3 == 2; // 대성(index 2, 5, 8...) = 대경지

    public sealed class CultivationState : GlobalState
    {
        public RealmTier Tier       { get; set; } = RealmTier.삼류입문;
        public double    CurrentExp { get; set; } = 0;
        public long      LastTickAt { get; set; } = 0; // Unix timestamp

        // 현재 경지 이름 (UI 표시용).
        public string RealmName => Tier.ToString();

        // 현재 경지에서 다음 경지로 넘어가는 데 필요한 경험치.
        // 경지가 높을수록 더 많이 필요 (지수 증가).
        public double RequiredExp => 100.0 * System.Math.Pow(1.5, (int)Tier);

        // 경험치 진행도 0~1 (프로그레스 바용).
        public float Progress => (float)(CurrentExp / RequiredExp);
    }
}