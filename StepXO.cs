using UnityEngine;

namespace LTTDIT.TicTacToe
{
    public class StepXO : MonoBehaviour
    {
        [SerializeField] private Sprite sprite_x;
        [SerializeField] private Sprite sprite_o;
        [SerializeField] private RectTransform rectTransform;

        private Board.Players boardplayer;

        public void SetX(Board.Players player)
        {
            boardplayer = player;
            GetComponent<UnityEngine.UI.Image>().sprite = sprite_x;
        }

        public void SetO(Board.Players player)
        {
            boardplayer = player;
            GetComponent<UnityEngine.UI.Image>().sprite = sprite_o;
        }

        public Board.Players GetBoardPlayer()
        {
            return boardplayer;
        }

        public void SetSize(Vector2 upperRight, Vector2 bottomLeft)
        {
            rectTransform.offsetMax = upperRight;
            rectTransform.offsetMin = bottomLeft;
        }
    }
}
