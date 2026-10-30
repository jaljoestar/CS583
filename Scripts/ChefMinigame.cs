using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChefMinigame : MonoBehaviour
{
    [Header("Inscribed")]
    public Station station;            
    public RectTransform barRect;        
    public RectTransform indicatorRect; 
    public RectTransform zoneRect;       
    public Button cookButton;
    public Slider patienceBar;
    public Image patienceBarFill;
    public Color patienceFullColor = Color.green;
    public Color patienceEmptyColor = Color.red;
    public int rewardPerOrder = 3;
    public int sessionCoinCap = 30;      
    public float indicatorSpeed = 1f;
    public float zoneWidth = 0.18f;      
    public float missPenalty = 0.2f;    
    public AudioClip coinSound;

    [Header("Fish")]
    public Image fishImage;
    public Sprite[] fishWholeSprites;

    [Header("Feedback")]
    public TMP_Text feedbackText;        // floating "+amount" text
    public Color feedbackColor = Color.yellow;
    public float feedbackFloatDistance = 40f;
    public float feedbackDuration = 0.6f;

    [Header("Dynamic")]
    public int sessionEarnings = 0;
    public float patience = 1f;          // 1 = full, 0 = empty (closes popup)
    float zoneMin;                       // target zone start, 0-1 along the bar
    float zoneMax;                       // target zone end, 0-1 along the bar

    
    void OnEnable()         // run when the popup is opened & reset round
    {
        sessionEarnings = 0;
        patience = 1f;
        UpdatePatienceVisual();

        PickRandomFish();
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        cookButton.onClick.RemoveAllListeners();
        cookButton.onClick.AddListener(OnCookClicked);

        RollNewZone();
    }

    
    void Update()           // move the knife back and forth
    {
        float t = Mathf.PingPong(Time.time * indicatorSpeed, 1f);
        PositionAlongBar(indicatorRect, t);
    }

    
    void PositionAlongBar(RectTransform rect, float t)      // rect transform along the bar's width
    {
        float width = barRect.rect.width;
        float x = Mathf.Lerp(-width / 2f, width / 2f, t);
        rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
    }

    
    void RollNewZone()       // randomized spot and size for target zone
    {
        zoneMin = Random.Range(0f, 1f - zoneWidth);
        zoneMax = zoneMin + zoneWidth;

        float width = barRect.rect.width;
        float zoneCenterT = (zoneMin + zoneMax) / 2f;
        float zoneX = Mathf.Lerp(-width / 2f, width / 2f, zoneCenterT);
        zoneRect.anchoredPosition = new Vector2(zoneX, zoneRect.anchoredPosition.y);
        zoneRect.sizeDelta = new Vector2(width * zoneWidth, zoneRect.sizeDelta.y);
    }


    void PickRandomFish()       // swaps between tuna and salmon
    {
        if (fishImage == null || fishWholeSprites.Length == 0) return;
        fishImage.sprite = fishWholeSprites[Random.Range(0, fishWholeSprites.Length)];
    }

    
    void OnCookClicked()        // when a player clicks cooked:
    {
        if (KnifeOverlapsZone())    // if in the zone pays out, plays sfx and moves to next fish
        {
            sessionEarnings += rewardPerOrder;
            CurrencyManager.Instance.AddCurrency(rewardPerOrder);
            SfxManager.Instance.PlaySfx(coinSound);

            ShowFeedback(rewardPerOrder, indicatorRect.position);
            PickRandomFish();

            RollNewZone();

            if (sessionEarnings >= sessionCoinCap) ClosePopup();
        }
        else
        {
            // miss zone then penalizes
            patience -= missPenalty;
            UpdatePatienceVisual();

            if (patience <= 0f) ClosePopup();
        }
    }

    // if the knifes rect overlaps the target zones rect, then true
    bool KnifeOverlapsZone()
    {
        float knifeLeft = indicatorRect.anchoredPosition.x - indicatorRect.rect.width / 2f;
        float knifeRight = indicatorRect.anchoredPosition.x + indicatorRect.rect.width / 2f;
        float zoneLeft = zoneRect.anchoredPosition.x - zoneRect.rect.width / 2f;
        float zoneRight = zoneRect.anchoredPosition.x + zoneRect.rect.width / 2f;

        return knifeLeft <= zoneRight && knifeRight >= zoneLeft;
    }

    // updating the patience bar
    void UpdatePatienceVisual()
    {
        float clamped = Mathf.Max(patience, 0f);
        if (patienceBar != null) patienceBar.value = clamped;
        if (patienceBarFill != null) patienceBarFill.color = Color.Lerp(patienceEmptyColor, patienceFullColor, clamped);
    }

    // shows how much player earns with each hit
    void ShowFeedback(int amount, Vector3 worldPos)
    {
        if (feedbackText == null) return;
        feedbackText.transform.position = worldPos;
        feedbackText.text = "+" + amount;
        feedbackText.gameObject.SetActive(true);
        StopCoroutine(nameof(FeedbackRoutine));
        StartCoroutine(FeedbackRoutine());
    }

    // visual fade upwards of feedback text
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

    // close minigame and start cooldown
    public void ClosePopup()
    {
        if (station != null) station.StartCooldown();
        gameObject.SetActive(false);
    }
}