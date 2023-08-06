using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToUI;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core.Easing;

/// <summary>
/// Class representing a menu item for a target
/// </summary>
public class UITargetInfoMenuItem : UIMenuItem
{
    // List of all entities this menu selection corresponds to
    protected List<EntityController> Targets;
    protected List<UITargetArrow> Arrows;
    protected EntityController Player;

    [Header("Target Info Properties")]
    [SerializeField]
    protected TextMeshProUGUI Text;
    [SerializeField]
    protected Vector2 Offset = new Vector3(0, 50);

    protected List<Sequence> sequences = new List<Sequence>();


    protected override void Awake()
    {
        base.Awake();
    }


    public void Initialize(List<EntityController> targets, List<UITargetArrow> arrows, 
        List<Vector2> positions, EntityController player)
    {
        Targets = targets;
        sequences = new List<Sequence>(targets.Count);
        Arrows = arrows;
        Player = player;

        for(int i=0; i<Arrows.Count; i++)
        {
            if(i < positions.Count)
            {
                (Arrows[i].transform as RectTransform).anchoredPosition =
                    positions[i] + Offset;
            }
            else
            {
                (Arrows[i].transform as RectTransform).anchoredPosition =
                    (transform as RectTransform).anchoredPosition + Offset;
            }
        }
    }


    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);

        for (int i = 0; i < Arrows.Count; i++)
        {
            Arrows[i].image.enabled = selected;

            if (selected)
            {
                Arrows[i].tweenSystem.PlayAnimation("Bounce");
            }
        }

        if (selected)
        {
            // Give exact information about the entity if we're targetting one.
            if(Targets.Count == 1)
            {
                EventManager.Instance.RaiseEntityControllerEvent(
                    EventConstants.INITIALIZE_SELECTED_ENEMY_INFO, Targets[0]);
            }
            else
            {
                EventManager.Instance.RaiseGameEvent(EventConstants.INITIALIZE_ALL_ENEMY_INFO);
            }


            foreach (var target in Targets)
            {
                var spriteRenderer = target.GetSpriteRenderer();

                if(!spriteRenderer) continue;

                spriteRenderer.color = Color.white;
                Sequence seq = DOTween.Sequence();
                seq.Append(spriteRenderer.material.DOFloat(0.6f, "_Amount", 1.2f));
                seq.Append(spriteRenderer.material.DOFloat(0.0f, "_Amount", 1.2f));
                seq.SetLoops(-1);
                seq.Play();
                sequences.Add(seq);
            }
        }
        else
        {
            for(int i = 0; i < Targets.Count; i++)
            {
                Targets[i].GetSpriteRenderer().material.SetFloat("_Amount", 0);

                if (i < sequences.Count && sequences[i] != null) sequences[i].Kill();
            }

            sequences.Clear();
        }
    }


    public override void OnConfirmPressed()
    {
        // Set target and advance
        Player.target = Targets;
        Player.ready = true;

        base.OnConfirmPressed();
    }
}
