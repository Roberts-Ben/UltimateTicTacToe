using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public List<SubBoard> boardState; // Owner of each of the final 9 grid spaces

    private const int NO_OWNER = -1;

    public int[,] winConditions =
    {
        {0,1,2}, // Top row
        {3,4,5}, // Mid row
        {6,7,8}, // Bottom row
        {0,3,6}, // Left column
        {1,4,7}, // Mid column
        {2,5,8}, // Right column
        {0,4,8}, // Diagonal down right
        {2,4,6} // Diagonal down left
    };

    public List<TileInfo> finalTiles; // Reference for final grid victor sprite

    public bool gameActive = false;

    public GameObject UIHolder; // Reference to the "Canvas" objec to populate the board data

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
    /// <summary>
    /// Initial board setup, cleanup the UI and set valid moves on the board
    /// </summary>
    public void SetupBoard()
    {
        boardState.Clear();
        foreach (SubBoard subBoard in UIHolder.GetComponentsInChildren<SubBoard>()) // Grab each of the 9 boards and initialise them
        {
            subBoard.SetupBoard();
            boardState.Add(subBoard);
        }

        ResetBoard();
        ResetValidMoveState(true);

        for (int i = 0; i < boardState.Count; i++) // Set all tiles to active for the first move
        {      
            foreach (TileInfo tileInfo in boardState[i].boardInfo.Values)
            {
                tileInfo.UpdateTileValidity(true);
            }
        }
    }
    /// <summary>
    /// Sets the owner of the tile that has been clicked, update the sprite and owner
    /// </summary>
    /// <param name="playerTurn"></param>
    /// <param name="subBoardID"></param>
    /// <param name="tileID"></param>
    public void UpdateTile(int playerTurn, int subBoardID, int tileID)
    {
        // Access the correct SubBoard from the list
        SubBoard subBoard = boardState[subBoardID];
        var key = (subBoardID, tileID); // Grab TileInfo from the SubBoard's dictionary

        if (subBoard.boardInfo.ContainsKey(key))
        {
            TileInfo tile = subBoard.boardInfo[key];

            // Update the tile state
            tile.Owner = playerTurn;
            tile.Occupied = true;
            UIManager.instance.UpdateSprite(playerTurn, tile);

            // Update the sub-board state within SubBoard
            subBoard.UpdateBoardSate(tileID, playerTurn);

            CheckBoard(playerTurn, subBoardID); // Check if this move completed a sub board or made it end in a tie
            SetNextValidMove(tileID); // Set the next batch of legal moves
        }
    }

    /// <summary>
    /// Set which tiles, in which sub grids, are now legal moves
    /// </summary>
    /// <param name="tile"></param>
    public void SetNextValidMove(int tile)
    {
        ResetValidMoveState(false);

        if (boardState[tile].finalBoardState != NO_OWNER) // Check to see if the proposed tile has already been won
        {
            for(int i = 0; i < boardState.Count;i++) // If it has, allow all non-completed grids to be valid moves
            {
                if (boardState[i].finalBoardState == NO_OWNER)
                {
                    foreach (TileInfo tileInfo in boardState[i].boardInfo.Values)
                    {
                        tileInfo.UpdateTileValidity(true);
                    }
                }
            }
        }
        else
        {
            if(boardState[tile].endedInTie) // Check if is has ended in a tie
            {
                for (int i = 0; i < boardState.Count; i++) // If it has, allow all non-completed grids to be valid moves
                {
                    if (boardState[i].finalBoardState == NO_OWNER)
                    {
                        foreach (TileInfo tileInfo in boardState[i].boardInfo.Values)
                        {
                            tileInfo.UpdateTileValidity(true);
                        }
                    }
                }
            }
            else
            {
                foreach (TileInfo tileInfo in boardState[tile].boardInfo.Values) // Otherwise we update the tiles in the intended sub grid
                {
                    tileInfo.UpdateTileValidity(true);
                }
            }
        }
    }
    /// <summary>
    /// Set tiles in all sub grids to prevent interaction (calcuate valid moves after)
    /// </summary>
    /// <param name="initialSetup"></param>
    void ResetValidMoveState(bool initialSetup)
    {
        for (int i = 0; i < boardState.Count; i++)
        {
            foreach (TileInfo tileInfo in boardState[i].boardInfo.Values)
            {
                tileInfo.UpdateTileValidity(initialSetup);
            }
        }
    }
    /// <summary>
    /// Check if any win con (in a subgrid) is fully met by one player
    /// </summary>
    /// <param name="playerTurn"></param>
    /// <param name="subBoardID"></param>
    public void CheckBoard(int playerTurn, int subBoardID)
    {

        for (int i = 0; i < winConditions.GetLength(0); i++) // Check the current sub board against all win conditions (3 in a row/column/diagonal)
        {
            if (boardState[subBoardID].subBoardState[winConditions[i, 0]] == playerTurn 
                && boardState[subBoardID].subBoardState[winConditions[i, 1]] == playerTurn 
                && boardState[subBoardID].subBoardState[winConditions[i, 2]] == playerTurn)
            {
                CompleteBoard(subBoardID, playerTurn); // If a player has won, mark the board as inaccessible here and store the winner
                CheckForTie(); // Then check if this was the last move, resulting in a tie
                return;
            }
        }

        // Check if the sub board ended in a draw by checking occupied state of all tiles
        SubBoard subBoard = BoardManager.instance.boardState[subBoardID];
        foreach (var tile in subBoard.boardInfo.Values)
        {
            if (!tile.Occupied)
            {
                return; // Return early if the sub board is still valid
            }
        }

        CompleteBoard(subBoardID, NO_OWNER); // If we reach this far, there are no more legal moves on this sub grid, resulting in a tie
        CheckForTie();
    }

    /// <summary>
    /// Set the final grid tile's owner based on the winner of that sub grid
    /// </summary>
    /// <param name="subBoardID"></param>
    /// <param name="playerTurn"></param>
    public void CompleteBoard(int subBoardID, int playerTurn)
    {
        // Prevent further moves on this tile
        foreach (TileInfo tileInfo in boardState[subBoardID].boardInfo.Values)
        {
            tileInfo.SetOwner(playerTurn);
            tileInfo.SetOccupied(true);
        }

        if (playerTurn == NO_OWNER) // If the board ended in a tie
        {
            boardState[subBoardID].endedInTie = true;
            return;
        }

        // Update the tile image to the winner
        UIManager.instance.CompleteBoard(finalTiles[subBoardID], playerTurn);
        boardState[subBoardID].finalBoardState = playerTurn;

        // If a player has won the correct sub grids to win the overall game
        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            // If any win con is met on the final board state
            if (boardState[winConditions[i, 0]].finalBoardState == playerTurn && boardState[winConditions[i, 1]].finalBoardState == playerTurn && boardState[winConditions[i, 2]].finalBoardState == playerTurn)
            {
                UIManager.instance.EndGame(playerTurn, false);
            }
        }
    }
    /// <summary>
    /// Check each subgrid to see if any moves are still valid or if the game has ended in a full tie
    /// </summary>
    private void CheckForTie()
    {
        int subBoardsEnded = 0;

        for (int i = 0; i < boardState.Count; i++)
        {
            if (boardState[i].endedInTie) // Check if all sub boards ended in a tie
            {
                subBoardsEnded++; // Mark 1 more sub board as ended
            }
            else if (boardState[i].finalBoardState != NO_OWNER) // Check if all tiles are owned
            {
                subBoardsEnded++;
            }
        }

        if (subBoardsEnded == boardState.Count) // If all sub boards have ended, we cna end in a tie
        {
            UIManager.instance.EndGame(NO_OWNER, true);
        }
    }

    /// <summary>
    /// Reset the state of all grid and sub grid tiles
    /// </summary>
    public void ResetBoard()
    {
        for (int i = 0; i < boardState.Count; i++)
        {
            // Clear final 3x3 grid
            finalTiles[i].tileImage.sprite = null;
            finalTiles[i].tileImage.color = Color.clear;

            boardState[i].endedInTie = false;
            boardState[i].finalBoardState = NO_OWNER;

            foreach (TileInfo tileInfo in boardState[i].boardInfo.Values)
            {
                UIManager.instance.UpdateSprite(2, tileInfo);
            }

            SubBoard subBoard = BoardManager.instance.boardState[i];
            subBoard.finalBoardState = NO_OWNER;

            // Clear sub grids
            foreach (var tile in subBoard.boardInfo.Values)
            {
                tile.button.interactable = true;
                tile.Owner = NO_OWNER;
                tile.Occupied = false;
            }
        }
        UIManager.instance.ResetUI();
    }
}
