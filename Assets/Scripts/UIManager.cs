using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text turnText;

    public void UpdateTurnIndicator(PlayerType currentPlayer)
    {
        turnText.text = currentPlayer == PlayerType.Player1 ? "Player 1 Turn" : "Player 2 Turn";
    }
}