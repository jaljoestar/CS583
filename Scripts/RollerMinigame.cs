using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RollerMinigame : MonoBehaviour
{
    [Header("Inscribed")]
    public Station station;
    public Button[] sushiButtons;
    public Color highlightColor = Color.white;
    public TMP_Text countdownText;
    public TMP_Text mistakesText;
    public int rewardPerOrder = 5;
    public int sessionCoinCap = 25;
    public int sequenceLength = 5;              // 5 buttons to memorize
    public int maxMistakes = 3;
    public float flashDuration = 0.4f;
    public float flashGap = 0.15f;
    public float inputTimeLimit = 6f;            // time allowed to repeat sequence
    public float wrongPauseDuration = 1f;        // time pause after mistake made
    public float dimAmount = 0.6f;
    public float highlightScale = 1.25f;         // sized up sushi thats lit
    public float wiggleAngle = 12f;
    public float punchDuration = 0.3f;
    public AudioClip coinSound;

    [Header("Feedback")]
    public TMP_Text feedbackText;
    public Color successColor = Color.yellow;
    public Color wrongColor = Color.red;
    public float feedbackFloatDistance = 40f;
    public float feedbackDuration = 0.6f;

    [Header("Dynamic")]
    public int sessionEarnings = 0;
    public int mistakeCount = 0;
    int[] sequence;                 // randomized order of buttons to repeat
    int progressIndex = 0;          // how many the player got correctly
    bool acceptingInput = false;        // true while waiting for the player input
    float inputTimer = 0f;
    Color[] originalColors;
    Coroutine roundCoroutine;
    Coroutine punchRoutine;

    void OnEnable()         // makes sure the popup resets 
    {
        sessionEarnings = 0;
        mistakeCount = 0;
        UpdateMistakesText();
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        originalColors = new Color[sushiButtons.Length];
        for (int i = 0; i < sushiButtons.Length; i++)
        {
            originalColors[i] = sushiButtons[i].image.color;
            int index = i;
            sushiButtons[i].onClick.RemoveAllListeners();
            sushiButtons[i].onClick.AddListener(() => OnSushiClicked(index));
        }

        StartNewRound();
    }

    void Update()
    {
        if (!acceptingInput) return;

        inputTimer -= Time.deltaTime;       // updates input timer 
        if (inputTimer <= 0f) HandleMistake();
    }

    void StartNewRound()
    {
        sequence = new int[sequenceLength];
        for (int i = 0; i < sequenceLength; i++)
            sequence[i] = Random.Range(0, sushiButtons.Length);         // randomized sequence

        progressIndex = 0;
        acceptingInput = false;

        if (roundCoroutine != null) StopCoroutine(roundCoroutine);
        roundCoroutine = StartCoroutine(RoundRoutine());
    }

    IEnumerator RoundRoutine()      // routine: coundown, sequence, player repeats
    {
        yield return StartCoroutine(Countdown());
        yield return StartCoroutine(FlashSequence());

        acceptingInput = true;
        inputTimer = inputTimeLimit;
    }

    IEnumerator Countdown()
    {
        if (countdownText == null) yield break;

        countdownText.gameObject.SetActive(true);
        string[] steps = { "3", "2", "1" };
        foreach (string step in steps)
        {
            countdownText.text = step;
            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
    }

    IEnumerator FlashSequence()
    {
        foreach (int index in sequence)
        {
            SetHighlight(index, true);                  // lights up button one at a time
            yield return new WaitForSeconds(flashDuration);
            SetHighlight(index, false);
            yield return new WaitForSeconds(flashGap);
        }
    }

    void OnSushiClicked(int index)
    {
        if (!acceptingInput) return;

        if (index == sequence[progressIndex])       // increment progressidx when player gets right
        {
            StartCoroutine(QuickFlash(index));
            progressIndex++;

            if (progressIndex >= sequence.Length)
                SucceedSequence();
        }
        else
        {
            HandleMistake();
        }
    }

    IEnumerator QuickFlash(int index)       // correct click triggers a flash
    {
        SetHighlight(index, true);
        yield return new WaitForSeconds(0.12f);
        SetHighlight(index, false);
    }

    void SetHighlight(int index, bool on)
    {
        sushiButtons[index].image.color = on ? highlightColor : originalColors[index];

        RectTransform rt = sushiButtons[index].GetComponent<RectTransform>();

        if (punchRoutine != null)
            StopCoroutine(punchRoutine);

        if (on)
        {
            punchRoutine = StartCoroutine(WigglePunch(rt));
        }
        else
        {
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }
    }

    IEnumerator WigglePunch(RectTransform rt)       // sushi wiggles and scales when chosen
    {
        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float progress = t / punchDuration;

            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * (highlightScale - 1f);
            float angle = Mathf.Sin(progress * Mathf.PI * 5f) * wiggleAngle;

            rt.localScale = Vector3.one * scale;
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    void SucceedSequence()      // triggered when player repeats sequence right
    {
        acceptingInput = false;
        sessionEarnings += rewardPerOrder;
        CurrencyManager.Instance.AddCurrency(rewardPerOrder);
        SfxManager.Instance.PlaySfx(coinSound);

        ShowFeedback("+" + rewardPerOrder, successColor, sushiButtons[sequence[sequence.Length - 1]].transform.position);

        if (sessionEarnings >= sessionCoinCap)  // checks that they havent reached max coins
            ClosePopup();
        else
            StartNewRound();
    }

    void HandleMistake()        // triggered when player picks wrong sushi or if they ran out of time
    {
        acceptingInput = false;
        mistakeCount++;
        UpdateMistakesText();

        int flashIndex = Mathf.Min(progressIndex, sequence.Length - 1);
        ShowFeedback("Wrong!", wrongColor, sushiButtons[sequence[flashIndex]].transform.position);

        if (mistakeCount >= maxMistakes)
            StartCoroutine(DelayedClose());
        else
            StartCoroutine(DelayedNewRound());
    }

    IEnumerator DelayedNewRound()
    {
        yield return new WaitForSeconds(wrongPauseDuration);
        StartNewRound();
    }

    IEnumerator DelayedClose()
    {
        yield return new WaitForSeconds(wrongPauseDuration);
        ClosePopup();
    }

    void UpdateMistakesText()
    {
        if (mistakesText != null)
            mistakesText.text = "mistakes: " + mistakeCount + "/" + maxMistakes;
    }

    void ShowFeedback(string message, Color color, Vector3 worldPos)    // shows both success and wrong text
    {
        if (feedbackText == null) return;
        feedbackText.transform.position = worldPos;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        StopCoroutine(nameof(FeedbackRoutine));
        StartCoroutine(FeedbackRoutine(color));
    }

    IEnumerator FeedbackRoutine(Color color)        // same feedback text as chef/waiter
    {
        float t = 0f;
        Vector3 startPos = feedbackText.transform.position;
        feedbackText.color = color;

        while (t < feedbackDuration)
        {
            t += Time.deltaTime;
            float pct = t / feedbackDuration;
            feedbackText.transform.position = startPos + Vector3.up * feedbackFloatDistance * pct;
            feedbackText.color = new Color(color.r, color.g, color.b, 1f - pct);
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }

    public void ClosePopup()        // close and start cooldown
    {
        if (station != null) station.StartCooldown();
        gameObject.SetActive(false);
    }
}