using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Session
{
    public sealed class SessionEngine : GlobalSubEngine<SessionState>
    {
        protected override void UpdateState(SessionState state, ITrigger trigger)
        {
            switch (trigger)
            {
                case LoginTrigger t:
                    state.PlayerId   = t.PlayerId;
                    state.PlayerName = t.PlayerName;
                    state.CreatedAt  = t.CreatedAt;
                    break;

                case LogoutTrigger:
                    state.PlayerId   = "";
                    state.PlayerName = "수련생";
                    break;

                case RestoreSessionTrigger t:
                    state.PlayerId   = t.PlayerId;
                    state.PlayerName = t.PlayerName;
                    state.CreatedAt  = t.CreatedAt;
                    break;
            }
        }
    }
}