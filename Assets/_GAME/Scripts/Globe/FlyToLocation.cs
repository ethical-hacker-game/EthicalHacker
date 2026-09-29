using System.Collections;
using UnityEngine;

public class FlyToLocation : MonoBehaviour
{
    [Header("Globe")]
    [SerializeField] private Transform globe;
    [SerializeField] private float globeRadius = 5f;
    [SerializeField] private float orbitHeight = 3f;


    [Header("Animation")]
    [SerializeField] private float flyDuration = 2f;
    [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine flyCoroutine;

    [Range(-90f, 90f)]
    private float latitude;
    [Range(-180f, 180f)]
    private float longitude;

    public static FlyToLocation Instance { get; private set; }
    public bool IsFlying { get; private set; }


    public void FlyTo(float newLatitude, float newLongitude)
    {
        latitude = newLatitude;
        longitude = newLongitude;

        if (globe == null)
        {
            Debug.LogWarning("Globe is not assigned.");
            return;
        }

        if (flyCoroutine != null)
            StopCoroutine(flyCoroutine);

        IsFlying = true;
        flyCoroutine = StartCoroutine(FlyRoutine());
    }

    private IEnumerator FlyRoutine()
    {
        Vector3 center = globe.position;
        float orbitRadius = globeRadius + orbitHeight;

        Vector3 startDir = (transform.position - center).normalized;
        Vector3 endDir = LatLonToDirection(latitude, longitude);

        if (startDir == Vector3.zero)
            startDir = endDir;

        float time = 0f;

        while (time < flyDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / flyDuration);
            float easedT = flyCurve.Evaluate(t);

            Vector3 dir = Vector3.Slerp(startDir, endDir, easedT).normalized;

            transform.position = center + dir * orbitRadius;
            transform.LookAt(center);

            yield return null;
        }

        transform.position = center + endDir * orbitRadius;
        transform.LookAt(center);

        flyCoroutine = null;
        IsFlying = false;
    }

    private void Awake()
    {
        Instance = this;
    }

    private Vector3 LatLonToDirection(float lat, float lon)
    {
        float latRad = lat * Mathf.Deg2Rad;
        float lonRad = lon * Mathf.Deg2Rad;

        float x = Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float y = Mathf.Sin(latRad);
        float z = Mathf.Cos(latRad) * Mathf.Sin(lonRad);

        return new Vector3(x, y, z).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (globe == null)
            return;

        Vector3 center = globe.position;
        float orbitRadius = globeRadius + orbitHeight;
        Vector3 dir = LatLonToDirection(latitude, longitude);
        Vector3 camPos = center + dir * orbitRadius;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(camPos, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(camPos, center);
    }
}