using System;
using System.Collections.Generic;
using System.Text;

namespace VaporStore.Data
{
    public class GameTag
    {
        public int GameId { get; set; }

        public Game Game { get; set; }


        public int TagId { get; set; }

        public Tag Tag { get; set; }
    }
}
