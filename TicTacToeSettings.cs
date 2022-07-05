using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.TicTacToe
{
    public class TicTacToeSettings : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button boardSizeButton;
        [SerializeField] private Button winSizeButton;
        [SerializeField] private Button backButton;

        [SerializeField] private GameObject ticTacToeSettingsPanel;

        [SerializeField] private Text boardSizeText;
        [SerializeField] private Text winSizeText;

        private int boardSize = 3;
        private int winSize = 3;

        private const int MaxBoardSize = 11;
        private const int MinBoardSize = 3;

        private void Start()
        {
            startButton.onClick.AddListener(StartButtonPressed);
            boardSizeButton.onClick.AddListener(ChangeBoardSize);
            winSizeButton.onClick.AddListener(ChangeWinSize);
            backButton.onClick.AddListener(BackButtonPressed);
            ShowSelectedBoardSize();
            ShowSelectedWinSize();
        }

        private void StartButtonPressed()
        {
            Net.NetScript1.instance.TicTacToeSelected(boardSize, winSize);
        }

        private void BackButtonPressed()
        {
            ticTacToeSettingsPanel.SetActive(false);
        }

        public void ActivatePanel()
        {
            ticTacToeSettingsPanel.SetActive(true);
        }

        private void ChangeBoardSize()
        {
            boardSize++;
            if (boardSize > MaxBoardSize)
            {
                boardSize = MinBoardSize;
                CorrectWinSize();
                ShowSelectedWinSize();
            }
            ShowSelectedBoardSize();
        }

        private void ChangeWinSize()
        {
            winSize++;
            CorrectWinSize();
            ShowSelectedWinSize();
        }

        private void CorrectWinSize()
        {
            if (winSize > boardSize)
            {
                winSize = MinBoardSize;
            }
        }

        private void ShowSelectedBoardSize()
        {
            boardSizeText.text = boardSize.ToString() + " X " + boardSize.ToString();
        }

        private void ShowSelectedWinSize()
        {
            winSizeText.text = winSize.ToString();
        }
    }
}
