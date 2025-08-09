using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerType { Player1, Player2 }
public enum GameState { WaitingForRoll, TokenSelection, TokenMove }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerType currentPlayer = PlayerType.Player1;
    public GameState gameState = GameState.WaitingForRoll;

    public UIManager uiManager;
    public Dice player1Dice;
    public Dice player2Dice;
    public PlayerController player1;
    public PlayerController player2;

    private int diceLastValue;

    // Boost logic tracking
    private int player1TurnsWithout6 = 0;
    private int player2TurnsWithout6 = 0;
    private bool boostedChanceUsed = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateDiceInteractivity();
        uiManager.UpdateTurnIndicator(currentPlayer);
    }

    // Called by Dice when roll finishes
    public void OnDiceRolled(int value)
    {
        // Boost chance logic: only once per game for either player
        if (!boostedChanceUsed)
        {
            if ((currentPlayer == PlayerType.Player1 && player1TurnsWithout6 >= 5) ||
                (currentPlayer == PlayerType.Player2 && player2TurnsWithout6 >= 5))
            {
                value = RollWithIncreasedChance();
                boostedChanceUsed = true;
            }
        }

        diceLastValue = value;

        // Update dice image to show correct face after boost roll
        if (currentPlayer == PlayerType.Player1)
        {
            player1Dice.SetDiceFace(value);
        }
        else
        {
            player2Dice.SetDiceFace(value);
        }

        gameState = GameState.TokenSelection;

        // Update no-6 counters
        if (value == 6)
        {
            if (currentPlayer == PlayerType.Player1) player1TurnsWithout6 = 0;
            else player2TurnsWithout6 = 0;
        }
        else
        {
            if (currentPlayer == PlayerType.Player1) player1TurnsWithout6++;
            else player2TurnsWithout6++;
        }

        // Highlight tokens for current player to select
        GetCurrentPlayer().HighlightMovableTokens(value);

        // If no tokens movable, immediately end turn
        if (!GetCurrentPlayer().HasMovableTokens(value))
        {
            EndTurn();
        }
    }


    // Called when player clicks a token
    public void OnTokenClicked(Token token)
    {
        if (gameState != GameState.TokenSelection) return;
        if (token.owner != currentPlayer) return;
        if (!token.CanMove(diceLastValue)) return;

        gameState = GameState.TokenMove;

        // Disable dice interaction while token moves
        player1Dice.SetInteractable(false);
        player2Dice.SetInteractable(false);

        // Disable token highlights & selection immediately
        GetCurrentPlayer().ClearTokenSelection();

        // Move token; pass a callback to know when move is done
        token.Move(diceLastValue, OnTokenMoveComplete);
    }

    // Called after token move + capture finish
    private void OnTokenMoveComplete()
    {
        // Check if player won
        if (GetCurrentPlayer().HasWon())
        {
            SceneManager.LoadScene("Win");
            return;
        }

        // If rolled a 6, player rolls again, else turn ends
        if (diceLastValue == 6)
        {
            gameState = GameState.WaitingForRoll;
            UpdateDiceInteractivity();
        }
        else
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        currentPlayer = currentPlayer == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
        gameState = GameState.WaitingForRoll;

        UpdateDiceInteractivity();
        uiManager.UpdateTurnIndicator(currentPlayer);
    }

    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer == PlayerType.Player1 ? player1 : player2;
    }

    private void UpdateDiceInteractivity()
    {
        player1Dice.SetInteractable(currentPlayer == PlayerType.Player1 && gameState == GameState.WaitingForRoll);
        player2Dice.SetInteractable(currentPlayer == PlayerType.Player2 && gameState == GameState.WaitingForRoll);
    }

    // Boost dice roll (50% chance of 6)
    private int RollWithIncreasedChance()
    {
        return Random.value < 0.5f ? 6 : Random.Range(1, 7);
    }
}
