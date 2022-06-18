using UnityEngine;

namespace LTTDIT.TicTacToe
{
    public class TicTacToeRules : MonoBehaviour
    {
        public void ExitRoom()
        {
            Net.NetScript1.instance.Exitt();
        }
    }
}
