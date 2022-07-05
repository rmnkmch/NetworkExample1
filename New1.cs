using System.Collections.Generic;

namespace LTTDIT.Net
{
    public static class Information
    {
        public enum CommandFlags
        {
            StartHeaderFlag,
            EndHeaderFlag,
            ApplicationFlag,
            AvailableFlag,
            StartInfoFlag,
            EndInfoFlag,
            IpAddressFlag,
            Divider,
            NicknameFlag,
            DataFlag,
            StartDataFlag,
            EndDataFlag,
            InvitationAcceptedFlag,
            InvitationRefusedFlag,
            InvitationFlag,
            RequestFlag,
        }

        public static readonly Dictionary<CommandFlags, string> StringCommands = new Dictionary<CommandFlags, string>()
        {
            [CommandFlags.StartHeaderFlag] = "<header>",
            [CommandFlags.EndHeaderFlag] = "</header>",
            [CommandFlags.ApplicationFlag] = "ApplicationFlag",
            [CommandFlags.AvailableFlag] = "<AvailableFlag>",
            [CommandFlags.StartInfoFlag] = "<info>",
            [CommandFlags.EndInfoFlag] = "</info>",
            [CommandFlags.IpAddressFlag] = "IpAddressFlag",
            [CommandFlags.Divider] = "<_!_>",
            [CommandFlags.NicknameFlag] = "NicknameFlag",
            [CommandFlags.DataFlag] = "DataFlag",
            [CommandFlags.StartDataFlag] = "<data>",
            [CommandFlags.EndDataFlag] = "</data>",
            [CommandFlags.InvitationAcceptedFlag] = "<InvitationAcceptedFlag>",
            [CommandFlags.InvitationRefusedFlag] = "<InvitationRefusedFlag>",
            [CommandFlags.InvitationFlag] = "<InvitationFlag>",
            [CommandFlags.RequestFlag] = "RequestFlag",
        };

        public enum Applications
        {
            ApplicationError,
            Chat,
            TicTacToe,
        }

        public static readonly Dictionary<Applications, string> StringApplications = new Dictionary<Applications, string>()
        {
            [Applications.ApplicationError] = "ApplicationError!",
            [Applications.Chat] = "Chat",
            [Applications.TicTacToe] = "TicTacToe",
        };

        public enum TransceiverData
        {
            DataError,
            TicTacToePosX,
            TicTacToePosY,
            TurnWasMade,
            ChatMessage,
            TicTacToeBoardSize,
            TicTacToeWinSize,
            MyNickname,
            OtherNickname,
        }

        private static readonly Dictionary<TransceiverData, string> StringTypesOfData = new Dictionary<TransceiverData, string>()
        {
            [TransceiverData.DataError] = "DataError",
            [TransceiverData.TicTacToePosX] = "TicTacToePosX",
            [TransceiverData.TicTacToePosY] = "TicTacToePosY",
            [TransceiverData.TurnWasMade] = "TurnWasMade",
            [TransceiverData.ChatMessage] = "ChatMessage",
            [TransceiverData.TicTacToeBoardSize] = "TicTacToeBoardSize",
            [TransceiverData.TicTacToeWinSize] = "TicTacToeWinSize",
            [TransceiverData.MyNickname] = "MyNickname",
            [TransceiverData.OtherNickname] = "OtherNickname",

        };

        public static List<string> GetDividedCommands(string message)
        {
            List<string> retList = new List<string>();
            string currentCommand = string.Empty;
            string dividerCommand = string.Empty;
            int dividerSymbols = 0;
            int symbol_number = 0;
            while (symbol_number < message.Length)
            {
                if (message[symbol_number] == GetStringCommand(CommandFlags.Divider)[dividerSymbols])
                {
                    dividerCommand += message[symbol_number];
                    dividerSymbols++;
                }
                else
                {
                    if (dividerCommand.Length > 0)
                    {
                        currentCommand += dividerCommand;
                        dividerCommand = string.Empty;
                        dividerSymbols = 0;
                        currentCommand += message[symbol_number];
                    }
                    else currentCommand += message[symbol_number];
                }
                symbol_number++;
                if (dividerCommand == GetDividerCommand())
                {
                    retList.Add(currentCommand);
                    dividerCommand = string.Empty;
                    currentCommand = string.Empty;
                    dividerSymbols = 0;
                }
            }
            retList.Add(currentCommand);
            return retList;
        }

        private static string GetStringCommand(CommandFlags command)
        {
            return StringCommands[command];
        }

