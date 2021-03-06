using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lex
{
    public class LexDebug
    {
        public static LexLogLevel LogLevel = LexLogLevel.Info;
        public static bool logSend = false;
        public static bool logReceive = false;
        public static void Log(string msg)
        {
            if (LogLevel >= LexLogLevel.Info)
                Debug.Log("<color=#00c800>" + msg + "</color>");
        }
        public static void LogWarning(string msg)
        {

            if (LogLevel >= LexLogLevel.Warning)
                Debug.LogWarning("<color=#FF7000>" + msg + "</color>");
        }
        public static void LogError(string msg)
        {

            if (LogLevel >= LexLogLevel.Error)
                Debug.LogError("<color=#c80000>" + msg + "</color>");
        }
        public static void LogSend(string msg)
        {
            if (logSend)
            {
                Debug.Log("보냄: <color=#0000c8>" + msg + "</color>");
            }
        }
        public static void LogReceived(string msg)
        {
            if (logReceive)
            {
                Debug.Log("받음: <color=#FF7000>" + msg + "</color>");
            }
        }
    }
    public enum LexLogLevel
    {
        None, Error, Warning, Info
    }
}