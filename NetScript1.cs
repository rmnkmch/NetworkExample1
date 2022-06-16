using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Net
{
    public class NetScript1 : MonoBehaviour
    {
        [SerializeField] private Button buttonHost;
        [SerializeField] private Button buttonClient;
        [SerializeField] private Button buttonSend;
        [SerializeField] private Button buttonExit;

        [SerializeField] private InputField nicknameInput;

        [SerializeField] private Text textSend;
        [SerializeField] private ChatMessage chatMessagePrefab;
        [SerializeField] private RectTransform chatMessageTransform;

        private int messageNumber = 0;
        private delegate void Act();
        private Act act;
        private Act sendMessageAs;
        private Act avalCheck;
        private string _userName;

        private TcpListener tcpListener;
        private List<ClientObject> clientObjects = new List<ClientObject>();
        private float availableTime = 3f;

        private TcpClient cl_tcpClient;
        private NetworkStream cl_networkStream;

        private readonly IPAddress broadcastUDP = IPAddress.Parse("255.255.255.255");
        private const int broadcastUDPport = 55555;
        private const int listenerTCPport = 55556;

        private string receivedUDPMessage = string.Empty;
        private float timeToUDPSend = 0f;
        private UdpClient udpClient;
        private UdpClient udpServer;
        private System.Threading.Thread udpReceiveThread;
        private IPAddress receivedHostIP;
        private IPAddress myIPAddress;

        private void AddTCPConnection(ClientObject clientObject)
        {
            clientObjects.Add(clientObject);
        }

        private void RemoveTCPConnection(ClientObject clientObject)
        {
            if ((clientObject != null) && clientObjects.Contains(clientObject)) clientObjects.Remove(clientObject);
        }

        private void AvailableCheck()
        {
            avalCheck?.Invoke();
        }

        private void AvailableCheckServer()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= 3f)
            {
                BroadcastTCPMessage(Information.GetAvailableCommand(), string.Empty);
            }
        }

        private void AvailableCheckClient()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= 3f)
            {
                SendMessageAsClient(Information.GetAvailableCommand());
            }
        }

        private void BroadcastTCPMessage(string message, string id)
        {
            availableTime = 0f;
            ClientObject client = null;
            try
            {
                byte[] data = GetByteArrayFromString(message);
                for (int i = 0; i < clientObjects.Count; i++)
                {
                    client = clientObjects[i];
                    if (clientObjects[i].UID != id) clientObjects[i].UserStream.Write(data, 0, data.Length);
                }
            }
            catch (System.IO.IOException)
            {
                RemoveTCPConnection(client);
                client.Close();
                string messagex = string.Format("\"{0}\" покинул чат!", client.userName);
                ShowInfo(messagex);
                BroadcastTCPMessage(messagex, client.UID);
            }
            catch (Exception ex)
            {
                ShowInfo("BroadcastTCPMessage - " + ex.Message);
            }
        }

        private void BroadcastTCPResendA()
        {
            try
            {
                for (int i = 0; i < clientObjects.Count; i++)
                {
                    if (clientObjects[i].HasData())
                    {
                        string message = clientObjects[i].GetMessage();
                        if (Information.IsCommand(message)) return;
                        message = string.Format("{0}: {1}", clientObjects[i].userName, message);
                        ShowInfo(message);
                        BroadcastTCPMessage(message, clientObjects[i].UID);
                    }
                }
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("BroadcastTCPResendA - " + ex.Message);
            }
        }

        private void s_Disconnect()
        {
            RemAct(TCPListenA);
            RemAct(BroadcastTCPResendA);
            sendMessageAs = null;
            RemAct(AvailableCheck);
            avalCheck = null;
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }
            for (int i = 0; i < clientObjects.Count; i++)
            {
                clientObjects[i].Close();
            }
            clientObjects.Clear();
            EnableButtons();
        }

        private void TCPListenA()
        {
            try
            {
                if (tcpListener.Pending())
                {
                    TcpClient tcpClient1 = tcpListener.AcceptTcpClient();
                    ClientObject clientObject1 = new ClientObject(tcpClient1);
                    AddTCPConnection(clientObject1);
                    string helloMessage = string.Format("\"{0}\" вошёл в чат!", clientObject1.userName);
                    ShowInfo(helloMessage);
                    BroadcastTCPMessage(helloMessage, clientObject1.UID);
                }
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("TCPListenA - " + ex.Message);
            }
        }

        private void ShowInfo(string info)
        {
            Debug.Log(info);
            ChatMessage chatMessage1 = Instantiate(chatMessagePrefab, chatMessageTransform);
            chatMessage1.SetText(messageNumber.ToString() + ") " + info);
            messageNumber += 1;
        }

        public void Exitt()
        {
            s_Disconnect();
            c_Disconnect();
            StopBroadcastUDP();
            StopReceiveUDP();
        }

        private void Update()
        {
            act?.Invoke();
        }

        private void Start()
        {
            ShowHostIpMask();
        }

        private void ShowHostIpMask()
        {
            foreach (IPAddress ipm in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ipm.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    myIPAddress = ipm;
                    ShowInfo(ipm.ToString() + " - ip");
                }
            }
        }

        private void SetAct(Act act1)
        {
            act += act1;
        }

        private void RemAct(Act act1)
        {
            act -= act1;
        }

        private void ClearAct()
        {
            act = null;
        }

        private IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
        {
            int[] addressBits = new int[4];
            int[] maskBits = new int[4];
            string addressStr = address.ToString();
            string maskStr = mask.ToString();
            string retAddress = string.Empty;
            int numb = 0;
            for (int i = 0; i < addressStr.Length; i++)
            {
                if (addressStr[i] == '.')
                {
                    addressBits[numb] = int.Parse(retAddress);
                    retAddress = string.Empty;
                    numb++;
                }
                else retAddress += addressStr[i];
            }
            addressBits[numb] = int.Parse(retAddress);
            retAddress = string.Empty;
            numb = 0;
            for (int i = 0; i < maskStr.Length; i++)
            {
                if (maskStr[i] == '.')
                {
                    maskBits[numb] = int.Parse(retAddress);
                    retAddress = string.Empty;
                    numb++;
                }
                else retAddress += maskStr[i];
            }
            maskBits[numb] = int.Parse(retAddress);
            retAddress = string.Empty;
            for (int i = 0; i < addressBits.Length - 1; i++)
            {
                retAddress += GetBroadcastByte(maskBits[i], addressBits[i]).ToString();
                retAddress += ".";
            }
            retAddress += GetBroadcastByte(maskBits[addressBits.Length - 1], addressBits[addressBits.Length - 1]).ToString();
            return IPAddress.Parse(retAddress);
        }

        private int GetBroadcastByte(int maskByte, int addressByte)
        {
            if (maskByte == 255) return addressByte;
            else if (maskByte == 0) return 255;
            else
            {
                int[] bytes = new int[8] { 128, 64, 32, 16, 8, 4, 2, 1 };
                int currentByte = 0;
                int currentSteps = 0;
                if (maskByte == 128) currentSteps = 1;
                else if (maskByte == 192) currentSteps = 2;
                else if (maskByte == 224) currentSteps = 3;
                else if (maskByte == 240) currentSteps = 4;
                else if (maskByte == 248) currentSteps = 5;
                else if (maskByte == 252) currentSteps = 6;
                else if (maskByte == 254) currentSteps = 7;
                for (int i = 0; i < currentSteps; i++)
                {
                    if (addressByte > bytes[i])
                    {
                        currentByte += bytes[i];
                        addressByte -= bytes[i];
                    }
                }
                for (int i = 7; i >= currentSteps; i--)
                {
                    currentByte += bytes[i];
                }
                return currentByte;
            }
        }

        private void ReceiveUDPMessageA()
        {
            if (receivedUDPMessage.Length > 0)
            {
                ShowInfo(receivedUDPMessage);
                receivedUDPMessage = string.Empty;
            }
        }

        private void StartReceiveUDP()
        {
            if (!HasNickname()) return;
            try
            {
                DisableButtons();
                udpClient = new UdpClient(broadcastUDPport);
                ReceiveBugSolve();
                SetAct(ReceiveUDPMessageA);
                udpReceiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(ReceiveUDP));
                udpReceiveThread.Start();
            }
            catch (Exception ex)
            {
                StopReceiveUDP();
                ShowInfo("StartReceiveUDP - " + ex.Message);
            }
        }

        private void ReceiveBugSolve()
        {
            byte[] data = GetByteArrayFromString(Information.GetAvailableCommand());
            udpClient.Send(data, data.Length, new IPEndPoint(broadcastUDP, broadcastUDPport));
        }

        private void StopReceiveUDP(bool enableButtons = true)
        {
            RemAct(ReceiveUDPMessageA);
            ReceiveUDPMessageA();
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
            if ((udpReceiveThread != null) && udpReceiveThread.IsAlive)
            {
                udpReceiveThread.Interrupt();
                udpReceiveThread = null;
            }
            if (enableButtons) EnableButtons();
        }

        private void StopReceiveUDPFromMainThread()
        {
            RemAct(StopReceiveUDPFromMainThread);
            StopReceiveUDP(false);
        }

        private void ReceiveUDP()
        {
            IPEndPoint udpEndPointClient = null;
            while (true)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref udpEndPointClient);
                    string message = GetStringFromByteArray(data);
                    if (Information.IsIpAddress(message))
                    {
                        receivedHostIP = IPAddress.Parse(Information.GetIpAddress(message));
                        SetAct(ConnectTCPToHostFromMainThread);
                        break;
                    }
                    else ProcessReceivedUDPMessage(message);
                }
                catch (Exception ex)
                {
                    receivedUDPMessage = "ReceiveUDP - " + ex.Message;
                    SetAct(StopReceiveUDPFromMainThread);
                    break;
                }
            }
        }

        private void ConnectTCPToHostFromMainThread()
        {
            RemAct(ConnectTCPToHostFromMainThread);
            StartTCPClientProcess(receivedHostIP, listenerTCPport);
            SetAct(StopReceiveUDPFromMainThread);
        }

        private void ProcessReceivedUDPMessage(string message)
        {
            if (!Information.IsAvailableCommand(message))
            {
                receivedUDPMessage = "UDP <- " + message;
            }
        }

        private void StartBroadcastUDP()
        {
            if (!HasNickname()) return;
            try
            {
                udpServer = new UdpClient();
                udpServer.Connect(broadcastUDP, broadcastUDPport);
                SetAct(BroadcastUDP);
            }
            catch (Exception ex)
            {
                StopBroadcastUDP();
                ShowInfo("StartBroadcastUDP - " + ex.Message);
            }
        }

        private void StopBroadcastUDP()
        {
            RemAct(BroadcastUDP);
            if (udpServer != null)
            {
                udpServer.Close();
                udpServer = null;
            }
        }

        private void BroadcastUDP()
        {
            timeToUDPSend += Time.deltaTime;
            if (timeToUDPSend > 2f)
            {
                timeToUDPSend = 0f;
                try
                {
                    string message = Information.SetIpAddressToCommand(myIPAddress.ToString());
                    byte[] data = GetByteArrayFromString(message);
                    udpServer.Send(data, data.Length);
                }
                catch (Exception ex)
                {
                    StopBroadcastUDP();
                    ShowInfo("BroadcastUDP - " + ex.Message);
                }
            }
        }

        private static byte[] GetByteArrayFromString(string infoToEncode)
        {
            return Encoding.Unicode.GetBytes(infoToEncode);
        }

        private static string GetStringFromByteArray(byte[] infoToEncode)
        {
            return Encoding.Unicode.GetString(infoToEncode);
        }

        public void ServerChoosed()
        {
            StartTCPHostProcess();
            StartBroadcastUDP();
        }

        private void StartTCPHostProcess()
        {
            if (!HasNickname()) return;
            try
            {
                DisableButtons();
                tcpListener = new TcpListener(IPAddress.Any, listenerTCPport);
                tcpListener.Start();
                SetAct(TCPListenA);
                SetAct(BroadcastTCPResendA);
                sendMessageAs = SendMessageAsHost;
                SetAct(AvailableCheck);
                avalCheck = AvailableCheckServer;
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("StartTCPHostProcess - " + ex.Message);
            }
        }

        public void SendTCPMessage()
        {
            sendMessageAs?.Invoke();
        }

        private void SendMessageAsClient()
        {
            string message = textSend.text;
            SendMessageAsClient(message);
            ShowInfo("me: " + message);
        }

        private void SendMessageAsClient(string message)
        {
            try
            {
                if (message.Length < 1) return;
                availableTime = 0f;
                byte[] data = GetByteArrayFromString(message);
                cl_networkStream.Write(data, 0, data.Length);
            }
            catch (System.IO.IOException)
            {
                c_Disconnect();
                ShowInfo("Связь с хостом разорвана!");
            }
            catch (Exception ex)
            {
                c_Disconnect();
                ShowInfo("SendMessageAsClient - " + ex.Message);
            }
        }

        private void SendMessageAsHost()
        {
            string message = textSend.text;
            if (message.Length < 1) return;
            ShowInfo("me(host): " + message);
            message = string.Format("{0}(host): {1}", _userName, message);
            BroadcastTCPMessage(message, string.Empty);
        }

        private void ReceiveTCPMessageA()
        {
            try
            {
                if (cl_networkStream.DataAvailable)
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = cl_networkStream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (cl_networkStream.DataAvailable);
                    string message = builder.ToString();
                    if (!Information.IsCommand(message)) ShowInfo(message);
                }
            }
            catch (Exception ex)
            {
                c_Disconnect();
                ShowInfo("ReceiveTCPMessageA - " + ex.Message);
            }
        }

        private void c_Disconnect()
        {
            RemAct(ReceiveTCPMessageA);
            sendMessageAs = null;
            RemAct(AvailableCheck);
            avalCheck = null;
            if (cl_networkStream != null)
            {
                cl_networkStream.Close();
                cl_networkStream = null;
            }
            if (cl_tcpClient != null)
            {
                cl_tcpClient.Close();
                cl_tcpClient = null;
            }
            EnableButtons();
        }

        public void ClientChoosed()
        {
            StartReceiveUDP();
        }

        private void StartTCPClientProcess(IPAddress hostIP, int hostPort)
        {
            if (!HasNickname()) return;
            cl_tcpClient = new TcpClient();
            try
            {
                cl_tcpClient.Connect(hostIP, hostPort);
                cl_networkStream = cl_tcpClient.GetStream();
                SetAct(ReceiveTCPMessageA);
                sendMessageAs = SendMessageAsClient;
                string firstMessage = _userName;
                byte[] data = GetByteArrayFromString(firstMessage);
                cl_networkStream.Write(data, 0, data.Length);
                ShowInfo("Добро пожаловать, " + _userName);
                SetAct(AvailableCheck);
                avalCheck = AvailableCheckClient;
            }
            catch (Exception ex)
            {
                c_Disconnect();
                ShowInfo("StartTCPClientProcess - " + ex.Message);
            }
        }

        private void DisableButtons()
        {
            buttonHost.interactable = false;
            buttonClient.interactable = false;
            buttonSend.interactable = true;
            buttonExit.interactable = true;
            nicknameInput.interactable = false;
        }

        private void EnableButtons()
        {
            buttonHost.interactable = true;
            buttonClient.interactable = true;
            buttonSend.interactable = false;
            buttonExit.interactable = false;
            nicknameInput.interactable = true;
        }

        private bool HasNickname()
        {
            _userName = nicknameInput.textComponent.text;
            return _userName.Length > 2;
        }

        public class ClientObject
        {
            protected internal string UID { get; private set; }
            protected internal NetworkStream UserStream { get; private set; }
            protected internal readonly string userName;
            private readonly TcpClient userClient;

            public ClientObject(TcpClient tcpClient)
            {
                UID = Guid.NewGuid().ToString();
                userClient = tcpClient;
                UserStream = userClient.GetStream();
                userName = GetMessage();
            }

            protected internal bool HasData()
            {
                return UserStream.DataAvailable;
            }

            protected internal string GetMessage()
            {
                if (HasData())
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        int bytes = UserStream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (HasData());
                    return builder.ToString();
                }
                return string.Empty;
            }

            protected internal void Close()
            {
                if (UserStream != null) UserStream.Close();
                if (userClient != null) userClient.Close();
            }
        }
    }
}
