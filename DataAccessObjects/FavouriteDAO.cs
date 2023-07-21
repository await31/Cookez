using BusinessObjects.Models;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class FavouriteDAO {
        private static FavouriteDAO instance = null;
        private static readonly object instacelock = new object();
        private FavouriteDAO() { }
        public static FavouriteDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new FavouriteDAO();
                    }
                    return instance;
                }
            }
        }

        public List<Favourite> GetFavourites() {
            List<Favourite> lists = new List<Favourite>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Favourites
                        .Include(a => a.Recipes)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Favourite> GetFavouritesIndex(Account acc) {
            List<Favourite> lists = new List<Favourite>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Accounts
                .Include(u => u.Favourites)
                .ThenInclude(f => f.Recipes)
                .FirstOrDefault(u => u.Id == acc.Id)
                .Favourites.ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Favourite> GetFavouritesUserProfile(Account acc) {
            List<Favourite> lists = new List<Favourite>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Favourites
                .Where(a => a.Account == acc)
                .Include(a => a.Recipes)
                .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public Favourite GetFavouritesDetails(int? id) {
            Favourite f = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    f = context.Favourites
                .Include(a => a.Account)
                .Include(a => a.Recipes)
                .ThenInclude(recipe => recipe.FkRecipeCategory)
                .FirstOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return f;
        }

        public Favourite GetFavouriteById(int? favouriteId) {
            using (var context = new RecipeOrganizerContext()) {
                return context.Favourites.Include(fav => fav.Recipes).FirstOrDefault(fav => fav.Id == favouriteId);
            }
        }

        public Recipe GetAllFavouriteRecipes(int recipeId, Account acc) {
            Recipe r = new Recipe();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    r = context.Favourites
                    .Where(a => a.Account == acc)
                    .Include(f => f.Recipes)
                    .SelectMany(c => c.Recipes)
                    .FirstOrDefault(f => f.Id == recipeId);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return r;
        }

        public Recipe GetAllFavouriteLists(int recipeId, int favouriteId, Account acc) {
            Recipe r = new Recipe();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    r = context.Favourites
                        .Where(a => a.Account == acc)
                        .Include(f => f.Recipes)
                        .FirstOrDefault(a => a.Id == favouriteId).Recipes
                        .FirstOrDefault(f => f.Id == recipeId);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return r;
        }

        public void InsertFavourite(Favourite f) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetFavouriteById(f.Id);
                    if (cus == null) {
                        // Attach the Account object to the context
                        context.Accounts.Attach(f.Account);
                        context.Favourites.Add(f);
                        context.SaveChanges();
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public void UpdateFavourite(int id, string name, string description, bool isPrivate) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = context.Favourites.FirstOrDefault(a=> a.Id == id);
                    if (cus != null) {
                        cus.Name = name;
                        cus.Description = description;
                        cus.isPrivate = isPrivate;
                        context.Favourites.Update(cus);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Favourite is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public void DeleteFavourite(Favourite f) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetFavouriteById(f.Id);
                    if (cus != null) {
                        context.Favourites.Remove(f);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Favourite is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
         public void InsertRecipeToFavourite(Favourite f, Recipe r) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var favourite = context.Favourites.Include(fav => fav.Recipes).FirstOrDefault(fav => fav.Id == f.Id);
                    var recipe = context.Recipes.FirstOrDefault(rec => rec.Id == r.Id);

                    if (recipe != null && favourite != null) {
                        if (!favourite.Recipes.Any(fr => fr.Id == recipe.Id)) {
                            favourite.Recipes.Add(recipe);
                            context.SaveChanges();
                        }
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public void DeleteRecipeFromFavourite(Favourite f, Recipe r) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var favourite = context.Favourites.Include(fav => fav.Recipes).FirstOrDefault(fav => fav.Id == f.Id);
                    var recipe = context.Recipes.FirstOrDefault(rec => rec.Id == r.Id);

                    if (recipe != null && favourite != null) {
                        if (favourite.Recipes.Any(fr => fr.Id == recipe.Id)) {
                            favourite.Recipes.Remove(recipe);
                            context.SaveChanges();
                        }
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.InnerException.Message);
            }
        }

    }
}
