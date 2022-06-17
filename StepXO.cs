using UnityEngine;

namespace LTTDIT.TicTacToe
{
    public class StepXO : MonoBehaviour
    {
        [SerializeField] private Sprite spriteX;
        [SerializeField] private Sprite spriteO;
        [SerializeField] private RectTransform rectTransform;

        private Board.Players boardPlayer;

        public void SetX(Board.Players player)
        {
            boardPlayer = player;
            GetComponent<UnityEngine.UI.Image>().sprite = spriteX;
        }

        public void SetO(Board.Players player)
        {
            boardPlayer = player;
            GetComponent<UnityEngine.UI.Image>().sprite = spriteO;
        }

        public Board.Players GetBoardPlayer()
        {
            return boardPlayer;
        }

        public void SetOffsets(Vector2 upper_right, Vector2 bottom_left)
        {
            rectTransform.offsetMax = upper_right;
            rectTransform.offsetMin = bottom_left;
        }
    }
}
