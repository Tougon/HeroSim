using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using DG.Tweening;

namespace DOTweenConfigs
{
    /// <summary>
    /// Represents any possible object that would need to be tweened for a given action.
    /// This is used to avoid excessive GetComponent and cast calls that may slow down the game. 
    /// </summary>
    public struct TweenActionCache
    {
        public Transform transform;
        public RectTransform rectTransform;
        public SpriteRenderer sprite;
        public Image image;
        public TextMeshProUGUI text;
        public CanvasGroup canvasGroup;


        public void KillAllTweens()
        {
            transform.DOKill();
            rectTransform.DOKill();
            sprite.DOKill();
            image.DOKill();
            text.DOKill();
            canvasGroup.DOKill();
        }
    }


    /// <summary>
    /// Represents a single Tween action
    /// </summary>
    [System.Serializable]
    public struct TweenAction
    {
        public enum TweenType { Position, Rotation, Scale, AnchoredPosition, 
            ColorSprite, ColorImage, ColorText, Alpha, Shake }

        public TweenType tweenType;

        [ShowIf("@tweenType == TweenType.AnchoredPosition")]
        public Position2DTweenConfigAsset position2dTweenConfig;
        [ShowIf("@tweenType == TweenType.Position || tweenType == TweenType.Rotation")]
        public Position3DTweenConfigAsset position3dTweenConfig;
        [ShowIf("@tweenType == TweenType.Scale")]
        public Scale3DTweenConfigAsset scale3dTweenConfig;
        [ShowIf("@tweenType == TweenType.ColorSprite || tweenType == TweenType.ColorImage || tweenType == TweenType.ColorText")]
        public ColorTweenConfigAsset colorTweenConfig;
        [ShowIf("@tweenType == TweenType.Alpha")]
        public Position1DTweenConfigAsset alphaTweenConfig;
        [ShowIf("@tweenType == TweenType.Shake")]
        public SnapShake3DTweenConfigAsset shake3dTweenConfig;


        public void Execute(TweenActionCache target, TweenStateInstance source)
        {
            bool bTweening = false;

            switch (tweenType)
            {
                case TweenType.Position:

                    if(target.transform)
                    {
                        bTweening = true;
                        target.transform.DOLocalMove(position3dTweenConfig.TweenConfig).
                            ChangeStartValue(position3dTweenConfig.TweenConfig.UseFrom ? 
                                position3dTweenConfig.TweenConfig.From : target.transform.localPosition).
                            SetDelay(position3dTweenConfig.TweenConfig.Delay).
                            SetEase(position3dTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.Rotation:

                    if (target.transform)
                    {
                        bTweening = true;
                        target.transform.DOLocalRotate(position3dTweenConfig.TweenConfig).
                            ChangeStartValue(position3dTweenConfig.TweenConfig.UseFrom ?
                                position3dTweenConfig.TweenConfig.From : target.transform.localEulerAngles).
                            SetDelay(position3dTweenConfig.TweenConfig.Delay).
                            SetEase(position3dTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.Scale:

                    if (target.transform)
                    {
                        bTweening = true;
                        target.transform.DOScale(scale3dTweenConfig.TweenConfig).
                            ChangeStartValue(scale3dTweenConfig.TweenConfig.UseFrom ?
                                scale3dTweenConfig.TweenConfig.From : target.transform.localScale).
                            SetDelay(scale3dTweenConfig.TweenConfig.Delay).
                            SetEase(scale3dTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.AnchoredPosition:

                    if (target.rectTransform)
                    {
                        bTweening = true;
                        target.rectTransform.DOAnchorPos(position2dTweenConfig.TweenConfig).
                            ChangeStartValue(position2dTweenConfig.TweenConfig.UseFrom ?
                                position2dTweenConfig.TweenConfig.From : target.rectTransform.anchoredPosition).
                            SetDelay(position2dTweenConfig.TweenConfig.Delay).
                            SetEase(position2dTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.ColorSprite:

                    if (target.sprite)
                    {
                        bTweening = true;
                        target.sprite.DOColor(colorTweenConfig.TweenConfig).
                            ChangeStartValue(colorTweenConfig.TweenConfig.UseFrom ?
                                colorTweenConfig.TweenConfig.From : target.sprite.color).
                            SetDelay(colorTweenConfig.TweenConfig.Delay).
                            SetEase(colorTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.ColorText:

                    if (target.text)
                    {
                        bTweening = true;
                        target.text.DOColor(colorTweenConfig.TweenConfig).
                            ChangeStartValue(colorTweenConfig.TweenConfig.UseFrom ?
                                colorTweenConfig.TweenConfig.From : target.text.color).
                            SetDelay(colorTweenConfig.TweenConfig.Delay).
                            SetEase(colorTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.ColorImage:

                    if (target.image)
                    {
                        bTweening = true;
                        target.image.DOColor(colorTweenConfig.TweenConfig).
                            ChangeStartValue(colorTweenConfig.TweenConfig.UseFrom ?
                                colorTweenConfig.TweenConfig.From : target.image.color).
                            SetDelay(colorTweenConfig.TweenConfig.Delay).
                            SetEase(colorTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.Alpha:

                    if (target.canvasGroup)
                    {
                        bTweening = true;
                        target.canvasGroup.DOFade(alphaTweenConfig.TweenConfig).
                            ChangeStartValue(alphaTweenConfig.TweenConfig.UseFrom ?
                                alphaTweenConfig.TweenConfig.From : target.canvasGroup.alpha).
                            SetDelay(alphaTweenConfig.TweenConfig.Delay).
                            SetEase(alphaTweenConfig.TweenConfig.Ease).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;

                case TweenType.Shake:

                    if (target.transform)
                    {
                        bTweening = true;
                        target.transform.DOShakePosition(shake3dTweenConfig.TweenConfig).
                            OnComplete(() => source.OnTweenActionEnd());
                    }
                    break;
            }

            if(!bTweening)
            {
                source.OnTweenActionEnd();
            }
        }
    }


    /// <summary>
    /// Represents one or more tweens that begin simultaneously
    /// </summary>
    [System.Serializable]
    public struct TweenFrame
    {
        public List<TweenAction> FrameActions;

        /// <summary>
        /// Executes all TweenActions on the provided target
        /// </summary>
        public bool Execute(TweenActionCache target, TweenStateInstance source)
        {
            foreach(var action in FrameActions)
            {
                action.Execute(target, source);
            }

            return FrameActions.Count > 0;
        }
    }
}
