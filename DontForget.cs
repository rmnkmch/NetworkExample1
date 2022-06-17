
using UnityEngine;

public class DontForget : MonoBehaviour
{
    //private IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
    //{
    //    int[] addressBits = new int[4];
    //    int[] maskBits = new int[4];
    //    string addressStr = address.ToString();
    //    string maskStr = mask.ToString();
    //    string retAddress = string.Empty;
    //    int numb = 0;
    //    for (int i = 0; i < addressStr.Length; i++)
    //    {
    //        if (addressStr[i] == '.')
    //        {
    //            addressBits[numb] = int.Parse(retAddress);
    //            retAddress = string.Empty;
    //            numb++;
    //        }
    //        else retAddress += addressStr[i];
    //    }
    //    addressBits[numb] = int.Parse(retAddress);
    //    retAddress = string.Empty;
    //    numb = 0;
    //    for (int i = 0; i < maskStr.Length; i++)
    //    {
    //        if (maskStr[i] == '.')
    //        {
    //            maskBits[numb] = int.Parse(retAddress);
    //            retAddress = string.Empty;
    //            numb++;
    //        }
    //        else retAddress += maskStr[i];
    //    }
    //    maskBits[numb] = int.Parse(retAddress);
    //    retAddress = string.Empty;
    //    for (int i = 0; i < addressBits.Length - 1; i++)
    //    {
    //        retAddress += GetBroadcastByte(maskBits[i], addressBits[i]).ToString();
    //        retAddress += ".";
    //    }
    //    retAddress += GetBroadcastByte(maskBits[addressBits.Length - 1], addressBits[addressBits.Length - 1]).ToString();
    //    return IPAddress.Parse(retAddress);
    //}

    //private int GetBroadcastByte(int maskByte, int addressByte)
    //{
    //    if (maskByte == 255) return addressByte;
    //    else if (maskByte == 0) return 255;
    //    else
    //    {
    //        int[] bytes = new int[8] { 128, 64, 32, 16, 8, 4, 2, 1 };
    //        int currentByte = 0;
    //        int currentSteps = 0;
    //        if (maskByte == 128) currentSteps = 1;
    //        else if (maskByte == 192) currentSteps = 2;
    //        else if (maskByte == 224) currentSteps = 3;
    //        else if (maskByte == 240) currentSteps = 4;
    //        else if (maskByte == 248) currentSteps = 5;
    //        else if (maskByte == 252) currentSteps = 6;
    //        else if (maskByte == 254) currentSteps = 7;
    //        for (int i = 0; i < currentSteps; i++)
    //        {
    //            if (addressByte > bytes[i])
    //            {
    //                currentByte += bytes[i];
    //                addressByte -= bytes[i];
    //            }
    //        }
    //        for (int i = 7; i >= currentSteps; i--)
    //        {
    //            currentByte += bytes[i];
    //        }
    //        return currentByte;
    //    }
    //}
}
