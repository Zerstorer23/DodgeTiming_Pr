using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lex
{
    public class LexDebug
    {
        public static LexLogLevel LogLevel = LexLogLevel.Info;
        public static bool logIn = false;
        public static bool logOut = false;
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
        public static void LogIn(string msg)
        {
            if (logIn)
            {
                Debug.Log("보냄: <color=#00c800>" + msg + "</color>");
            }
        }
        public static void LogOut(string msg)
        {
            if (logOut)
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