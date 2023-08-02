using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToUI;
using UnityEngine.UI;

public class UITargetBattleMenu : UIMenu
{
    // NOTE: Selection Matrix is dependent on the layout of the enemies on the screen.
    [SerializeField][Header("Target Menu Properties")]
    protected UITargetInfoMenuItem TargetInfoPrefab;
    [SerializeField]
    protected UITargetArrow TargetArrowPrefab;
    [SerializeField]
    protected int PoolSize = 10;
    protected List<UITargetInfoMenuItem> TargetInfoPool = new List<UITargetInfoMenuItem>();
    protected List<UITargetArrow> TargetArrowPool = new List<UITargetArrow>();
    [SerializeField]
    protected float xRange = 50; 
    [SerializeField]
    protected float yRange = 50;

    private Camera _camera;
    private RectTransform rootTransform;


    protected override void Awake()
    {
        _camera = Camera.main;
        if(_camera == null) _camera = FindObjectOfType<Camera>();

        rootTransform = transform.root as RectTransform;

        EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_TARGET_MENU).
            AddListener(InitializeMenu);

        for (int i=0; i<PoolSize; i++)
        {
            var item = Instantiate(TargetInfoPrefab, ScrollArea);
            item.name = $"Info {(i + 1)}";
            item.gameObject.SetActive(false);
            TargetInfoPool.Add(item);

            var arrow = Instantiate(TargetArrowPrefab, ScrollArea).GetComponent<UITargetArrow>();
            arrow.name = $"Arrow {(i + 1)}";
            arrow.image.enabled = false;
            TargetArrowPool.Add(arrow);
        }

        base.Awake();
    }


    private void InitializeMenu(EntityController player)
    {
        if(player.action.GetSpellTarget() == Spell.SpellTarget.RandomEnemy)
        {
            // TODO: Improve this. Random should allow random per hit, but also random all.
            // This logic is unsupported.
            player.target = player.enemies;
            player.ready = true;
            return;
        }

        var targets = player.GetPossibleTargets();
        var icons = new List<UIMenuItem>();

        for(int i=0; i<targets.Count; i++)
        {
            var target = targets[i];
            var icon = TargetInfoPool[i];
            icons.Add(icon);
            icon.gameObject.SetActive(true);

            Vector2 pos = _camera.WorldToViewportPoint(target.transform.position);
            (icon.transform as RectTransform).anchoredPosition = new Vector2(
                (pos.x * rootTransform.sizeDelta.x) - (rootTransform.sizeDelta.x * 0.5f),
                (pos.y * rootTransform.sizeDelta.y) - (rootTransform.sizeDelta.y * 0.5f));

            icon.Initialize(new List<EntityController> { targets[i] }, 
                new List<UITargetArrow> { TargetArrowPool[i] }, player);
        }

        RegenerateSelectionMatrix(icons, xRange, yRange);

        // Disable any objects that do not need to be active anymore.
        for(int i=targets.Count; i<TargetInfoPool.Count; i++)
        {
            TargetArrowPool[i].image.enabled = false;
            TargetInfoPool[i].gameObject.SetActive(false);
        }
    }


    public override void OnCancelPressed()
    {
        base.OnCancelPressed();

        EventManager.Instance.RaiseUIGameEvent(EventConstants.HIDE_ALL_SCREENS,
            new UIOpenCloseCall
            {
                Callback = () =>
                {
                    EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
                        new UIOpenCloseCall
                        {
                            MenuName = ScreenConstants.ActionMenu.ToString()
                        });
                }
            });
    }


    protected virtual void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_TARGET_MENU).
            RemoveListener(InitializeMenu);
    }
}
