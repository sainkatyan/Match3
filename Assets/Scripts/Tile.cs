using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int index;
    public int colorIndex;
    public SpriteRenderer spriteRenderer;
    public bool isSelected = false;
    public bool isEmpty
    {
        get
        {
            return spriteRenderer.sprite == null ? true : false;
        }
    }

    
}
