using BaiTap.App_Start;
using BaiTap.Models;
using BaiTap.Repository;
using BaiTap.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Controllers
{

    public class UserController : Controller
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService=userService;
        }
        // GET: User
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(String email, string passwordHash)
        {
            var user = _userService.Get(email, passwordHash);
            if(user != null)
            {
                SessionConfig.SetUser(user);
                if(user.role == "Admin")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "Customer" });

                }
            }
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng";
            return View();
        }

        public ActionResult LoadUser()
        {
            return View(_userService.GetUser());
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(User user)
        {
            if(_userService.Add(user)== true)
            {
                return Redirect("~/User/Login");
            }
            else
            {
                return View(user);
            }
        }

        public ActionResult Edit(int id) { 
            return View(_userService.Detail(id));
        }
        [HttpPost]
        public ActionResult Edit(User model) {
            if (_userService.Update(model)== true)
            {
                return Redirect("~/User/LoadUser");
            }
            else { 
                return View(model);
            }
        }
    }
}