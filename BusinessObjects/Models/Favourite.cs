using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models;

public partial class Favourite {

    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public bool? isPrivate { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();


}