using UnityEngine;
using UnityEngine.UI;

public class TileInfo : MonoBehaviour
{
    public bool Occupied { get; set; }
    public int Owner { get; set; }
    public bool ValidMove { get; set; }

    public int boardID;
    public int tileID;

    public Image tileImage;
    public Button button;
    public RectTransform rectTransform;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(() => TileClicked(boardID, tileID));
        }
        else
        {
            Debug.LogWarning(gameObject.name + " has no button");
        }

        tileImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    void TileClicked(int boardID, int tileID)
    {
        TurnManager.instance.PlaceTile(boardID, tileID);
    }

    public bool CheckValidMove()
    {
        if ((Occupied) || !ValidMove)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public int GetBoardID()
    {
        return boardID;
    }

    public int GetTileID()
    {
        return tileID;
    }

    public void SetSprite(Sprite sprite)
    {
        tileImage.sprite = sprite;
    }
    /// <summary>
    /// Set tiles to be interactive or not
    /// </summary>
    /// <param name="isValid"></param>
    public void UpdateTileValidity(bool isValid)
    {
        tileImage.color = (Occupied || !isValid) ? Color.white : Color.green;
        ValidMove = isValid;
    }
}
