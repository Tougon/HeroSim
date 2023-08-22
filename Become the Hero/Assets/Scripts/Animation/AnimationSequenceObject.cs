using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// An object that contains the text data for an <see cref="AnimationSequence"/>
/// </summary>
[CreateAssetMenu(fileName = "NewAnimationSequence", menuName = "Animation/Animation Sequence Object", order = 2)]
public class AnimationSequenceObject : ScriptableObject
{
    public string animationName = "";

    [Header("Legacy")][PropertyOrder(25)]
    [FormerlySerializedAs("animationSequence")]
    public TextAsset animationSequenceText;

    [ListDrawerSettings(DraggableItems = false, ShowPaging = false)]
    [OnValueChanged("SortByFrameOrder", true)]
    [PropertyOrder(20)][PropertySpace(10)]
    public List<AnimationSequenceFrame> animationSequence = new List<AnimationSequenceFrame>();

    public bool disableUI = true;


    [Button("Parse Text Asset")]
    [PropertyOrder(25)]
    public void ParseTextAsset()
    {
        // Split the animation script.
        string[] sequence = animationSequenceText.text.Split('\n');

        animationSequence = new List<AnimationSequenceFrame>();
        int currentFrame = 0;
        int subFrame = 0;

        for (int i = 0; i < sequence.Length; i++)
        {
            // Remove dividing characters
            string[] line = sequence[i].Split('|');

            // If line does not have enough values, show an error
            if (line.Length > 3 || line.Length < 2)
            {
                Debug.LogError("Invalid format on line " + (i+1) + "!");
                continue;
            }

            // Create the action for the given line
            AnimationSequenceFrame seq = new AnimationSequenceFrame();

            seq.frame = int.Parse(line[0]);

            if(seq.frame != currentFrame)
            {
                currentFrame = seq.frame;
                subFrame = 0;
            }
            else
            {
                subFrame++;
            }

            seq.frameOrder = subFrame;
            seq.action = (AnimationSequenceAction.Action)Enum.Parse(typeof(AnimationSequenceAction.Action), line[1]);
            seq.OnActionChanged();
            seq.FromString(line.Length > 2 ? line[2] : "");

            animationSequence.Add(seq);
        }
    }


    private void SortByFrameOrder()
    {
        animationSequence.Sort((x, y) =>
        {
            int frames = x.frame.CompareTo(y.frame);

            if(frames == 0 ) return x.frameOrder.CompareTo(y.frameOrder);

            return frames;
        });

        bool refresh = false;

        foreach(var frame in animationSequence)
        {
            if (frame.bFrameDirty) refresh = true;
            frame.bFrameDirty = false;
        }

#if UNITY_EDITOR

        if (refresh)
        {
            Selection.objects = null;
            EditorApplication.delayCall += () => Selection.objects = new[] { this };
        }
#endif
    }
}


[System.Serializable]
public class AnimationSequenceFrame
{
    [HideInInspector]
    public bool bFrameDirty;

    [HorizontalGroup("Primary", Gap = 3)]
    [OnValueChanged("OnActionChanged")]
    public AnimationSequenceAction.Action action;

    [HorizontalGroup("Primary")]
    [OnValueChanged("OnFrameChanged")]
    public int frame;

    [HorizontalGroup("Primary")]
    [OnValueChanged("OnFrameChanged")]
    [PropertyTooltip("Use to determine order within a frame")]
    public int frameOrder;

    [HideLabel]
    public AnimationSequenceParams param;


    public void OnActionChanged()
    {
        switch (action)
        {
            case AnimationSequenceAction.Action.ChangeUserAnimation:
                param = new UserAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.ChangeTargetAnimation:
                param = new UserAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.GenerateEffect:
                param = new GenerateEffectAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.TerminateEffect:
                param = new TerminateEffectAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.Move:
                param = new TransformAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.Rotate:
                param = new TransformAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.Scale:
                param = new TransformAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.Color:
                param = new ColorAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.Vibrate:
                param = new VibrateAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.ChangeAnimationSpeed:
                param = new FrameSpeedAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.ChangeAnimationState:
                param = new TriggerAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.BeginLoop:
                param = new BeginLoopAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.EndLoop:
                param = new AnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.ApplyDamage:
                param = new DamageAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.PlaySound:
                param = new AudioAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.UpdateHPUI:
                param = new EntityAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.UpdateMPUI:
                param = new EntityAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.SetOverlayTexture:
                param = new SetOverlayAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.SetOverlayAnimation:
                param = new OverlayAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.ChangeBGColor:
                param = new BGColorAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.StartBGFade:
                param = new BGFadeAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.ResetBGColor:
                param = new ResetBGAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.TerminateAnimation:
                param = new AnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.Sprite:
                param = new SpriteAnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.BeginOnSuccess:
                param = new AnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.EndOnSuccess:
                param = new AnimationSequenceParams();
                break;
            case AnimationSequenceAction.Action.SetTargetIndex:
                param = new TargetIndexAnimationSequenceParams();
                break;
        }
    }


