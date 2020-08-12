using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VaporStore.Data.Models.Enums;

namespace VaporStore.Data
{
    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        public PurchaseType Type { get; set; }

        [Required]
        [RegularExpression(@"^([A-Z]|[\d]){4}-([A-Z]|[\d]){4}-([A-Z]|[\d]){4}$")]
        public string ProductKey { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int CardId { get; set; }
        [Required]
        public Card Card { get; set; }

        [Required]
        public int GameId { get; set; }
        [Required]
        public Game Game { get; set; }


    }
}
