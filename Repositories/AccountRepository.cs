using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using DataAccessObjects;
namespace Repositories {
    public class AccountRepository : IAccountRepository {
        public IEnumerable<Account> GetAccounts() => AccountDAO.Instance.GetAccounts();
    }
}
