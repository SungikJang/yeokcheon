namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public enum ViewEnterAnim
    {
        None, FadeIn, SlideFromRight, SlideFromBottom, ScaleUp,
    }

    public enum ViewExitAnim
    {
        None, FadeOut, SlideToLeft, SlideToBottom, ScaleDown,
    }

    public struct ViewAnimationConfig
    {
        public ViewEnterAnim Enter;
        public ViewExitAnim  Exit;
        public float         Duration;

        public static readonly ViewAnimationConfig Default = new()
            { Enter = ViewEnterAnim.FadeIn, Exit = ViewExitAnim.FadeOut, Duration = 0.25f };

        public static readonly ViewAnimationConfig None = new()
            { Enter = ViewEnterAnim.None, Exit = ViewExitAnim.None, Duration = 0f };

        public static readonly ViewAnimationConfig Slide = new()
            { Enter = ViewEnterAnim.SlideFromRight, Exit = ViewExitAnim.SlideToLeft, Duration = 0.3f };

        public static readonly ViewAnimationConfig Popup = new()
            { Enter = ViewEnterAnim.ScaleUp, Exit = ViewExitAnim.ScaleDown, Duration = 0.2f };
    }
}