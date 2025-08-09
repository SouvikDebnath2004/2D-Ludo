using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour
{
    [Header("Movement & Positions")]
    public Transform[] pathPoints;          // The path points for this token
    public Transform basePosition;          // The token's base position in the scene

    [Header("Ownership")]
    public PlayerType owner;

    [Header("Highlight")]
    public GameObject highlightObject;      // Optional highlight object

    public int currentPosition = -1;        // -1 means in base
    private bool isInHome = false;

    private bool isHighlighted = false;
    private bool isSelectable = false;

    private System.Action onMoveComplete;

    public bool CanMove(int steps)
    {
        if (isInHome) return false;

        if (currentPosition == -1 && steps == 6) return true;

        if (currentPosition != -1 && currentPosition + steps < pathPoints.Length) return true;

        if (currentPosition + steps == pathPoints.Length - 1) return true;

        return false;
    }

    public void Move(int steps, System.Action moveCompleteCallback)
    {
        SetHighlight(false);
        SetSelectable(false);

        onMoveComplete = moveCompleteCallback;

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (currentPosition == -1)
        {
            if (steps == 6)
            {
                currentPosition = 0;
                StartCoroutine(MoveToStartAndContinue());
            }
            else
            {
                Debug.LogWarning($"Token cannot move out of base on roll {steps}");
                onMoveComplete?.Invoke();
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
        onMoveComplete?.Invoke();
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
            yield return StartCoroutine(CheckCaptureCoroutine());
        }

        onMoveComplete?.Invoke();
    }

    private IEnumerator CheckCaptureCoroutine()
    {
        Token[] allTokens = FindObjectsOfType<Token>();
        float captureDistanceThreshold = 0.5f; // adjust based on your scale, smaller than token size

        foreach (Token t in allTokens)
        {
            if (t == this) continue;                  // skip self
            if (t.owner == this.owner) continue;      // skip same player tokens
            if (t.isInHome) continue;                  // skip tokens already home

            // Only consider tokens currently on the board (not in base)
            if (t.currentPosition == -1 || this.currentPosition == -1) continue;

            // Check actual world distance instead of index
            float dist = Vector3.Distance(t.transform.position, this.transform.position);

            if (dist <= captureDistanceThreshold)
            {
                // Capture happens: send other token to base
                t.SendToBase();

                // Optional: small delay to show the capture visually
                yield return new WaitForSeconds(0.3f);
            }
        }
    }


    private IEnumerator MoveTo(Transform target)
    {
        float elapsed = 0f;
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

    private void OnMouseDown()
    {
        if (isSelectable)
        {
            GameManager.Instance.OnTokenClicked(this);
        }
    }

    public void SendToBase()
    {
        isInHome = false;
        currentPosition = -1;
        transform.position = basePosition != null ? basePosition.position : (pathPoints.Length > 0 ? pathPoints[0].position : transform.position);

        // Optionally disable selection/highlight when sent to base
        SetSelectable(false);
        SetHighlight(false);
    }

    public void SetHighlight(bool active)
    {
        isHighlighted = active;
        if (highlightObject != null)
            highlightObject.SetActive(active);
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = active ? Color.yellow : Color.white;
        }
    }

    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        SetHighlight(selectable);
    }
}
