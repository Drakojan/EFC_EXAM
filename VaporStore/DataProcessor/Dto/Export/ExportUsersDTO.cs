using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Xml.Serialization;

namespace VaporStore.DataProcessor.Dto.Export
{
    [XmlType("User")]
    public class ExportUsersDTO
    {
        [XmlAttribute("username")]
        public string username { get; set; }
        
        [XmlArray("Purchases")]
        public ExportPurchaseDTO[] Purchases { get; set; }

        [XmlElement("TotalSpent")]
        public string TotalSpent { get; set; }
    }
    [XmlType("Purchase")]
    public class ExportPurchaseDTO
    {
        [XmlElement("Card")]
        public string Card { get; set; }

        [XmlElement("Cvc")]
        public string Cvc { get; set; }

        [XmlElement("Date")]
        public string Date { get; set; }

        [XmlElement("Game")]
        public ExportGameDTO Game {get;set;}

    }
    [XmlType("Game")]
    public class ExportGameDTO
    {
        [XmlAttribute("title")]
        public string title { get; set; }

        [XmlElement("Genre")]
        public string Genre { get; set; }

        [XmlElement("Price")]
        public string Price { get; set; }
    }
}
