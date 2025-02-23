using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    public List<SubBoard> boardState; // final 9 tile board

    public int[,] winConditions =
    {
        {0,1,2}, // Top row
        {3,4,5}, // Mid row
        {6,7,8}, // Bottom row
        {0,3,6}, // Left column
        {1,4,7}, // Mid column
        {2,5,6}, // right column
        {0,4,8}, // Diagonal down right
        {2,4,6} // Diagonal down left
    };

    public GameObject[] tiles; // Reference for tile victor sprite

    public Sprite[] pieceSprites;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateTile(int playerTurn, int subBoardID, int tile)
    {
        // Update the owner o the tile
        boardState[subBoardID].tiles[tile].GetComponent<TileInfo>().SetOwner(playerTurn);
        boardState[subBoardID].tiles[tile].GetComponent<Image>().sprite = pieceSprites[playerTurn];
        boardState[subBoardID].subBoardState[tile] = playerTurn;

        CheckBoard(playerTurn, subBoardID);
    }

    public void CheckBoard(int playerTurn, int subBoardID)
    {
        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            // If any win con is fully owned by one player
            if (boardState[subBoardID].subBoardState[winConditions[i, 0]] == playerTurn && boardState[subBoardID].subBoardState[winConditions[i, 1]] == playerTurn && boardState[subBoardID].subBoardState[winConditions[i, 2]] == playerTurn)
            {
                CompleteBoard(subBoardID, playerTurn);
            }
        }
    }

    public void CompleteBoard(int subBoardID, int playerTurn)
    {
        // Update the tile image to the winner
        tiles[subBoardID].GetComponent<Image>().sprite = pieceSprites[playerTurn];
        tiles[subBoardID].GetComponent<Image>().color = Color.white;
        boardState[subBoardID].boardState = playerTurn;

        // Prevent further moves on this tile
        foreach (GameObject go in boardState[subBoardID].tiles)
        {
            go.GetComponent<TileInfo>().occupied = true;
        }

        if (playerTurn == 1) // X wins
        {
            Debug.Log("Player X has won: " + subBoardID);
        }
        else 
        {
            Debug.Log("Player O has won: " + subBoardID);
        }

        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            // If any win con is met on the final board state
            if (boardState[winConditions[i, 0]].boardState == playerTurn && boardState[winConditions[i, 1]].boardState == playerTurn && boardState[winConditions[i, 2]].boardState == playerTurn)
            {
                EndGame(playerTurn);
            }
        }
    }

    public void EndGame(int playerTurn)
    {
        Debug.Log("Winner: " + playerTurn);
    }
}
[Serializable]
public class SubBoard
{
    public int boardID;
    public int boardState;

    public List<int> subBoardState;

    public GameObject[] tiles;
}
