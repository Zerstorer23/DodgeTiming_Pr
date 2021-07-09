using System;

namespace Lex
{
    public class Controller : MonoBehaviourLex
    {
        public ControllerType controllerType = ControllerType.Human;
        public string uid;
        LexPlayer uPlayer;
        public LexPlayer Owner
        {
            get => GetPlayer();
            private set => uPlayer = value;
        }



        public void SetControllerInfo(bool isBot, string uid)
        {
            controllerType = (isBot) ? ControllerType.Bot : ControllerType.Human;
            this.uid = uid;
            if (isBot)
            {
                Owner = LexNetwork.GetPlayerByID(uid);
            }
        }
        public void SetControllerInfo(string uid)
        {
            controllerType = ControllerType.Bot;
            this.uid = uid;
            Owner = LexNetwork.GetPlayerByID(uid);
        }

        internal void SetControllerInfo(LexPlayer owner)
        {
            controllerType = owner.IsHuman ? ControllerType.Human : ControllerType.Bot;
            this.uid = owner.uid;
            Owner = owner;
        }

        private LexPlayer GetPlayer()
        {
            if (IsBot)
            {
                return uPlayer;
            }
            else
            {
                return lexView.Owner;
            }
        }

        public bool IsLocal
        {
            get { return (lexView.IsMine && controllerType == ControllerType.Human); }
        }
        public bool IsBot
        {
            get { return controllerType == ControllerType.Bot; }
        }
        public bool IsMine
        {
            get => lexView.IsMine;
        }
        public bool Equals(string compareID) => this.uid == compareID;
        public bool Equals(Controller controller) => this.uid == controller.uid;
        public bool Equals(LexPlayer user) => this.uid == user.uid;



    }

    public enum ControllerType
    {
        Human, Bot
    }
}