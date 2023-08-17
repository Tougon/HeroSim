using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Hero.Core;


/// <summary>
/// Represents an action that should be taken on a frame of animation
/// </summary>
public class AnimationSequenceAction
{
    public enum Action
    {
        ChangeUserAnimation, ChangeTargetAnimation, TerminateAnimation, GenerateEffect, TerminateEffect,
        Move, Rotate, Scale, Sprite, Color, Vibrate, ChangeAnimationSpeed, ChangeAnimationState, PlaySound, BeginLoop, EndLoop,
        ApplyDamage, UpdateHPUI, UpdateMPUI, SetOverlayTexture, SetOverlayAnimation, ChangeBGColor, StartBGFade, ResetBGColor,
        SetTargetIndex, BeginOnSuccess, EndOnSuccess
    }

    public int frame;
    public Action action;
    public string param;
}

/// <summary>
/// Represents a <see cref="Sequence"/> made up of an Entity's motion.
/// </summary>
public class AnimationSequence : Hero.Core.Sequence
{
    private bool initialized = false;
    private bool running;
    private bool looping;
    private bool onSuccess;
    private int currentFrame = 0;
    private float currentTime = 0;
    private int loop = 1;
    private float directionX = 1;
    private float directionY = 1;

    private List<AnimationSequenceAction.Action> IgnoreSuccess =  new List<AnimationSequenceAction.Action>() {
        AnimationSequenceAction.Action.BeginOnSuccess,
        AnimationSequenceAction.Action.EndOnSuccess,
        AnimationSequenceAction.Action.BeginLoop,
        AnimationSequenceAction.Action.EndLoop
    };

    public string sequenceName { get; private set; }

    private EntityController user;
    private List<EntityController> allTargets = new List<EntityController>();

    private List<SpellCast> spell;
    private AnimationSequenceObject aso;
    private int targetIndex;

    #region Initial Values
    /// <summary>
    /// Initial position of the user and target
    /// </summary>
    private Vector3 userPosition;
    private Vector3[] targetPosition;
    private Vector3 userRotation;
    private Vector3[] targetRotation;
    private Vector3 userScale;
    private Vector3[] targetScale;
    private Color userColor;
    private Color[] targetColor;
    private float userAmount;
    private float[] targetAmount;
    #endregion

    private SpriteRenderer userSprite;
    private SpriteRenderer[] targetSprite;

    private List<EntityBase> effects = new List<EntityBase>();

    private List<AnimationSequenceLoop> loops = new List<AnimationSequenceLoop>();


    /// <summary>
    /// Creates a sequence cast from the given spell with all loops matching the number of hits
    /// </summary>
    public AnimationSequence(AnimationSequenceObject obj, EntityController u, List<EntityController> t, List<SpellCast> s)
    {
        allTargets = t;
        InitSequence(obj, u);

        spell = s;

        foreach(var sp in spell)
        {
            if(sp.GetNumHits() > loop)
                loop = sp.GetNumHits();
        }
    }


    /// <summary>
    /// Creates a sequence
    /// </summary>
    public AnimationSequence(AnimationSequenceObject obj, EntityController u, EntityController t)
    {
        allTargets.Add(t);
        InitSequence(obj, u);
    }


