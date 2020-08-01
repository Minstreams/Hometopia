using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/TheMatrix/GameMessageSender")]
        public class GameMessageSender : MonoBehaviour
        {
            public GameMessage message;
            public bool sendOnStart;

            private void Start()
            {
                if (sendOnStart) SendGameMessage();
            }

            //Input
            public void SendGameMessage()
            {
                TheMatrix.SendGameMessage(message);
            }
        }
    }
}
