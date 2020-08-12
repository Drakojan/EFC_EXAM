using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VaporStore.Data;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class importUsersDTO
    {
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
        [Range(3, 103)]
        public int Age { get; set; }

        public CardDTO[] Cards { get; set; }
    }

    public class CardDTO
    {
        [Required]
        [RegularExpression(@"^[\d]{4} [\d]{4} [\d]{4} [\d]{4}$")]
        public string Number { get; set; }

        [Required]
        [RegularExpression(@"^[\d]{3}$")]
        public string Cvc { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