    /// <summary>
    /// Initializes a sequence
    /// </summary>
    public void InitSequence(AnimationSequenceObject obj, EntityController u)
    {
        aso = obj;
        user = u;

        // Iinitialize position of user and target
        userPosition = user.transform.position;
        userRotation = user.transform.eulerAngles;
        userScale = user.transform.localScale;
        userSprite = user.GetSpriteRenderer();
        userColor = userSprite.color;
        userAmount = user.GetMaterial().GetFloat("_Amount");

        // Calculate direction of motion. This allows all animations to be uniform regardless of positioning.
        directionX = user.transform.localScale.x / Mathf.Abs(user.transform.localScale.x);
        directionY = user.transform.localScale.y / Mathf.Abs(user.transform.localScale.y);

        targetPosition = new Vector3[allTargets.Count];
        targetRotation = new Vector3[allTargets.Count];
        targetScale = new Vector3[allTargets.Count];
        targetSprite = new SpriteRenderer[allTargets.Count];
        targetColor = new Color[allTargets.Count];
        targetAmount = new float[allTargets.Count];

        for(int i=0; i<allTargets.Count; ++i)
        {
            targetPosition[i] = allTargets[i].transform.position;
            targetRotation[i] = allTargets[i].transform.eulerAngles;
            targetScale[i] = allTargets[i].transform.localScale;
            targetSprite[i] = allTargets[i].GetSpriteRenderer();
            targetColor[i] = targetSprite[i].color;
            targetAmount[i] = allTargets[i].GetMaterial().GetFloat("_Amount");
        }

        // Split the animation script.
        /*string[] sequence = obj.animationSequenceText.text.Split('\n');

        for(int i=0; i<sequence.Length; i++)
        {
            // Remove dividing characters
            string[] line = sequence[i].Split('|');

            // If line does not have enough values, show an error
            if(line.Length > 3 || line.Length < 2)
            {
                //Debug.LogError("Invalid format on line " + (i+1) + "!");
                return;
            }

            // Create the action for the given line
            AnimationSequenceAction seq = new AnimationSequenceAction();

            seq.frame = int.Parse(line[0]);
            seq.action = (AnimationSequenceAction.Action)Enum.Parse(typeof(AnimationSequenceAction.Action), line[1]);
            
            // If the line length is greater than 2, add the param
            if (line.Length > 2)
                seq.param = line[2];

            // Add the action to our list
            sequenceActions.Add(seq);
        }*/

        initialized = true;
    }


    /// <summary>
    /// Start the sequence
    /// </summary>
    public override void SequenceStart()
    {
        /*if (!initialized)
            Debug.LogError("Sequence has not been initialized!");*/

        active = true;
        running = true;

        if (aso.disableUI) EventManager.Instance.RaiseGameEvent(EventConstants.HIDE_UI);
    }


    public override IEnumerator SequenceLoop()
    {
        while (running)
        {
            // Increment frame
            currentTime += Time.deltaTime;

            if(currentTime >= (1.0f / 60.0f))
            {
                int numFrames = (int)(currentTime / (1.0f / 60.0f));

                for(int f=0; f < numFrames; f++)
                {
                    currentFrame++;

                    // If the current frame has an action associated, call it.
                    for (int i = 0; i < aso.animationSequence.Count; i++)
                    {
                        if (aso.animationSequence[i].frame != currentFrame)
                            continue;
                        else
                            CallSequenceFunction(aso.animationSequence[i].action, aso.animationSequence[i].param);
                    }
                }

                currentTime -= (numFrames * (1.0f / 60.0f));
            }

            yield return null;
        }

        SequenceEnd();
        active = false;
    }


    public override void SequenceEnd()
    {
        while(effects.Count > 0)
        {
            MonoBehaviour.Destroy(effects[0].gameObject);
            effects.RemoveAt(0);
        }

        user.transform.position = userPosition;
        user.transform.eulerAngles = userRotation;
        user.transform.localScale = userScale;
        userSprite.color = userColor;
        user.GetMaterial().SetFloat("_Amount", userAmount);

        for (int i = 0; i < allTargets.Count; ++i)
        {
            allTargets[i].transform.position = targetPosition[i];
            allTargets[i].transform.eulerAngles = targetRotation[i];
            allTargets[i].transform.localScale = targetScale[i];
            targetSprite[i].color = targetColor[i];
            allTargets[i].GetMaterial().SetFloat("_Amount", targetAmount[i]);
        }
    }



    #region Animation Events

