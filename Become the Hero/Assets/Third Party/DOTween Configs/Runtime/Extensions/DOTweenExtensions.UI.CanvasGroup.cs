using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

namespace DOTweenConfigs
{
    public static partial class DOTweenExtensions
    {
        public static Tweener DOFade(this CanvasGroup target, Position1DTweenConfig f)
        {
            return target.DOFade(f.To, f.Duration);
        }
    }
}