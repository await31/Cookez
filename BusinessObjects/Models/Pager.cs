using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models {

    [NotMapped]
    public class Pager {
        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }

        public string IncludeList { get; private set; }
        public string ExcludeList { get; private set; }
        public string RecipeCategory { get; private set; }
        public string PrepTime { get; private set; }
        public string Difficulty { get; private set; }
        public string SortBy { get; private set; }
        public Pager() { }
        public Pager(int totalItems, int page, int pageSize) {

            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if (startPage <= 0) {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages) {
                endPage = totalPages;
                if (endPage > pageSize) {
                    startPage = endPage - (pageSize - 1);
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }

        public Pager(int totalItems, int page, int pageSize, string includeList, string excludeList, string recipeCategory, string prepTime, string difficulty, string sortBy) {

            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if (startPage <= 0) {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages) {
                endPage = totalPages;
                if (endPage > pageSize) {
                    startPage = endPage - (pageSize - 1);
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
            ExcludeList = excludeList;
            IncludeList = includeList;
            RecipeCategory = recipeCategory;
            Difficulty = difficulty;
            SortBy = sortBy;
            PrepTime = prepTime;
        }
    }
}