    /// <summary>
    /// Runs specific behaviors based on the given action and its param.
    /// </summary>
    private void CallSequenceFunction(AnimationSequenceAction.Action a, AnimationSequenceParams param)
    {
        if (onSuccess && !IgnoreSuccess.Contains(a) && !spell[targetIndex].HasDoneAnything())
            return;

        switch (a)
        {
            case AnimationSequenceAction.Action.ChangeUserAnimation:
                ChangeUserAnimation(param);
                break;

            case AnimationSequenceAction.Action.ChangeTargetAnimation:
                ChangeTargetAnimation(param);
                break;

            case AnimationSequenceAction.Action.GenerateEffect:

                var effPos = param.GetVector3(AnimationConstants.ANIM_PARAM_POSITION);
                var effScale = param.GetVector3(AnimationConstants.ANIM_PARAM_SCALE);
                var effVar = param.GetVector3(AnimationConstants.ANIM_PARAM_VARIANCE);

                // Generate an effect with the given values
                GenerateEffect(param.GetString(AnimationConstants.ANIM_PARAM_PATH), 
                    param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE), 
                    effPos.x, effPos.y, effPos.z, effScale.x, effScale.y, effScale.z,
                    param.GetBool(AnimationConstants.ANIM_PARAM_MATCH), param.GetBool(AnimationConstants.ANIM_PARAM_CHILD), 
                    effVar.x, effVar.y, effVar.z);
                break;

            case AnimationSequenceAction.Action.TerminateEffect:
                TerminateEffect(param.GetInt(AnimationConstants.ANIM_PARAM_ID));
                break;

            case AnimationSequenceAction.Action.Move:

                Transform tM;
                string sM = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sM.Equals("User"))
                    tM = user.transform;
                else if (sM.Equals("Target"))
                    tM = allTargets[targetIndex].transform;
                else
                    tM = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)].transform;

                float durationM = (param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION)) / 60.0f;

                Vector3 targetPos = param.GetVector3(AnimationConstants.ANIM_PARAM_VALUE);
                float xM = (targetPos.x * directionX) + tM.position.x;
                float yM = (targetPos.y * directionY) + tM.position.y;
                float zM = targetPos.z + tM.position.z;

                // Move the target
                TweenPosition(tM, xM, yM, zM, durationM);
                break;

            case AnimationSequenceAction.Action.Rotate:

                Transform tR;
                string sR = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sR.Equals("User"))
                    tR = user.transform;
                else if (sR.Equals("Target"))
                    tR = allTargets[targetIndex].transform;
                else
                    tR = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)].transform;

                float durationR = (param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION)) / 60.0f;

                Vector3 targetRot = param.GetVector3(AnimationConstants.ANIM_PARAM_VALUE);
                float xR = targetRot.x;
                float yR = targetRot.y;
                float zR = targetRot.z * directionX;

                // Rotate the target
                TweenRotation(tR, xR, yR, zR, durationR);
                break;

            case AnimationSequenceAction.Action.Scale:

                Transform tS;
                string sS = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sS.Equals("User"))
                    tS = user.transform;
                else if (sS.Equals("Target"))
                    tS = allTargets[targetIndex].transform;
                else
                    tS = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)].transform;

                float durationS = (param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION)) / 60.0f;

                // Increase the target scale
                Vector3 targetScale = param.GetVector3(AnimationConstants.ANIM_PARAM_VALUE);

                float xS = !(sS.Equals("User") || sS.Equals("Target")) ? (targetScale.x) :
                    (targetScale.x * directionX) * Mathf.Abs(tS.localScale.x);
                float yS = !(sS.Equals("User") || sS.Equals("Target")) ? (targetScale.y) :
                    (targetScale.y * directionY) * Mathf.Abs(tS.localScale.y);
                float zS = targetScale.z * Mathf.Abs(tS.localScale.z);

                TweenScale(tS, xS, yS, zS, durationS);
                break;

            case AnimationSequenceAction.Action.Color:

                EntityBase eC;
                string sC = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sC.Equals("User"))
                    eC = user;
                else if (sC.Equals("Target"))
                    eC = allTargets[targetIndex];
                else
                    eC = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)];

                float durationC = (param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION)) / 60.0f;

                float aC = param.GetFloat(AnimationConstants.ANIM_PARAM_COLOR_AMOUNT);

                // Change the target's color
                TweenColor(eC, param.GetColor(AnimationConstants.ANIM_PARAM_COLOR), aC, durationC);
                break;

            case AnimationSequenceAction.Action.Vibrate:

                Transform tV;
                string sV = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sV.Equals("User"))
                    tV = user.transform;
                else if (sV.Equals("Target"))
                    tV = allTargets[targetIndex].transform;
                else
                    tV = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)].transform;

                float durationV = (param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION) / 60.0f);

                Vector3 strengthV = new Vector3(param.GetVector2(AnimationConstants.ANIM_PARAM_STRENGTH).x,
                    param.GetVector2(AnimationConstants.ANIM_PARAM_STRENGTH).y, 0.0f);
                int vibratoV = (int)(param.GetFloat(AnimationConstants.ANIM_PARAM_VIBRATO));

                // Vibrae the target
                Vibrate(tV, durationV, strengthV, vibratoV);
                break;

            case AnimationSequenceAction.Action.ChangeAnimationSpeed:

                string sSp = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();
                float sSpeed = param.GetFloat(AnimationConstants.ANIM_PARAM_FRAME_SPEED);

                // Modifies the speed of the target animator

                if (sSp.Equals("User"))
                    user.FrameSpeedModify(sSpeed);
                else if (sSp.Equals("Target"))
                    allTargets[targetIndex].FrameSpeedModify(sSpeed);
                else
                    effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)].FrameSpeedModify(sSpeed);
                break;

            case AnimationSequenceAction.Action.ChangeAnimationState:

                EntityBase eAS;
                string sAS = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sAS.Equals("User"))
                    eAS = user;
                else if (sAS.Equals("Target"))
                    eAS = allTargets[targetIndex];
                else
                    eAS = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)];

                eAS.SetAnimationState(param.GetString(AnimationConstants.ANIM_PARAM_TRIGGER),
                    param.GetBool(AnimationConstants.ANIM_PARAM_VALUE));
                break;

            case AnimationSequenceAction.Action.BeginLoop:
                // Begins a loop
                int numLoops = allTargets.Count;
                string p = param.GetString(AnimationConstants.ANIM_PARAM_LOOP).Trim();

                if (!p.Equals("#"))
                {
                    numLoops = int.Parse(p);

                    if (numLoops < 0)
                        numLoops = loop;
                }

                loops.Add(new AnimationSequenceLoop(currentFrame, numLoops));
                looping = true;
                break;

            case AnimationSequenceAction.Action.EndLoop:
                // Checks if the loop should repeat or terminate
                AnimationSequenceLoop currLoop = loops[loops.Count - 1];
                currLoop.numIterations++;

                if (currLoop.numIterations < currLoop.numLoops)
                    currentFrame = currLoop.startFrame;
                else
                    loops.Remove(currLoop);

                looping = loops.Count <= 0;
                break;

            case AnimationSequenceAction.Action.ApplyDamage:
                // Applies damage
                allTargets[targetIndex].ApplyDamage
                    (spell[targetIndex].GetDamageOfCurrentHit(), 
                    spell[targetIndex].GetIsCurrentHitCritical(), param.GetBool(AnimationConstants.ANIM_PARAM_PLAY_HIT), 
                    spell[targetIndex].success && spell[targetIndex].GetCurrentHitSuccess());
                spell[targetIndex].IncrementHit();
                break;

            case AnimationSequenceAction.Action.PlaySound:
                // Plays a sound effect
                PlaySound(param);
                break;

            case AnimationSequenceAction.Action.UpdateHPUI:

                if (spell != null && spell[targetIndex] != null &&
                    spell[targetIndex].GetDamageOfPreviousHit() == 0)
                    break;

                string hpt = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (hpt.Equals("User"))
                    user.UpdateHPUI();
                else if (hpt.Equals("Target"))
                    allTargets[targetIndex].UpdateHPUI();
                break;

            case AnimationSequenceAction.Action.SetOverlayTexture:

                EntityBase eOT;
                string sOT = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sOT.Equals("User"))
                    eOT = user;
                else if (sOT.Equals("Target"))
                    eOT = allTargets[targetIndex];
                else
                    eOT = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)];

                string pOT = param.GetString(AnimationConstants.ANIM_PARAM_PATH).Trim();
                var texture = Resources.Load<Texture2D>(pOT);

                eOT.SetOverlayTexture(texture,
                    param.GetVector2(AnimationConstants.ANIM_PARAM_OFFSET));
                break;

            case AnimationSequenceAction.Action.SetOverlayAnimation:

                EntityBase eOA;
                string sOA = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();

                if (sOA.Equals("User"))
                    eOA = user;
                else if (sOA.Equals("Target"))
                    eOA = allTargets[targetIndex];
                else
                    eOA = effects[param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX)];

                eOA.SetOverlayTween(param.GetFloat(AnimationConstants.ANIM_PARAM_AMOUNT),
                    param.GetVector2(AnimationConstants.ANIM_PARAM_SPEED),
                    param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION) / 60.0f);
                break;

            case AnimationSequenceAction.Action.ChangeBGColor:

                VariableManager.Instance.SetFloatVariableValue(
                    VariableConstants.BACKGROUND_FADE_TIME, param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION));

                Color c = param.GetColor(AnimationConstants.ANIM_PARAM_COLOR);
                EventManager.Instance.RaiseVector3Event(EventConstants.SET_BACKGROUND_COLOR,
                    new Vector3(c.r, c.g, c.b));
                break;

            case AnimationSequenceAction.Action.StartBGFade:

                EventManager.Instance.RaiseVector2Event(EventConstants.START_BACKGROUND_FADE,
                    new Vector2(param.GetFloat(AnimationConstants.ANIM_PARAM_AMOUNT),
                    param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION) / 60.0f));
                break;

            case AnimationSequenceAction.Action.ResetBGColor:

                VariableManager.Instance.SetFloatVariableValue(VariableConstants.BACKGROUND_FADE_TIME, 
                    param.GetFloat(AnimationConstants.ANIM_PARAM_DURATION));

                EventManager.Instance.RaiseGameEvent(EventConstants.RESET_BACKGROUND_COLOR);
                break;

            case AnimationSequenceAction.Action.TerminateAnimation:
                // End the animation
                running = false;
                break;

            case AnimationSequenceAction.Action.Sprite:

                string target = param.GetString(AnimationConstants.ANIM_PARAM_RELATIVE).Trim();
                bool spriteUser = target.ToLower().Equals("user");

                if (param.GetString(AnimationConstants.ANIM_PARAM_PATH).ToLower().Equals("entity"))
                {
                    int index = param.GetInt(AnimationConstants.ANIM_PARAM_EFFECT_INDEX);

                    if (spriteUser)
                        user.SetSprite(user.GetEntity().vals.additionalEntitySprites[index]);
                    else
                        allTargets[targetIndex].SetSprite(allTargets[targetIndex].GetEntity().vals.additionalEntitySprites[index]);
                }
                else
                {
                    // TODO: Load a custom sprite
                }
                break;

            case AnimationSequenceAction.Action.SetTargetIndex:

                string tgi = param.GetString(AnimationConstants.ANIM_PARAM_LOOP).Trim();
                if (tgi.Equals("#"))
                    targetIndex = loops[loops.Count - 1].numIterations;
                else
                    int.TryParse(tgi, out targetIndex);
                break;

            case AnimationSequenceAction.Action.BeginOnSuccess:
                onSuccess = true;
                break;

            case AnimationSequenceAction.Action.EndOnSuccess:
                onSuccess = false;
                break;
        }
    }

    /// <summary>
    /// Change animator animation
    /// </summary>
    private void ChangeUserAnimation(AnimationSequenceParams t) 
    { user.SetAnimation(t.GetString(AnimationConstants.ANIM_PARAM_ANIM_NAME)); }
    private void ChangeTargetAnimation(AnimationSequenceParams t) 
    { allTargets[targetIndex].SetAnimation(t.GetString(AnimationConstants.ANIM_PARAM_ANIM_NAME)); }
    

    /// <summary>
    /// Create an effect
    /// </summary>
    private void GenerateEffect(string path, string relative, float x, float y, float z, float scaleX, float scaleY, float scaleZ, bool match,
        bool child, float varX, float varY, float varZ)
    {
        path = path.Trim();
        relative = relative.Trim();
        Transform par = null;

        // Loads the effect
        EntityBase effect = GameObject.Instantiate(Resources.Load(path, typeof(EntityBase))) as EntityBase;

        // Determines where the effect should spawn
        if (relative == "User")
        {
            Vector3 rel = user.transform.position;

            x = (rel.x) + (x * directionX);
            y = (rel.y) + (y * directionY);
            z += rel.z;

            if(child)
                par = user.transform;

            effect.Init(user);
        }
        else if(relative == "Target")
        {
            Vector3 rel = allTargets[targetIndex].transform.position;

            x = (rel.x) + (x * directionX);
            y = (rel.y) + (y * directionY);
            z += rel.z;

            if(child)
                par = allTargets[targetIndex].transform;

            effect.Init(allTargets[targetIndex]);
        }

        if (par != null)
            effect.transform.SetParent(par);

        varX /= 2;
        varY /= 2;
        varZ /= 2;

        // Sets positioning
        Vector3 offset = new Vector3(UnityEngine.Random.Range(-varX, varX), UnityEngine.Random.Range(-varY, varY), 
            UnityEngine.Random.Range(-varZ, varZ));

        effect.transform.position = new Vector3(x, y, z) + offset;
        effect.transform.localScale = match ? new Vector3(scaleX * directionX, scaleY * directionY, scaleZ) : 
            new Vector3(scaleX, scaleY, scaleZ);

        effects.Add(effect);
    }


    /// <summary>
    /// Disabled the given effect
    /// </summary>
    private void TerminateEffect(int id)
    {
        id = Mathf.Clamp(id, 0, effects.Count - 1);
        effects[id].gameObject.SetActive(false);
        effects.RemoveAt(id);
    }


    /// <summary>
    /// Tweens position to the target over the given duration
    /// </summary>
    private void TweenPosition(Transform t, float x, float y, float z, float duration)
    {
        t.DOComplete();
        t.DOMove(new Vector3(x, y, z), duration);
    }


    /// <summary>
    /// Tweens rotation to the target over the given duration
    /// </summary>
    private void TweenRotation(Transform t, float x, float y, float z, float duration)
    {
        t.DOComplete();
        t.DORotate(new Vector3(x, y, z), duration, RotateMode.Fast);
    }


    /// <summary>
    /// Tweens scale to the target over the given duration
    /// </summary>
    private void TweenScale(Transform t, float x, float y, float z, float duration)
    {
        //t.DOScale(new Vector3(x, y, z), duration);
        t.DOComplete();
        t.DOScaleX(x, duration);
        t.DOScaleY(y, duration);
    }


    /// <summary>
    /// Tweens color to the target over the given duration
    /// </summary>
    private void TweenColor(EntityBase e, Color c, float amt, float duration)
    {
        e.SetColorTween(c, amt, duration);
    }


    /// <summary>
    /// Vibrates the target over the given duration
    /// </summary>
    private void Vibrate(Transform t, float duration, Vector3 strength, int vibrato)
    {
        t.DOComplete();
        t.transform.DOShakePosition(duration, strength, vibrato);
    }

    private void PlaySound(AnimationSequenceParams s)
    {
        //SoundManager.Instance.PlaySound(s.Trim());
    }

    #endregion
}


/// <summary>
/// Data for an animation loop
/// </summary>
public class AnimationSequenceLoop
{
    public int startFrame;
    public int numLoops;
    public int numIterations;

    public AnimationSequenceLoop(int start, int loops)
    {
        startFrame = start;
        numLoops = loops;
        numIterations = 0;
    }
}
