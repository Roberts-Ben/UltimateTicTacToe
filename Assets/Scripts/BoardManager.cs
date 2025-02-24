using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[Serializable]
public class SubBoard // Each standard tic-tac-toe board within the larger 3x3 grid
{
    public int subBoardID; // Position in the grid, 0 = top left
    public int finalBoardState; // Who owns the board (-1 is default for no-one)

    public bool endedInTie;

    public List<int> subBoardState; // who owns each tile within this board

    public GameObject[] tiles; // Reference for each button/image on the board
}
public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public List<SubBoard> boardState; // Owner of each of the final 9 grid spaces

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

    public GameObject[] finalTiles; // Reference for final grid victor sprite
    public Sprite[] pieceSprites; // X and 0 sprites

    public GameObject endGameBanner; // References for victory UI
    public TMP_Text endGameText;
    public Image endGameImage;

    public List<GameObject> gameButtons;

    // TODO:
    // Handle full tie
    private void Awake()
    {
        instance = this;
    }
    /// <summary>
    /// Initial board setup, cleanup the UI and set valid moves on the board
    /// </summary>
    public void SetupBoard()
    {
        ResetBoard();
        foreach (GameObject go in gameButtons)
        {
            go.SetActive(false);
        }
        
        ResetValidMoveState(true);

        for (int i = 0; i < boardState.Count; i++)
        {
            foreach (GameObject go in boardState[i].tiles) //TODO: cleanup duplication of these
            {
                go.GetComponent<Button>().interactable = true;
                go.GetComponent<TileInfo>().SetValidMove(true);
                go.GetComponent<Image>().color = Color.green;
            }
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
        boardState[subBoardID].tiles[tile].GetComponent<TileInfo>().SetOwner(playerTurn);
        boardState[subBoardID].tiles[tile].GetComponent<Image>().sprite = pieceSprites[playerTurn];
        boardState[subBoardID].subBoardState[tile] = playerTurn;

        CheckBoard(playerTurn, subBoardID);

        SetNextValidMove(tile);
    }

    /// <summary>
    /// Set which tiles, in which sub grids, are now legal moves
    /// </summary>
    /// <param name="tile"></param>
    public void SetNextValidMove(int tile)
    {
        ResetValidMoveState(false);

        if (boardState[tile].finalBoardState != -1)// Check to see if the proposed tile has already been completed
        {
            for(int i = 0; i < boardState.Count;i++) // If it has, allow all non-completed grids to be valid moves
            {
                if (boardState[i].finalBoardState == -1)
                {
                    foreach (GameObject go in boardState[i].tiles) //TODO: cleanup duplication of these
                    {
                        go.GetComponent<TileInfo>().SetValidMove(true);
                        if (go.GetComponent<Image>().sprite != pieceSprites[0] && go.GetComponent<Image>().sprite != pieceSprites[1])
                        {
                            go.GetComponent<Image>().color = Color.green; // Ensure we only update valid tiles, not occupied ones
                        }
                    }
                }
            }
        }
        else
        {
            if(boardState[tile].endedInTie)
            {
                for (int i = 0; i < boardState.Count; i++) // If it has, allow all non-completed grids to be valid moves
                {
                    if (boardState[i].finalBoardState == -1)
                    {
                        foreach (GameObject go in boardState[i].tiles) //TODO: cleanup duplication of these
                        {
                            go.GetComponent<TileInfo>().SetValidMove(true);
                            if (go.GetComponent<Image>().sprite != pieceSprites[0] && go.GetComponent<Image>().sprite != pieceSprites[1])
                            {
                                go.GetComponent<Image>().color = Color.green; // Ensure we only update valid tiles, not occupied ones
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject go in boardState[tile].tiles) // Otherwise, we set the legal sub grid based on where the current piece was placed
                {
                    go.GetComponent<TileInfo>().SetValidMove(true);
                    if (go.GetComponent<Image>().sprite != pieceSprites[0] && go.GetComponent<Image>().sprite != pieceSprites[1])
                    {
                        go.GetComponent<Image>().color = Color.green;
                    }
                }
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
            foreach (GameObject go in boardState[i].tiles)
            {
                go.GetComponent<TileInfo>().SetValidMove(initialSetup);
                go.GetComponent<Image>().color = Color.white;
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
        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            if (boardState[subBoardID].subBoardState[winConditions[i, 0]] == playerTurn && boardState[subBoardID].subBoardState[winConditions[i, 1]] == playerTurn && boardState[subBoardID].subBoardState[winConditions[i, 2]] == playerTurn)
            {
                CompleteBoard(subBoardID, playerTurn);
                return;
            }
        }

        bool boardEndedIntTie = false;
        // Check if the sub board ended in a draw by checking occupied state of all tiles
        for (int i = 0; i < boardState[subBoardID].subBoardState.Count; i++)
        {
            if (boardState[subBoardID].tiles[i].GetComponent<TileInfo>().GetOccupied() == false)
            {
                boardEndedIntTie = false;
                break; // Return early if the sub board is still valid
            }
            boardEndedIntTie = true;
        }

        if(boardEndedIntTie)
        {
            boardState[subBoardID].endedInTie = true;
        }
    }

    /// <summary>
    /// Set the final grid tile's owner based on the winner of that sub grid
    /// </summary>
    /// <param name="subBoardID"></param>
    /// <param name="playerTurn"></param>
    public void CompleteBoard(int subBoardID, int playerTurn)
    {
        // Update the tile image to the winner
        finalTiles[subBoardID].GetComponent<Image>().sprite = pieceSprites[playerTurn];
        finalTiles[subBoardID].GetComponent<Image>().color = Color.white;
        boardState[subBoardID].finalBoardState = playerTurn;

        // Prevent further moves on this tile
        foreach (GameObject go in boardState[subBoardID].tiles)
        {
            go.GetComponent<TileInfo>().occupied = true;
        }

        // If a player has won the correct sub grids to win the overall game
        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            // If any win con is met on the final board state
            if (boardState[winConditions[i, 0]].finalBoardState == playerTurn && boardState[winConditions[i, 1]].finalBoardState == playerTurn && boardState[winConditions[i, 2]].finalBoardState == playerTurn)
            {
                EndGame(playerTurn);
            }
        }
    }

    /// <summary>
    /// Handle end game state, disable tiles, display vitory UI
    /// </summary>
    /// <param name="playerTurn"></param>
    public void EndGame(int playerTurn)
    {
        endGameText.text = "Player " + (playerTurn + 1);
        endGameImage.sprite = pieceSprites[playerTurn];
        endGameBanner.SetActive(true);

        foreach (GameObject go in gameButtons)
        {
            go.SetActive(true);
        }
        for (int i = 0; i < boardState.Count; i++)
        {
            foreach (GameObject go in boardState[i].tiles) //TODO: cleanup duplication of these
            {
                go.GetComponent<Button>().interactable = false;
                go.GetComponent<TileInfo>().SetValidMove(false);
                go.GetComponent<Image>().color = Color.white;
            }
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
            finalTiles[i].GetComponent<Image>().sprite = null;
            finalTiles[i].GetComponent<Image>().color = Color.clear;
            boardState[i].endedInTie = false;
            boardState[i].finalBoardState = -1;
            foreach (GameObject go in boardState[i].tiles)
            {
                go.GetComponent<Image>().sprite = pieceSprites[2];
            }

            // Clear sub grids
            for(int o = 0; o < boardState[i].subBoardState.Count; o++)
            {
                TileInfo tileInfo = boardState[i].tiles[o].GetComponent<TileInfo>();
                boardState[i].subBoardState[o] = -1;
                tileInfo.occupied = false;
                tileInfo.owner = -1;
            }
        }
        endGameBanner.SetActive(false);
    }
}
