using DOTweenConfigs;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToUI
{
    public class UIMenuItem : MonoBehaviour
    {
        private const string AnimInfo = "Menu Item Animations are named Normal, Select, and Disable";

        [Header("Animation")]
        [InfoBox("@AnimInfo")]
        [SerializeField]
        protected TweenSystem AnimationSource;

        protected bool bSelected;


        protected virtual void Awake()
        {
            if (!this.enabled)
                AnimationSource?.PlayAnimation("Disable");
        }


        protected virtual void OnEnable()
        {
            AnimationSource?.PlayAnimation("Normal");
        }

        /// <summary>
        /// Sets the selected state of this menu item
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelected(bool selected)
        {
            if (!this.enabled)
            {
                return;
            }

            bSelected = selected;

            if (bSelected)
            {
                AnimationSource?.PlayAnimation("Select");
            }
            else
            {
                AnimationSource?.PlayAnimation("Normal");
            }
        }

        protected virtual void OnDisable()
        {
            if(gameObject.activeInHierarchy)
                AnimationSource?.PlayAnimation("Disable");
        }

        #region Input Responses

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
