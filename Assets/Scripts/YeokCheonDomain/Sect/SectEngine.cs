using System;
using UnityEngine;
using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Sect
{
    public sealed class SectEngine : GlobalSubEngine<SectState>
    {
        protected override void UpdateState(SectState state, ITrigger trigger)
        {
            switch (trigger)
            {
                case JoinSectTrigger t:
                    if (state.IsInSect)
                    {
                        Debug.LogWarning("[SectEngine] 이미 문파에 속해있음 — 변경 불가");
                        return;
                    }
                    state.SectId   = t.SectId;
                    state.Faction  = t.Faction;
                    state.JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    break;

                case RestoreSectTrigger t:
                    state.SectId    = t.SectId;
                    state.SectGrade = t.SectGrade;
                    state.JoinedAt  = t.JoinedAt;
                    // Faction은 SectId로 결정 (SectId 0~2 = 정파, 3~5 = 사파, 6~8 = 마교)
                    if (t.SectId >= 0)
                        state.Faction = (Faction)(t.SectId / 3);
                    break;

                default:
                    Debug.LogWarning($"[SectEngine] 알 수 없는 트리거: {trigger.GetType().Name}");
                    break;

            }
        }
    }
}