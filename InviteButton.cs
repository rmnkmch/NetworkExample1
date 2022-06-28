using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Net
{
    public class InviteButton : MonoBehaviour
    {
        [SerializeField] private Text buttonText;

        private string ipAddress;
        private string nickname;
        private Information.Applications application;

        private NetScript1.CreateButtonDelegate buttonPressed;

        private const float LifeTime = NetScript1.BroadcastUdpCooldown * 3f;
        private float currentLifeTime = 0f;

        public void SetButton(string ip, string nick, Information.Applications app)
        {
            buttonText.text = "Ip: " + ip + ",   Nick: " + nick;
            ipAddress = ip;
            nickname = nick;
            application = app;
        }

        public void SetPressedDelegate(NetScript1.CreateButtonDelegate createButtonDelegate)
        {
            buttonPressed = createButtonDelegate;
        }

        public void ButtonPressed()
        {
            buttonPressed?.Invoke(ipAddress, nickname, application);
            buttonPressed = null;
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
