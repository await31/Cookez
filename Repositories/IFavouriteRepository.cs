using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public interface IFavouriteRepository {
        IEnumerable<Favourite> GetFavourites();
        IEnumerable<Favourite> GetFavouritesIndex(Account acc);
        IEnumerable<Favourite> GetFavouritesUserProfile(Account acc);
        Favourite GetFavouritesDetails(int? id);
        Recipe GetAllFavouriteRecipes(int recipeId, Account acc);
        Recipe GetAllFavouriteLists(int recipeId, int favouriteId, Account acc);
        Favourite GetFavouriteById(int? id);
        void InsertFavourite(Favourite f);
        void DeleteFavourite(Favourite f);
        void UpdateFavourite(int id, string name, string description, bool isPrivate);
        void InsertRecipeToFavourite(Favourite f, Recipe r);
        void DeleteRecipeFromFavourite(Favourite f, Recipe r);
    }
}
