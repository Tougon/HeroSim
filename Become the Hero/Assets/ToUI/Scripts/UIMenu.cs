using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToUI
{
    public class UIMenu : UIScreen
    {
        [Header("Menu Options")]
        [SerializeField]
        private List<GameObject> MenuOptions;

        private bool bAllowInput;


        public override void Show()
        {
            base.Show();
            bAllowInput = false;
        }

        protected override void OnScreenShown()
        {
            base.OnScreenShown();

            bAllowInput = true;
        }

        public override void FocusChanged(bool bFocus)
        {
            base.FocusChanged(bFocus);

            bAllowInput = bFocus;
        }


        #region Input Responses

        /// <summary>
        /// Action to execute when up is pressed
        /// </summary>
        public override void OnUpPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when down is pressed
        /// </summary>
        public override void OnDownPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when left is pressed
        /// </summary>
        public override void OnLeftPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when right is pressed
        /// </summary>
        public override void OnRightPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when Confirm is pressed
        /// </summary>
        public override void OnConfirmPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when Cancel is pressed
        /// </summary>
        public override void OnCancelPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when Aux1 is pressed
        /// </summary>
        public override void OnAux1Pressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        /// <summary>
        /// Action to execute when Aux2 is pressed
        /// </summary>
        public override void OnAux2Pressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");
        }

        #endregion
    }
}
