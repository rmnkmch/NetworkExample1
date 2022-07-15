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
        [SerializeField] private Button enemyButton;

        [SerializeField] private GameObject ticTacToeSettingsPanel;

        [SerializeField] private Text boardSizeText;
        [SerializeField] private Text winSizeText;
        [SerializeField] private Text enemyText;

        private int boardSize = 3;
        private int winSize = 3;

        private const int MaxBoardSize = 11;
        private const int MinBoardSize = 3;

        private TicTacToeEnemies ticTacToeEnemy = TicTacToeEnemies.Bot;

        public enum TicTacToeEnemies
        {
            Bot,
            OneDevice,
            LocalNetwork,
            Multiplayer,
        }

        private void Start()
        {
            startButton.onClick.AddListener(StartButtonPressed);
            boardSizeButton.onClick.AddListener(ChangeBoardSize);
            winSizeButton.onClick.AddListener(ChangeWinSize);
            backButton.onClick.AddListener(BackButtonPressed);
            enemyButton.onClick.AddListener(ChangeEnemy);
            ShowSelectedBoardSize();
            ShowSelectedWinSize();
        }

        private void StartButtonPressed()
        {
            Net.NetScript1.instance.TicTacToeSelected(boardSize, winSize, ticTacToeEnemy);
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

        private void ChangeEnemy()
        {
            if (ticTacToeEnemy.Equals(TicTacToeEnemies.Bot)) ticTacToeEnemy = TicTacToeEnemies.OneDevice;
            else if (ticTacToeEnemy.Equals(TicTacToeEnemies.OneDevice)) ticTacToeEnemy = TicTacToeEnemies.LocalNetwork;
            else if (ticTacToeEnemy.Equals(TicTacToeEnemies.LocalNetwork)) ticTacToeEnemy = TicTacToeEnemies.Multiplayer;
            else if (ticTacToeEnemy.Equals(TicTacToeEnemies.Multiplayer)) ticTacToeEnemy = TicTacToeEnemies.Bot;
            ShowSelectedEnemy();
        }

        private void ShowSelectedBoardSize()
        {
            boardSizeText.text = boardSize.ToString() + " X " + boardSize.ToString();
        }

        private void ShowSelectedWinSize()
        {
            winSizeText.text = winSize.ToString();
        }

        private void ShowSelectedEnemy()
        {
            if (ticTacToeEnemy.Equals(TicTacToeEnemies.Bot)) enemyText.text = "Bot";
            else if (ticTacToeEnemy.Equals(TicTacToeEnemies.OneDevice)) enemyText.text = "OneDevice";
            else if (ticTacToeEnemy.Equals(TicTacToeEnemies.LocalNetwork)) enemyText.text = "LocalNetwork";
            else if (ticTacToeEnemy.Equals(TicTacToeEnemies.Multiplayer)) enemyText.text = "Multiplayer";
        }
    }
}
