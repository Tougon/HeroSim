using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEffectDisplayController : MonoBehaviour
{
    [SerializeField]
    private GameObject effectDisplayPrefab;

    [SerializeField][Range(0, 30)]
    private int maxNumEffectDisplays = 20;

    public int numEffectsDisplayed { get; private set; }

    [SerializeField]
    private Transform displayRoot;

    private UIEffectDisplay[] effectDisplay;

    
    void Awake()
    {
        effectDisplay = new UIEffectDisplay[maxNumEffectDisplays];

        for(int i=0; i<maxNumEffectDisplays; i++)
        {
            GameObject g = Instantiate(effectDisplayPrefab);
            g.name = "Effect Display";
            g.transform.SetParent(displayRoot, false);
            g.transform.localScale = Vector3.one;
            g.SetActive(false);

            effectDisplay[i] = g.GetComponent<UIEffectDisplay>();
        }
    }


    public void UpdateEffectDisplay(List<EffectInstance> effects)
    {
        // Remove all effects that should not be displayed.
        effects.RemoveAll(f => f.effect.display == null);
        numEffectsDisplayed = effects.Count;

        for(int i=0; i<maxNumEffectDisplays; i++)
        {
            if (i < numEffectsDisplayed)
            {
                effectDisplay[i].gameObject.SetActive(true);
                effectDisplay[i].Init(effects[i]);
            }
            else
            {
                effectDisplay[i].gameObject.SetActive(false);
            }
        }
    }
}
