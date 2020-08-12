using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.Data
{
    public class Card
    {
        public Card()
        {
            this.Purchases = new HashSet<Purchase>();
        }
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[\d]{4} [\d]{4} [\d]{4} [\d]{4}$")]
        public string Number { get; set; }

        [Required]
        [RegularExpression(@"^[\d]{3}$")]
        public string Cvc { get; set; }

        [Required]
        public CardType Type { get; set; }

        [Required]
        public int UserId { get; set; }
        [Required]
        public User User { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}
