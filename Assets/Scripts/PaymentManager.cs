using UnityEngine;
using UnityEngine.SceneManagement;

public class PaymentManager : MonoBehaviour
{
    public GameObject paymentUI;
    public GameObject successNote;

    public void StartPayment()
    {
        paymentUI.SetActive(true);
    }

    public void ConfirmPayment()
    {
        paymentUI.SetActive(false);
        successNote.SetActive(true);
        // store paid mode if needed
    }

    public void ProceedToGame()
    {
        SceneManager.LoadScene("Game");
    }
}
