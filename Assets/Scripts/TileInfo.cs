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

        tileImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    void TileClicked(int boardID, int tileID)
    {
        TurnManager.instance.PlaceTile(boardID, tileID);
    }

    public bool CheckValidMove()
    {
        return ValidMove;
    }

    public void SetOwner(int _owner)
    {
        Owner = _owner;
    }

    public int GetOwner()
    {
        return Owner;
    }

    public void SetOccupied(bool _occupied)
    {
        Occupied = _occupied;
    }

    public bool GetOccupied()
    { 
        return Occupied; 
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
        if (!GetOccupied())
        {
            tileImage.color = isValid ? Color.green : Color.white;
        }
        else
        {
            tileImage.color = Color.white;
        }
        ValidMove = isValid;
    }
}
