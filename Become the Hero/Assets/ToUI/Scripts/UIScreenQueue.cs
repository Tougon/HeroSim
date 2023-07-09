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


        public void AddToQueue(UIScreen screen)
        {
            ModifyQueue(screen, true);
        }


        public void RemoveFromQueue(UIScreen screen)
        {
            ModifyQueue(screen, false);
        }


        private void ModifyQueue(UIScreen screen, bool add)
        {
            if (add)
            {
                if(!_queue.Contains(screen))
                {
                    _queue.Add(screen);
                }
                else
                {
                    _queue.Remove(screen);
                    _queue.Add(screen);
                }
            }
            else
            {
                _queue.Remove(screen);
            }
        }
    }
}
