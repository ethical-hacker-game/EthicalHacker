using UnityEngine;

public class PlaceOnGlobe : MonoBehaviour
{
    [Header("Globe")]
    [SerializeField] private Transform globe;
    [SerializeField] private float globeRadius = 5f;

    [Header("References")]
    [SerializeField] private GameObject defaultPrefabToSpawn;

    [Header("Settings")]
    [SerializeField] private float heightAboveSurface = 0f;

    public static PlaceOnGlobe Instance { get; private set; }

    public void Place(GameObject obj, float latitude, float longitude)
    {
        PlaceAndReturn(obj, latitude, longitude);
    }

    public GameObject PlaceAndReturn(GameObject obj, float latitude, float longitude)
    {
        if (obj == null)
        {
            obj = defaultPrefabToSpawn;
        }

        if (obj == null || globe == null)
        {
            Debug.LogWarning("Missing object or globe reference.");
            return null;
        }

        Vector3 center = globe.position;

        Vector3 normal = LatLonToDirection(latitude, longitude);

        Vector3 position = center + normal * (globeRadius + heightAboveSurface);

        GameObject spawnedObject = Instantiate(obj);
        spawnedObject.transform.position = position;

        Vector3 northDirection = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;

        if (northDirection.sqrMagnitude < 0.001f)
        {
            northDirection = Vector3.forward;
        }

        spawnedObject.transform.rotation = Quaternion.LookRotation(-normal, northDirection);
        return spawnedObject;
    }

    public static Vector3 LatLonToDirection(float latitude, float longitude)
    {
        float latRad = latitude * Mathf.Deg2Rad;
        float lonRad = longitude * Mathf.Deg2Rad;

        float x = Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float y = Mathf.Sin(latRad);
        float z = Mathf.Cos(latRad) * Mathf.Sin(lonRad);

        return new Vector3(x, y, z).normalized;
    }

    private void Awake()
    {
        Instance = this;
    }
}