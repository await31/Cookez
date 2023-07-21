using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class FavouriteRepository : IFavouriteRepository {
        public IEnumerable<Favourite> GetFavourites() => FavouriteDAO.Instance.GetFavourites();
        public IEnumerable<Favourite> GetFavouritesIndex(Account acc) => FavouriteDAO.Instance.GetFavouritesIndex(acc);
        public IEnumerable<Favourite> GetFavouritesUserProfile(Account acc) => FavouriteDAO.Instance.GetFavouritesUserProfile(acc);
        public Favourite GetFavouritesDetails(int? id) => FavouriteDAO.Instance.GetFavouritesDetails(id);
        public Recipe GetAllFavouriteRecipes(int recipeId, Account acc) => FavouriteDAO.Instance.GetAllFavouriteRecipes(recipeId, acc);
        public Recipe GetAllFavouriteLists(int recipeId, int favouriteId, Account acc) => FavouriteDAO.Instance.GetAllFavouriteLists(recipeId, favouriteId, acc);
        public Favourite GetFavouriteById(int? id) => FavouriteDAO.Instance.GetFavouriteById(id);
        public void InsertFavourite(Favourite f) => FavouriteDAO.Instance.InsertFavourite(f);
        public void DeleteFavourite(Favourite f) => FavouriteDAO.Instance.DeleteFavourite(f);
        public void UpdateFavourite(int id, string name, string description, bool isPrivate) => FavouriteDAO.Instance.UpdateFavourite(id,name,description,isPrivate);
        public void InsertRecipeToFavourite(Favourite f, Recipe r) => FavouriteDAO.Instance.InsertRecipeToFavourite(f,r);
        public void DeleteRecipeFromFavourite(Favourite f, Recipe r) => FavouriteDAO.Instance.DeleteRecipeFromFavourite(f,r);
    }
}
