// ViewAnimator.cs
// UniTask 기반 Enter/Exit 애니메이션.
// DOTween 없이 직접 구현 (의존성 최소화).

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YeokCheonEngine.ElementSystem.ViewSystem
{
    public sealed class ViewAnimator : MonoBehaviour
    {
        [SerializeField] private ViewAnimationType _enterType = ViewAnimationType.FadeIn;
        [SerializeField] private ViewAnimationType _exitType  = ViewAnimationType.FadeOut;
        [SerializeField] private float             _duration  = 0.25f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            // CanvasGroup이 없으면 추가 (Fade 애니에 필요).
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public async UniTask PlayEnter()
        {
            switch (_enterType)
            {
                case ViewAnimationType.FadeIn:
                    await Fade(0f, 1f, _duration);
                    break;
                case ViewAnimationType.SlideUp:
                    await SlideY(-100f, 0f, _duration);
                    break;
                case ViewAnimationType.None:
                default:
                    break;
            }
        }

        public async UniTask PlayExit()
        {
            switch (_exitType)
            {
                case ViewAnimationType.FadeOut:
                    await Fade(1f, 0f, _duration);
                    break;
                case ViewAnimationType.SlideDown:
                    await SlideY(0f, -100f, _duration);
                    break;
                case ViewAnimationType.None:
                default:
                    break;
            }
        }

        private async UniTask Fade(float from, float to, float duration)
        {
            _canvasGroup.alpha = from;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed           += Time.deltaTime;
                _canvasGroup.alpha =  Mathf.Lerp(from, to, elapsed / duration);
                await UniTask.Yield(); // 1프레임 대기 (GC 없음)
            }
            _canvasGroup.alpha = to;
        }

        private async UniTask SlideY(float fromY, float toY, float duration)
        {
            var rt      = transform as RectTransform;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed      += Time.deltaTime;
                var pos       = rt.anchoredPosition;
                pos.y         = Mathf.Lerp(fromY, toY, elapsed / duration);
                rt.anchoredPosition = pos;
                await UniTask.Yield();
            }
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, toY);
        }
    }

    public enum ViewAnimationType
    {
        None,
        FadeIn,
        FadeOut,
        SlideUp,
        SlideDown
    }
}