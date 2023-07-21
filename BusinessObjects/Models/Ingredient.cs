using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models;

public partial class Ingredient {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ImgPath { get; set; }

    public string? Description { get; set; }

    public bool? Status { get; set; }

    [NotMapped]
    public IFormFile? file { get; set; }

    public int? FkCategoryId { get; set; }

    public virtual IngredientCategory? FkCategory { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual IngredientNutrition? IngredientNutrition { get; set; }
}

public partial class IngredientNutrition {

    [ForeignKey("Ingredient")]
    public int Id { get; set; }

    public double? Calories { get; set; }

    public double? Fat { get; set; }

    public double? Protein { get; set; }

    public double? Fibre { get; set; }

    public double? Carbohydrate { get; set; }

    public double? Cholesterol { get; set; }

    public virtual Ingredient? Ingredient { get; set; }
}




