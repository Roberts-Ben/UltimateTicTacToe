using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public int playerTurn;

    private void Awake()
    {
        instance = this;
    }
    public void PlaceTile(int boardID, int tileID)
    {
        TileInfo tileInfo = BoardManager.instance.boardState[boardID].tiles[tileID].GetComponent<TileInfo>();

        if (tileInfo.GetOccupied())
        {
            return; // Don't place a tile if it's already taken
        }

        BoardManager.instance.UpdateTile(playerTurn, boardID, tileID);

        if (playerTurn == 0)
        {
            playerTurn = 1;
        }
        else
        {
            playerTurn = 0;
        } 
    }
}
