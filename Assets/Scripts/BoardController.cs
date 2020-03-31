using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour //чуть нужно использовать наследование от Board //различная логика должна быть в различных классах
{
    public static BoardController instance;
    public int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();

    public AnimationCurve animationCurve = new AnimationCurve();

    public Tile[] tileArray;

    private int oldSelectedIndex = int.MaxValue;

    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private bool isFindMatch = false;
    private bool isShift = false;
    private bool isSearchEmptyTile = false;

    public int GetIndex(int x, int y)
    {
        return y * xSize + x;
    }

    public int GetIndexX(int index)
    {        
        return index - GetIndexY(index) * xSize;
    }

    public int GetIndexY(int index)
    {
        return Mathf.FloorToInt((float)index / xSize);
    }

    public void SetValue(BoardSetting boardSetting ) //метод получения данных
    {        
        this.ySize = boardSetting.ySize;
        this.xSize = boardSetting.xSize;
        this.tileSprite = boardSetting.tileSprite;
        this.tileArray = CreateBoard(boardSetting.tileGO);
    }

    private Tile[] CreateBoard(Tile tileParent) //возвращет двухмерный массив
    {
        Tile[] tileArray = new Tile[xSize * ySize];
        float xPos = transform.position.x;
        float yPos = transform.position.y;
        Vector2 tileSize = tileParent.spriteRenderer.bounds.size;

        int cache = int.MaxValue;
        int random = int.MaxValue;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Tile newTile = Instantiate(tileParent, transform.position, Quaternion.identity);
                newTile.transform.position = new Vector3(xPos + (tileSize.x * x), yPos + (tileSize.y * y), 0);
                newTile.transform.parent = transform;

                var index = GetIndex(x, y);

                tileArray[index] = newTile;

                do
                {
                    random = Random.Range(0, tileSprite.Count);
                }
                while (cache == random);
                
                newTile.spriteRenderer.sprite = tileSprite[random];
                newTile.index = index;
                newTile.colorIndex = random;
                cache = random;
            }
        }
        return tileArray;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (isSearchEmptyTile)
        {
            SearchEmptyTile();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (ray != false)
            {
                var tile = ray.collider.gameObject.GetComponent<Tile>();
                Debug.Log(tile.index);
                CheckSelectionTile(tile.index);
            }
        }
    }

    #region(Выделить тайл, снятие выделения тайла, Управление выделением)
    private void SelectTile(int index)
    {
        tileArray[index].isSelected = true;
        tileArray[index].spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelectedIndex = index;
    }

    private void DeselectTile(int index)
    {
        tileArray[index].isSelected = false;
        tileArray[index].spriteRenderer.color = new Color(1, 1, 1);
        oldSelectedIndex = int.MaxValue;
    }

    private void CheckSelectionTile(int index)
    {
        if (index == oldSelectedIndex) // выделение снимаем
        {
            DeselectTile(index);
        }
        else
        {
            if (oldSelectedIndex == int.MaxValue)  //выделяем
            {                 
                SelectTile(index);
            }
            //попытка выбрать другой тайл
            else
            {
                // если 2й выбранный тайл сосед предыдущего
                if (AdjacentTiles(index))
                {
                    SwapToTile(index);
                    FindAllMatch(index);
                    DeselectTile(index);
                }
                else //выделение нового тайла, забываем старый
                {
                    Debug.Log(2);
                    DeselectTile(oldSelectedIndex);
                    SelectTile(index);
                }
            }
        }

    }
    #endregion

    #region(Поиск совпадения, удаление спрайтов, поиск всех совпадений)
    private List<Tile> FindMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while (hit.collider != null && hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.transform.position, dir);
        }
        return cashFindTiles;
    }
    public int celectedColor = 0;
    private void FindAllMatch(int index)
    {
        Debug.Log("Index: " + index + " color index " + tileArray[index].colorIndex);
        celectedColor = tileArray[index].colorIndex;

        int nextStepIndex = index + xSize;
        List<int> cacheIndexes = new List<int>();
        while (GetIndexY(nextStepIndex) < ySize && celectedColor == tileArray[nextStepIndex].colorIndex)
        {
            cacheIndexes.Add(nextStepIndex);
            nextStepIndex += xSize;
        }

        nextStepIndex = index - xSize;
        while (GetIndexY(nextStepIndex) >= 0 && celectedColor == tileArray[nextStepIndex].colorIndex)
        {
            cacheIndexes.Add(nextStepIndex);
            nextStepIndex -= xSize;
        }


        nextStepIndex = index + 1;

        while (GetIndexY(nextStepIndex) == GetIndexY(index) && celectedColor == tileArray[nextStepIndex].colorIndex)
        {
            cacheIndexes.Add(nextStepIndex);
            nextStepIndex += 1;
        }

        nextStepIndex = index - 1;
        while (GetIndexY(nextStepIndex) == GetIndexY(index) && celectedColor == tileArray[nextStepIndex].colorIndex)
        {
            cacheIndexes.Add(nextStepIndex);
            nextStepIndex -= 1;
        }

        for (int i = 0; i < cacheIndexes.Count; i++)
        {
            Debug.Log("cache: " + cacheIndexes[i]);
        }
       
    }
    #endregion

    #region(Смена 2х тайлов, Соседние тайлы)
    private void SwapToTile(int index)
    {
        Tile tempTile = tileArray[index];
        tileArray[index] = tileArray[oldSelectedIndex];
        tileArray[index].index = index;
        tileArray[oldSelectedIndex] = tempTile;
        tileArray[oldSelectedIndex].index = oldSelectedIndex;

        StartCoroutine(SwapAnim(index, oldSelectedIndex));
    }

    private IEnumerator SwapAnim(int tileIndex1, int tileIndex2)
    {
        float timer = 0;

        Vector3 startPos1 = tileArray[tileIndex1].transform.position;
        Vector3 startPos2 = tileArray[tileIndex2].transform.position;

        float speed = 0.3f;

        while (timer < speed)
        {
            timer += Time.deltaTime;

            tileArray[tileIndex1].GetComponent<SpriteRenderer>().sortingOrder = 1;
            var time = animationCurve.Evaluate(timer / speed);
            tileArray[tileIndex1].transform.position = Vector3.Lerp(startPos1, startPos2, time);
            tileArray[tileIndex2].transform.position = Vector3.Lerp(startPos2, startPos1, time);

            yield return null;
        }        
    }

    private bool AdjacentTiles(int index)
    {
        if (oldSelectedIndex == index - 1) //левый сосед
        {
            if (GetIndexY(oldSelectedIndex) == GetIndexY(index)) // проверяем находятся ли соседи в одном этаже
            {
                return true;
            }
        }
        if (oldSelectedIndex == index + 1) //правый сосед
        {
            if (GetIndexY(oldSelectedIndex) == GetIndexY(index)) // проверяем находятся ли соседи в одном этаже
            {
                return true;
            }            
        }
        if (oldSelectedIndex == index + xSize) //верхний сосед
        {
            return true;
        }
        if (oldSelectedIndex == index - xSize) //нижний сосед
        {
            return true;
        }
        return false;
    }
    #endregion

    #region (Поиск пустого тайла, Сдвиг тайла вниз, Установить новое изображение, Выбрать новое изображение)
    private void SearchEmptyTile()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tileArray[GetIndex(x, y)].isEmpty)
                {
                    ShiftTileDown(x, y);
                    break;
                }

                if (x == xSize && y == ySize) //есди не обнаружим путые тайлы
                {
                    isSearchEmptyTile = false;
                }
            }
        }

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                //FindAllMatch(tileArray[GetIndex(x, y)]);
            }
        }
    }

    private void ShiftTileDown(int xPos, int yPos)
    {
        isShift = true;
        List<SpriteRenderer> cashRenderer = new List<SpriteRenderer>();
        for (int y = yPos; y < ySize; y++)
        {
            Tile tile = tileArray[GetIndex(xPos, y)];
            cashRenderer.Add(tile.spriteRenderer);
            //if (tile.isEmpty)
            //{
            //    cashRenderer.Add(tile.spriteRenderer);
            //}
        }
        SetNewSprite(xPos, cashRenderer);
        isShift = false;
    }

    private void SetNewSprite(int xPos, List<SpriteRenderer> renderer)
    {
        for (int y = 0; y < renderer.Count - 1; y++)
        {
            renderer[y].sprite = renderer[y + 1].sprite;
            renderer[y + 1].sprite = GetNewSprite(xPos, ySize - 1);             //создаем новые тайлы сверху             //y -1
        }
    }

    private Sprite GetNewSprite(int xPos,int yPos)
    {
        List<Sprite> cashSprite = new List<Sprite>();
        cashSprite.AddRange(tileSprite);
        if (xPos > 0)
        {
            cashSprite.Remove(tileArray[GetIndex(xPos - 1, yPos)].spriteRenderer.sprite);
        }

        if (xPos < xSize - 1)
        {
            cashSprite.Remove(tileArray[GetIndex(xPos + 1, yPos)].spriteRenderer.sprite);
        }

        if (yPos > 0)
        {
            cashSprite.Remove(tileArray[GetIndex(xPos, yPos - 1)].spriteRenderer.sprite);
        }

        return cashSprite[Random.Range(0, cashSprite.Count)];
    }

    #endregion
}
