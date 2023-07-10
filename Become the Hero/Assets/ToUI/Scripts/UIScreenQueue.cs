using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToUI
{
    public class UIScreenQueue
    {
        public static UIScreenQueue Instance;
        static UIScreenQueue()
        {
            Instance = new UIScreenQueue();
        }

        /// <summary>
        /// Returns the current focused screen
        /// </summary>
        public UIScreen CurrentScreen
        {
            get
            {
                if (_queue.Count == 0)
                    return null;
                else
                    return _queue[_queue.Count - 1];
            }
        }

        private List<UIScreen> _queue = new List<UIScreen>();


        /// <summary>
        /// Adds the given screen to the screen queue
        /// </summary>
        /// <param name="screen"></param>
        public void AddToQueue(UIScreen screen)
        {
            ModifyQueue(screen, true);
        }


        /// <summary>
        /// Removes the given screen from the screen queue
        /// </summary>
        public void RemoveFromQueue(UIScreen screen)
        {
            ModifyQueue(screen, false);
        }


        private void ModifyQueue(UIScreen screen, bool add)
        {
            if (add)
            {
                // Don't do anything if its focused, it's a pointless operation.
                if (screen == CurrentScreen)
                    return;

                // Unfocus the current screen
                CurrentScreen?.FocusChanged(false);

                // Add to queue or bring the target screen to the front
                if(!_queue.Contains(screen))
                {
                    _queue.Add(screen);
                }
                else
                {
                    _queue.Remove(screen);
                    _queue.Add(screen);
                }

                // Focus the new current screen
                CurrentScreen?.FocusChanged(true);
            }
            else
            {
                // Emergency check, should never occur.
                if (!_queue.Contains(screen))
                    return;

                _queue.Remove(screen);

                // Focus the new current screen
                CurrentScreen?.FocusChanged(true);
            }
        }
    }
}
