using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ToUI;
using Sirenix.OdinInspector;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Handles display of and queueing of all <see cref="DialogueSequence"/>
/// </summary>
public class DialogueManager : UIScreen
{
    protected enum DialoguePrintType { ByCharacter, ByWord }

    public bool isPrinting { get; protected set; }
    public bool isPrintingLine { get; protected set; }

    protected float dialogueSpeed;
    protected float dialoguePause = 0.0f;
    protected float dialogueStart = 0.0f;

    [Header("Dialogue Manager Properties")]
    [SerializeField]
    protected TextMeshProUGUI display;
    [SerializeField]
    protected bool bUseUnderlay;
    [SerializeField][ShowIf("bUseUnderlay")]
    protected TextMeshProUGUI underlay;
    [SerializeField][ShowIf("bUseUnderlay")]
    protected string underlayFormat;
    [SerializeField]
    protected DialoguePrintType printType;
    [SerializeField]
    protected float defaultDialogueSpeed = 0.015f;
    [SerializeField]
    protected float defaultDialogueStartDelay = 0.015f;
    [SerializeField]
    protected float defaultDialogueEndDelay = 0.015f;
    [SerializeField] protected TextMeshProUGUI textScaler;


    IEnumerator result;

    protected override void Awake()
    {
        base.Awake();

        VariableManager.Instance.SetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE, false);

        // Whenever the queue event is invoked, queue the string as a dialogue sequence
        EventManager.Instance.GetStringEvent(EventConstants.ON_DIALOGUE_QUEUE).AddListener(QueueDialogue);
        EventManager.Instance.GetStringEvent(EventConstants.ON_MESSAGE_QUEUE).AddListener(QueueMessage);

        dialogueSpeed = defaultDialogueSpeed;
        dialogueStart = defaultDialogueStartDelay;
        dialoguePause = defaultDialogueEndDelay;

        if (textScaler == null) textScaler = display;
        textScaler.text = "";

        ClearText();
    }


    /// <summary>
    /// Turns the given string into a <see cref="DialogueSequence"/> and queues it
    /// </summary>
    public void QueueDialogue(string dialogue)
    {
        DialogueSequence seq = new DialogueSequence(dialogue, this);
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, seq);
    }


    /// <summary>
    /// Turns the given string into a <see cref="DialogueSequence"/> and queues it
    /// </summary>
    public void QueueMessage(string dialogue)
    {
        DialogueSequence seq = new DialogueSequence(dialogue, this);
        seq.runInactive = true;
        seq.bPlayAuto = true;
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, seq);
    }


    /// <summary>
    /// Removes the currently displayed text.
    /// </summary>
    public virtual void ClearText()
    {
        display.text = "";

        if (bUseUnderlay) underlay.text = "";
    }


    protected string GetTextOutputForLine(TMP_LineInfo line, string target)
    {
        return $"<size=5%><color=#00000000>.</color></size>" +
                $"{target.Substring(line.firstCharacterIndex, line.characterCount)}";
    }


    /// <summary>
    /// Prints each character in the text string after a delay
    /// </summary>
    public virtual IEnumerator PrintText(string target)
    {
        string fixedTarget = "";

        foreach (var line in textScaler.GetTextInfo(target).lineInfo)
        {
            if (line.characterCount == 0) continue;

            fixedTarget += GetTextOutputForLine(line, target);
        }

        textScaler.text = "";

        StartCoroutine(PrintLine(fixedTarget));

        // Waits for a short delay before ending.
        yield return new WaitForSeconds(dialoguePause);

        isPrinting = false;

        dialogueSpeed = defaultDialogueSpeed;
        dialogueStart = defaultDialogueStartDelay;
        dialoguePause = defaultDialogueEndDelay;
    }


    protected virtual IEnumerator PrintLine(string target)
    {
        isPrinting = true;
        isPrintingLine = true;
        display.text = $"{target}";
        display.maxVisibleCharacters = 0;

        if (bUseUnderlay)
        {
            underlay.text = underlayFormat.Replace("text", target);
            underlay.maxVisibleCharacters = 0;
        }

        yield return new WaitForSeconds(dialogueSpeed);

        switch (printType)
        {
            case DialoguePrintType.ByCharacter:

                // Prints a character, then pauses for X seconds.
                for (int i = 0; i < display.textInfo.characterCount; i++)
                {
                    display.maxVisibleCharacters = i + 1;

                    if (bUseUnderlay)
                    {
                        underlay.maxVisibleCharacters = i + 1;
                    }

                    yield return new WaitForSeconds(dialogueSpeed);
                }
                break;

            case DialoguePrintType.ByWord:

                // Prints a word, then pauses for X seconds.
                for (int i = 0; i < display.textInfo.wordCount; i++)
                {
                    if (display.textInfo.wordInfo[i].GetWord() == ".") continue;

                    if (i == display.textInfo.wordCount - 1)
                    {
                        display.maxVisibleCharacters = display.textInfo.characterCount;

                        if(bUseUnderlay)
                            underlay.maxVisibleCharacters = display.textInfo.characterCount;
                    }
                    else
                    {
                        display.maxVisibleCharacters = display.textInfo.wordInfo[i].lastCharacterIndex + 2;

                        if (bUseUnderlay)
                        {
                            underlay.maxVisibleCharacters = display.maxVisibleCharacters;
                        }

                        yield return new WaitForSeconds(dialogueSpeed);
                    }
                }

                break;
        }

        isPrintingLine = false;
    }


    /// <summary>
    /// Begins the text print animation
    /// </summary>
    public IEnumerator BeginTextAnimation(string target)
    {
        result = PrintText(target);
        StartCoroutine(result);

        return result;
    }


    /// <summary>
    /// Ends the current printing animation.
    /// </summary>
    public void EndTextAnimation(IEnumerator anim, string target)
    {
        StopCoroutine(anim);

        display.text = target;
        display.maxVisibleCharacters = target.Length;

        if (bUseUnderlay)
        {
            underlay.text = target;
            underlay.maxVisibleCharacters = target.Length;
        }

        isPrinting = false;
    }


    protected override void OnScreenShown()
    {
        VariableManager.Instance.SetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE, true);
        base.OnScreenShown();
    }


    public override void Hide()
    {
        VariableManager.Instance.SetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE, false);
        base.Hide();
    }


    #region Input Handling

    public override void OnConfirmPressed()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.ON_DIALOGUE_ADVANCE);
        base.OnConfirmPressed();
    }

    public override void OnCancelPressed()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.ON_DIALOGUE_ADVANCE);
        base.OnCancelPressed();
    }

    #endregion


    void OnDestroy()
    {
        EventManager.Instance.GetStringEvent(EventConstants.ON_DIALOGUE_QUEUE).RemoveListener(QueueDialogue);
        EventManager.Instance.GetStringEvent(EventConstants.ON_MESSAGE_QUEUE).RemoveListener(QueueMessage);
    }
}
