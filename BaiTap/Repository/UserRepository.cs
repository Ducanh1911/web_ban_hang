using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaiTap.Repository
{
    public class UserRepository
    {
        private readonly ShopEntities _db;
        public UserRepository()
        {
        }
        public UserRepository(ShopEntities db)
        {
            _db = db;
        }
        public User GetUser(string email, string passwordHash)
        {
            return _db.Users.FirstOrDefault(x => x.email == email && x.passwordHash == passwordHash);
        }
        public bool AddUser(User user)
        {
            try
            {
                _db.Users.Add(user);
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<User> LoadUser()
        {
            return _db.Users.ToList();
        }
        public User Details(int id) { 
            return _db.Users.Find(id);
        }

        public bool UpdateUser(User user) {
            var u= _db.Users.FirstOrDefault(x=> x.userId == user.userId);
            if (u == null) { return false; }
            u.fullName = user.fullName;
            u.email = user.email;
            u.passwordHash = user.passwordHash;
            u.phoneNumber = user.phoneNumber;
            u.address = user.address;
            u.role = user.role;
            _db.SaveChanges();
            return true;
        }

    }
}