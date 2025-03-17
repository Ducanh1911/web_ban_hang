using BaiTap.Models;
using BaiTap.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace BaiTap.Service
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public bool Add(User user)
        {
            return _userRepository.AddUser(user);
        }
        public List<User> GetUser() {
           return _userRepository.LoadUser();
        }
        public User Get(string email, string passwordHash)
        {
            return _userRepository.GetUser(email, passwordHash);
        }

        public User Detail(int id)
        {
            return _userRepository.Details(id);
        }

        public bool Update(User user)
        {
            return _userRepository.UpdateUser(user);
        }
    }
}