using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

namespace ToUI
{
    public class UIMenuSlider : UIMenuItem
    {
        public enum SliderType { Smooth, Hinged }
        /// <summary>
        /// Represents the input direction when using keyboard or joystick input. Ignored for mouse.
        /// </summary>
        public enum InputDirection { Horizontal, Vertical }

        [Header("Slider Parameters")]
        [SerializeField] protected SliderType Type;
        [OnValueChanged("ValidateValues")][PropertyRange("MinValue", "MaxValue")]
        [SerializeField] protected float StartValue = 0;
        [OnValueChanged("ValidateValues")]
        [SerializeField] protected float MinValue = 0;
        [OnValueChanged("ValidateValues")]
        [SerializeField] protected float MaxValue = 1;
        [OnValueChanged("ValidateValues", true)][ShowIf("@Type == SliderType.Smooth")]
        [PropertyTooltip("List of points to snap to when smoothly sliding. Percentage based.")]
        [SerializeField] protected float[] HingePoints;
        [ShowIf("@Type == SliderType.Hinged")]
        [PropertyTooltip("Percent at which to insert a hinge.")]
        [SerializeField] protected float HingePercentage = 0.5f;
        private const float HINGE_DELAY_TIME = 0.2f;

        /// <summary>
        /// Validates the initial values of the slider
        /// </summary>
        private void ValidateValues()
        {
            if(MinValue > MaxValue)
            {
                MinValue = MaxValue;
            }

            if(MaxValue < MinValue)
            {
                MaxValue = MinValue;
            }

            StartValue = Mathf.Clamp(StartValue, MinValue, MaxValue);

            for(int i=0; i<HingePoints.Length; i++)
            {
                HingePoints[i] = Mathf.Clamp(HingePoints[i], 0, 1);
            }
        }


        [PropertySpace(20)]
        [Header("Slider Visuals")]
        [SerializeField]
        protected InputDirection inputDirection;
        public Image Fill;
        [SerializeField]
        protected float FillRate = 1.0f;

        [PropertySpace(20)]
        public UnityEvent<float> OnValueChanged;

        public delegate void OnValueChangedRuntimeSignature(float value);
        public OnValueChangedRuntimeSignature OnValueChangedRuntime;

        private float currentValue;
        private float currentDirection;
        private int nextHingeIndex = 0;
        private int prevHingeIndex = 0;
        private bool bProcessInput = true;
        private bool bUseHinge;
        private IEnumerator HingeDelay;


        protected override void Awake()
        {
            base.Awake();

            // Automatically generate hinge points if using the hinged slider.
            if(Type == SliderType.Hinged)
            {
                List<float> hinges = new List<float>();
                float currentHinge = HingePercentage;

                while (currentHinge < 1)
                {
                    hinges.Add(currentHinge);
                    currentHinge += HingePercentage;
                }

                HingePoints = hinges.ToArray();
            }

            // Default the current value. Do not use callbacks
            SetValue(StartValue, false);

            // Calculate the current hinge index.
            float percentage = currentValue / MaxValue;

            nextHingeIndex = 0;

            for (int i = 0; i < HingePoints.Length; i++)
            {
                if (HingePoints[i] < percentage)
                    nextHingeIndex++;

            }

            bUseHinge = HingePoints.Length > 0;
            prevHingeIndex = nextHingeIndex - 1;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            bProcessInput = true;
        }


        #region Input Responses

        public override void OnRightPressed()
        {
            base.OnRightPressed();

            if(inputDirection == InputDirection.Horizontal)
                AdjustValue(1);
        }

        public override void OnLeftPressed()
        {
            base.OnRightPressed();

            if (inputDirection == InputDirection.Horizontal)
                AdjustValue(-1);
        }

        public override void OnUpPressed()
        {
            base.OnUpPressed();

            if (inputDirection == InputDirection.Vertical)
                AdjustValue(1);
        }

        public override void OnDownPressed()
        {
            base.OnDownPressed();

            if (inputDirection == InputDirection.Vertical)
                AdjustValue(-1);
        }

        #endregion


        protected virtual void AdjustValue(float direction)
        {
            if (direction != currentDirection)
            {
                currentDirection = direction;

                if (HingeDelay != null)
                    StopCoroutine(HingeDelay);

                bProcessInput = true;
            }

            // Ignore if we just snapped to a new value
            if (!bProcessInput) return;

            direction *= (Time.deltaTime * FillRate);
            SetValueInternal(currentValue + direction);

        }


        /// <summary>
        /// Directly sets the slider's value to the given amount.
        /// </summary>
        public void SetValue(float NewValue, bool callbacks = true)
        {
            SetValueInternal(NewValue, callbacks);
        }


        /// <summary>
        /// Function to set the slider's value. This will handle clamping and callbacks.
        /// </summary>
        private void SetValueInternal(float NewValue, bool callbacks = true)
        {
            NewValue = Mathf.Clamp(NewValue, MinValue, MaxValue);

            if(NewValue != currentValue)
            {
                if (bUseHinge)
                {
                    // Calculate percentage.
                    float newPercent = NewValue / MaxValue;

                    // If the next hinge index is valid and the new percentage exceeds the limit, advance.
                    if ((nextHingeIndex >= 0 && nextHingeIndex < HingePoints.Length) &&
                        currentDirection > 0 && newPercent >= HingePoints[nextHingeIndex])
                    {
                        // Clamp new value
                        NewValue = HingePoints[nextHingeIndex] * MaxValue;

                        // Update indices
                        prevHingeIndex = nextHingeIndex;
                        nextHingeIndex++;

                        // Start the delay
                        HingeDelay = HingeRoutine();
                        StartCoroutine(HingeDelay);
                    }
                    // If the previous hinge index is valid and the new percentage is less than the limit, advance.
                    else if ((prevHingeIndex >= 0 && prevHingeIndex < HingePoints.Length) &&
                        currentDirection < 0 && newPercent <= HingePoints[prevHingeIndex])
                    {
                        // Clamp new value
                        NewValue = HingePoints[prevHingeIndex] * MaxValue;

                        // Update indices
                        nextHingeIndex = prevHingeIndex;
                        prevHingeIndex--;

                        HingeDelay = HingeRoutine();
                        StartCoroutine(HingeDelay);
                    }
                }

                currentValue = NewValue;

                if (callbacks)
                {
                    OnValueChanged.Invoke(currentValue);
                    OnValueChangedRuntime?.Invoke(currentValue);
                }
            }

            UpdateSliderVisuals();
        }


        private IEnumerator HingeRoutine()
        {
            bProcessInput = false;
            yield return new WaitForSeconds(HINGE_DELAY_TIME);
            bProcessInput = true;
        }


        /// <summary>
        /// Function to update the slider's fill and the position of its handle if any.
        /// </summary>
        private void UpdateSliderVisuals()
        {
            Fill.fillAmount = (currentValue / MaxValue);

            // TODO: handle
        }
    }
}
