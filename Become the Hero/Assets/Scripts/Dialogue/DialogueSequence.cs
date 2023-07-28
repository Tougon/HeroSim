using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hero.Core;

/// <summary>
/// Represents a <see cref="Sequence"/> of dialogue.
/// </summary>
public class DialogueSequence : Sequence
{
    // The dialogue manager this text will be displayed in.
    private DialogueManager manager;
    private IEnumerator printAnimation;

    private bool running;

    private string text;

    public DialogueSequence(string s, DialogueManager dm)
    {
        manager = dm;
        text = s;
    }


    /// <summary>
    /// Clears displayed text and begins the sequence
    /// </summary>
    public override void SequenceStart()
    {
        active = true;
        manager.ClearText();
    }


    public override IEnumerator SequenceLoop()
    {
        // Waits until the text box is fully displayed
        while (!VariableManager.Instance.GetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE))
        {
            yield return null;
        }

        bool bWaitForInput = VariableManager.Instance.GetBoolVariableValue(VariableConstants.WAIT_FOR_INPUT);
        if (bWaitForInput)
            EventManager.Instance.GetGameEvent(EventConstants.ON_DIALOGUE_ADVANCE).AddListener(OnDialogueAdvance);

        // Begin text print animation
        running = true;
        printAnimation = manager.BeginTextAnimation(text);
        yield return null;

        while (running)
        {
            if(!bWaitForInput && !manager.isPrinting)
            {
                float delay = VariableManager.Instance.GetFloatVariableValue(VariableConstants.TEXT_PRINT_DELAY);

                yield return new WaitForSeconds(delay);
                running = false;
            }

            yield return null;
        }

        SequenceEnd();
    }


    private void OnDialogueAdvance()
    {
        // Stop printing and fully display text if the animation is still going
        if (manager.isPrinting)
            manager.EndTextAnimation(printAnimation, text);
        // Otherwise end sequence
        else
            running = false;
    }


    public override void SequenceEnd()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_DIALOGUE_ADVANCE).RemoveListener(OnDialogueAdvance);
        active = false;
    }
}
