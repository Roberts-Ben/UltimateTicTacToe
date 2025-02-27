using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private readonly float startAnimTime = 1f;
    private readonly float placeAnimTime = 0.5f;
    private readonly float turnIconAnimTime = 1f;
    private readonly float endBannerAnimTime = 2f;
    private readonly float resetAnimTime = 0.2f;

    public List<RectTransform> playerTurnIcons;
    public GameObject gameButtons;

    public Sprite[] pieceSprites;

    public GameObject endGameBanner; // References for victory UI
    public TMP_Text endGameText;
    public Image endGameImage;

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

    public void StartGame()
    {
        for (int i = 0; i < BoardManager.instance.boardState.Count; i++)
        {
            SubBoard subBoard = BoardManager.instance.boardState[i];
            // Clear sub grids
            foreach (var tile in subBoard.boardInfo.Values)
            {
                tile.rectTransform.transform.localScale = Vector3.zero;
                tile.rectTransform.DOScale(Vector3.one, startAnimTime);
            }
        }
    }

    public void PlaceTile(int playerTurn, TileInfo tileInfo)
    {
        tileInfo.rectTransform.transform.localScale = Vector3.zero;
        tileInfo.rectTransform.DOScale(Vector3.one, placeAnimTime); // Animate in the new tile piece

        playerTurnIcons[playerTurn].DOScale(Vector3.zero, turnIconAnimTime);
        if (playerTurn == 0)
        {
            playerTurnIcons[1].DOScale(Vector3.one, turnIconAnimTime);
        }
        else
        {
            playerTurnIcons[0].DOScale(Vector3.one, turnIconAnimTime);
        }
    }

    public void UpdateSprite(int playerTurn, TileInfo tileInfo)
    {
        tileInfo.tileImage.sprite = pieceSprites[playerTurn];
    }

    public void CompleteBoard(TileInfo tileInfo, int playerTurn)
    {
        tileInfo.tileImage.sprite = pieceSprites[playerTurn];
        tileInfo.tileImage.color = Color.white;

        tileInfo.rectTransform.transform.localScale = Vector3.zero;
        tileInfo.rectTransform.DOScale(Vector3.one, placeAnimTime); // Animate in the new tile piece

        AudioManager.instance.PlayAudio("Subboard Won", false);
    }

    public void EndGame(int playerTurn, bool endedInTie)
    {
        gameButtons.SetActive(true);

        // Update end game UI elements based on the outcome
        if (endedInTie)
        {
            endGameText.text = "TIE!";
            endGameImage.color = Color.clear;
            AudioManager.instance.PlayAudio("Game Tied", false);
        }
        else
        {
            endGameText.text = "Player " + (playerTurn + 1) + " wins!";
            endGameImage.sprite = pieceSprites[playerTurn];
            endGameImage.color = Color.white;
            AudioManager.instance.PlayAudio("Game Won", false);
        }

        endGameBanner.GetComponent<RectTransform>().DOScale(Vector3.one, endBannerAnimTime); // Animate in the end game UI
        endGameBanner.GetComponent<Image>().DOFade(0.5f, endBannerAnimTime);
    }

    public void ResetUI()
    {
        DOTween.KillAll();
        endGameBanner.GetComponent<RectTransform>().DOScale(Vector3.zero, resetAnimTime);
        endGameBanner.GetComponent<Image>().DOFade(0f, resetAnimTime);

        gameButtons.SetActive(false);
    }
}