        private static string SetIpAddressCommand(string ipAddress)
        {
            return SetHeader(GetStringCommand(CommandFlags.IpAddressFlag)) + SetInfo(ipAddress);
        }

        public static bool IsIpAddress(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.IpAddressFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndInfoFlag));
        }

        public static string GetIpAddress(string ip)
        {
            string retIP = string.Empty;
            int ii = (SetHeader(GetStringCommand(CommandFlags.IpAddressFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)).Length;
            while (ii < ip.Length - GetStringCommand(CommandFlags.EndInfoFlag).Length)
            {
                retIP += ip[ii];
                ii++;
            }
            return retIP;
        }

        public static string SetNicknameCommand(string nickname)
        {
            return SetHeader(GetStringCommand(CommandFlags.NicknameFlag)) + SetInfo(nickname);
        }

        public static bool IsNickname(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.NicknameFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndInfoFlag));
        }

        public static string GetNickname(string command)
        {
            string ret = string.Empty;
            int symbolNumber = (SetHeader(GetStringCommand(CommandFlags.NicknameFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)).Length;
            while (symbolNumber < command.Length - GetStringCommand(CommandFlags.EndInfoFlag).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return ret;
        }

        private static string SetApplicationCommand(Applications application)
        {
            return SetHeader(GetStringCommand(CommandFlags.ApplicationFlag)) + SetInfo(StringApplications[application]);
        }

        public static bool IsApplication(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.ApplicationFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndInfoFlag));
        }

        public static Applications GetApplication(string command)
        {
            string ret = string.Empty;
            int symbolNumber = (SetHeader(GetStringCommand(CommandFlags.ApplicationFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)).Length;
            while (symbolNumber < command.Length - GetStringCommand(CommandFlags.EndInfoFlag).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return GetApplicationByString(ret);
        }

        private static Applications GetApplicationByString(string app)
        {
            foreach (Applications ap in StringApplications.Keys)
            {
                if (app == StringApplications[ap]) return ap;
            }
            return Applications.ApplicationError;
        }

        public static string SetDataCommand(TransceiverData typeOfData, string data)
        {
            return SetHeader(GetStringCommand(CommandFlags.DataFlag)) + SetTypeOfData(typeOfData) + SetData(data);
        }

        public static bool IsData(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.DataFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndDataFlag));
        }

        public static TransceiverData GetTypeOfData(string command)
        {
            string currentTypeOfData = string.Empty;
            string endInfoFlag = string.Empty;
            int endInfoFlagSymbols = 0;
            int symbol_number = (SetHeader(GetStringCommand(CommandFlags.DataFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)).Length;
            while (symbol_number < command.Length)
            {
                if (command[symbol_number] == GetStringCommand(CommandFlags.EndInfoFlag)[endInfoFlagSymbols])
                {
                    endInfoFlag += command[symbol_number];
                    endInfoFlagSymbols++;
                }
                else
                {
                    if (endInfoFlag.Length > 0)
                    {
                        currentTypeOfData += endInfoFlag;
                        endInfoFlag = string.Empty;
                        endInfoFlagSymbols = 0;
                        currentTypeOfData += command[symbol_number];
                    }
                    else currentTypeOfData += command[symbol_number];
                }
                symbol_number++;
                if (endInfoFlag == GetStringCommand(CommandFlags.EndInfoFlag))
                {
                    return GetTransceiverDataFromString(currentTypeOfData);
                }
            }
            return TransceiverData.DataError;
        }

        public static string GetData(string command, TransceiverData typeOfData)
        {
            string ret = string.Empty;
            int symbolNumber = (SetHeader(GetStringCommand(CommandFlags.DataFlag)) + SetTypeOfData(typeOfData) +
                GetStringCommand(CommandFlags.StartDataFlag)).Length;
            while (symbolNumber < command.Length - GetStringCommand(CommandFlags.EndDataFlag).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return ret;
        }

        private static string SetHeader(string toHeader)
        {
            return GetStringCommand(CommandFlags.StartHeaderFlag) + toHeader + GetStringCommand(CommandFlags.EndHeaderFlag);
        }

        private static string SetInfo(string toInfo)
        {
            return GetStringCommand(CommandFlags.StartInfoFlag) + toInfo + GetStringCommand(CommandFlags.EndInfoFlag);
        }

        private static string SetData(string toData)
        {
            return GetStringCommand(CommandFlags.StartDataFlag) + toData + GetStringCommand(CommandFlags.EndDataFlag);
        }

        private static string SetTypeOfData(TransceiverData typeOfData)
        {
            return SetInfo(StringTypesOfData[typeOfData]);
        }

        public static string SetJoinCommand(string ipAddress, Applications application, string nickname)
        {
            return SetIpAddressCommand(ipAddress) + GetDividerCommand() + SetApplicationCommand(application) +
                GetDividerCommand() + SetNicknameCommand(nickname);
        }

        public static string SetInviteMeCommand(string ipAddress, string nickname)
        {
            return SetIpAddressCommand(ipAddress) + GetDividerCommand() + SetNicknameCommand(nickname);
        }

        public static string SetChatMessageCommand(string nickname, string message)
        {
            return SetNicknameCommand(nickname) + GetDividerCommand() + SetDataCommand(TransceiverData.ChatMessage, message);
        }

        public static string GetAvailableCommand()
        {
            return GetStringCommand(CommandFlags.AvailableFlag);
        }

        public static bool IsAvailableCommand(string toCheck)
        {
            return toCheck == GetAvailableCommand();
        }

        public static string SetInvitationAcceptedCommand(string nickname)
        {
            return SetNicknameCommand(nickname) + GetDividerCommand() + GetInvitationAcceptedCommand();
        }

        private static string GetInvitationAcceptedCommand()
        {
            return GetStringCommand(CommandFlags.InvitationAcceptedFlag);
        }

        public static bool IsInvitationAcceptedCommand(string toCheck)
        {
            return toCheck == GetInvitationAcceptedCommand();
        }

        public static string SetInvitationRefusedCommand(string nickname)
        {
            return SetNicknameCommand(nickname) + GetDividerCommand() + GetInvitationRefusedCommand();
        }

        private static string GetInvitationRefusedCommand()
        {
            return GetStringCommand(CommandFlags.InvitationRefusedFlag);
        }

        public static bool IsInvitationRefusedCommand(string toCheck)
        {
            return toCheck == GetInvitationRefusedCommand();
        }

        public static string SetInvitationCommand(string nickname, Applications application)
        {
            return SetNicknameCommand(nickname) + GetDividerCommand() + SetApplicationCommand(application) + GetDividerCommand() + GetInvitationCommand();
        }

        private static string GetInvitationCommand()
        {
            return GetStringCommand(CommandFlags.InvitationFlag);
        }

        public static bool IsInvitationCommand(string toCheck)
        {
            return toCheck == GetInvitationCommand();
        }

        private static string GetDividerCommand()
        {
            return GetStringCommand(CommandFlags.Divider);
        }

        public static string SetTicTacToeTurnCommand(string posX, string posY)
        {
            return SetDataCommand(TransceiverData.TicTacToePosX, posX) + GetDividerCommand() + SetDataCommand(TransceiverData.TicTacToePosY, posY) +
                GetDividerCommand() + SetDataCommand(TransceiverData.TurnWasMade, "");
        }

        public static string SetRequestCommand(TransceiverData[] transceiverDatas)
        {
            string datas = string.Empty;
            for (int i = 0; i < transceiverDatas.Length; i++)
            {
                datas += SetHeader(GetStringCommand(CommandFlags.RequestFlag)) + SetTypeOfData(transceiverDatas[i]);
                if (i < transceiverDatas.Length - 1)
                {
                    datas += GetDividerCommand();
                }
            }
            return datas;
        }

        public static bool IsRequestCommand(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.RequestFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndInfoFlag));
        }

        public static TransceiverData GetTransceiverDataFromRequestCommand(string command)
        {
            string ret = string.Empty;
            int symbolNumber = (SetHeader(GetStringCommand(CommandFlags.RequestFlag)) + GetStringCommand(CommandFlags.StartInfoFlag)).Length;
            while (symbolNumber < command.Length - GetStringCommand(CommandFlags.EndInfoFlag).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return GetTransceiverDataFromString(ret);
        }

        private static TransceiverData GetTransceiverDataFromString(string _transceiverData)
        {
            foreach (TransceiverData transceiverData in StringTypesOfData.Keys)
            {
                if (_transceiverData == StringTypesOfData[transceiverData]) return transceiverData;
            }
            return TransceiverData.DataError;
        }

        public class Data
        {
            private readonly TransceiverData transceiverData;
            private readonly object data;

            public Data(TransceiverData _transceiverData, object _data)
            {
                transceiverData = _transceiverData;
                data = _data;
            }

            public TransceiverData GetTransceiverData()
            {
                return transceiverData;
            }

            public bool Is(TransceiverData _transceiverData)
            {
                return transceiverData.Equals(_transceiverData);
            }

            public object GetData()
            {
                return data;
            }
        }
    }
}
