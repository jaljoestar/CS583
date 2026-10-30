using UnityEngine;
using TMPro;

// to track players money and HUD updates
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int currency = 0;
    public TextMeshProUGUI currencyText;

    void Awake()
    {
        Instance = this;
    }

    
    void Update()   // updates the currency
    {
        currencyText.text = currency.ToString("#,0");
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
    }

    public bool TrySpend(int amount)    // does not allow player to spend more than their currency
    {
        if (currency < amount) return false;
        currency -= amount;
        return true;
    }
}