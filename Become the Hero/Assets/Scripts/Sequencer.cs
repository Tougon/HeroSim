using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hero.Core;

/// <summary>
/// Plays back a queue of <see cref="Sequence"/>
/// </summary>
public class Sequencer : MonoBehaviour
{
    private Queue<Sequence> sequence = new Queue<Sequence>();

    public bool active { get; private set; }

    private IEnumerator currentSeq;
    private IEnumerator currentStep;


    /// <summary>
    /// Adds a <see cref="Sequence"/> into the queue.
    /// </summary>
    public void AddSequence(Sequence s)
    {
        sequence.Enqueue(s);
    }


    /// <summary>
    /// Plays a <see cref="Sequence"/>.
    /// </summary>
    public void PlaySequence(Sequence s)
    {
        s.SequenceStart();
        currentStep = s.SequenceLoop();
        StartCoroutine(currentStep);
    }


    /// <summary>
    /// Starts the <see cref="Sequence"/> queue
    /// </summary>
    public void StartSequence()
    {
        if (sequence.Count < 1)
            return;

        currentSeq = RunSequence();
        StartCoroutine(currentSeq);
    }


    /// <summary>
    /// Loops while a <see cref="Sequence"/> is active.
    /// When the <see cref="Sequence"/> has finished playback, runs the next one in the queue.
    /// </summary>
    public IEnumerator RunSequence()
    {
        Sequence seq = sequence.Dequeue();
        seq.SequenceStart();
        currentStep = seq.SequenceLoop();
        StartCoroutine(currentStep);
        active = true;

        while (seq.active)
        {
            yield return null;
        }

        PlayNextSequence();
    }


    /// <summary>
    /// Checks if there are any more Sequences to play and if so, runs the next <see cref="Sequence"/>.
    /// </summary>
    public void PlayNextSequence()
    {
        if (sequence.Count > 0)
        {
            currentSeq = RunSequence();
            StartCoroutine(currentSeq);
        }
        else
            active = false;
    }
}
