using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace BusinessObjects.Models;

public partial class RecipeOrganizerContext : IdentityDbContext<Account> {
    public RecipeOrganizerContext() {
    }

    public RecipeOrganizerContext(DbContextOptions<RecipeOrganizerContext> options)
        : base(options) {
    }


    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IngredientCategory> IngredientCategories { get; set; }

    public virtual DbSet<MealPlan> MealPlans { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeCategory> RecipeCategories { get; set; }

    public virtual DbSet<RecipeFeedback> RecipeFeedbacks { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredient { get; set; }

    public virtual DbSet<Contact>? Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        // Create a configuration object
        var configuration = new ConfigurationBuilder()
          .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Set the base path for the appsettings.json file
          .AddJsonFile("appsettings.json") // Load the appsettings.json file
          .Build();

        // Get the connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        modelBuilder.Entity<Ingredient>()
        .Property(i => i.Id)
        .UseIdentityColumn()
        .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);


        base.OnModelCreating(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}