using DOTweenConfigs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToUI
{
    public class UIMenuItem : MonoBehaviour
    {
        // TODO: Unselect Idle, Select Idle, Confirm, Disable, Disable Idle
        [Header("Animation")]
        [SerializeField]
        private ColorTweenConfigAsset SelectedAnimation;
        [SerializeField]
        private ColorTweenConfigAsset UnselectedAnimation;
        // Temp
        [SerializeField]
        private Image Temp;

        protected bool bSelected;

        /// <summary>
        /// Sets the selected state of this menu item
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelected(bool selected)
        {
            bSelected = selected;

            if (bSelected)
            {
                Debug.Log("Selected!!!!" + this.name);
                Temp.DOColor(SelectedAnimation.TweenConfig);
            }
            else
            {
                Debug.Log("Unselected!!!! " + this.name);
                Temp.DOColor(UnselectedAnimation.TweenConfig);
            }
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
