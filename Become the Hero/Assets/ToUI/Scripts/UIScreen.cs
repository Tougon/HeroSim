using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using DOTweenConfigs;
using Sirenix.Serialization;

namespace ToUI
{
    /// <summary>
    /// Base class for anything that constitutes a separate "tab" of UI, 
    /// be it cutscenes, menus, selection, etc. UIScreen handles all
    /// basic open/close operations.
    /// </summary>
    public class UIScreen : SerializedMonoBehaviour
    {
        [Header("Properties")]
        [PropertyTooltip("Opens this screen on Awake. Only recommended for testing")]
        [SerializeField] private bool bOpenOnAwake;
        [PropertyTooltip("Closes this screen when focus is lost.")]
        [SerializeField] private bool bCloseOnLoseFocus;

        [Header("Animation")]
        [SerializeField]
        protected TweenSystem AnimationSource;

        private RectTransform rectTransform;

        protected bool bActive { get => UIScreenQueue.Instance.CurrentScreen == this; }

        protected bool bShowing;
        // Move to child class
        //protected bool bAllowInput;

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
        }


        protected virtual void Start()
        {
            if (bOpenOnAwake)
            {
                Show();
            }
        }


        /// <summary>
        /// Opens the screen
        /// </summary>
        public virtual void Show()
        {
            UIScreenQueue.Instance.AddToQueue(this);

            AnimationSource?.PlayAnimation("Open", OnScreenShown);
        }


        /// <summary>
        /// Virtual function called when a screen is shown
        /// </summary>
        protected virtual void OnScreenShown()
        {

        }


        /// <summary>
        /// Virtual function called when a screen is hidden
        /// </summary>
        protected virtual void OnScreenHide()
        {

        }


        /// <summary>
        /// Called when this screen's focus changes from another menu opening.
        /// </summary>
        public virtual void FocusChanged(bool bFocus)
        {
            if (bFocus)
            {

            }
            else
            {
                if(bCloseOnLoseFocus)
                {
                    Hide();
                }
            }
        }


        /// <summary>
        /// Closes the screen
        /// </summary>
        public virtual void Hide()
        {
            UIScreenQueue.Instance.RemoveFromQueue(this);

            AnimationSource?.PlayAnimation("Close", OnScreenShown);
        }


        #region Input Responses

        /// <summary>
        /// Executs the proper movement functions based on provided value
        /// </summary>
        public virtual void OnMovementUpdate(Vector2 movement)
        {
            if (movement.y > 0.5f)
                OnUpPressed();
            else if (movement.y < -0.5f)
                OnDownPressed();

            if (movement.x > 0.5f)
                OnRightPressed();
            else if (movement.x < -0.5f)
                OnLeftPressed();
        }

        /// <summary>
        /// Action to execute when up is pressed
        /// </summary>
        public virtual void OnUpPressed()
        {

        }

        /// <summary>
        /// Action to execute when down is pressed
        /// </summary>
        public virtual void OnDownPressed()
        {

        }

        /// <summary>
        /// Action to execute when left is pressed
        /// </summary>
        public virtual void OnLeftPressed()
        {

        }

        /// <summary>
        /// Action to execute when right is pressed
        /// </summary>
        public virtual void OnRightPressed()
        {

        }

        /// <summary>
        /// Action to execute when Confirm is pressed
        /// </summary>
        public virtual void OnConfirmPressed()
        {

        }

        /// <summary>
        /// Action to execute when Cancel is pressed
        /// </summary>
        public virtual void OnCancelPressed()
        {

        }

        /// <summary>
        /// Action to execute when Aux1 is pressed
        /// </summary>
        public virtual void OnAux1Pressed()
        {

        }

        /// <summary>
        /// Action to execute when Aux2 is pressed
        /// </summary>
        public virtual void OnAux2Pressed()
        {

        }

        #endregion
    }
}