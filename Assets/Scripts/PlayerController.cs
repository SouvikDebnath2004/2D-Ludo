using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Token[] tokens;
    private int completedTokens = 0;

    // Used by GameManager to highlight tokens for selection
    public void HighlightMovableTokens(int diceValue)
    {
        foreach (var token in tokens)
        {
            bool canMove = token.CanMove(diceValue);
            token.SetHighlight(canMove);
            token.SetSelectable(canMove);
        }
    }

    public void ClearTokenSelection()
    {
        foreach (var token in tokens)
        {
            token.SetHighlight(false);
            token.SetSelectable(false);
        }
    }

    public bool HasMovableTokens(int diceValue)
    {
        foreach (var token in tokens)
        {
            if (token.CanMove(diceValue)) return true;
        }
        return false;
    }

    public void TokenReachedHome()
    {
        completedTokens++;
    }

    public bool HasWon()
    {
        return completedTokens >= 2;
    }
}
