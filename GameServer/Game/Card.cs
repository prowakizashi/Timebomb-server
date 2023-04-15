using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    class Card
    {
        public int Owner { get; set; }
        public CardType Type { get; private set; }

        public Card(CardType _type)
        {
            Type = _type;
            Owner = -1;
        }
    }
}
