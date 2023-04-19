using UnityEngine;
using UnityEngine.UI;

public class TypingController : MonoBehaviour
{
    [SerializeField] private Text displayedTextComponent;
    [SerializeField] private Text blinkingCursorTextComponent;

    private float timeUntilBlink = 0.5f;

    private void Update()
    {
        if (!InterfaceController.gameIsPaused)
        {
            timeUntilBlink -= Time.deltaTime;

            if (timeUntilBlink <= 0)
            {
                timeUntilBlink = 0.5f;
                TypingCursorBlink();
            }

            if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Backspace) && !Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Escape))
            {
                displayedTextComponent.text += Input.inputString;

                MoveTypingCursor();

                HighlightText();
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && displayedTextComponent.text.Length > 0)
            {
                Backspace();

                MoveTypingCursor();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                PerformTypedAction(RemoveRichText());

                MoveTypingCursor();
            }
        }
    }

    private void MoveTypingCursor()
    {
        blinkingCursorTextComponent.text = " ";

        RemoveRichText();

        for (int i = 0; i < displayedTextComponent.text.Length; i++)
        {
            blinkingCursorTextComponent.text += " ";
        }

        blinkingCursorTextComponent.text += "_";
    }

    private void TypingCursorBlink()
    {
        if (blinkingCursorTextComponent.color == new Color32(255, 255, 255, 255))
        {
            blinkingCursorTextComponent.color = new Color32(255, 255, 255, 0);
        }
        else
        {
            blinkingCursorTextComponent.color = new Color32(255, 255, 255, 255);
        }
    }

    private void HighlightText()
    {
        if (displayedTextComponent.text.Contains("Action.") && !displayedTextComponent.text.Contains("<color=green>Action</color>."))
        {
            string tempString = displayedTextComponent.text;

            tempString = ReplaceFirstOccurence(tempString, "Action.", "<color=green>Action</color>.");
            displayedTextComponent.text = tempString;
        }
        if (displayedTextComponent.text.Contains("Invoke()") && !displayedTextComponent.text.Contains("<color=yellow>Invoke</color>()"))
        {
            string tempString = displayedTextComponent.text;

            tempString = ReplaceFirstOccurence(tempString, "Invoke()", "<color=yellow>Invoke</color>()");
            displayedTextComponent.text = tempString;
        }
        else if (!displayedTextComponent.text.Contains("Action") && !displayedTextComponent.text.Contains("Invoke"))
        {
            if (displayedTextComponent.text.Contains("Left()") && !displayedTextComponent.text.Contains("<color=yellow>Left</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Left()", "<color=yellow>Left</color>()");
                displayedTextComponent.text = tempString;
            }
            if (displayedTextComponent.text.Contains("Right()") && !displayedTextComponent.text.Contains("<color=yellow>Right</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Right()", "<color=yellow>Right</color>()");
                displayedTextComponent.text = tempString;
            }
            if (displayedTextComponent.text.Contains("Jump()") && !displayedTextComponent.text.Contains("<color=yellow>Jump</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Jump()", "<color=yellow>Jump</color>()");
                displayedTextComponent.text = tempString;
            }
            if (displayedTextComponent.text.Contains("Shoot()") && !displayedTextComponent.text.Contains("<color=yellow>Shoot</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Shoot()", "<color=yellow>Shoot</color>()");
                displayedTextComponent.text = tempString;
            }
            if (displayedTextComponent.text.Contains("Grenade()") && !displayedTextComponent.text.Contains("<color=yellow>Grenade</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Grenade()", "<color=yellow>Grenade</color>()");
                displayedTextComponent.text = tempString;
            }
            if (displayedTextComponent.text.Contains("Reload()") && !displayedTextComponent.text.Contains("<color=yellow>Reload</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Reload()", "<color=yellow>Reload</color>()");
                displayedTextComponent.text = tempString;
            }
            if (displayedTextComponent.text.Contains("Pickup()") && !displayedTextComponent.text.Contains("<color=yellow>Pickup</color>()"))
            {
                string tempString = displayedTextComponent.text;

                tempString = ReplaceFirstOccurence(tempString, "Pickup()", "<color=yellow>Pickup</color>()");
                displayedTextComponent.text = tempString;
            }
        }
    }

    private void Backspace()
    {
        string tempString = displayedTextComponent.text.Remove(displayedTextComponent.text.Length - 1);
        displayedTextComponent.text = tempString;

        if (displayedTextComponent.text.Contains("<color=green>"))
        {
            tempString = displayedTextComponent.text.Remove(displayedTextComponent.text.LastIndexOf("<color=green>"), "<color=green>".Length);
            displayedTextComponent.text = tempString;
            tempString = displayedTextComponent.text.Remove(displayedTextComponent.text.LastIndexOf("</color>"), "</color>".Length);
            displayedTextComponent.text = tempString;
        }
        if (displayedTextComponent.text.Contains("<color=yellow>"))
        {
            tempString = displayedTextComponent.text.Replace("<color=yellow>", "");
            displayedTextComponent.text = tempString;
            tempString = displayedTextComponent.text.Replace("</color>", "");
            displayedTextComponent.text = tempString;
        }
    }

    private string ReplaceFirstOccurence(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    private string RemoveRichText()
    {
        string tempString = displayedTextComponent.text;

        if (displayedTextComponent.text.Contains("<color=green>"))
        {
            tempString = displayedTextComponent.text.Remove(displayedTextComponent.text.LastIndexOf("<color=green>"), "<color=green>".Length);
            displayedTextComponent.text = tempString;
            tempString = displayedTextComponent.text.Remove(displayedTextComponent.text.LastIndexOf("</color>"), "</color>".Length);
            displayedTextComponent.text = tempString;
        }
        if (displayedTextComponent.text.Contains("<color=yellow>"))
        {
            tempString = displayedTextComponent.text.Replace("<color=yellow>", "");
            displayedTextComponent.text = tempString;
            tempString = displayedTextComponent.text.Replace("</color>", "");
            displayedTextComponent.text = tempString;
        }

        return tempString;
    }

    void PerformTypedAction(string line)
    {
        if (line == "Left();" || line == "Action.Left();" || line == "Action.Left?.Invoke();")
        {
            ActionInventory.Left?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else if (line == "Right();" || line == "Action.Right();" || line == "Action.Right?.Invoke();")
        {
            ActionInventory.Right?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else if (line == "Jump();" || line == "Action.Jump();" || line == "Action.Jump?.Invoke();")
        {
            ActionInventory.Jump?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else if (line == "Shoot();" || line == "Action.Shoot();" || line == "Action.Shoot?.Invoke();")
        {
            ActionInventory.Shoot?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else if (line == "Grenade();" || line == "Action.Grenade();" || line == "Action.Grenade?.Invoke();")
        {
            ActionInventory.Grenade?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else if (line == "Reload();" || line == "Action.Reload();" || line == "Action.Reload?.Invoke();")
        {
            ActionInventory.Reload?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else if (line == "Pickup();" || line == "Action.Pickup();" || line == "Action.Pickup?.Invoke();")
        {
            ActionInventory.Pickup?.Invoke(line);
            displayedTextComponent.text = "";
        }
        else
        {
            displayedTextComponent.text = "";
            Player.DisplayInvalidActionCall?.Invoke();
        }
    }
}
