using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour
{
    [SerializeField] private List<Sprite> npcSprites = new List<Sprite>();

    public static ImageManager Instance { get; private set; }

    public static Sprite GetNpcImage(string imageId)
    {
        return Instance.npcSprites.Find(x => x.name == imageId);
    }

    private void Awake()
    {
        Instance = this;
    }
}