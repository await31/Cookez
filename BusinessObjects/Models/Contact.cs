using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models {
    public partial class Contact {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
        public string Message { get; set; } = null!;

        public string? UserId { get; set; }
    }
}