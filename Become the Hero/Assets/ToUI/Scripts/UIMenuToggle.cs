using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToUI
{
    public class UIMenuToggle : UIMenuItem
    {
        [Header("Toggle Parameters")]
        [SerializeField] private bool InitialState = false;
        [SerializeField] private GameObject ShowOn;
        [SerializeField] private GameObject ShowOff;

        public BetterEvent OnToggleOn;
        public BetterEvent OnToggleOff;

        public delegate void OnToggleRuntimeSignature(bool toggle);
        public OnToggleRuntimeSignature OnToggleRuntime;

        private bool bToggled = false;
        public bool Toggle { get => bToggled; }


        protected override void Awake()
        {
            base.Awake();

            ToggleState(InitialState, false);
        }

        public override void OnConfirmPressed()
        {
            if (!this.enabled) return;

            base.OnConfirmPressed();

            ToggleState(!bToggled);
        }


        private void ToggleState(bool value, bool bCallbacks = true)
        {
            bToggled = value;

            if(ShowOn) ShowOn.SetActive(bToggled);
            if(ShowOff) ShowOff.SetActive(!bToggled);

            if(bToggled)
            {
                OnToggleOn.Invoke();
            }
            else
            {
                OnToggleOff.Invoke();
            }

            OnToggleRuntime?.Invoke(bToggled);
        }
    }
}