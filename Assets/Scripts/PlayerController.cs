using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Token[] tokens;
    private int completedTokens = 0;
    private int currentRollValue;

    public void HandleDiceResult(int rollValue)
    {
        currentRollValue = rollValue;
        HighlightMovableTokens(rollValue);
    }

    public void HighlightMovableTokens(int diceValue)
    {
        bool hasMovable = false;

        foreach (var token in tokens)
        {
            bool canMove = token.CanMove(diceValue);
            token.SetHighlight(canMove);

            if (canMove)
            {
                hasMovable = true;
                token.SetSelectable(true, () => OnTokenSelected(token));
            }
            else
            {
                token.SetSelectable(false, null);
            }
        }

        if (!hasMovable)
        {
            // No tokens can move, end turn immediately
            GameManager.Instance.EndTurn();
        }
    }

    private void OnTokenSelected(Token token)
    {
        token.Move(currentRollValue);

        // Disable selection for all tokens
        foreach (var t in tokens)
        {
            t.SetSelectable(false, null);
            t.SetHighlight(false);
        }
    }

    public void TokenReachedHome()
    {
        completedTokens++;
    }

    public bool HasWon()
    {
        return completedTokens >= 2; // Or whatever your win condition is
    }
}
