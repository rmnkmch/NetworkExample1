using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LTTDIT.TicTacToe
{
    public class Board : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField] private BoardLine boardLinePrefab;
        [SerializeField] private StepXO stepXOPrefab;

        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Camera mCamera;

        private int sizeX = 0;
        private int sizeY = 0;
        private const int toWin = 3;

        private const float widthBoardLineRelative = 0.09f;
        private float widthHalfBoardLineAbsolute;
        private float proportionalRectWidth;
        private float proportionalRectHeight;
        private Vector2 centerPos;

        private Players currentPlayerTurn;
        private int currentX;
        private int currentY;
        private BoardLine currentLineX;
        private BoardLine currentLineY;

        private bool drawRedLines = false;

        private Dictionary<BoardPoint, StepXO> stepXOs = new Dictionary<BoardPoint, StepXO>();

        private readonly List<BoardPoint> boardPointsForIteratinos = new List<BoardPoint>()
        {
            new BoardPoint(1, 1),
            new BoardPoint(1, 0),
            new BoardPoint(1, -1),
            new BoardPoint(0, -1),
        };

        public enum Players
        {
            Nobody,
            Me,
            MyEnemy,
            Both,
            Draw,
            PlayerError,
        }

        public void SetSize(int width, int height)
        {
            if ((sizeX == 0) && (sizeY == 0))
            {
                sizeX = width;
                sizeY = height;
                DrawLines();
            }
        }

        public void SetSize(int width_height)
        {
            SetSize(width_height, width_height);
        }

        public void SetCamera(Camera _camera)
        {
            Vector2 UpperRightViewBorder = _camera.ViewportToScreenPoint(Vector3.one);
            float proportionX = UpperRightViewBorder.x / 1920f;
            proportionalRectWidth = rectTransform.rect.width * proportionX;
            proportionalRectHeight = rectTransform.rect.height * proportionX;
            centerPos = _camera.WorldToScreenPoint(rectTransform.position);
            centerPos -= new Vector2(proportionalRectWidth * 0.5f, proportionalRectHeight * 0.5f);
        }

        private void Start()
        {
            SetCamera(mCamera);
            SetSize(10);
            currentPlayerTurn = Players.Me;
        }

        private void DrawLines()
        {
            float boardWidth = rectTransform.rect.width;
            float boardHeight = rectTransform.rect.height;
            widthHalfBoardLineAbsolute = Mathf.Min(boardWidth / (sizeX - 1), boardHeight / (sizeY - 1));
            widthHalfBoardLineAbsolute *= widthBoardLineRelative / 2f;
            widthHalfBoardLineAbsolute = Mathf.Max(widthHalfBoardLineAbsolute, 2f);
            for (int i = 1; i < sizeX; i++)
            {
                BoardLine boardLine = Instantiate(boardLinePrefab, rectTransform);
                boardLine.SetOffsets(new Vector2(-(boardWidth / sizeX) * i + widthHalfBoardLineAbsolute, 0f),
                    new Vector2(boardWidth / sizeX * (sizeX - i) - widthHalfBoardLineAbsolute, 0f));
            }
            for (int i = 1; i < sizeY; i++)
            {
                BoardLine boardLine = Instantiate(boardLinePrefab, rectTransform);
                boardLine.SetOffsets(new Vector2(0f, -(boardHeight / sizeY) * i + widthHalfBoardLineAbsolute),
                    new Vector2(0f, boardHeight / sizeY * (sizeY - i) - widthHalfBoardLineAbsolute));
            }
        }

        private void DrawRedLineX()
        {
            DestroyLineX();
            currentLineX = Instantiate(boardLinePrefab, rectTransform);
            currentLineX.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.2f, 0.5f);
            currentLineX.SetOffsets(new Vector2(-(sizeX - currentX) * rectTransform.rect.width / sizeX - widthHalfBoardLineAbsolute, 0f),
                new Vector2((currentX - 1) * rectTransform.rect.width / sizeX + widthHalfBoardLineAbsolute, 0f));
        }

        private void DrawRedLineY()
        {
            DestroyLineY();
            currentLineY = Instantiate(boardLinePrefab, rectTransform);
            currentLineY.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.3f, 0.5f);
            currentLineY.SetOffsets(new Vector2(0f, -(sizeY - currentY) * rectTransform.rect.height / sizeY - widthHalfBoardLineAbsolute),
                new Vector2(0f, (currentY - 1) * rectTransform.rect.height / sizeY + widthHalfBoardLineAbsolute));
        }

        private void DestroyLineX()
        {
            if (currentLineX != null) Destroy(currentLineX.gameObject);
        }

        private void DestroyLineY()
        {
            if (currentLineY != null) Destroy(currentLineY.gameObject);
        }

        private int GetPosX(float pos_x)
        {
            if (pos_x < 0f) return 1;
            else if (pos_x > proportionalRectWidth) return sizeX;
            else return Mathf.CeilToInt(pos_x / proportionalRectWidth * sizeX);
        }

        private int GetPosY(float pos_y)
        {
            if (pos_y < 0f) return 1;
            else if (pos_y > proportionalRectHeight) return sizeY;
            else return Mathf.CeilToInt(pos_y / proportionalRectHeight * sizeY);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!drawRedLines) return;
            int newCurrentX = GetPosX((eventData.position - centerPos).x);
            int newCurrentY = GetPosY((eventData.position - centerPos).y);
            if (currentX != newCurrentX)
            {
                currentX = newCurrentX;
                DrawRedLineX();
            }
            if (currentY != newCurrentY)
            {
                currentY = newCurrentY;
                DrawRedLineY();
            }
            currentX = newCurrentX;
            currentY = newCurrentY;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (drawRedLines) MakeTurn(currentX, currentY, currentPlayerTurn);
            currentX = 0;
            currentY = 0;
            DestroyLineX();
            DestroyLineY();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            currentX = GetPosX((eventData.position - centerPos).x);
            currentY = GetPosY((eventData.position - centerPos).y);
            DrawRedLineX();
            DrawRedLineY();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            drawRedLines = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            drawRedLines = false;
            currentX = 0;
            currentY = 0;
            DestroyLineX();
            DestroyLineY();
        }

        public void ClearStepsXO()
        {
            foreach (StepXO xo in stepXOs.Values)
            {
                Destroy(xo.gameObject);
            }
            stepXOs.Clear();
        }

        private bool DictionaryContainsPoint(BoardPoint point)
        {
            foreach (BoardPoint boardPoint in stepXOs.Keys)
            {
                if (BoardPoint.IsPointsEquals(point, boardPoint)) return true;
            }
            return false;
        }

        private Players GetPlayerFromDictionary(BoardPoint point)
        {
            foreach (BoardPoint boardPoint in stepXOs.Keys)
            {
                if (BoardPoint.IsPointsEquals(point, boardPoint)) return stepXOs[boardPoint].GetBoardPlayer();
            }
            return Players.PlayerError;
        }

        private Players WhoWins()
        {
            if (stepXOs.Count < 2 * toWin - 1) return Players.Nobody;
            foreach (BoardPoint boardPoint in stepXOs.Keys)
            {
                Players playerPoint = stepXOs[boardPoint].GetBoardPlayer();
                foreach (BoardPoint pointIteration in boardPointsForIteratinos)
                {
                    int count = 1;
                    while (CheckToContinue(boardPoint + pointIteration * count, playerPoint))
                    {
                        count++;
                    }
                    if (count >= toWin) return playerPoint;
                }
            }
            if (stepXOs.Count == sizeX * sizeY) return Players.Draw;
            return Players.Nobody;
        }

        private bool CheckToContinue(BoardPoint boardPoint, Players player)
        {
            if ((boardPoint.Pos_X <= 0) || (boardPoint.Pos_Y <= 0) || (boardPoint.Pos_X > sizeX) || boardPoint.Pos_Y > sizeY) return false;
            if (DictionaryContainsPoint(boardPoint))
            {
                if (GetPlayerFromDictionary(boardPoint) == player) return true;
            }
            return false;
        }

        private void MakeTurn(int pos_x, int pos_y, Players player)
        {
            BoardPoint boardPoint = new BoardPoint(pos_x, pos_y);
            if (DictionaryContainsPoint(boardPoint)) return;
            StepXO stepXOl = Instantiate(stepXOPrefab, rectTransform);
            stepXOs.Add(boardPoint, stepXOl);
            stepXOl.SetOffsets(new Vector2(-(sizeX - pos_x) * rectTransform.rect.width / sizeX - 2f * widthHalfBoardLineAbsolute,
                -(sizeY - pos_y) * rectTransform.rect.height / sizeY - 2f * widthHalfBoardLineAbsolute),
                new Vector2((pos_x - 1) * rectTransform.rect.width / sizeX + 2f * widthHalfBoardLineAbsolute,
                (pos_y - 1) * rectTransform.rect.height / sizeY + 2f * widthHalfBoardLineAbsolute));
            if (player == Players.Me)
            {
                stepXOl.SetX(player);
                currentPlayerTurn = Players.MyEnemy;
            }
            else if (player == Players.MyEnemy)
            {
                stepXOl.SetO(player);
                currentPlayerTurn = Players.Me;
            }
            Players winner = WhoWins();
            if (winner == Players.Me) IWon();
            else if (winner == Players.MyEnemy) ILose();
            else if (winner == Players.Draw) Draw();
        }

        private void IWon()
        {
            ClearStepsXO();
        }

        private void ILose()
        {
            ClearStepsXO();
        }

        private void Draw()
        {
            ClearStepsXO();
        }
    }

    public class BoardPoint
    {
        public readonly int Pos_X;
        public readonly int Pos_Y;

        public BoardPoint(int pos_x, int pos_y)
        {
            Pos_X = pos_x;
            Pos_Y = pos_y;
        }

        public static bool IsPointsEquals(BoardPoint point1, BoardPoint point2)
        {
            return (point1.Pos_X == point2.Pos_X) && (point1.Pos_Y == point2.Pos_Y);
        }

        public static BoardPoint operator + (BoardPoint boardPoint1, BoardPoint boardPoint2)
        {
            return new BoardPoint(boardPoint1.Pos_X + boardPoint2.Pos_X, boardPoint1.Pos_Y + boardPoint2.Pos_Y);
        }

        public static BoardPoint operator * (BoardPoint boardPoint, int multiply)
        {
            return new BoardPoint(boardPoint.Pos_X * multiply, boardPoint.Pos_Y * multiply);
        }

        public override string ToString()
        {
            return "(" + Pos_X.ToString() + ", " + Pos_Y.ToString() + ")";
        }
    }
}
