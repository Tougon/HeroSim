using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using DOTweenConfigs;

namespace ToUI
{
    /// <summary>
    /// Base class for anything that constitutes a separate "tab" of UI, 
    /// be it cutscenes, menus, selection, etc. UIScreen handles all
    /// basic open/close operations.
    /// </summary>
    public class UIScreen : MonoBehaviour
    {
        [PropertyTooltip("Only recommended for testing")]
        [SerializeField] private bool bOpenOnAwake;

        [SerializeField]
        private Position3DTweenConfigAsset OpenAnimation;
        private Position3DTweenConfigAsset CloseAnimation;

        protected bool bActive { get => UIScreenQueue.Instance.CurrentScreen == this; }

        protected bool bShowing;
        protected bool bAllowInput;

        protected virtual void Awake()
        {
            if(bOpenOnAwake)
            {
                Show();
            }
        }


        public void Show()
        {
            UIScreenQueue.Instance.AddToQueue(this);

            Vector3 startPos = OpenAnimation.TweenConfig.UseFrom ?
                OpenAnimation.TweenConfig.From : transform.position;

            transform.DOMove(OpenAnimation.TweenConfig).
                SetDelay(OpenAnimation.TweenConfig.Delay).
                SetEase(OpenAnimation.TweenConfig.Ease).
                ChangeStartValue(startPos).
                OnComplete(() => Debug.Log("Complete"));
        }


        public void Hide()
        {
            bAllowInput = false;
        }
    }
}