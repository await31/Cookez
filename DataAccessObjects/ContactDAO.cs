using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class ContactDAO {
        private static ContactDAO instance = null;
        private static readonly object instacelock = new object();
        private ContactDAO() { }
        public static ContactDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new ContactDAO();
                    }
                    return instance;
                }
            }
        }

        public List<Contact> GetContacts() {
            List<Contact> list = new List<Contact>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    list = context.Contacts.ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return list;
        }

        public Contact GetContactById(int? id) {
            Contact c = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    c = context.Contacts.SingleOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return c;
        }

        public void InsertContact(Contact c) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetContactById(c.Id);
                    if (cus == null) {
                        context.Contacts.Add(c);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Contact is existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateContact(Contact c) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetContactById(c.Id);
                    if (cus != null) {
                        context.Contacts.Update(c);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Contact is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteContact(Contact c) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetContactById(c.Id);
                    if (cus != null) {
                        context.Contacts.Remove(c);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Contact is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
