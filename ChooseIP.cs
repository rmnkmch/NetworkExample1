using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Net
{
    public class ChooseIP : MonoBehaviour
    {
        [SerializeField] private Text showableText;

        private NetScript1.CreateJoinButtonDelegate pressedWithData;

        private string ipAddress;
        private string nickname;
        private Information.Applications application;

        private const float LifeTime = 5f;
        private float currentLifeTime = 0f;

        public void SetJoinButton(string ip, string nick_name, Information.Applications app)
        {
            ipAddress = ip;
            nickname = nick_name;
            application = app;
            showableText.text = "Ip: " + ip + "   Nickname: " + nick_name + "   App: " + Information.StringApplications[app];
        }

        public void SetDelegateWithData(NetScript1.CreateJoinButtonDelegate _pressedWithData)
        {
            pressedWithData = _pressedWithData;
        }

        public void ButtonPressed()
        {
            pressedWithData?.Invoke(ipAddress, nickname, application);
        }

        public string GetIpAddress()
        {
            return ipAddress;
        }

        public Information.Applications GetApplication()
        {
            return application;
        }

        public void AddTime(float deltaTime)
        {
            currentLifeTime += deltaTime;
        }

        public bool IsGonnaDeleted()
        {
            return currentLifeTime >= LifeTime;
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void PacketReceived()
        {
            currentLifeTime = 0f;
        }
    }
}
