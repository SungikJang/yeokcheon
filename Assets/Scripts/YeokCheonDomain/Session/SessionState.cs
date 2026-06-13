using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Session
{
    public sealed class SessionState : GlobalState
    {
        public string PlayerId   = "";
        public string PlayerName = "수련생";
        public long   CreatedAt  = 0;
        public bool   IsLoggedIn => !string.IsNullOrEmpty(PlayerId);
    }
}