using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    [Header("Dice Visuals")]
    public Sprite[] diceSides;       // Dice face sprites (0 = 1, 5 = 6)
    public Image diceImage;          // UI image to display dice
    public Button rollButton;

    [Header("Player Info")]
    public PlayerType owner;         // Assign in Inspector (Player1 or Player2)

    private int turnsWithoutSix = 0;
    private bool firstTimeBoostUsed = false;

    private System.Random random = new System.Random();

    public void Roll()
    {
        if (rollButton != null)
            rollButton.interactable = false;

        StartCoroutine(RollAnimation());
    }

    private IEnumerator RollAnimation()
    {
        int resultIndex = 0;

        for (int i = 0; i < 15; i++)
        {
            resultIndex = Random.Range(0, 6);
            diceImage.sprite = diceSides[resultIndex];
            yield return new WaitForSeconds(0.05f);
        }

        int finalRoll = RollWithBoostLogic();
        diceImage.sprite = diceSides[finalRoll - 1];

        GameManager.Instance.OnDiceRolled(finalRoll);
    }

    private int RollWithBoostLogic()
    {
        if (firstTimeBoostUsed)
            return random.Next(1, 7);

        int roll = random.Next(1, 7);

        if (roll == 6)
        {
            turnsWithoutSix = 0;
            firstTimeBoostUsed = true;
            return roll;
        }

        turnsWithoutSix++;

        if (turnsWithoutSix >= 5)
        {
            firstTimeBoostUsed = true;
            int[] weightedNumbers = { 1, 2, 3, 4, 5, 6, 6, 6 };
            roll = weightedNumbers[random.Next(weightedNumbers.Length)];
        }

        return roll;
    }

    public void SetInteractable(bool interactable)
    {
        if (rollButton != null)
            rollButton.interactable = interactable;
    }
}
