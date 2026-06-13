using YeokCheonEngine.EngineSystem;

namespace YeokCheonDomain.Session
{
    public struct LoginTrigger : ITrigger
    {
        public string PlayerId;
        public string PlayerName;
        public long   CreatedAt;
    }
    public struct LogoutTrigger : ITrigger { }
    public struct RestoreSessionTrigger : ITrigger
    {
        public string PlayerId; public string PlayerName; public long CreatedAt;
    }
}