using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MulticastApp
{
    public class Example1 : MonoBehaviour
    {
        IPAddress remoteAddress; // хост для отправки данных
        const int remotePort = 8001; // порт для отправки данных
        const int localPort = 8001; // локальный порт для прослушивания входящих подключений
        string username;
        private UdpClient sender;
        private UdpClient receiver;

        public void CloseProcess()
        {
            sender.Close();
            receiver.Close();
        }
        public void StartProcess()
        {
            try
            {
                username = "comp11111";
                remoteAddress = IPAddress.Parse("235.5.5.11");
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                Thread sendThread = new Thread(new ThreadStart(SendMessage));
                sendThread.Start();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
        private void SendMessage()
        {
            sender = new UdpClient(); // создаем UdpClient для отправки
            IPEndPoint endPoint = new IPEndPoint(remoteAddress, remotePort);
            try
            {
                while (true)
                {
                    string message = "UDP hi 1111 "; // сообщение для отправки
                    message = String.Format("{0}: {1}", username, message);
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    sender.Send(data, data.Length, endPoint); // отправка
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }
        private void ReceiveMessage()
        {
            receiver = new UdpClient(localPort); // UdpClient для получения данных
            receiver.JoinMulticastGroup(remoteAddress, 20);
            IPEndPoint remoteIp = null;
            string localAddress = LocalIPAddress();
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    if (remoteIp.Address.ToString().Equals(localAddress))
                        continue;
                    string message = Encoding.Unicode.GetString(data);
                    Debug.Log(message);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }
        private string LocalIPAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    
}
}
