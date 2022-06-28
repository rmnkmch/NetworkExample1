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

        public delegate void MakeTurnDelegate(int pos_x, int pos_y);
        private MakeTurnDelegate makeTurn;
        private delegate void WaitDelegate();
        private WaitDelegate wait;

        private const float WaitTime = 4f;
        private float currentWaitTime = 0f;

        private int sizeX = 0;
        private int sizeY = 0;
        private int toWin = 0;

        private const float widthBoardLineRelative = 0.09f;
        private float widthHalfBoardLineAbsolute;
        private float proportionalRectWidth;
        private float proportionalRectHeight;
        private Vector2 centerPos;

        private Players currentPlayerTurn = Players.PlayerError;
        private Players hostPlayer;
        private Players winner;
        private int currentX;
        private int currentY;
        private BoardLine currentLineX;
        private BoardLine currentLineY;

        private bool drawRedLines = false;

        private Dictionary<BoardPoint, StepXO> stepXOs = new Dictionary<BoardPoint, StepXO>();
        private List<BoardPoint> winOrLosePoints = new List<BoardPoint>();
        private List<BoardLine> winOrLoseLines = new List<BoardLine>();

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

        private void Update()
        {
            wait?.Invoke();
        }

        public void SetMakeTurnDelegate(MakeTurnDelegate makeTurnDelegate)
        {
            makeTurn = makeTurnDelegate;
        }

        public void SetMeAsFirstPlayerTurn()
        {
            currentPlayerTurn = Players.Me;
            hostPlayer = Players.Me;
        }

        public void SetMyEnemyAsFirstPlayerTurn()
        {
            currentPlayerTurn = Players.MyEnemy;
            hostPlayer = Players.MyEnemy;
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

        public void SetToWin(int _toWin)
        {
            toWin = _toWin;
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
            if (drawRedLines && (currentPlayerTurn == Players.Me) && !DictionaryContainsPoint(new BoardPoint(currentX, currentY)))
            {
                MakeTurn(currentX, currentY, currentPlayerTurn);
                makeTurn?.Invoke(currentX, currentY);
            }
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

        private void ClearStepsXO()
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
                    winOrLosePoints = new List<BoardPoint>() { boardPoint };
                    int count = 1;
                    while (CheckToContinue(boardPoint + pointIteration * count, playerPoint))
                    {
                        winOrLosePoints.Add(boardPoint + pointIteration * count);
                        count++;
                    }
                    if (count >= toWin) return playerPoint;
                }
            }
            winOrLosePoints.Clear();
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

        public void EnemyMadeMove(int pos_x, int pos_y)
        {
            MakeTurn(pos_x, pos_y, Players.MyEnemy);
        }

        private void MakeTurn(int pos_x, int pos_y, Players player)
        {
            StepXO stepXOl = Instantiate(stepXOPrefab, rectTransform);
            stepXOs.Add(new BoardPoint(pos_x, pos_y), stepXOl);
            stepXOl.SetOffsets(new Vector2(-(sizeX - pos_x) * rectTransform.rect.width / sizeX - 2f * widthHalfBoardLineAbsolute,
                -(sizeY - pos_y) * rectTransform.rect.height / sizeY - 2f * widthHalfBoardLineAbsolute),
                new Vector2((pos_x - 1) * rectTransform.rect.width / sizeX + 2f * widthHalfBoardLineAbsolute,
                (pos_y - 1) * rectTransform.rect.height / sizeY + 2f * widthHalfBoardLineAbsolute));
            if (player == hostPlayer) stepXOl.SetX(player);
            else stepXOl.SetO(player);
            CheckToWin();
        }

        private void ChangeCurrentPlayerTurn()
        {
            if (currentPlayerTurn == Players.MyEnemy) currentPlayerTurn = Players.Me;
            else if (currentPlayerTurn == Players.Me) currentPlayerTurn = Players.MyEnemy;
            else if (currentPlayerTurn == Players.PlayerError)
            {
                if (winner == Players.MyEnemy) currentPlayerTurn = Players.Me;
                else if (winner == Players.Me) currentPlayerTurn = Players.MyEnemy;
            }
        }

        private void CheckToWin()
        {
            winner = WhoWins();
            if (winner == Players.Me) IWon();
            else if (winner == Players.MyEnemy) ILose();
            else if (winner == Players.Draw) Draw();
            else if (winner == Players.Nobody) ChangeCurrentPlayerTurn();
        }

        private void ShowWinOrLosePointsAndStartWait()
        {
            foreach (BoardPoint point in winOrLosePoints)
            {
                ColorPoint(point);
            }
            winOrLosePoints.Clear();
            wait += LateClearAndChangePlayerTurn;
        }

        private void LateClearAndChangePlayerTurn()
        {
            currentWaitTime += Time.deltaTime;
            if (currentWaitTime >= WaitTime)
            {
                currentWaitTime = 0f;
                wait -= LateClearAndChangePlayerTurn;
                foreach (BoardLine line in winOrLoseLines)
                {
                    Destroy(line.gameObject);
                }
                winOrLoseLines.Clear();
                ClearStepsXO();
                ChangeCurrentPlayerTurn();
            }
        }

        private void ColorPoint(BoardPoint boardPoint)
        {
            BoardLine boardLine = Instantiate(boardLinePrefab, rectTransform);
            boardLine.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f, 0.7f);
            boardLine.SetOffsets(new Vector2(-(sizeX - boardPoint.Pos_X) * rectTransform.rect.width / sizeX - widthHalfBoardLineAbsolute,
                -(sizeY - boardPoint.Pos_Y) * rectTransform.rect.height / sizeY - widthHalfBoardLineAbsolute),
                new Vector2((boardPoint.Pos_X - 1) * rectTransform.rect.width / sizeX + widthHalfBoardLineAbsolute,
                (boardPoint.Pos_Y - 1) * rectTransform.rect.height / sizeY + widthHalfBoardLineAbsolute));
            winOrLoseLines.Add(boardLine);
        }

        private void IWon()
        {
            currentPlayerTurn = Players.PlayerError;
            ShowWinOrLosePointsAndStartWait();
        }

        private void ILose()
        {
            currentPlayerTurn = Players.PlayerError;
            ShowWinOrLosePointsAndStartWait();
        }

        private void Draw()
        {
            ShowWinOrLosePointsAndStartWait();
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
