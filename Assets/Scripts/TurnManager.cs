using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public List<GameObject> playerUI;
    public int playerTurn;

    public bool gameActive = false;
    public bool vsPlayer;
    public int AIDifficulty;

    private void Awake()
    {
        instance = this;
    }
    public void StartGamevsPlayer()
    {
        vsPlayer = true;
        BoardManager.instance.SetupBoard();
        gameActive = true;
    }
    public void StartGameVsAI(int dificulty)
    {
        vsPlayer = false;
        BoardManager.instance.SetupBoard();
        AIDifficulty = dificulty;
    }
    /// <summary>
    /// Check if the clicked tile is a valid placement, then swap active player
    /// </summary>
    /// <param name="boardID"></param>
    /// <param name="tileID"></param>
    public void PlaceTile(int boardID, int tileID)
    {
        TileInfo tileInfo = BoardManager.instance.boardState[boardID].tiles[tileID].GetComponent<TileInfo>();

        if (tileInfo.GetOccupied()|| !tileInfo.CheckValidMove())
        {
            return; // Don't place a tile if it's already taken or not allowed
        }

        BoardManager.instance.UpdateTile(playerTurn, boardID, tileID);

        if (playerTurn == 0)
        {
            playerTurn = 1;
            playerUI[0].SetActive(false);
            playerUI[1].SetActive(true);
        }
        else
        {
            playerTurn = 0; 
            playerUI[0].SetActive(true);
            playerUI[1].SetActive(false);
        } 
    }
}
