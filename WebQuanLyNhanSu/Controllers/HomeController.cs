using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebQuanLyNhanSu.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        DBDataContext db = new DBDataContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult QLCheDo()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            return View(db.CheDos.ToList());
        }
        public ActionResult QLNV()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            var nv = db.Employees.ToList();
            return View(nv);
        }
        public ActionResult GuiMail()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            return View();
        }
        public ActionResult PhongBan()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            return View(db.Departments.ToList());
        }
        public ActionResult NgayLe()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            return View(db.NgayLes.ToList());
        }
        public ActionResult BangCong()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");

            // Lấy danh sách các attendance trong tháng hiện tại
            DateTime today = DateTime.Today;
            var attendances = db.Attendances.Where(a => a.Ngay.Value.Month == today.Month && a.Ngay.Value.Year == today.Year).ToList();

            // Dictionary để lưu số lần đi trễ, số lần về sớm và tên của từng nhân viên
            Dictionary<int, Tuple<string, int, int>> soLanDiTreVaVeSom = new Dictionary<int, Tuple<string, int, int>>();

            foreach (var attendance in attendances)
            {
                // Lấy thông tin tên của nhân viên từ cơ sở dữ liệu
                string tenNhanVien = db.Employees.FirstOrDefault(e => e.EmployeeID == attendance.EmployeeID)?.LastName;

                // Kiểm tra nếu không tìm thấy tên hoặc không đi trễ và không về sớm, bỏ qua
                if (string.IsNullOrEmpty(tenNhanVien) || attendance.DiTre == "Không" && attendance.VeSom == "Không")
                    continue;

                int employeeID = attendance.EmployeeID ?? 0;
                int soLanDiTre = attendance.DiTre == "Có" ? 1 : 0;
                int soLanVeSom = attendance.VeSom == "Có" ? 1 : 0;

                // Nếu nhân viên đã có trong dictionary, tăng số lần đi trễ hoặc số lần về sớm lên tương ứng
                if (soLanDiTreVaVeSom.ContainsKey(employeeID))
                {
                    var tuple = soLanDiTreVaVeSom[employeeID];
                    soLanDiTreVaVeSom[employeeID] = Tuple.Create(tuple.Item1, tuple.Item2 + soLanDiTre, tuple.Item3 + soLanVeSom);
                }
                else // Nếu không, thêm vào dictionary và set số lần đi trễ và số lần về sớm
                {
                    soLanDiTreVaVeSom.Add(employeeID, Tuple.Create(tenNhanVien, soLanDiTre, soLanVeSom));
                }
            }

            // Lưu dictionary vào ViewBag
            ViewBag.SoLanDiTreVaVeSom = soLanDiTreVaVeSom;

            return View(attendances);
        }

        public ActionResult KTKL()
        {
            // Lấy danh sách nhân viên từ database
            var employees = db.RewardsPenalties.ToList();
            var nv = db.Employees.ToList();
            ViewBag.NV = nv;

            // Kiểm tra nếu danh sách nhân viên là null
            if (employees == null)
            {
                // Xử lý trường hợp danh sách nhân viên là null
                // Trong trường hợp này, ta tạo ra một danh sách nhân viên mới, trống
                employees = new List<WebQuanLyNhanSu.RewardsPenalty>();
            }
            ViewBag.Employees = employees;

            // Trả về View với danh sách nhân viên
            return View(employees);
        }
        public ActionResult BangLuong()
        {
            return View(db.Salaries.ToList());
        }
        public ActionResult TDBangLuong()
        {
            return View();
        }
        public ActionResult TinhLuong()
        {
            var sl = db.Salaries.ToList();
            return View(sl);
        }
        public ActionResult DuAn()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            var duan = db.Projects.ToList();
            ViewBag.chuaht = duan.Where(u=> u.EndDate < DateTime.Now.Date && u.TrangThai=="Chưa hoàn thành");
            return View(duan);
        }
        public ActionResult PhanCongDuAn()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            return View(db.PhanCongs.ToList());
        }
        public ActionResult NghiPhep()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            var leaves = db.Leaves.ToList();
            return View(leaves);
        }
        public ActionResult BaoCaoNhanSu()
        {
            return View();
        }
        public ActionResult BaoCaoLuong()
        {
            return View();
        }
        public ActionResult TraCuu()
        {
            return View(db.Employees.ToList());
        }
        public ActionResult HeThong()
        {
            var message = Session["Message"];
            ViewBag.Message = message;
            Session.Remove("Message");
            return View(db.UserAccounts.ToList());
        }
        public ActionResult DangNhap()
        {
            return View();
        }
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("DangNhap", "Home");
        }
        [HttpPost]
        public ActionResult DangNhap(string username, string password)
        {
                var user = db.UserAccounts.FirstOrDefault(u => u.Username == username && u.Password == HashPassword(password));
                if (user != null)
                {
                Session["user"] = user;
                var nv = db.Employees.SingleOrDefault(u=> u.EmployeeID == user.EmployeeID);
                Session["name"] = nv.LastName;
                if (nv.ChucVu == "Quản lý")
                    return RedirectToAction("Index", "Home");
                else return RedirectToAction("Index","NhanVien");
            }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View("DangNhap");
                }
            
        }
    }
}