using UnityEngine;

namespace LTTDIT.TicTacToe
{
    public class BoardLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;

        public void Set(Vector2 upperRight, Vector2 bottomLeft)
        {
            rectTransform.offsetMax = upperRight;
            rectTransform.offsetMin = bottomLeft;
        }
    }
}
