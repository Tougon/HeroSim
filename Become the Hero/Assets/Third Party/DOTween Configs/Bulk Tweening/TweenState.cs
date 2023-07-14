using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static DOTweenConfigs.TweenStateInstance;

namespace DOTweenConfigs
{
    /// <summary>
    /// Represents a animation consisting of a series of Tween Frames
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(
        fileName = "NewTweenState",
        menuName = "Tween State",
        order = EditorConstants.CreateAssetMenuOrder)]
    public class TweenState : ScriptableObject
    {
        public bool Loop;
        public List<TweenFrame> Frames;

        public TweenStateInstance ExecuteState(TweenActionCache target, OnTweenStateCompletedSignature callback)
        {
            return new TweenStateInstance(this, target, callback);
        }
    }


    /// <summary>
    /// Represents an instance of a tween state and handles runtime playback
    /// </summary>
    public class TweenStateInstance
    {
        public delegate void OnTweenStateCompletedSignature();
        public OnTweenStateCompletedSignature OnTweenStateComplete;

        private TweenState state;
        private TweenActionCache target;
        private int currentFrame = -1;
        private int currentNumActions = 0;
        private bool bFinished;

        public TweenStateInstance(TweenState state, TweenActionCache target, 
            OnTweenStateCompletedSignature onComplete)
        {
            this.state = state;
            this.target = target;
            OnTweenStateComplete += onComplete;

            OnTweenFrameEnd();
        }


        public void StopInstance(bool complete = false)
        {
            if (bFinished) return;

            target.KillAllTweens();

            if (complete)
            {
                OnStateEnd();
            }
        }


        /// <summary>
        /// Function called when tween state completes
        /// </summary>
        private void OnStateEnd()
        {
            OnTweenStateComplete?.Invoke();
        }


        /// <summary>
        /// Called whenever a tween action has completed
        /// </summary>
        public void OnTweenActionEnd()
        {
            currentNumActions++;

            if(currentNumActions >= state.Frames[currentFrame].FrameActions.Count)
            {
                OnTweenFrameEnd();
            }
        }


        /// <summary>
        /// Called whenever a tween frame has completed all of its actions
        /// </summary>
        private void OnTweenFrameEnd()
        {
            currentFrame++;

            // If there are no more actions to execute, end the state
            if(currentFrame >= state.Frames.Count)
            {
                if (state.Loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    bFinished = true;
                    OnStateEnd();
                    return;
                }
            }
            // Otherwise, advance to the next action and play it back

            currentNumActions = 0;

            if (!state.Frames[currentFrame].Execute(target, this))
            {
                OnTweenActionEnd();
            }
        }
    }
}
