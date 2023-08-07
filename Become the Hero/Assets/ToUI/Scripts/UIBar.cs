using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ToUI
{
    public class UIBar : MonoBehaviour
    {
        public enum AnimationType { Smooth, Delayed }

        private const string AnimInfo = "Bar Animations are named Show and Hide.";

        [Header("Bar Values")]
        public float maxValue = 100;

        [Space(10)]
        public Image barFill;
        public Image barUnderlay;
        public AnimationType animationType;
        [SerializeField]
        [InfoBox("@AnimInfo")]
        protected TweenSystem tweenSystem;
        [SerializeField]
        protected TextMeshProUGUI barText;
        [SerializeField]
        [InfoBox("The current value will use {CURRENT} while the max value will use {MAX}")]
        protected string barTextFormat = "{CURRENT}/{MAX}";

        [Space(10)]
        public float barSubtractSpeed = 2;

        public bool visible { get; set; }
        private bool animating;

        private IEnumerator currentFillAnim;
        private IEnumerator currentFadeOutAnim;


        public float GetCurrentValue()
        {
            return barFill.fillAmount * maxValue;
        }


        public float GetCurrentPercent()
        {
            return barFill.fillAmount;
        }


        public void ShowUI()
        {
            if (visible)
                return;

            tweenSystem.PlayAnimation("Show", () => visible = true);
        }


        public void HideUI()
        {
            if (!visible)
                return;

            if (currentFadeOutAnim != null)
                StopCoroutine(currentFadeOutAnim);

            currentFadeOutAnim = HideUIAfterAnim();
            StartCoroutine(currentFadeOutAnim);
        }


        private IEnumerator HideUIAfterAnim()
        {
            while (animating)
                yield return null;


            tweenSystem.PlayAnimation("Hide", () => visible = false);
        }


        public void SetValueImmediate(float value)
        {
            barFill.fillAmount = value / maxValue;
            barText.text = barTextFormat.Replace("{CURRENT}", value.ToString()).
                Replace("{MAX}", maxValue.ToString());
        }


        public void SetValue(float value)
        {
            if(currentFillAnim != null) StopCoroutine(currentFillAnim);

            currentFillAnim = SetBarFillAmount(value);
            StartCoroutine(currentFillAnim);
        }


        private IEnumerator SetBarFillAmount(float val)
        {
            ShowUI();

            while (!visible)
                yield return null;

            animating = true;

            if(animationType == AnimationType.Smooth)
            {
                barFill.DOFillAmount(val / maxValue, barSubtractSpeed == 0 ? 0 : 1 / barSubtractSpeed).
                    OnComplete(() => animating = false);

                while (animating)
                {
                    int percent = Mathf.RoundToInt(barFill.fillAmount * maxValue);
                    barText.text = barTextFormat.Replace("{CURRENT}", percent.ToString()).
                        Replace("{MAX}", maxValue.ToString());
                    yield return null;
                }

                barText.text = barTextFormat.Replace("{CURRENT}", val.ToString()).
                    Replace("{MAX}", maxValue.ToString());
            }
            else
            {
                barText.text = barTextFormat.Replace("{CURRENT}", val.ToString()).
                    Replace("{MAX}", maxValue.ToString());

                barUnderlay.fillAmount = barFill.fillAmount;
                barFill.fillAmount = val / maxValue;

                yield return new WaitForSeconds(barSubtractSpeed == 0 ? 0 : 1 / barSubtractSpeed);

                barUnderlay.fillAmount = barFill.fillAmount;
            }
        }
    }
}
