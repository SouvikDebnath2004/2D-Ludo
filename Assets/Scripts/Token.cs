using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour
{
    public Transform[] pathPoints;   // Full movement path including base and final home
    public int currentPosition = -1; // -1 means token is in base
    public PlayerType owner;
    private bool isInHome = false;   // True when token reaches final home

    [Header("Highlight Settings")]
    public GameObject highlightObject; // Optional highlight visual
    private bool isHighlighted = false;

    private System.Action onClickCallback;
    private bool isSelectable = false;

    // Base position transform (should be assigned in inspector or dynamically set)
    public Transform basePosition;

    public bool CanMove(int steps)
    {
        if (isInHome) return false;

        // Can only move out of base on a roll of 6
        if (currentPosition == -1 && steps == 6) return true;

        // Normal move if already on path and within bounds
        if (currentPosition != -1 && currentPosition + steps < pathPoints.Length) return true;

        // Exact move to final home position allowed
        if (currentPosition + steps == pathPoints.Length - 1) return true;

        return false;
    }

    public void Move(int steps)
    {
        SetHighlight(false);  // Remove highlight once move starts

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (currentPosition == -1)
        {
            if (steps == 6)
            {
                currentPosition = 0; // Move token out of base to starting path point
                StartCoroutine(MoveToStartAndContinue());
            }
            else
            {
                Debug.LogWarning($"Token owned by {owner} cannot move out of base on roll {steps}");
                GameManager.Instance.EndTurn();
            }
        }
        else
        {
            StartCoroutine(MoveSteps(steps));
        }
    }

    private IEnumerator MoveToStartAndContinue()
    {
        yield return StartCoroutine(MoveTo(pathPoints[currentPosition]));
        GameManager.Instance.CheckWinCondition();
    }

    private IEnumerator MoveSteps(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            currentPosition++;
            yield return StartCoroutine(MoveTo(pathPoints[currentPosition]));
        }

        if (currentPosition == pathPoints.Length - 1)
        {
            isInHome = true;
            GameManager.Instance.GetCurrentPlayer().TokenReachedHome();
        }
        else
        {
            CheckCapture();
        }

        GameManager.Instance.CheckWinCondition();
    }

    private IEnumerator MoveTo(Transform target)
    {
        float elapsed = 0;
        Vector3 start = transform.position;
        Vector3 end = target.position;

        while (elapsed < 0.3f)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    private void CheckCapture()
    {
        Token[] allTokens = FindObjectsOfType<Token>();
        foreach (Token t in allTokens)
        {
            if (t == this) continue;
            if (t.owner == this.owner) continue;
            if (t.isInHome) continue;

            // Skip tokens in base to prevent false capture
            if (t.currentPosition == -1 || this.currentPosition == -1) continue;

            if (t.currentPosition == this.currentPosition)
            {
                t.SendToBase();
            }
        }
    }

    private void OnMouseDown()
    {
        if (isSelectable && onClickCallback != null)
        {
            onClickCallback.Invoke();
            SetSelectable(false, null);
            SetHighlight(false);
        }
    }

    public void SendToBase()
    {
        isInHome = false;
        currentPosition = -1;

        // Move token visually back to base position (use basePosition if assigned)
        if (basePosition != null)
        {
            transform.position = basePosition.position;
        }
        else if (pathPoints.Length > 0)
        {
            // Fallback: If no basePosition set, use the first pathPoint (ensure this is correct)
            transform.position = pathPoints[0].position;
        }
        else
        {
            Debug.LogWarning("No basePosition or pathPoints assigned for token!");
        }
    }

    public void SetHighlight(bool active)
    {
        isHighlighted = active;

        if (highlightObject != null)
        {
            highlightObject.SetActive(active);
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = active ? Color.yellow : Color.white;
        }
    }

    public void SetSelectable(bool selectable, System.Action onClick)
    {
        isSelectable = selectable;
        onClickCallback = onClick;
    }
}
