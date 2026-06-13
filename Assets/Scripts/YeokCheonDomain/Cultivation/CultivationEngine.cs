// CultivationEngine.cs
// Trigger → CultivationState 전이 로직.
// GlobalSubEngine<CultivationState> 상속 → GlobalEngine에 등록.

using UnityEngine;
using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Cultivation
{
    public sealed class CultivationEngine : GlobalSubEngine<CultivationState>
    {
        // 최고 경지. 이 이상은 돌파 불가.
        private static readonly RealmTier MaxTier =
            (RealmTier)(System.Enum.GetValues(typeof(RealmTier)).Length - 1);

        protected override void UpdateState(CultivationState state, ITrigger trigger)
        {
            switch (trigger)
            {
                case AddExpTrigger t:
                    ApplyExp(state, t.Amount);
                    break;

                case BreakthroughTrigger:
                    TryBreakthrough(state);
                    break;

                case RestoreCultivationTrigger t:
                    state.Tier       = (RealmTier)t.RealmTier;
                    state.CurrentExp = t.CurrentExp;
                    state.LastTickAt = t.LastTickAt;
                    break;

                default:
                    Debug.LogWarning(
                        $"[CultivationEngine] 알 수 없는 트리거: {trigger.GetType().Name}");
                    break;
            }
        }

        // 경험치 적용. RequiredExp 넘으면 자동 돌파 (연속 돌파 가능).
        private static void ApplyExp(CultivationState state, double amount)
        {
            state.CurrentExp += amount;

            // 연속 돌파: while로 경험치 넘침 처리.
            while (state.CurrentExp >= state.RequiredExp
                   && state.Tier < MaxTier)
            {
                state.CurrentExp -= state.RequiredExp; // 초과분 이월
                state.Tier        = (RealmTier)((int)state.Tier + 1);
                Debug.Log($"[Cultivation] 돌파! → {state.RealmName}");
            }

            // 최고 경지: 경험치 캡.
            if (state.Tier == MaxTier)
                state.CurrentExp = System.Math.Min(
                    state.CurrentExp, state.RequiredExp);
        }

        private int _consecutiveFailCount = 0;
        private const int PityThreshold   = 5;    // 천장
        private const float FailRate      = 0.25f; // 25%
        private const float ExpLossRate   = 0.30f; // 실패 시 30% 손실

        private void TryBreakthrough(CultivationState state)
        {
            if (state.Tier >= MaxTier) return;
            if (state.CurrentExp < state.RequiredExp) return;

            // 소경지: 자동 돌파, 실패 없음
            if (!CultivationState.IsBigRealm(state.Tier))
            {
                DoBreakthrough(state);
                return;
            }

            // 대경지: 실패 확률 적용. 천장(5연속 실패) 시 무조건 성공.
            bool success = _consecutiveFailCount >= PityThreshold
                           || UnityEngine.Random.value > FailRate;

            if (success)
            {
                _consecutiveFailCount = 0;
                DoBreakthrough(state);
            }
            else
            {
                _consecutiveFailCount++;
                state.CurrentExp -= state.RequiredExp * ExpLossRate;
                if (state.CurrentExp < 0) state.CurrentExp = 0;
                Debug.Log($"[Cultivation] 돌파 실패! ({_consecutiveFailCount}회 연속)");
            }
        }

        private static void DoBreakthrough(CultivationState state)
        {
            state.CurrentExp -= state.RequiredExp;
            state.Tier        = (RealmTier)((int)state.Tier + 1);
            Debug.Log($"[Cultivation] 돌파! → {state.Tier}");
        }
    }
}