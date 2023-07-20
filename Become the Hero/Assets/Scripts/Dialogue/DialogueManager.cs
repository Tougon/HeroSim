using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ToUI;

/// <summary>
/// Handles display of and queueing of all <see cref="DialogueSequence"/>
/// </summary>
public class DialogueManager : UIScreen
{
    public bool isPrinting { get; private set; }

    private float dialogueSpeed = 0.015f;
    private float dialoguePause = 0.0f;

    [Header("Dialogue Manager Properties")]
    [SerializeField]
    private TextMeshProUGUI display;

    IEnumerator result;

    protected override void Awake()
    {
        base.Awake();

        // Whenever the queue event is invoked, queue the string as a dialogue sequence
        EventManager.Instance.GetStringEvent(EventConstants.ON_DIALOGUE_QUEUE).AddListener(QueueDialogue);
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
    }


    /// <summary>
    /// Prints each character in the text string after a delay
    /// </summary>
    public IEnumerator PrintText(string target)
    {
        isPrinting = true;
        display.text = target;
        display.maxVisibleCharacters = 0;

        yield return new WaitForSeconds(dialogueSpeed);

        // Prints a character, then pauses for X seconds.
        for (int i = 0; i < target.Length; i++)
        {
            display.maxVisibleCharacters = i+1;

            yield return new WaitForSeconds(dialogueSpeed);
        }

        // Waits for a short delay before ending.
        yield return new WaitForSeconds(dialoguePause);

        isPrinting = false;
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
        isPrinting = false;
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
