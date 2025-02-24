using UnityEngine;
using UnityEngine.UI;

public class TileInfo : MonoBehaviour
{
    public bool occupied;
    public int owner;
    public bool validMove;
    public int boardID;
    public int tileID;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() => TileClicked(boardID, tileID));
    }

    void TileClicked(int boardID, int tileID)
    {
        TurnManager.instance.PlaceTile(boardID, tileID);
    }

    public bool CheckValidMove()
    {
        return validMove;
    }

    public void SetValidMove(bool _validMove)
    {
        validMove = _validMove;
    }

    public void SetOwner(int _owner)
    {
        owner = _owner;
        occupied = true;
    }

    public int GetOwner()
    {
        return owner;
    }

    public bool GetOccupied()
    { 
        return occupied; 
    }

    public int GetBoardID()
    {
        return boardID;
    }

    public int GetTileID()
    {
        return tileID;
    }
}
