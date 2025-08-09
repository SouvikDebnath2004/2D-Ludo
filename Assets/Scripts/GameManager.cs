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
    private Token selectedToken;

    private int player1TurnsWithout6 = 0;
    private int player2TurnsWithout6 = 0;
    private bool boostedChanceUsed = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateDiceInteractivity();
        uiManager.UpdateTurnIndicator(currentPlayer);
    }

    public void OnDiceRolled(int value)
    {
        if (!boostedChanceUsed)
        {
            if (currentPlayer == PlayerType.Player1 && player1TurnsWithout6 >= 5)
            {
                value = RollWithIncreasedChance();
                boostedChanceUsed = true;
            }
            else if (currentPlayer == PlayerType.Player2 && player2TurnsWithout6 >= 5)
            {
                value = RollWithIncreasedChance();
                boostedChanceUsed = true;
            }
        }

        diceLastValue = value;
        gameState = GameState.TokenSelection;

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

        GetCurrentPlayer().HandleDiceResult(value);
    }

    public void OnTokenClicked(Token token)
    {
        if (gameState != GameState.TokenSelection) return;

        if (token.owner == currentPlayer && token.CanMove(diceLastValue))
        {
            selectedToken = token;
            gameState = GameState.TokenMove;
            token.Move(diceLastValue);
        }
    }

    public void EndTurn()
    {
        currentPlayer = currentPlayer == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
        UpdateDiceInteractivity();
        uiManager.UpdateTurnIndicator(currentPlayer);
        gameState = GameState.WaitingForRoll;
    }

    public void CheckWinCondition()
    {
        if (GetCurrentPlayer().HasWon())
        {
            SceneManager.LoadScene("Win");
        }
        else
        {
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
    }

    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer == PlayerType.Player1 ? player1 : player2;
    }

    private void UpdateDiceInteractivity()
    {
        player1Dice.SetInteractable(currentPlayer == PlayerType.Player1);
        player2Dice.SetInteractable(currentPlayer == PlayerType.Player2);
    }

    private int RollWithIncreasedChance()
    {
        // 50% chance to get a 6
        return Random.value < 0.5f ? 6 : Random.Range(1, 7);
    }
}
