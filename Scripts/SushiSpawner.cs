using UnityEngine;

public class SushiSpawner : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject[] sushiPrefabs;
    public Transform plate;
    public float plateHalfWidth = 2f;
    public float landOffset = 0.4f;
    public float spawnInterval = 0.8f;
    public float leftEdge = -8f;
    public float rightEdge = 8f;
    public float spawnY = 6f;
    public int maxSushiOnScreen = 40;       // max amt of sushi on screen

    void Start()
    {
        SpriteRenderer plateRenderer = plate.GetComponent<SpriteRenderer>();        // plate bounds, where sushi can land
        if (plateRenderer != null)
            plateHalfWidth = plateRenderer.bounds.extents.x;

        InvokeRepeating("SpawnSushi", 0.5f, spawnInterval);
    }
    void SpawnSushi()       // spawns random sushi from prefabs, calls sushifall 
    {
        if (GameObject.FindGameObjectsWithTag("MenuSushi").Length >= maxSushiOnScreen) return;

        GameObject prefab = sushiPrefabs[Random.Range(0, sushiPrefabs.Length)];
        Vector3 spawnPos = new Vector3(Random.Range(leftEdge, rightEdge), spawnY, 0);
        GameObject newSushi = Instantiate(prefab, spawnPos, Quaternion.identity);

        SushiFall fallScript = newSushi.GetComponent<SushiFall>();
        if (fallScript != null)
        {
            fallScript.SetPlate(plate, landOffset, plateHalfWidth);
        }
    }
}