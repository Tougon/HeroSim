using ScriptableObjectArchitecture;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class ScrollingDialogueManager : DialogueManager
{
    [Header("Scrolling Dialogue Manager Properties")]
    [SerializeField] private int numTextRows = 2;
    [SerializeField] private RectTransform rowArea;
    [SerializeField] private float delayBetweenLines = 0.2f;
    [SerializeField] private float lineDuration = -1f;
    private int targetLine = 0;

    private List<TextMeshProUGUI> displays;
    private List<TextMeshProUGUI> underlays;

    private List<IEnumerator> lineWipeRoutines;

    protected override void Awake()
    {
        base.Awake();

        if(rowArea == null)
        {
            Debug.LogError("ERROR: No row area assigned!");
            return;
        }

        displays = new List<TextMeshProUGUI>(numTextRows);
        underlays = new List<TextMeshProUGUI>(numTextRows);
        lineWipeRoutines = new List<IEnumerator>(numTextRows);
        displays.Add(display);

        if (bUseUnderlay)
        {
            underlays.Add(underlay);
        }

        for(int i=1; i<numTextRows; i++)
        {
            var textRow = Instantiate(display.transform.parent, rowArea);
            textRow.name = $"Row {i + 1}";

            if (bUseUnderlay)
            {
                displays.Add(textRow.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>());
                underlays.Add(textRow.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>());
            }
            else
            {
                displays.Add(textRow.GetComponentInChildren<TextMeshProUGUI>());
            }
        }

        targetLine = 0;

        ClearAllText();
    }


    // Suppress the base function. Text will be cleared differently for this Manager.
    public override void ClearText() { }


    private void ClearAllText()
    {
        foreach (var textRow in displays) textRow.text = "";
        foreach (var textRow in underlays) textRow.text = "";
    }


    public override IEnumerator PrintText(string target)
    {
        List<string> lines = new List<string>();

        foreach (var line in textScaler.GetTextInfo(target).lineInfo)
        {
            if (line.characterCount == 0) continue;

            lines.Add(GetTextOutputForLine(line, target));
        }

        textScaler.text = "";

        foreach(var line in lines)
        {
            // Change output line
            if(targetLine >= numTextRows)
            {
                display = displays[displays.Count - 1];
                if (bUseUnderlay) underlay = underlays[underlays.Count - 1];

                for (int i = 0; i < numTextRows; i++)
                {
                    // Check if this row is not empty
                    if (!string.IsNullOrEmpty(displays[i].text))
                    {
                        // Delete tracking for the top row
                        if (i == 0 && lineWipeRoutines.Count >= 1)
                        {
                            StopCoroutine(lineWipeRoutines[0]);
                            lineWipeRoutines.RemoveAt(0);
                            targetLine--;
                        }
                    }

                    if (i < numTextRows - 1)
                    {
                        displays[i].text = displays[i + 1].text;
                        displays[i].maxVisibleCharacters = displays[i + 1].maxVisibleCharacters;

                        if (bUseUnderlay)
                        {
                            underlays[i].text = underlays[i + 1].text;
                            underlays[i].maxVisibleCharacters = displays[1].maxVisibleCharacters;
                        }
                    }
                }
            }
            else
            {
                display = displays[targetLine];
                if(bUseUnderlay) underlay = underlays[targetLine];
            }

            // Print the line
            StartCoroutine(PrintLine(line));

            yield return new WaitUntil(() => !isPrintingLine);

            targetLine = Mathf.Clamp(targetLine + 1, 0, numTextRows);

            if (lineDuration >= 0)
            {
                IEnumerator lineWipe = LineWipeRoutine(line);
                lineWipeRoutines.Add(lineWipe);
                StartCoroutine(lineWipe);
            }

            yield return new WaitForSeconds(delayBetweenLines);
        }

        // Waits for a short delay before ending.
        yield return new WaitForSeconds(dialoguePause);

        isPrinting = false;

        dialogueSpeed = defaultDialogueSpeed;
        dialogueStart = defaultDialogueStartDelay;
        dialoguePause = defaultDialogueEndDelay;
    }



    private IEnumerator LineWipeRoutine(string text)
    {
        yield return new WaitForSeconds(lineDuration);

        bool removed = false;

        for(int i=0; i<numTextRows; i++)
        {
            var line = displays[i];
            if(line.text == text)
            {
                line.text = "";

                if (bUseUnderlay)
                {
                    int index = displays.IndexOf(line);
                    underlays[index].text = "";
                    removed = true;
                }
            }

            if (removed)
            {
                if (i < numTextRows - 1)
                {
                    displays[i].text = displays[i + 1].text;
                    displays[i].maxVisibleCharacters = displays[i + 1].maxVisibleCharacters;

                    if (bUseUnderlay)
                    {
                        underlays[i].text = underlays[i + 1].text;
                        underlays[i].maxVisibleCharacters = displays[i].maxVisibleCharacters;
                    }
                }
                else
                {
                    displays[i].text = "";

                    if (bUseUnderlay)
                        underlays[i].text = "";
                }
            }
        }

        targetLine--;
    }
}
