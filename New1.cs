using System.Collections.Generic;

namespace LTTDIT.Net
{
    public static class Information
    {
        public enum Commands
        {
            StartCommandFlag,
            EndCommandFlag,
            StartHeaderFlag,
            EndHeaderFlag,
            VersionFlag,
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
        }

        public static readonly Dictionary<Commands, string> StringCommands = new Dictionary<Commands, string>()
        {
            [Commands.StartCommandFlag] = "<!>",
            [Commands.EndCommandFlag] = "</!>",
            [Commands.StartHeaderFlag] = "<header>",
            [Commands.EndHeaderFlag] = "</header>",
            [Commands.VersionFlag] = "Version",
            [Commands.ApplicationFlag] = "Application",
            [Commands.AvailableFlag] = "Available",
            [Commands.StartInfoFlag] = "<info>",
            [Commands.EndInfoFlag] = "</info>",
            [Commands.IpAddressFlag] = "IPAddress",
            [Commands.Divider] = "<_!_>",
            [Commands.NicknameFlag] = "Nickname",
            [Commands.DataFlag] = "Data",
            [Commands.StartDataFlag] = "<data>",
            [Commands.EndDataFlag] = "</data>",
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

        public enum TypesOfData
        {
            DataError,
            TicTacToePosX,
            TicTacToePosY,
            TurnWasMade,
        }

        public static readonly Dictionary<TypesOfData, string> StringTypesOfData = new Dictionary<TypesOfData, string>()
        {
            [TypesOfData.DataError] = "DataError",
            [TypesOfData.TicTacToePosX] = "TicTacToePosX",
            [TypesOfData.TicTacToePosY] = "TicTacToePosY",
            [TypesOfData.TurnWasMade] = "TurnWasMade",
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
                if (message[symbol_number] == GetStringCommand(Commands.Divider)[dividerSymbols])
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

        private static string GetStringCommand(Commands command)
        {
            return StringCommands[command];
        }

        public static bool IsCommand(string toCheck)
        {
            if (toCheck.Length < GetStringCommand(Commands.StartCommandFlag).Length + GetStringCommand(Commands.EndCommandFlag).Length) return false;
            if (toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndCommandFlag))) return true;
            return false;
        }

