using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class SubBoard // Each standard tic-tac-toe board within the larger 3x3 grid
{
    public int subBoardID; // Position in the grid, 0 = top left
    public int finalBoardState; // Who owns the board (-1 is default for no-one)

    public bool endedInTie;
    public List<int> subBoardState; // Who owns each tile within this board

    public List<GameObject> tiles; // Reference for each button/image on the board
}
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

    public List<GameObject> finalTiles; // Reference for final grid victor sprite
    public Sprite[] pieceSprites; // X and 0 sprites

    public GameObject endGameBanner; // References for victory UI
    public TMP_Text endGameText;
    public Image endGameImage;

    public List<GameObject> gameButtons;

    public bool gameActive = false;

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
        ResetBoard();
        foreach (GameObject button in gameButtons)
        {
            button.SetActive(false);
        }
        
        ResetValidMoveState(true);

        for (int i = 0; i < boardState.Count; i++)
        {
            UpdateTileValidity(boardState[i].tiles, true);
        }
    }
    /// <summary>
    /// Sets the owner of the tile that has been clicked, update the sprite and owner
    /// </summary>
    /// <param name="playerTurn"></param>
    /// <param name="subBoardID"></param>
    /// <param name="tile"></param>
    public void UpdateTile(int playerTurn, int subBoardID, int tile)
    {
        TileInfo tileInfo = boardState[subBoardID].tiles[tile].GetComponent<TileInfo>();
        tileInfo.SetOwner(playerTurn);
        tileInfo.SetOccupied(true);

        boardState[subBoardID].tiles[tile].GetComponent<Image>().sprite = pieceSprites[playerTurn];
        boardState[subBoardID].subBoardState[tile] = playerTurn;

        CheckBoard(playerTurn, subBoardID);
        SetNextValidMove(tile);
    }

    /// <summary>
    /// Set tiles to be interactive or not
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="isValid"></param>
    void UpdateTileValidity(List<GameObject> tiles, bool isValid)
    {
        foreach (GameObject tile in tiles)
        {
            TileInfo tileInfo = tile.GetComponent<TileInfo>();
            Image tileImage = tile.GetComponent<Image>();

            if (!tileInfo.GetOccupied())
            {
                tileImage.color = isValid ? Color.green : Color.white;
            }
            else
            {
                tileImage.color = Color.white;
            }

            tileInfo.SetValidMove(isValid);
        }
    }

    /// <summary>
    /// Set which tiles, in which sub grids, are now legal moves
    /// </summary>
    /// <param name="tile"></param>
    public void SetNextValidMove(int tile)
    {
        ResetValidMoveState(false);

        if (boardState[tile].finalBoardState != NO_OWNER)// Check to see if the proposed tile has already been won
        {
            for(int i = 0; i < boardState.Count;i++) // If it has, allow all non-completed grids to be valid moves
            {
                if (boardState[i].finalBoardState == NO_OWNER)
                {
                    UpdateTileValidity(boardState[i].tiles, true);
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
                        UpdateTileValidity(boardState[i].tiles, true);
                    }
                }
            }
            else
            {
                UpdateTileValidity(boardState[tile].tiles, true); // Otherwise we update the tiles in the intended sub grid
            }
        }
    }
    /// <summary>
    /// Set tiles in all sub grids to prevent interaction (calcuate valid moves after)
    /// </summary>
    void ResetValidMoveState(bool initialSetup)
    {
        for (int i = 0; i < boardState.Count; i++)
        {
            UpdateTileValidity(boardState[i].tiles, initialSetup);
        }
    }
    /// <summary>
    /// Check if any win con (in a subgrid) is fully met by one player
    /// </summary>
    /// <param name="playerTurn"></param>
    /// <param name="subBoardID"></param>
    public void CheckBoard(int playerTurn, int subBoardID)
    {

        for (int i = 0; i < winConditions.GetLength(0); i++) // Check the current sub baord against all win conditions (3 in a row/column/diagonal)
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
        for (int i = 0; i < boardState[subBoardID].subBoardState.Count; i++)
        {
            if (boardState[subBoardID].tiles[i].GetComponent<TileInfo>().GetOccupied() == false)
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
        foreach (GameObject tile in boardState[subBoardID].tiles)
        {
            TileInfo tileInfo = tile.GetComponent<TileInfo>();
            tileInfo.SetOwner(playerTurn);
            tileInfo.SetOccupied(true);
        }

        if (playerTurn == NO_OWNER) // If the board ended in a tie
        {
            boardState[subBoardID].endedInTie = true;
            return;
        }

        // Update the tile image to the winner
        finalTiles[subBoardID].GetComponent<Image>().sprite = pieceSprites[playerTurn];
        finalTiles[subBoardID].GetComponent<Image>().color = Color.white;
        boardState[subBoardID].finalBoardState = playerTurn;

        // If a player has won the correct sub grids to win the overall game
        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            // If any win con is met on the final board state
            if (boardState[winConditions[i, 0]].finalBoardState == playerTurn && boardState[winConditions[i, 1]].finalBoardState == playerTurn && boardState[winConditions[i, 2]].finalBoardState == playerTurn)
            {
                EndGame(playerTurn, false);
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
            EndGame(NO_OWNER, true);
        }
    }

    /// <summary>
    /// Handle end game state, disable tiles, display vitory UI
    /// </summary>
    /// <param name="playerTurn"></param>
    public void EndGame(int playerTurn, bool gameEndedInTie)
    {
        if(gameEndedInTie)
        {
            endGameText.text = "TIE!";
            endGameImage.color = Color.clear;
        }
        else
        {
            endGameText.text = "Player " + (playerTurn + 1);
            endGameImage.sprite = pieceSprites[playerTurn];
            endGameImage.color = Color.white;
        }

        endGameBanner.SetActive(true);

        foreach (GameObject button in gameButtons)
        {
            button.SetActive(true);
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
            Image tileImage = finalTiles[i].GetComponent<Image>();
            tileImage.sprite = null;
            tileImage.color = Color.clear;
            boardState[i].endedInTie = false;
            boardState[i].finalBoardState = NO_OWNER;
            foreach (GameObject tile in boardState[i].tiles)
            {
                tile.GetComponent<Image>().sprite = pieceSprites[2];
            }

            // Clear sub grids
            for(int o = 0; o < boardState[i].subBoardState.Count; o++)
            {
                TileInfo tileInfo = boardState[i].tiles[o].GetComponent<TileInfo>();
                boardState[i].tiles[o].GetComponent<Button>().interactable = true;
                boardState[i].subBoardState[o] = NO_OWNER;
                tileInfo.SetOccupied(false);
                tileInfo.SetOwner(NO_OWNER);
            }
        }
        endGameBanner.SetActive(false);
    }
}
