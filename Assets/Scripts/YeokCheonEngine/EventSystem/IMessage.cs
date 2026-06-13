// IMessage.cs
// 이벤트 시스템에서 "메시지"로 취급받을 수 있는 타입의 조건을 정의한다.

namespace YeokCheonEngine.EventSystem
{
    // 메시지 마커 인터페이스.
    // 이 인터페이스를 구현한 struct만 MessageBus로 전달할 수 있다.
    // 내용이 비어있는 게 정상 — "이 타입은 메시지다"라는 표시 역할만 한다.
    public interface IMessage { }

    // 메시지가 전파되는 범위를 결정한다.
    public enum MessageScope
    {
        Global,  // 앱 전체에 전파. 씬이 바뀌어도 살아있음.
        Scene,   // 현재 씬에서만 전파. 씬 언로드 시 자동 해제.
        Context, // 가장 좁은 범위. 수동으로 ClearScope 해야 해제됨.
    }
}