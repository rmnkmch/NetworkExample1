using System;
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

        private int size_x = 0;
        private int size_y = 0;
        private const float sizeBoardLineRelative = 0.1f;
        private float sizeHalfBoardLineAbsolute;

        private int current_x;
        private int current_y;
        private BoardLine currentLine_x;
        private BoardLine currentLine_y;
        private bool drawLines = false;

        private const int toWin = 5;
        private Dictionary<BoardPoint, StepXO> stepXOs = new Dictionary<BoardPoint, StepXO>();
        private Players playersTurn;
        private readonly List<BoardPoint> boardPointsIteratinos = new List<BoardPoint>()
        {
            new BoardPoint(1, 1),
            new BoardPoint(1, 0),
            new BoardPoint(1, -1),
            new BoardPoint(0, -1),
        };

        private Vector2 centerPos;

        public enum Players
        {
            Nobody,
            Me,
            MyEnemy,
            Both,
            Draw,
        }

        public void SetSize(int width, int height)
        {
            if ((size_x == 0) && (size_y == 0))
            {
                size_x = width;
                size_y = height;
                DrawLines();
            }
        }

        public void SetSize(int width_height)
        {
            SetSize(width_height, width_height);
        }

        public void SetCamera(Camera camera)
        {
            centerPos = camera.WorldToScreenPoint(rectTransform.localPosition);
            centerPos -= new Vector2(rectTransform.rect.width * 0.5f, rectTransform.rect.height * 0.5f);
        }

        private void Start()
        {
            SetCamera(mCamera);
            SetSize(10);
            playersTurn = Players.Me;
        }

        private void DrawLines()
        {
            float boardWidth = rectTransform.rect.width;
            float boardHeight = rectTransform.rect.height;
            sizeHalfBoardLineAbsolute = Mathf.Min(boardWidth / (size_x - 1f), boardHeight / (size_y - 1f));
            sizeHalfBoardLineAbsolute *= sizeBoardLineRelative / 2f;
            sizeHalfBoardLineAbsolute = Mathf.Max(sizeHalfBoardLineAbsolute, 4f);
            for (int i = 1; i < size_x; i++)
            {
                BoardLine boardLine = Instantiate(boardLinePrefab, rectTransform);
                boardLine.Set(new Vector2(-(boardWidth / size_x) * i + sizeHalfBoardLineAbsolute, 0f),
                    new Vector2(boardWidth / size_x * (size_x - i) - sizeHalfBoardLineAbsolute, 0f));
            }
            for (int i = 1; i < size_y; i++)
            {
                BoardLine boardLine = Instantiate(boardLinePrefab, rectTransform);
                boardLine.Set(new Vector2(0f, -(boardHeight / size_y) * i + sizeHalfBoardLineAbsolute),
                    new Vector2(0f, boardHeight / size_y * (size_y - i) - sizeHalfBoardLineAbsolute));
            }
        }

        private void DrawRedLineX()
        {
            DestroyLineX();
            currentLine_x = Instantiate(boardLinePrefab, rectTransform);
            currentLine_x.GetComponent<Image>().color = new Color(0.8f, 0.25f, 0.2f, 0.5f);
            currentLine_x.Set(new Vector2(-(size_x - current_x) * rectTransform.rect.width / size_x - sizeHalfBoardLineAbsolute, 0f),
                new Vector2((current_x - 1) * rectTransform.rect.width / size_x + sizeHalfBoardLineAbsolute, 0f));
        }

        private void DrawRedLineY()
        {
            DestroyLineY();
            currentLine_y = Instantiate(boardLinePrefab, rectTransform);
            currentLine_y.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.25f, 0.5f);
            currentLine_y.Set(new Vector2(0f, -(size_y - current_y) * rectTransform.rect.height / size_y - sizeHalfBoardLineAbsolute),
                new Vector2(0f, (current_y - 1) * rectTransform.rect.height / size_y + sizeHalfBoardLineAbsolute));
        }

        private void DestroyLineX()
        {
            if (currentLine_x != null) Destroy(currentLine_x.gameObject);
        }

        private void DestroyLineY()
        {
            if (currentLine_y != null) Destroy(currentLine_y.gameObject);
        }

        private int GetPosX(float pos_x)
        {
            if (pos_x < 0f) return 1;
            else if (pos_x > rectTransform.rect.width) return size_x;
            else return Mathf.CeilToInt(pos_x / rectTransform.rect.width * size_x);
        }

        private int GetPosY(float pos_y)
        {
            if (pos_y < 0f) return 1;
            else if (pos_y > rectTransform.rect.height) return size_y;
            else return Mathf.CeilToInt(pos_y / rectTransform.rect.height * size_y);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!drawLines) return;
            if (current_x != GetPosX((eventData.position - centerPos).x))
            {
                current_x = GetPosX((eventData.position - centerPos).x);
                DrawRedLineX();
            }
            if (current_y != GetPosY((eventData.position - centerPos).y))
            {
                current_y = GetPosY((eventData.position - centerPos).y);
                DrawRedLineY();
            }
            current_x = GetPosX((eventData.position - centerPos).x);
            current_y = GetPosY((eventData.position - centerPos).y);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (drawLines) MakeTurn(current_x, current_y, playersTurn);
            current_x = 0;
            current_y = 0;
            DestroyLineX();
            DestroyLineY();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            current_x = GetPosX((eventData.position - centerPos).x);
            current_y = GetPosY((eventData.position - centerPos).y);
            DrawRedLineX();
            DrawRedLineY();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            drawLines = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            drawLines = false;
            current_x = 0;
            current_y = 0;
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
                if (BoardPoint.IsPointsEquals(point, boardPoint))
                {
                    return true;
                }
            }
            return false;
        }

        private Players GetPlayerFromDictionary(BoardPoint point)
        {
            foreach (BoardPoint boardPoint in stepXOs.Keys)
            {
                if (BoardPoint.IsPointsEquals(point, boardPoint)) return stepXOs[boardPoint].GetBoardPlayer();
            }
            throw new Exception("Dictionary does not contain point " + point.ToString());
        }

        private Players WhoWins()
        {
            if (stepXOs.Count < 2 * toWin - 1) return Players.Nobody;
            foreach (BoardPoint point in stepXOs.Keys)
            {
                Players playerPoint = stepXOs[point].GetBoardPlayer();
                foreach (BoardPoint pointIter in boardPointsIteratinos)
                {
                    int count = 1;
                    while (CheckToContinue(point + pointIter * count, playerPoint))
                    {
                        count++;
                    }
                    if (count >= toWin) return playerPoint;
                }
            }
            if (stepXOs.Count == size_x * size_y) return Players.Draw;
            return Players.Nobody;
        }

        private bool CheckToContinue(BoardPoint boardPoint, Players player)
        {
            if ((boardPoint.Pos_X <= 0) || (boardPoint.Pos_Y <= 0) || (boardPoint.Pos_X > size_x) || boardPoint.Pos_Y > size_y) return false;
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
            stepXOl.SetSize(new Vector2(-(size_x - pos_x) * rectTransform.rect.width / size_x - 2f * sizeHalfBoardLineAbsolute,
                -(size_y - pos_y) * rectTransform.rect.height / size_y - 2f * sizeHalfBoardLineAbsolute),
                new Vector2((pos_x - 1) * rectTransform.rect.width / size_x + 2f * sizeHalfBoardLineAbsolute,
                (pos_y - 1) * rectTransform.rect.height / size_y + 2f * sizeHalfBoardLineAbsolute));
            if (player == Players.Me)
            {
                stepXOl.SetX(player);
                playersTurn = Players.MyEnemy;
            }
            else if (player == Players.MyEnemy)
            {
                stepXOl.SetO(player);
                playersTurn = Players.Me;
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

        public  static BoardPoint operator +(BoardPoint boardPoint1, BoardPoint boardPoint2)
        {
            return new BoardPoint(boardPoint1.Pos_X + boardPoint2.Pos_X, boardPoint1.Pos_Y + boardPoint2.Pos_Y);
        }

        public static BoardPoint operator *(BoardPoint boardPoint, int multiply)
        {
            return new BoardPoint(boardPoint.Pos_X * multiply, boardPoint.Pos_Y * multiply);
        }

        public override string ToString()
        {
            return "(" + Pos_X.ToString() + ", " + Pos_Y.ToString() + ")";
        }
    }
}
