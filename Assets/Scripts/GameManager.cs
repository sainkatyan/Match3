using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardSetting
{
    public int xSize, ySize;
    public Tile tileGO;
    public List<Sprite> tileSprite;
}
public class GameManager : MonoBehaviour
{
    [Header ("Level Settings")]
    public BoardSetting boardSettings;

    private void Start()
    {
        BoardController.instance.SetValue(boardSettings);
    }
}

