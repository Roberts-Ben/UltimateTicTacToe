using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubBoard : MonoBehaviour // Each standard tic-tac-toe board within the larger 3x3 grid
{
    public int subBoardID; // Position in the final grid, 0 = top left
    public int finalBoardState; // Who owns the board

    public Dictionary<(int subBoardID, int tileID), TileInfo> boardInfo = new();

    public bool endedInTie;
    public List<int> subBoardState; // Who owns each tile within this board

    private const int NO_OWNER = -1;

    public void SetupBoard()
    {
        boardInfo.Clear();
        TileInfo[] tileInfos = GetComponentsInChildren<TileInfo>();

        for (int i = 0; i < tileInfos.Length; i++)
        {
            // Set board and tile IDs
            tileInfos[i].boardID = subBoardID;
            tileInfos[i].tileID = i;
            subBoardState = Enumerable.Repeat(NO_OWNER, 9).ToList();

            // Initialize the dictionary
            var key = (subBoardID, i);
            
            if (!boardInfo.ContainsKey(key))
            {
                boardInfo.Add(key, tileInfos[i]);
            }
        }
    }

    /// <summary>
    /// Updat the winner of this sub board
    /// </summary>
    /// <param name="tileID"></param>
    /// <param name="playerTurn"></param>
    public void UpdateBoardSate(int tileID, int playerTurn)
    {
        subBoardState[tileID] = playerTurn;
    }
}