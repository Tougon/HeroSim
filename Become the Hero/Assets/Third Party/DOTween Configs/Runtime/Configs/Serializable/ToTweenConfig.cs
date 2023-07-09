using System;
using UnityEngine;

namespace DOTweenConfigs
{
    /// <summary>
    /// Generic tween config for tweening to some value.
    /// </summary>
    [Serializable]
    public class ToTweenConfig<T> : TweenConfig
    {
        [SerializeField]
        private T m_to;

        [SerializeField]
        private T m_from;

        [SerializeField]
        private bool m_useFrom;

        [SerializeField]
        private float m_delay;

        [SerializeField]
        private DG.Tweening.Ease m_curve = DG.Tweening.Ease.Linear;

        public T To
        {
            get { return m_to; }
        }

        public T From
        {
            get { return m_from; }
        }

        public bool UseFrom
        {
            get { return m_useFrom; }
        }

        public float Delay
        {
            get { return m_delay; }
        }

        public DG.Tweening.Ease Ease
        {
            get { return m_curve; }
        }

        public ToTweenConfig()
        {
        }

        public ToTweenConfig(T to)
        {
            m_to = to;
        }
    }
}
