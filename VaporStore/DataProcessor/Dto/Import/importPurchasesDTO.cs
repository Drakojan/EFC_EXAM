using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text;
using System.Xml.Serialization;

namespace VaporStore.DataProcessor.Dto.Import
{
    [XmlType("Purchase")]
    public class importPurchasesDTO
    {
        [XmlAttribute("title")]
        public string GameName { get; set; }

        [XmlElement("Type")]
        [Required]
        public string Type { get; set; }

        [XmlElement("Key")]
        [Required]
        [RegularExpression(@"^([A-Z]|[\d]){4}-([A-Z]|[\d]){4}-([A-Z]|[\d]){4}$")]
        public string ProductKey { get; set; }

        [XmlElement("Date")]
        [Required]
        public string Date { get; set; }

        [XmlElement("Card")]
        [Required]
        [RegularExpression(@"^[\d]{4} [\d]{4} [\d]{4} [\d]{4}$")]
        public string CardNumber { get; set; }

    }
}
