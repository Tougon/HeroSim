using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

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
        protected bool bActive { get => UIScreenQueue.Instance.CurrentScreen == this; }

        protected bool bShowing;
        protected bool bAllowInput;

        protected virtual void Awake()
        {
            if(bOpenOnAwake)
            {
                
            }
        }


        public void Show()
        {

        }


        public void Hide()
        {
            bAllowInput = false;
        }
    }
}