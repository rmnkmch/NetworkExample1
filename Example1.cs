using UnityEngine;
using UnityEngine.SceneManagement;

namespace LTTDIT.Net
{
    public class Example1 : MonoBehaviour
    {
        private const int OpeningScene = 0;
        private const int ChatScene = 1;
        private const int TicTacToeScene = 2;

        private static void ChangeScene(int sceneId)
        {
            SceneManager.LoadScene(sceneId);
        }

        public static void LoadChatScene()
        {
            ChangeScene(ChatScene);
        }

        public static void LoadTicTacToeScene()
        {
            ChangeScene(TicTacToeScene);
        }

        public static void LoadOpeningScene()
        {
            ChangeScene(OpeningScene);
        }
    }
}
