using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebQuanLyNhanSu.Controllers
{
    public class CheckInController : Controller
    {
        DBDataContext db = new DBDataContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(int employeeID)
        {
            var checkIn = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
            var ngay = DateTime.Now.Date;
            // Kiểm tra giờ check in so với 7h30
            string diTre = checkIn > TimeSpan.FromHours(7) ? "Có" : "Không";

            // Tạo đối tượng mới
            Attendance newAttendance = new Attendance
            {
                EmployeeID = employeeID,
                CheckIn = checkIn,
                Ngay = ngay.Date, // Lấy ngày từ datetime
                DiTre = diTre
            };
            db.Attendances.InsertOnSubmit(newAttendance);
            db.SubmitChanges();
            // Thêm vào cơ sở dữ liệu
            // Cần thêm các bước xử lý thêm mới tại đây, ví dụ như sử dụng Entity Framework để thêm vào database

            // Sau khi thêm mới, chuyển hướng về trang chính
            return RedirectToAction("Index");
        }
        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Checkout(int employeeID)
        {
            var checkout = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
            var ngay = DateTime.Now.Date;
            // Kiểm tra giờ check in so với 7h30
            string vesom = checkout < TimeSpan.FromHours(16) ? "Có" : "Không";
            var dd = db.Attendances.SingleOrDefault(u=>u.EmployeeID == employeeID && u.Ngay.Value.Date == ngay);
            if (dd == null)
            {
                return HttpNotFound();
            }
            dd.CheckOut = checkout;
            var time = (int?)(checkout - dd.CheckIn.Value).TotalHours - 1;
            int sogiolam = 0;
            int sogiolamle = 0;
            var le = db.NgayLes.ToList();
            foreach (var ngayle in le)
            {
                if (ngayle.NgayBatDau <= ngay && ngay <= ngayle.NgayKetThuc)
                {
                    sogiolamle += ngayle.HeSo * ((int)(checkout - dd.CheckIn.Value).TotalHours - 1);
                }
            }
            if (time < 0 && sogiolamle == 0)
            {
                dd.Timer = 0;
            }
                
            else if (time < 8 && sogiolamle ==0)
            {
                sogiolam += (int)time;
                dd.Timer = time;
            }
            else if (sogiolamle == 0)
            {
                sogiolam += 8;
                dd.Timer = 8;
            }
            int ot =0 ;
            if (vesom == "Không" && sogiolamle == 0)
            {
                dd.OT = (int?)(checkout - TimeSpan.FromHours(16)).TotalHours;
                ot += (int)dd.OT;
            }
            else
            {
                dd.OT = 0;
            }
           
            dd.VeSom= vesom;
            var sl = db.Salaries.SingleOrDefault(u=>u.EmployeeID == employeeID);

                sl.SoGioLam += sogiolam;
                sl.OT += ot;
                sl.GioNgayLe += sogiolamle;
            if (sogiolamle == 0)
            {
                sl.TongSoGioLam += sl.OT + sl.SoGioLam;
            }
            else
            {
                sl.TongSoGioLam += sogiolamle;
            }
            db.SubmitChanges();
            // Thêm vào cơ sở dữ liệu
            // Cần thêm các bước xử lý thêm mới tại đây, ví dụ như sử dụng Entity Framework để thêm vào database

            // Sau khi thêm mới, chuyển hướng về trang chính
            return RedirectToAction("Checkout");
        }
    }
}