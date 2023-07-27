using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToUI;
using TMPro;
using UnityEngine.UI;

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


    protected override void Awake()
    {
        base.Awake();
    }


    public void Initialize(List<EntityController> targets, List<UITargetArrow> arrows, EntityController player)
    {
        Targets = targets;
        Arrows = arrows;
        Player = player;

        for(int i=0; i<Arrows.Count; i++)
        {
            (Arrows[i].transform as RectTransform).anchoredPosition =
                (transform as RectTransform).anchoredPosition + Offset;
        }
    }


    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);

        for (int i = 0; i < Arrows.Count; i++)
        {
            Arrows[i].image.enabled = selected;
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
