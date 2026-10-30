using UnityEngine;

public class SushiFall : MonoBehaviour      // sushi falls one by one until offscreen or lands on plate
{
    [Header("Inscribed")]
    public float fallSpeed = 3f;
    public float offScreenY = -10f;     // destroyed past -10

    [Header("Dynamic")]
    private Transform plate;
    private float landOffset = 0.4f;
    private float plateHalfWidth = 2f;      // how wide plate is for sushi to land
    private bool landed = false;

    public void SetPlate(Transform plateTransform, float offset, float halfWidth)
    {
        plate = plateTransform;
        landOffset = offset;
        plateHalfWidth = halfWidth;
    }

    void Update()
    {
        if (landed || plate == null) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        float landY = plate.position.y + landOffset;
        bool withinPlateX = Mathf.Abs(transform.position.x - plate.position.x) <= plateHalfWidth;

        if (withinPlateX && transform.position.y <= landY)      // sushi that lands on the plate snaps on once it reaches landing height 
        {
            Vector3 pos = transform.position;
            pos.y = landY;
            transform.position = pos;
            landed = true;
            return;
        }

        if (transform.position.y <= offScreenY)     // clean up after sushi goes past screen
        {
            Destroy(gameObject);
        }
    }
}