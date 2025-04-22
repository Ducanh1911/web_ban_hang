using BaiTap.App_Start;
using BaiTap.Models;
using BaiTap.Repository;
using BaiTap.Service;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using Microsoft.Ajax.Utilities;

namespace BaiTap.Controllers
{

    public class UserController : Controller
    {
        private readonly ShopEntities _db = new ShopEntities();
        private readonly UserService _userService;

        public UserController(UserService userService, ShopEntities db)
        {
            _userService = userService;
            _db = db;
        }

        // GET: User/Login
        [RoleUser]

        public ActionResult Profile()
        {
            var user = BaiTap.App_Start.SessionConfig.GetUser();
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);

        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return View();
            }

            string passwordHash = HashPassword(password);
            var user = _userService.Get(email, passwordHash);

            if (user != null)
            {
                SessionConfig.SetUser(user);
                SessionConfig.SetUserId(user.userId);
                if (user.role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        // GET: User/LoadUser
        [RoleUser]
        public ActionResult LoadUser()
        {
            return View(_userService.GetUser());
        }

        // GET: User/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Register(User model, string password)
        {
            var existingUser = _db.Users.FirstOrDefault(u => u.email == model.email);
            if (existingUser != null)
            {
                ModelState.AddModelError("email", "Email đã tồn tại. Vui lòng sử dụng email khác.");
                return View(model);
            }
            var existingPhone = _db.Users.FirstOrDefault(u => u.phoneNumber == model.phoneNumber);
            if (existingPhone != null)
            {
                ModelState.AddModelError("phoneNumber", "Số điện thoại đã được sử dụng. Vui lòng nhập số khác.");
                return View(model);
            }
            model.passwordHash = HashPassword(password);
            model.createdAt = DateTime.Now;
            model.role = "Customer";

            _db.Users.Add(model);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }


        // GET: User/Edit
        [RoleUser]

        public ActionResult Edit(int id)
        {
            return View(_userService.Detail(id));
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            var u = _db.Users.FirstOrDefault(x => x.userId == user.userId);
            //if (u == null) { return false; }
            u.fullName = user.fullName;
            u.email = user.email;
            //u.passwordHash = user.passwordHash;
            u.phoneNumber = user.phoneNumber;
            u.address = user.address;
            u.role = user.role;
            u.createdAt = user.createdAt;
            u.otp = user.otp;
            u.otpExpiry = user.otpExpiry;
            _db.SaveChanges();
            return Redirect("/User/LoadUser");
        }

        // GET: User/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.email == email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại.";
                return View();
            }

            string otp = GenerateOtp();
            user.otp = otp;
            user.otpExpiry = DateTime.Now.AddMinutes(10);
            _db.SaveChanges();

            if (SendOtpEmail(email, otp))
            {
                ViewBag.Message = "Mã OTP đã được gửi đến email của bạn.";
                return RedirectToAction("VerifyOtp", new { email = email });
            }
            else
            {
                ViewBag.Error = "Không thể gửi email. Vui lòng thử lại.";
                return View();
            }
        }

        // GET: User/EditProfile
        public ActionResult EditProfile()
        {
            var userId = SessionConfig.GetUserId();
            var user = _db.Users.FirstOrDefault(u => u.userId == userId);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = SessionConfig.GetUserId();
            var user = _db.Users.FirstOrDefault(u => u.userId == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.fullName = model.fullName;
            user.phoneNumber = model.phoneNumber;
            user.address = model.address;
            _db.SaveChanges();

            // Cập nhật lại thông tin mới trong Session
            SessionConfig.SetUser(user);

            TempData["Success"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("Profile");
        }

        // GET: User/VerifyOtp
        public ActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOtp(string email, string otp, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin.";
                ViewBag.Email = email;
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                ViewBag.Email = email;
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.email == email);
            if (user != null && user.otp == otp && user.otpExpiry > DateTime.Now)
            {
                user.passwordHash = HashPassword(password);
                user.otp = null;
                user.otpExpiry = null;
                _db.SaveChanges();
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Error = "Mã OTP không hợp lệ hoặc đã hết hạn.";
                ViewBag.Email = email;
                return View();
            }
        }

        // Helper Methods
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        private bool SendOtpEmail(string email, string otp)
        {
            try
            {
                var fromAddress = new MailAddress("ducanhlanhtanh@gmail.com", "webbanhang");
                var toAddress = new MailAddress(email);
                const string fromPassword = "vqvhueulzjjqmsnp";
                const string subject = "Mã OTP để đặt lại mật khẩu";
                string body = $"Mã OTP của bạn là: {otp}. Mã này có hiệu lực trong 10 phút.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi email: " + ex.Message);
                return false;
            }
        }

    }
}
