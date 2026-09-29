using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlyToLocation))]
public class FlyToLocation_Test : MonoBehaviour
{
    [Header("Coordinates")]
    [Range(-90f, 90f)]
    [SerializeField] private float latitude;

    [Range(-180f, 180f)]
    [SerializeField] private float longitude;


    [ContextMenu("Fly To Current Location")]
    public void FlyToCurrentLocation()
    {
        FlyTo(latitude, longitude);
    }

    // EUROPE
    [ContextMenu("Fly: Warsaw")]
    public void FlyToWarsaw() => FlyTo(52.2297f, 21.0122f);

    [ContextMenu("Fly: London")]
    public void FlyToLondon() => FlyTo(51.5074f, -0.1278f);

    [ContextMenu("Fly: Paris")]
    public void FlyToParis() => FlyTo(48.8566f, 2.3522f);

    [ContextMenu("Fly: Berlin")]
    public void FlyToBerlin() => FlyTo(52.5200f, 13.4050f);

    // USA
    [ContextMenu("Fly: New York")]
    public void FlyToNewYork() => FlyTo(40.7128f, -74.0060f);

    [ContextMenu("Fly: Los Angeles")]
    public void FlyToLosAngeles() => FlyTo(34.0522f, -118.2437f);

    [ContextMenu("Fly: Denver")]
    public void FlyToDenver() => FlyTo(39.7392f, -104.9903f);

    [ContextMenu("Fly: Chicago")]
    public void FlyToChicago() => FlyTo(41.8781f, -87.6298f);

    // ASIA
    [ContextMenu("Fly: Tokyo")]
    public void FlyToTokyo() => FlyTo(35.6762f, 139.6503f);

    [ContextMenu("Fly: Dubai")]
    public void FlyToDubai() => FlyTo(25.2048f, 55.2708f);

    [ContextMenu("Fly: Singapore")]
    public void FlyToSingapore() => FlyTo(1.3521f, 103.8198f);

    // OTHER
    [ContextMenu("Fly: Sydney")]
    public void FlyToSydney() => FlyTo(-33.8688f, 151.2093f);

    [ContextMenu("Fly: Rio de Janeiro")]
    public void FlyToRio() => FlyTo(-22.9068f, -43.1729f);

    private void FlyTo(float lat, float lon)
    {
        FlyToLocation ftl = GetComponent<FlyToLocation>();
        ftl.FlyTo(lat, lon);
    }
}
