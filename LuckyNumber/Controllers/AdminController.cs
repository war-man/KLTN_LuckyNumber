﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LuckyNumber.ViewModel;
using LuckyNumber.Models;

namespace LuckyNumber.Controllers
{
    public class AdminController : Controller
    {
        LuckyNumContext db = new LuckyNumContext();   
        //
        // GET: /Admin/
        public ActionResult Login()
        {
            if(Session["userName"]==null || Session["Role"].ToString()!="Admin" )
            return View();
            else return Redirect("~/Admin/adminProfile");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return Redirect("~/User/Index");
        }

        [HttpPost]
        public ActionResult Login(Admin admin)
        {
            using (LuckyNumContext db = new LuckyNumContext())
            {
                Admin admin2 = db.Admins.SingleOrDefault(x => x.username == admin.username && x.password == admin.password);
                if (admin2 != null)
                {
                    string Role = "Admin";
                    Session["Role"] = Role;
                    Session["userName"] = admin2.nickname;
                    Session["email"] = admin2.email;
                    Session["IDs"] = admin2.AdminID;
                    //Session["phone"] = admin2.phone;
                    return Redirect("~/Admin/adminProfile");
                }
                return Redirect("~/Admin/signError");
            }
        }

        public ActionResult adminProfile()
        {
            if (Session["userName"] != null && Session["Role"].ToString()=="Admin")
            {
                String name = Session["userName"].ToString();
                ViewBag.Name = name;
                string email = Session["email"].ToString();
                ViewBag.Email = email;
                return View();
            }
            else return RedirectToAction("Login");
        }

        public ActionResult signError()
        {
                return View();
        }

        public ActionResult QuanLyPhienChoi()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin" )
            {
                String name = Session["userName"].ToString();
                ViewBag.Name = name;
                var model = db.CuocChois.ToList();
                return View(model);
            }
            else return RedirectToAction("Login");
        }

        public ActionResult ThemPhienChoi(CuocChoi cuocchoi)
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")
            {
                int trangthai=0;
                DateTime ngaychoi = DateTime.Parse(Request.Form["NgayDoanSo"]);
                var list = from u in db.CuocChois select u;
                foreach(var i in list)
                {
                    if (i.NgayDoanSo==ngaychoi)
                    {
                        trangthai = 1;
                    }
                }
                if (trangthai != 1)
                {
                    db.CuocChois.Add(cuocchoi);
                    cuocchoi.TrangThai = true;
                    db.SaveChanges();
                    int ma = cuocchoi.MaCuocChoi;
                    DanhSachTrungThuong danhsach = new DanhSachTrungThuong();

                    db.DanhSachTrungThuongs.Add(danhsach);
                    danhsach.MaCuocChoi = ma;
                    danhsach.TongTienThuong = 50000;
                    db.SaveChanges();
                    var selectlist = db.Users.ToList();
                    foreach (var i in selectlist)
                    {
                        i.diemdanh = 1;
                        db.SaveChanges();
                    }
                    return Redirect("~/Admin/QuanLyPhienChoi");
                }
                else return Content("<script language='javascript' type='text/javascript'> " +
                        "alert('Ngày chơi bị trùng');" +
                        "window.location= '/Admin/QuanLyPhienChoi';" +
                        "</script>");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult KetThucPhienChoi()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin" )
            {
                String name = Session["userName"].ToString();
                ViewBag.Name = name;
                return View();
            }
            else return RedirectToAction("Login");
        }


