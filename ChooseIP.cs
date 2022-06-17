using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Net
{
    public class ChooseIP : MonoBehaviour
    {
        [SerializeField] private Text showableText;

        private string ipAddress;
        private string nickname;
        private Information.Applications application;

        public void SetJoinButton(string ip, string nick_name, Information.Applications app)
        {
            ipAddress = ip;
            nickname = nick_name;
            application = app;
            showableText.text = "Ip:  " + ip + "   Nickname:  " + nick_name + "   App:  " + app.ToString();
        }

        public void ButtonPressed()
        {
            NetScript1.instance.StartTCPClientProcess(ipAddress);
            NetScript1.instance.LoadChatScene();
        }
    }
}
