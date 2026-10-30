using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaiterMinigame : MonoBehaviour
{
    [Header("Inscribed")]
    public Station station;
    public Button[] tableButtons;
    public GameObject[] orderIndicators;
    public Slider patienceBar;
    public Image patienceBarFill;
    public Color patienceFullColor = Color.green;
    public Color patienceEmptyColor = Color.red;
    public int rewardPerOrder = 5;
    public int sessionCoinCap = 25;
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 1.25f;
    public float orderTimeLimit = 3f;       // time limit for player to serve table
    public float missPenalty = 0.2f;
    public AudioClip coinSound;

    [Header("Rush Phase")]
    public int rushPhaseThreshold = 15;     // how much currency is needed for rush phase to start
    public float rushSpawnDelayMin = 0.25f;
    public float rushSpawnDelayMax = 0.6f;
    public float rushOrderTimeLimit = 1.75f;

    [Header("Combo")]
    public float comboMultiplierStep = 0.5f;        // multiplier activated when player gets consecutive serves
    public float maxComboMultiplier = 2f;
    public TMP_Text comboText;

    [Header("Feedback")]
    public TMP_Text feedbackText;
    public Color feedbackColor = Color.yellow;
    public float feedbackFloatDistance = 40f;
    public float feedbackDuration = 0.6f;

    [Header("Dynamic")]
    public int sessionEarnings = 0;
    public int comboCount = 0;
    public float patience = 1f;
    bool[] tableHasOrder;
    float[] tableOrderTimer;
    float nextSpawnTime = 0f;

    bool InRush() => sessionEarnings >= rushPhaseThreshold;     // checks if player earned enough to trigger rush phase

    void OnEnable()     // resets minigame and spawns orders
    {
        sessionEarnings = 0;
        comboCount = 0;
        patience = 1f;
        UpdatePatienceVisual();
        if (comboText != null) comboText.gameObject.SetActive(false);

        tableHasOrder = new bool[tableButtons.Length];
        tableOrderTimer = new float[tableButtons.Length];

        foreach (var indicator in orderIndicators) indicator.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        ScheduleNextSpawn();

        for (int i = 0; i < tableButtons.Length; i++)
        {
            int index = i;
            tableButtons[i].onClick.RemoveAllListeners();
            tableButtons[i].onClick.AddListener(() => OnTableClicked(index));
        }
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)     // spawn new order when time
        {
            TrySpawnOrder();
            ScheduleNextSpawn();
        }

        for (int i = 0; i < tableHasOrder.Length; i++)      // count down for orders, miss when it runs out
        {
            if (!tableHasOrder[i]) continue;
            tableOrderTimer[i] -= Time.deltaTime;
            if (tableOrderTimer[i] <= 0f)
            {
                ClearOrder(i);
                MissOrder();
            }
        }
    }

    void TrySpawnOrder()
    {
        if (sessionEarnings >= sessionCoinCap) return;  // checks if coin cap has been reached, if it hasnt spawn a new order

        int freeCount = 0;
        for (int i = 0; i < tableHasOrder.Length; i++)
            if (!tableHasOrder[i]) freeCount++;
        if (freeCount == 0) return;

        int skip = Random.Range(0, freeCount);      // pick the next free table at random
        for (int i = 0; i < tableHasOrder.Length; i++)
        {
            if (tableHasOrder[i]) continue;
            if (skip == 0)
            {
                tableHasOrder[i] = true;
                tableOrderTimer[i] = InRush() ? rushOrderTimeLimit : orderTimeLimit;
                orderIndicators[i].SetActive(true);
                return;
            }
            skip--;
        }
    }

    void OnTableClicked(int index)      // checks if player clicked the table that has order
    {
        if (!tableHasOrder[index]) return;

        comboCount++;
        float multiplier = Mathf.Min(1f + (comboCount - 1) * comboMultiplierStep, maxComboMultiplier);
        int reward = Mathf.RoundToInt(rewardPerOrder * multiplier);

        sessionEarnings += reward;
        CurrencyManager.Instance.AddCurrency(reward);
        SfxManager.Instance.PlaySfx(coinSound);
        ClearOrder(index);

        UpdateComboVisual(multiplier);
        ShowFeedback(reward, tableButtons[index].transform.position);

        if (sessionEarnings >= sessionCoinCap) ClosePopup();
    }

    void MissOrder()    // when order missed it breaks combo and decreases patience
    {
        comboCount = 0;
        if (comboText != null) comboText.gameObject.SetActive(false);

        patience -= missPenalty;
        UpdatePatienceVisual();

        if (patience <= 0f) ClosePopup();
    }

    void UpdateComboVisual(float multiplier)        // shows multiplier
    {
        if (comboText == null) return;

        if (multiplier > 1f)
        {
            comboText.text = "x" + multiplier.ToString("0.0") + " Combo!";
            comboText.gameObject.SetActive(true);
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    void UpdatePatienceVisual()
    {
        float clamped = Mathf.Max(patience, 0f);
        if (patienceBar != null) patienceBar.value = clamped;
        if (patienceBarFill != null) patienceBarFill.color = Color.Lerp(patienceEmptyColor, patienceFullColor, clamped);
    }

    void ShowFeedback(int amount, Vector3 worldPos)     // same feedback shows money earned
    {
        if (feedbackText == null) return;
        feedbackText.transform.position = worldPos;
        feedbackText.text = "+" + amount;
        feedbackText.gameObject.SetActive(true);
        StopCoroutine(nameof(FeedbackRoutine));
        StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        float t = 0f;
        Vector3 startPos = feedbackText.transform.position;
        feedbackText.color = feedbackColor;

        while (t < feedbackDuration)
        {
            t += Time.deltaTime;
            float pct = t / feedbackDuration;
            feedbackText.transform.position = startPos + Vector3.up * feedbackFloatDistance * pct;
            feedbackText.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 1f - pct);
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }

    void ClearOrder(int index)
    {
        tableHasOrder[index] = false;
        orderIndicators[index].SetActive(false);
    }

    void ScheduleNextSpawn()
    {
        if (InRush())
            nextSpawnTime = Time.time + Random.Range(rushSpawnDelayMin, rushSpawnDelayMax);
        else
            nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    public void ClosePopup()
    {
        if (station != null) station.StartCooldown();
        gameObject.SetActive(false);
    }
}