    public void FromString(string str)
    {
        if(param != null) param.FromString(str);
    }


    private void OnFrameChanged()
    {
        if (frame < 0)
        {
            frame = 0;
        }

        bFrameDirty = true;
    }
}


[System.Serializable]
public class AnimationSequenceParams
{
    [ListDrawerSettings(IsReadOnly = true, ShowIndexLabels = false, ShowPaging = false, 
        DefaultExpandedState = true, ShowFoldout = true)]
    public List<AnimationSequenceParam> parameters;

    public AnimationSequenceParams()
    {
        Initialize();
    }


    protected virtual void Initialize()
    {

    }


    public bool GetBool(string key)
    {
        var param = parameters.Find((a) => a.name == key && a.paramType == AnimationSequenceParam.ParamType.Bool);

        if (param != null) return param.boolValue;
        return false;
    }


    public int GetInt(string key)
    {
        var param = parameters.Find((a) => a.name == key && a.paramType == AnimationSequenceParam.ParamType.Int);

        if (param != null) return param.intValue;
        return 0;
    }


    public float GetFloat(string key)
    {
        var param = parameters.Find((a) => a.name == key && a.paramType == AnimationSequenceParam.ParamType.Float);

        if (param != null) return param.floatValue;
        return 0;
    }


    public string GetString(string key)
    {
        var param = parameters.Find((a) => a.name == key && a.paramType == AnimationSequenceParam.ParamType.String);

        if (param != null) return param.stringValue;
        return "";
    }


    public Vector2 GetVector2(string key)
    {
        var param = parameters.Find((a) => a.name == key && a.paramType == AnimationSequenceParam.ParamType.Vector2);

        if (param != null) return param.vector2Value;
        return Vector2.zero;
    }


    public Vector3 GetVector3(string key)
    {
        var param = parameters.Find((a) => a.name == key && a.paramType == AnimationSequenceParam.ParamType.Vector3);

        if (param != null) return param.vector3Value;
        return Vector3.zero;
    }


    public Color GetColor(string key)
    {
        var param = parameters.Find((a) => a.name == key && 
            (a.paramType == AnimationSequenceParam.ParamType.Color || 
             a.paramType == AnimationSequenceParam.ParamType.ColorNoAlpha));

        if (param != null) return param.colorValue;
        return Color.white;
    }


    public void FromString(string str)
    {
        if (string.IsNullOrEmpty(str)) return;

        // Split the param
        string[] param = str.Split(',');
        int paramIndex = 0;

        for(int i=0; i<param.Length; i++)
        {
            if (paramIndex < parameters.Count)
            {
                parameters[paramIndex].FromString(str, i);

                if (parameters[paramIndex].paramType == AnimationSequenceParam.ParamType.Vector3) i += 2;
                else if (parameters[paramIndex].paramType == AnimationSequenceParam.ParamType.Color) i += 3;
                else if (parameters[paramIndex].paramType == AnimationSequenceParam.ParamType.ColorNoAlpha) i += 2;
                else if (parameters[paramIndex].paramType == AnimationSequenceParam.ParamType.Vector2) i += 1;

                paramIndex++;
            }
        }
    }
}


[System.Serializable]
public class AnimationSequenceParam
{
    public enum ParamType { Bool, Int, Float, String, Vector2, Vector3, Color, ColorNoAlpha }

    [HideInInspector]
    public ParamType paramType;

    [ReadOnly]
    [HideLabel][HorizontalGroup("Primary", Gap = 3)]
    public string name;
    
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.Bool")]
    public bool boolValue;
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.Int")]
    public int intValue;
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.Float")]
    public float floatValue;
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.String")]
    public string stringValue;
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.Vector2")]
    public Vector2 vector2Value;
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.Vector3")]
    public Vector3 vector3Value;
    [HideLabel][HorizontalGroup("Primary")]
    [ShowIf("@paramType == ParamType.Color || paramType == ParamType.ColorNoAlpha")]
    public Color colorValue;


