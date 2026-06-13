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
                    state.SectId   = t.SectId;
                    state.JoinedAt = t.JoinedAt;
                    break;

                case LeaveSectTrigger:
                    state.SectId    = -1;
                    state.SectGrade = 0;
                    state.JoinedAt  = 0;
                    break;

                case RestoreSectTrigger t:
                    state.SectId    = t.SectId;
                    state.SectGrade = t.SectGrade;
                    state.JoinedAt  = t.JoinedAt;
                    break;
            }
        }
    }
}