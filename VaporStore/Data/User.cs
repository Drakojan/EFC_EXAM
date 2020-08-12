using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.Data
{
    public class User
    {
        public User()
        {
            this.Cards = new HashSet<Card>();
        }
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        public string Username { get; set; }

        [RegularExpression(@"^[A-Z][a-z]+ [A-Z][a-z]+$")]
        [Required]
        public string FullName { get; set; }

        [EmailAddress] //not sure if required
        [Required]
        public string Email { get; set; }

        [Required]
        [Range(3,103)]
        public int Age { get; set; }

        public virtual ICollection<Card> Cards { get; set; }
    }
}