        public static bool IsIpAddress(string toCheck)
        {
            return toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.IpAddressFlag)) +
                GetStringCommand(Commands.StartInfoFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndInfoFlag) + GetStringCommand(Commands.EndCommandFlag));
        }

        public static string GetIpAddress(string ip)
        {
            string retIP = string.Empty;
            int ii = (GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.IpAddressFlag)) + GetStringCommand(Commands.StartInfoFlag)).Length;
            while (ii < ip.Length - (GetStringCommand(Commands.EndInfoFlag) + GetStringCommand(Commands.EndCommandFlag)).Length)
            {
                retIP += ip[ii];
                ii++;
            }
            return retIP;
        }

        public static bool IsNickname(string toCheck)
        {
            return toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.NicknameFlag)) +
                GetStringCommand(Commands.StartInfoFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndInfoFlag) + GetStringCommand(Commands.EndCommandFlag));
        }

        public static string GetNickname(string command)
        {
            string ret = string.Empty;
            int symbolNumber = (GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.NicknameFlag)) + GetStringCommand(Commands.StartInfoFlag)).Length;
            while (symbolNumber < command.Length - (GetStringCommand(Commands.EndInfoFlag) + GetStringCommand(Commands.EndCommandFlag)).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return ret;
        }

        public static bool IsApplication(string toCheck)
        {
            return toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.ApplicationFlag)) +
                GetStringCommand(Commands.StartInfoFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndInfoFlag) + GetStringCommand(Commands.EndCommandFlag));
        }

        public static Applications GetApplication(string command)
        {
            string ret = string.Empty;
            int symbolNumber = (GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.ApplicationFlag)) +
                GetStringCommand(Commands.StartInfoFlag)).Length;
            while (symbolNumber < command.Length - (GetStringCommand(Commands.EndInfoFlag) + GetStringCommand(Commands.EndCommandFlag)).Length)
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

        public static bool IsData(string toCheck)
        {
            return toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.DataFlag)) +
                GetStringCommand(Commands.StartInfoFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndDataFlag) + GetStringCommand(Commands.EndCommandFlag));
        }

        public static TypesOfData GetTypeOfData(string command)
        {
            string currentTypeOfData = string.Empty;
            string endInfoFlag = string.Empty;
            int endInfoFlagSymbols = 0;
            int symbol_number = (GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.DataFlag)) + GetStringCommand(Commands.StartInfoFlag)).Length;
            while (symbol_number < command.Length)
            {
                if (command[symbol_number] == GetStringCommand(Commands.EndInfoFlag)[endInfoFlagSymbols])
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
                if (endInfoFlag == GetStringCommand(Commands.EndInfoFlag))
                {
                    return GetTypeOfDataByString(currentTypeOfData);
                }
            }
            return TypesOfData.DataError;
        }

        private static TypesOfData GetTypeOfDataByString(string typeOfData)
        {
            foreach (TypesOfData type in StringTypesOfData.Keys)
            {
                if (typeOfData == StringTypesOfData[type]) return type;
            }
            return TypesOfData.DataError;
        }

        public static string GetData(string command, TypesOfData typeOfData)
        {
            string ret = string.Empty;
            int symbolNumber = (GetStringCommand(Commands.StartCommandFlag) + SetHeader(GetStringCommand(Commands.DataFlag)) + SetInfo(StringTypesOfData[typeOfData]) +
                GetStringCommand(Commands.StartDataFlag)).Length;
            while (symbolNumber < command.Length - (GetStringCommand(Commands.EndDataFlag) + GetStringCommand(Commands.EndCommandFlag)).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return ret;
        }

        private static string SetStringToCommand(string toCommand)
        {
            return GetStringCommand(Commands.StartCommandFlag) + toCommand + GetStringCommand(Commands.EndCommandFlag);
        }

        private static string SetHeader(string toHeader)
        {
            return GetStringCommand(Commands.StartHeaderFlag) + toHeader + GetStringCommand(Commands.EndHeaderFlag);
        }

        private static string SetInfo(string toInfo)
        {
            return GetStringCommand(Commands.StartInfoFlag) + toInfo + GetStringCommand(Commands.EndInfoFlag);
        }

        private static string SetData(string toData)
        {
            return GetStringCommand(Commands.StartDataFlag) + toData + GetStringCommand(Commands.EndDataFlag);
        }

        private static string SetTypeOfData(TypesOfData typeOfData)
        {
            return SetInfo(StringTypesOfData[typeOfData]);
        }

        public static string SetDataCommand(TypesOfData typeOfData, string data)
        {
            return SetStringToCommand(SetHeader(GetStringCommand(Commands.DataFlag)) + SetTypeOfData(typeOfData) + SetData(data));
        }

        public static string SetJoinCommand(string ipAddress, Applications application, string nickname)
        {
            return SetStringToCommand(SetHeader(GetStringCommand(Commands.IpAddressFlag)) + SetInfo(ipAddress)) +
                GetDividerCommand() + SetStringToCommand(SetHeader(GetStringCommand(Commands.ApplicationFlag)) + SetInfo(StringApplications[application])) +
                GetDividerCommand() + SetStringToCommand(SetHeader(GetStringCommand(Commands.NicknameFlag)) + SetInfo(nickname));
        }

        public static string GetAvailableCommand()
        {
            return SetStringToCommand(GetStringCommand(Commands.AvailableFlag));
        }

        public static bool IsAvailableCommand(string toCheck)
        {
            return toCheck == GetAvailableCommand();
        }

        public static string GetDividerCommand()
        {
            return GetStringCommand(Commands.Divider);
        }
    }
}
