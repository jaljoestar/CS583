using UnityEngine;

public class StationClickIndicator : MonoBehaviour      // allows the player to click on the click indicator
{
    public Station station;

    void OnMouseDown()
    {
        if (station != null) station.TryOpenPopup();
    }
}