using BaiTap.App_Start;
using BaiTap.Models;
using ClosedXML.Excel;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Controllers
{
    [RoleUser]
    public class OrderController : Controller
    {
        // GET: Admin/Order
        private readonly ShopEntities _db;
        public OrderController(ShopEntities db)
        {
            _db = db;
        }
        public ActionResult Index(string statusFilter, int? page)
        {
            var orders = _db.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.status == statusFilter);
            }

            ViewBag.StatusList = _db.Orders
                                    .Select(o => o.status)
                                    .Distinct()
                                    .ToList();

            ViewBag.CurrentFilter = statusFilter;

            int pageSize = 10;
            int pageNumber = (page ?? 1); // Mặc định là trang 1 nếu không có tham số trang

            // Trả về danh sách đơn hàng đã phân trang
            return View(orders.OrderBy(o => o.orderId).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Edit(int id)
        {
            var order = _db.Orders.FirstOrDefault(o => o.orderId == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order model)
        {
            var order = _db.Orders.FirstOrDefault(o => o.orderId == model.orderId);
            if (order == null)
            {
                return HttpNotFound();
            }
            order.status = model.status;

            _db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult ExportExcel()
        {
            var orders = _db.Orders.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Danh sách đơn hàng");

                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "Địa chỉ";
                worksheet.Cell(1, 4).Value = "Thành tiền";
                worksheet.Cell(1, 5).Value = "Trạng thái";
                worksheet.Cell(1, 6).Value = "Ngày đặt đơn";

                int row = 2;
                int stt = 1;
                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = order.User.email;
                    worksheet.Cell(row, 3).Value = order.User.address;
                    worksheet.Cell(row, 4).Value = order.finalAmount;
                    worksheet.Cell(row, 5).Value = order.status;
                    worksheet.Cell(row, 6).Value = order.orderDate?.ToString("dd/MM/yyyy");
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "DonHang.xlsx");
                }
            }
        }
    }
}