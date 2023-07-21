using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories {
    public interface IContactRepository {
        IEnumerable<Contact> GetContacts();
        Contact GetContactById(int? id);
        void InsertContact(Contact c);
        void DeleteContact(Contact c);
        void UpdateContact(Contact c);
    }
}
