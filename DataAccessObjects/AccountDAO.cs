using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class AccountDAO {
        private static AccountDAO instance = null;
        private static readonly object instacelock = new object();
        private AccountDAO() { }
        public static AccountDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new AccountDAO();
                    }
                    return instance;
                }
            }
        }

        public List<Account> GetAccounts() {
            List<Account> lists = new List<Account>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Accounts
                        .Include(a=>a.Recipes)
                        .Include(u=>u.Favourites)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }
    }
}
