using UnityEngine;

[RequireComponent(typeof(PlaceOnGlobe))]
public class PlaceOnGlobe_Test : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject prefabToSpawn;

    [Header("Coordinates")]
    [Range(-90f, 90f)]
    [SerializeField] private float latitude;

    [Range(-180f, 180f)]
    [SerializeField] private float longitude;

    [ContextMenu("Spawn On Globe")]
    public void SpawnOnGlobe()
    {
        if (prefabToSpawn == null)
            return;

        PlaceOnGlobe globePlacer = GetComponent<PlaceOnGlobe>();
        globePlacer.Place(prefabToSpawn, latitude, longitude);
    }
}