        public ActionResult ThemLuotChoiAo()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin" )
            {
                String name = Session["userName"].ToString();
                ViewBag.Name = name;
                return View();
            }
            else return RedirectToAction("Login");
        }

        public ActionResult ThemNguoiChoiAo()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")
            {
                int a = int.Parse(Request.Form["SoLuongNumber"].ToString());

                int[] listInt;
                //string day = DateTime.Now.Day.ToString();
                //string month = DateTime.Now.Month.ToString();
                //string year = DateTime.Now.Year.ToString();

                DateTime serverTime = DateTime.Now;
                DateTime utcTime = DateTime.UtcNow;

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
                string timeNow = localTime.ToString("t");

                ////////////////////////////////////

                string day = localTime.ToString("dd");
                string month = localTime.ToString("MM");
                string year = localTime.ToString("yyyy");

                DateTime datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                CuocChoi cuocchoi = db.CuocChois.SingleOrDefault(x => x.NgayDoanSo == datetime);

                int maChoi = int.Parse(cuocchoi.MaCuocChoi.ToString()); // chỗ này là lấy ra mã chơi nha đạt


                listInt = new int[a];
                int ptr = 0;
                int maxInt;
                int idInt = 0;
                int soDuDoan;
                bool isNext = true;
                Random rd = new Random();
                var list = from u in db.Users
                           select new { Id = u.ID };
                maxInt = list.Count() - 1;
                foreach (var i in list)
                {
                    listInt[ptr++] = i.Id;
                }
                for (int i = 0; i < a; i++)
                {
                    ptr = rd.Next(0, maxInt);
                    ChiTietCuocChoi chitiet = new ChiTietCuocChoi();
                    idInt = listInt[ptr];
                    chitiet.UserID = idInt;
                    chitiet.MaCuocChoi = maChoi;
                    isNext = true;
                    do
                    {
                        soDuDoan = rd.Next(0, 999);
                        ChiTietCuocChoi chitiet2 = db.ChiTietCuocChois.SingleOrDefault(x => x.MaCuocChoi == maChoi && x.UserID == idInt && x.SoDuDoan == soDuDoan);
                        if (chitiet2 == null)
                            isNext = false;
                    } while (isNext);
                    chitiet.SoDuDoan = soDuDoan;
                    db.ChiTietCuocChois.Add(chitiet);
                    db.SaveChanges();
                }

                return Redirect("~/Admin/adminProfile");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult KetThucPhien()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")

            // ---- Lấy ra ngày tương ứng ------
            //string day = DateTime.Now.Day.ToString();
            //string month = DateTime.Now.Month.ToString();
            //string year = DateTime.Now.Year.ToString();

            {
                DateTime serverTime = DateTime.Now;
                DateTime utcTime = DateTime.UtcNow;

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
                string timeNow = localTime.ToString("t");

                ////////////////////////////////////

                string day = localTime.ToString("dd");
                string month = localTime.ToString("MM");
                string year = localTime.ToString("yyyy");

                DateTime datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                CuocChoi cuocchoi = db.CuocChois.SingleOrDefault(x => x.NgayDoanSo == datetime);


                // ---------- End -------------------

                if (cuocchoi.TrangThai == true)
                {
                    int maChoi = int.Parse(cuocchoi.MaCuocChoi.ToString()); // Lấy ra mã cuộc chơi từ ngày chơi

                    // ----- Lấy ra danh sách theo mã cuộc chơi --------
                    DanhSachTrungThuong danhsach = db.DanhSachTrungThuongs.SingleOrDefault(x => x.MaCuocChoi == maChoi);

                    int maDS = int.Parse(danhsach.MaDSTrungThuong.ToString());

                    // ----------- End --------------

                    var tongSoLan = from u in db.ChiTietCuocChois
                                    where u.MaCuocChoi == maChoi
                                    group u by u.SoDuDoan into Counted
                                    select new
                                    {
                                        soDuDoan = Counted.Key,
                                        soLan = Counted.Count(),
                                        soTrongSo = Counted.Sum(x => x.TrongSo)
                                    };
                    int? soLanItNhat = tongSoLan.Min(x => (int?)x.soLan);
                    if (soLanItNhat != 0)
                    {
                        var tongSoLanItNhat = from t in tongSoLan
                                              where t.soLan == soLanItNhat
                                              select t;
                        int tongSoItNhat = tongSoLanItNhat.Count();
                        int? tongTrongSo = tongSoLanItNhat.Sum(x => x.soTrongSo);
                        float? tienThuong = float.Parse(danhsach.TongTienThuong.ToString()) / tongTrongSo; // số tiền

                        foreach (var i in tongSoLanItNhat)
                        {
                            var danhSachTrung = from y in db.ChiTietCuocChois
                                                where y.SoDuDoan == i.soDuDoan && y.MaCuocChoi == maChoi && y.TrongSo == i.soTrongSo
                                                select y;
                            foreach (var o in danhSachTrung)
                            {
                                ChiTietTrungThuong chiTietTrungThuong = new ChiTietTrungThuong();
                                chiTietTrungThuong.UserID = o.UserID;
                                chiTietTrungThuong.MaDSTrungThuong = maDS;
                                chiTietTrungThuong.SoDuDoan = o.SoDuDoan;
                                chiTietTrungThuong.TienThuong = tienThuong * o.TrongSo;
                                User user = db.Users.SingleOrDefault(x => x.ID == o.UserID);
                                user.taikhoan += tienThuong * o.TrongSo;
                                user.checktt = 1;


                                db.ChiTietTrungThuongs.Add(chiTietTrungThuong);
                            }
                        }
                    }

                    cuocchoi.TrangThai = false;
                    var selectlist = db.Users.ToList();
                    foreach (var i in selectlist)
                    {
                        i.diemdanh = 0;
                        db.SaveChanges();
                    }


                    db.SaveChanges();
                    return Redirect("~/Admin/adminProfile");
                }
                else return Content("<script language='javascript' type='text/javascript'> " +
                            "alert('BẠN ĐÃ KẾT THÚC CUỘC CHƠI RỒI. VUI LÒNG TẠO PHIÊN CHƠI MỚI VÀ THỬ LẠI');" +
                            "window.location= '/Admin/QuanLyPhienChoi';" +
                            "</script>");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult DieuChinhGiaiThuong()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")
            {
                int ma = int.Parse(Request.Form["mads"].ToString());
                string a = Request.Form["giaithuong"].ToString();
                double tien = double.Parse(a);


                DanhSachTrungThuong ds = db.DanhSachTrungThuongs.SingleOrDefault(x => x.MaDSTrungThuong == ma);

                if (ds == null) return Json("Mã danh sách không tồn tại", JsonRequestBehavior.AllowGet);
                else if (a == null) return Json("Sai Định Dạng", JsonRequestBehavior.AllowGet);
                else
                {
                    ds.TongTienThuong = tien;
                    db.SaveChanges();
                }

                return Redirect("~/DieuChinhGiaiThuong/DieuChinhGiaiThuong");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult DanhSachTrungThuong()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")
            //string day = DateTime.Now.Day.ToString();
            //string month = DateTime.Now.Month.ToString();
            //string year = DateTime.Now.Year.ToString();

            {
                DateTime serverTime = DateTime.Now;
                DateTime utcTime = DateTime.UtcNow;

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
                string timeNow = localTime.ToString("t");

                ////////////////////////////////////

                string day = localTime.ToString("dd");
                string month = localTime.ToString("MM");
                string year = localTime.ToString("yyyy");


                DateTime datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                CuocChoi cuocchoi = db.CuocChois.SingleOrDefault(x => x.NgayDoanSo == datetime);
                //int mamax = db.CuocChois.Max(x => x.MaCuocChoi);
                //CuocChoi cuocchoi = db.CuocChois.SingleOrDefault(x => x.MaCuocChoi == mamax);

                int machoi;

                if (cuocchoi.TrangThai == true)
                {
                    machoi = cuocchoi.MaCuocChoi - 1;

                }
                //if (cuocchoi == null) machoi = 7;
                //else machoi = cuocchoi.MaCuocChoi;

                else machoi = cuocchoi.MaCuocChoi;


                //int maChoi = int.Parse(cuocchoi.MaCuocChoi.ToString());
                List<DanhSachTrungThuongViewModel> model = new List<DanhSachTrungThuongViewModel>();
                var join = (from US in db.Users
                            join CTC in db.ChiTietCuocChois
                                on US.ID equals CTC.UserID
                            join CC in db.CuocChois on CTC.MaCuocChoi equals CC.MaCuocChoi
                            join DSTT in db.DanhSachTrungThuongs on CC.MaCuocChoi equals DSTT.MaCuocChoi
                            join CTTT in db.ChiTietTrungThuongs on DSTT.MaDSTrungThuong equals CTTT.MaDSTrungThuong
                            where CC.MaCuocChoi == machoi && CTTT.SoDuDoan == CTC.SoDuDoan
                            select new
                            {
                                userName = US.username,
                                Phone = US.phone.Substring(0, 6) + "xxxx",
                                Email = "xxxx" + US.email.Substring(4),
                                ngayDoanSo = CC.NgayDoanSo
                            }).ToList().Distinct();
                foreach (var item in join)
                {
                    model.Add(new DanhSachTrungThuongViewModel()
                    {
                        username = item.userName,
                        phone = item.Phone,
                        email = item.Email,
                        NgayDoanSo = item.ngayDoanSo
                    });
                }


                return View(model);
            }
            else return RedirectToAction("Login");
        }

        public ActionResult DoiMatKhau()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")
                return View();
            else return RedirectToAction("Login");
        }

        public ActionResult ChangePass()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "Admin")
            {
                int adID = int.Parse(Session["IDs"].ToString());
                Admin admin = db.Admins.SingleOrDefault(x => x.AdminID == adID);

                string pass1 = Request.Form["pass1"].ToString();
                string pass2 = Request.Form["pass2"].ToString();
                string pass3 = Request.Form["pass3"].ToString();

                if (admin.password != pass1) return Json("Mật khẩu hiện tại của bạn không chính xác", JsonRequestBehavior.AllowGet);
                else if (pass2 != pass3) return Json("Sai Mật Khẩu Nhập Lại", JsonRequestBehavior.AllowGet);

                admin.password = pass2;
                db.SaveChanges();

                return Redirect("~/Admin/adminProfile");
            }
            else return RedirectToAction("Login");
        }
	}
}