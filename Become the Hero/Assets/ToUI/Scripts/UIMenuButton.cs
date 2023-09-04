using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ToUI
{
    public class UIMenuButton : UIMenuItem
    {
        public BetterEvent OnConfirm;
        public BetterEvent OnCancel;

        public delegate void OnConfirmRuntimeSignature();
        public OnConfirmRuntimeSignature OnConfirmRuntime;

        public override void OnConfirmPressed()
        {
            if (!this.enabled) return;

            AnimationSource?.PlayAnimation("Press");

            base.OnConfirmPressed();

            OnConfirm.Invoke();
            OnConfirmRuntime?.Invoke();
        }


        public override void OnCancelPressed()
        {
            if (!this.enabled) return;

            AnimationSource?.PlayAnimation("Press");

            base.OnCancelPressed();

            OnCancel.Invoke();
        }
    }
}