    public AnimationSequenceParam(string name, ParamType paramType)
    {
        this.name = name;
        this.paramType = paramType;
    }


    public void FromString(string str, int index)
    {
        // Split the param
        string[] param = str.Split(',');

        if (paramType == ParamType.Vector2 || paramType == ParamType.Vector3 || 
            paramType == ParamType.Color || paramType == ParamType.ColorNoAlpha)
        {
            if(paramType == ParamType.Vector3)
            {
                vector3Value = new Vector3(Single.Parse(param[index]), 
                    Single.Parse(param[index + 1]), Single.Parse(param[index + 2]));
            }
            else if(paramType == ParamType.Color)
            {
                colorValue = new Color(Single.Parse(param[index]), Single.Parse(param[index + 1]),
                    Single.Parse(param[index + 2]), Single.Parse(param[index + 3]));
            }
            else if (paramType == ParamType.ColorNoAlpha)
            {
                colorValue = new Color(Single.Parse(param[index]), Single.Parse(param[index + 1]),
                    Single.Parse(param[index + 2]), 1);
            }
            else
            {
                vector2Value = new Vector2(Single.Parse(param[index]), Single.Parse(param[index + 1]));
            }
        }
        else
        {
            switch (paramType)
            {
                case ParamType.Bool:
                    boolValue = bool.Parse(param[index]);
                    break;
                case ParamType.Int:
                    intValue = int.Parse(param[index]);
                    break;
                case ParamType.Float:
                    floatValue = Single.Parse(param[index]);
                    break;
                default:
                    stringValue = param[index];
                    break;
            }
        }
    }
}


[System.Serializable]
public class UserAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> { 
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_ANIM_NAME, AnimationSequenceParam.ParamType.String)
        };
    }
}


[System.Serializable]
public class GenerateEffectAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_PATH, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_POSITION, AnimationSequenceParam.ParamType.Vector3),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_SCALE, AnimationSequenceParam.ParamType.Vector3),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_MATCH, AnimationSequenceParam.ParamType.Bool),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_CHILD, AnimationSequenceParam.ParamType.Bool),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_VARIANCE, AnimationSequenceParam.ParamType.Vector3)
        };
    }
}


[System.Serializable]
public class TerminateEffectAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_ID, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class TransformAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_VALUE, AnimationSequenceParam.ParamType.Vector3),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int),
        };
    }
}


[System.Serializable]
public class ColorAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_COLOR, AnimationSequenceParam.ParamType.Color),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_COLOR_AMOUNT, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class VibrateAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_STRENGTH, AnimationSequenceParam.ParamType.Vector2),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_VIBRATO, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class FrameSpeedAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_FRAME_SPEED, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class TriggerAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_TRIGGER, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_VALUE, AnimationSequenceParam.ParamType.Bool),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class BeginLoopAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_LOOP, AnimationSequenceParam.ParamType.String)
        };
    }
}


[System.Serializable]
public class DamageAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_PLAY_HIT, AnimationSequenceParam.ParamType.Bool)
        };
    }
}


[System.Serializable]
public class AudioAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_AUDIO_NAME, AnimationSequenceParam.ParamType.String)
        };
    }
}


[System.Serializable]
public class EntityAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String)
        };
    }
}


[System.Serializable]
public class SetOverlayAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_PATH, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_OFFSET, AnimationSequenceParam.ParamType.Vector2),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class OverlayAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_AMOUNT, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_SPEED, AnimationSequenceParam.ParamType.Vector2),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class BGColorAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_COLOR, AnimationSequenceParam.ParamType.ColorNoAlpha),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float)
        };
    }
}


[System.Serializable]
public class BGFadeAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_AMOUNT, AnimationSequenceParam.ParamType.Float),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float)
        };
    }
}


[System.Serializable]
public class ResetBGAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_DURATION, AnimationSequenceParam.ParamType.Float)
        };
    }
}


[System.Serializable]
public class SpriteAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_RELATIVE, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_PATH, AnimationSequenceParam.ParamType.String),
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_EFFECT_INDEX, AnimationSequenceParam.ParamType.Int)
        };
    }
}


[System.Serializable]
public class TargetIndexAnimationSequenceParams : AnimationSequenceParams
{
    protected override void Initialize()
    {
        base.Initialize();
        parameters = new List<AnimationSequenceParam> {
            new AnimationSequenceParam(AnimationConstants.ANIM_PARAM_LOOP, AnimationSequenceParam.ParamType.String)
        };
    }
}