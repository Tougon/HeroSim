using UnityEngine.UI;
using DG.Tweening;

namespace DOTweenConfigs
{
    public static partial class DOTweenExtensions
    {
        public static Tweener DOFade(this Graphic target, Position1DTweenConfig f)
        {
            return target.DOFade(f.To, f.Duration);
        }
    }
}