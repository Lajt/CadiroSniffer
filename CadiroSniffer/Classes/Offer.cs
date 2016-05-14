﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadiroSniffer.Classes
{
    public class Offer
    {
        public DateTime timeOfOffer { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public int qty { get; set; }
        public string itemName { get; set; }
        public int price { get; set; }
    }
}
