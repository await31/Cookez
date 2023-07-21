using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class ContactRepository : IContactRepository {
        public IEnumerable<Contact> GetContacts() => ContactDAO.Instance.GetContacts();
        public Contact GetContactById(int? id) => ContactDAO.Instance.GetContactById(id);
        public void InsertContact(Contact c) => ContactDAO.Instance.InsertContact(c);
        public void DeleteContact(Contact c) => ContactDAO.Instance.DeleteContact(c);
        public void UpdateContact(Contact c) => ContactDAO.Instance.UpdateContact(c);

    }
}
