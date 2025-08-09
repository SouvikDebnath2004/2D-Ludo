using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    public Sprite[] diceSides;
    public Image diceImage;
    public Button rollButton;

    public PlayerType owner;

    private bool rolling = false;

    public void Roll()
    {
        if (rolling || rollButton == null || !rollButton.interactable) return;

        rollButton.interactable = false;
        rolling = true;
        StartCoroutine(RollAnimation());
    }

    public void SetDiceFace(int faceValue)
    {
        // faceValue is 1-6, so index = faceValue - 1
        if (diceSides != null && diceSides.Length >= faceValue && diceImage != null)
        {
            diceImage.sprite = diceSides[faceValue - 1];
        }
    }


    private IEnumerator RollAnimation()
    {
        int rollResult = 0;

        for (int i = 0; i < 15; i++)
        {
            rollResult = Random.Range(1, 7);
            diceImage.sprite = diceSides[rollResult - 1];
            yield return new WaitForSeconds(0.05f);
        }

        // Inform GameManager of the roll result, which will handle boosted chance if needed
        GameManager.Instance.OnDiceRolled(rollResult);

        rolling = false;
    }

    public void SetInteractable(bool interactable)
    {
        if (rollButton != null)
            rollButton.interactable = interactable;
    }
}
