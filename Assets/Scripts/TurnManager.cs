using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public int playerTurn;

    public bool vsPlayer;
    public int AIDifficulty;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void StartGamevsPlayer()
    {
        vsPlayer = true;
        BoardManager.instance.SetupBoard();
        BoardManager.instance.gameActive = true;
        AudioManager.instance.PlayAudio("UI Button", false);
    }
    public void StartGameVsAI(int difficulty)
    {
        vsPlayer = false;
        AIDifficulty = difficulty;
        BoardManager.instance.SetupBoard();
        BoardManager.instance.gameActive = true; 
    }
    /// <summary>
    /// Check if the clicked tile is a valid placement, then swap active player
    /// </summary>
    /// <param name="boardID"></param>
    /// <param name="tileID"></param>
    public void PlaceTile(int boardID, int tileID)
    {
        SubBoard subBoard = BoardManager.instance.boardState[boardID];
        if (subBoard.boardInfo.TryGetValue((boardID, tileID), out TileInfo tileInfo))
        {
            if (!tileInfo.CheckValidMove())
            {
                AudioManager.instance.PlayAudio("Place Piece Failed", false);
                return; // Don't place a tile if it's already taken or not allowed
            }

            BoardManager.instance.UpdateTile(playerTurn, boardID, tileID);
            UIManager.instance.PlaceTile(playerTurn, tileInfo);
            AudioManager.instance.PlayAudio("Place Piece " + Random.Range(0, 1), false);

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
}
