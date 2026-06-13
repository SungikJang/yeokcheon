using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public static class ViewAnimator
    {
        public static async UniTask PlayEnterAsync(
            ViewBase view, ViewAnimationConfig config,
            CancellationToken ct = default)
        {
            if (config.Enter == ViewEnterAnim.None || config.Duration <= 0f) return;

            var cg   = GetOrAddCanvasGroup(view);
            var rect = view.GetComponent<RectTransform>();
            var dur  = config.Duration;

            PrepareEnter(config.Enter, cg, rect);
            view.gameObject.SetActive(true);

            var elapsed = 0f;
            while (elapsed < dur)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.deltaTime;
                var t    = Mathf.Clamp01(elapsed / dur);
                ApplyEnter(config.Enter, cg, rect, EaseOut(t));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            ApplyEnter(config.Enter, cg, rect, 1f);
        }

        public static async UniTask PlayExitAsync(
            ViewBase view, ViewAnimationConfig config,
            CancellationToken ct = default)
        {
            if (config.Exit == ViewExitAnim.None || config.Duration <= 0f) return;

            var cg   = GetOrAddCanvasGroup(view);
            var rect = view.GetComponent<RectTransform>();
            var dur  = config.Duration;

            var elapsed = 0f;
            while (elapsed < dur)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.deltaTime;
                var t    = Mathf.Clamp01(elapsed / dur);
                ApplyExit(config.Exit, cg, rect, EaseIn(t));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            ApplyExit(config.Exit, cg, rect, 1f);
        }

        private static void PrepareEnter(ViewEnterAnim anim, CanvasGroup cg, RectTransform rect)
        {
            switch (anim)
            {
                case ViewEnterAnim.FadeIn:   cg.alpha = 0f; break;
                case ViewEnterAnim.SlideFromRight:
                    if (rect) rect.anchoredPosition = new Vector2(Screen.width, 0f); break;
                case ViewEnterAnim.SlideFromBottom:
                    if (rect) rect.anchoredPosition = new Vector2(0f, -Screen.height); break;
                case ViewEnterAnim.ScaleUp:
                    if (rect) rect.localScale = Vector3.one * 0.85f;
                    cg.alpha = 0f; break;
            }
        }

        private static void ApplyEnter(ViewEnterAnim anim, CanvasGroup cg, RectTransform rect, float t)
        {
            switch (anim)
            {
                case ViewEnterAnim.FadeIn:   cg.alpha = t; break;
                case ViewEnterAnim.SlideFromRight:
                    if (rect) rect.anchoredPosition = new Vector2(Mathf.Lerp(Screen.width, 0f, t), 0f); break;
                case ViewEnterAnim.SlideFromBottom:
                    if (rect) rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(-Screen.height, 0f, t)); break;
                case ViewEnterAnim.ScaleUp:
                    if (rect) rect.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, t);
                    cg.alpha = t; break;
            }
        }

        private static void ApplyExit(ViewExitAnim anim, CanvasGroup cg, RectTransform rect, float t)
        {
            switch (anim)
            {
                case ViewExitAnim.FadeOut:   cg.alpha = 1f - t; break;
                case ViewExitAnim.SlideToLeft:
                    if (rect) rect.anchoredPosition = new Vector2(Mathf.Lerp(0f, -Screen.width, t), 0f); break;
                case ViewExitAnim.SlideToBottom:
                    if (rect) rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, -Screen.height, t)); break;
                case ViewExitAnim.ScaleDown:
                    if (rect) rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, t);
                    cg.alpha = 1f - t; break;
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(ViewBase view)
        {
            var cg = view.GetComponent<CanvasGroup>();
            return cg != null ? cg : view.gameObject.AddComponent<CanvasGroup>();
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseIn(float t)  => t * t;
    }
}