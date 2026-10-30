using UnityEngine;

public class Station : MonoBehaviour            // represent chef, waiter, roller and handles hiring, speed, income, opening minigames
{
    [Header("Inscribed")]
    public GameObject popupPanel;
    public GameObject workerVisual;
    public GameObject clickIndicator;
    public int hireCost = 50;
    public int idleIncomePerTick = 5;
    public float tickInterval = 3f;
    public float cooldownDuration = 5f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;
    public AudioClip bubbleSound;
    public AudioClip deniedSound;
    public AudioClip openSound;

    [Header("Speed Upgrade")]       // speed upgrades bought in store and their matching speed animations/tick intervals
    public int speedTier = 0;
    public int[] speedUpgradeCosts = { 100, 250, 500 };
    public float[] speedUpgradeTickIntervals = { 2.5f, 2.0f, 1.5f };
    public float[] speedUpgradeAnimSpeeds = { 1.2f, 1.5f, 2.0f };

    [Header("Dynamic")]
    public bool isStaffed = false;
    float cooldownEndTime = 0f;
    Vector3 indicatorStartPos;

    void Start()
    {
        if (clickIndicator != null)
            indicatorStartPos = clickIndicator.transform.localPosition;
    }

    void Update()       // bobs the click indicator up and down, hides if minigame still on cooldown
    {
        if (clickIndicator == null) return;

        bool shouldShow = !isStaffed && Time.time >= cooldownEndTime;
        if (clickIndicator.activeSelf != shouldShow)
            clickIndicator.SetActive(shouldShow);

        if (shouldShow)
        {
            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            clickIndicator.transform.localPosition = indicatorStartPos + new Vector3(0, offset, 0);
        }
    }

    void OnMouseDown()      // open minigame if player clicks on it
    {
        TryOpenPopup();
    }

    public void TryOpenPopup()  // checks if its staffed, the cooldown, and plays sfx
    {
        if (isStaffed) return;
        if (Time.time < cooldownEndTime) return;
        SfxManager.Instance.PlaySfx(openSound);
        if (popupPanel != null) popupPanel.SetActive(true);
    }

    public void StartCooldown()     // strts cooldown before it can be played again
    {
        cooldownEndTime = Time.time + cooldownDuration;
    }

    public bool TryHire()       // checks if station is staffed, if player has enough money; 
    {
        if (isStaffed) return false;
        if (!CurrencyManager.Instance.TrySpend(hireCost)) return false;

        isStaffed = true;
        if (workerVisual != null) workerVisual.SetActive(true);     // shows worker sprite when hired
        InvokeRepeating(nameof(GenerateIncome), tickInterval, tickInterval);        // activates passive income of that worker
        return true;
    }

    public void Hire()      // hire sound and cancel sound
    {
        if (TryHire())
            SfxManager.Instance.PlaySfx(bubbleSound);
        else
            SfxManager.Instance.PlaySfx(deniedSound);
    }

    public bool TryUpgradeSpeed()           // when player upgrades speed checks if maxed out or if not enough currency
    {
        if (speedTier >= speedUpgradeCosts.Length) return false; // already maxed out

        int cost = speedUpgradeCosts[speedTier];
        if (!CurrencyManager.Instance.TrySpend(cost)) return false;

        tickInterval = speedUpgradeTickIntervals[speedTier];        // speeds up worker
        float newAnimSpeed = speedUpgradeAnimSpeeds[speedTier];
        speedTier++;

        if (workerVisual != null)
        {
            Animator anim = workerVisual.GetComponent<Animator>();      // speeds up worker visually
            if (anim != null) anim.speed = newAnimSpeed;
        }

        if (isStaffed)
        {
            CancelInvoke(nameof(GenerateIncome));
            InvokeRepeating(nameof(GenerateIncome), tickInterval, tickInterval);        // lcoks in its interval when called; must be restarted w/ new interval
        }

        return true;
    }

    public void UpgradeSpeed()      // same sfx sounds played as hire
    {
        if (TryUpgradeSpeed())
            SfxManager.Instance.PlaySfx(bubbleSound);
        else
            SfxManager.Instance.PlaySfx(deniedSound);
    }

    void GenerateIncome() // called every tick interval when station is staffed
    {
        CurrencyManager.Instance.AddCurrency(idleIncomePerTick);
    }
}