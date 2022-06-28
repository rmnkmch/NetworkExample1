using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.TicTacToe
{
    public class TicTacToeRules : MonoBehaviour
    {
        [SerializeField] private Text myNickNameText;
        [SerializeField] private Text myEnemyNicknameText;

        [SerializeField] private GameObject myTurn;
        [SerializeField] private GameObject myEnemyTurn;

        [SerializeField] private Board gameBoard;
        [SerializeField] private Camera mCamera;

        private void Start()
        {
            myNickNameText.text = Net.NetScript1.instance.GetNickname();
            gameBoard.SetCamera(mCamera);
            gameBoard.SetSize(9);
            gameBoard.SetToWin(5);
            gameBoard.SetMakeTurnDelegate(MyTurnWasMade);
            Net.NetScript1.instance.SetEnemyMadeMoveDelegate(EnemyTurnWasMade);
            if (Net.NetScript1.instance.IsHost()) Net.NetScript1.instance.SetShowSecondPlayerNicknameAndSetTurnDelegate(ShowSecondPlayerNickname);
            else if (Net.NetScript1.instance.IsClient()) gameBoard.SetMyEnemyAsFirstPlayerTurn();
        }

        private void MyTurnWasMade(int pos_x, int pos_y)
        {
            myTurn.SetActive(false);
            myEnemyTurn.SetActive(true);
            string message = Net.Information.SetDataCommand(Net.Information.TypesOfData.TicTacToePosX, pos_x.ToString()) +
                Net.Information.GetDividerCommand() + Net.Information.SetDataCommand(Net.Information.TypesOfData.TicTacToePosY, pos_y.ToString()) +
                Net.Information.GetDividerCommand() + Net.Information.SetDataCommand(Net.Information.TypesOfData.TurnWasMade, "");
            if (Net.NetScript1.instance.IsHost()) Net.NetScript1.instance.SendXODataAsHost(message);
            else if (Net.NetScript1.instance.IsClient()) Net.NetScript1.instance.SendXODataAsClient(message);
        }

        private void EnemyTurnWasMade(int pos_x, int pos_y)
        {
            myTurn.SetActive(true);
            myEnemyTurn.SetActive(false);
            gameBoard.EnemyMadeMove(pos_x, pos_y);
        }

        private void ShowSecondPlayerNickname(string nick)
        {
            myEnemyNicknameText.text = nick;
            gameBoard.SetMeAsFirstPlayerTurn();
            myTurn.SetActive(true);
        }

        public void ExitRoom()
        {
            Net.NetScript1.instance.Exitt();
        }
    }
}
