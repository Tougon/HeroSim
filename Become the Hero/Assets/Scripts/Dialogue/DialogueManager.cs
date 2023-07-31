using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ToUI;
using Sirenix.OdinInspector;

/// <summary>
/// Handles display of and queueing of all <see cref="DialogueSequence"/>
/// </summary>
public class DialogueManager : UIScreen
{
    protected enum DialoguePrintType { ByCharacter, ByWord }

    public bool isPrinting { get; private set; }

    private float dialogueSpeed;
    private float dialoguePause = 0.0f;
    private float dialogueStart = 0.0f;

    [Header("Dialogue Manager Properties")]
    [SerializeField]
    private TextMeshProUGUI display;
    [SerializeField]
    private bool bUseUnderlay;
    [SerializeField][ShowIf("bUseUnderlay")]
    private TextMeshProUGUI underlay;
    [SerializeField][ShowIf("bUseUnderlay")]
    private string underlayFormat;
    [SerializeField]
    private DialoguePrintType printType;
    [SerializeField]
    private float defaultDialogueSpeed = 0.015f;
    [SerializeField]
    private float defaultDialogueStartDelay = 0.015f;
    [SerializeField]
    private float defaultDialogueEndDelay = 0.015f;


    IEnumerator result;

    protected override void Awake()
    {
        base.Awake();

        VariableManager.Instance.SetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE, false);

        // Whenever the queue event is invoked, queue the string as a dialogue sequence
        EventManager.Instance.GetStringEvent(EventConstants.ON_DIALOGUE_QUEUE).AddListener(QueueDialogue);

        dialogueSpeed = defaultDialogueSpeed;
        dialogueStart = defaultDialogueStartDelay;
        dialoguePause = defaultDialogueEndDelay;

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
    /// Removes the currently displayed text.
    /// </summary>
    public void ClearText()
    {
        display.text = "";

        if (bUseUnderlay) underlay.text = "";
    }


    /// <summary>
    /// Prints each character in the text string after a delay
    /// </summary>
    public IEnumerator PrintText(string target)
    {
        string fixedTarget = "";

        foreach (var line in display.GetTextInfo(target).lineInfo)
        {
            if(line.characterCount ==  0) continue;

            fixedTarget += $"<size=5%><color=#00000000>.</color></size>" +
                $"{target.Substring(line.firstCharacterIndex, line.characterCount)}";
        }

        isPrinting = true;
        display.text = $"{fixedTarget}";
        display.maxVisibleCharacters = 0;

        if (bUseUnderlay)
        {
            underlay.text = underlayFormat.Replace("text", fixedTarget);
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

                    display.maxVisibleCharacters = display.textInfo.wordInfo[i].lastCharacterIndex + 2;

                    if (bUseUnderlay)
                    {
                        underlay.maxVisibleCharacters = display.maxVisibleCharacters;
                    }

                    yield return new WaitForSeconds(dialogueSpeed);
                }
                break;
        }

        // Waits for a short delay before ending.
        yield return new WaitForSeconds(dialoguePause);

        isPrinting = false;

        dialogueSpeed = defaultDialogueSpeed;
        dialogueStart = defaultDialogueStartDelay;
        dialoguePause = defaultDialogueEndDelay;
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
    }
}
