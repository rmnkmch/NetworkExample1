using UnityEngine;

namespace LTTDIT.TicTacToe
{
    public class BoardLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;

        public void SetOffsets(Vector2 upper_right, Vector2 bottom_left)
        {
            rectTransform.offsetMax = upper_right;
            rectTransform.offsetMin = bottom_left;
        }
    }
}
