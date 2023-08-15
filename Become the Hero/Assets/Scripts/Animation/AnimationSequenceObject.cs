using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
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
    [FormerlySerializedAs("animationSequence")]
    public TextAsset animationSequenceText;

    [ListDrawerSettings(DraggableItems = false, ShowPaging = false)]
    [OnValueChanged("SortByFrameOrder", true)]
    [PropertyOrder(25)]
    public List<AnimationSequenceFrame> animationSequence = new List<AnimationSequenceFrame>();

    public bool disableUI = true;


    [Button("Parse Text Asset")]
    [PropertySpace(20)][PropertyOrder(25)]
    public void ParseTextAsset()
    {
        // Split the animation script.
        string[] sequence = animationSequenceText.text.Split('\n');

        animationSequence = new List<AnimationSequenceFrame>();

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

        foreach(var  frame in animationSequence)
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

    [HorizontalGroup("Secondary", Gap = 3)]
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
        DefaultExpandedState = true, ShowFoldout = false)]
    public List<AnimationSequenceParam> parameters;

    public AnimationSequenceParams()
    {
        Initialize();
    }


    protected virtual void Initialize()
    {

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
                else if (parameters[paramIndex].paramType == AnimationSequenceParam.ParamType.Vector2) i += 1;

                paramIndex++;
            }
        }
    }
}


[System.Serializable]
public class AnimationSequenceParam
{
    public enum ParamType { Bool, Int, Float, String, Vector2, Vector3 }

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


    public AnimationSequenceParam(string name, ParamType paramType)
    {
        this.name = name;
        this.paramType = paramType;
    }


    public void FromString(string str, int index)
    {
        // Split the param
        string[] param = str.Split(',');

        if (paramType == ParamType.Vector2 || paramType == ParamType.Vector3)
        {
            if(paramType == ParamType.Vector3)
            {
                vector3Value = new Vector3(Single.Parse(param[index]), 
                    Single.Parse(param[index + 1]), Single.Parse(param[index + 2]));
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