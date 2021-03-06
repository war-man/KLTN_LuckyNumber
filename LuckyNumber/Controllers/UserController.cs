﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LuckyNumber.Models;
using System.Text;
using System.Data.Entity;
using LuckyNumber.SMSAPI;
using Facebook;
using System.Configuration;
using LuckyNumber.ViewModel;
using System.Net;
using System.Web.UI;

namespace LuckyNumber.Controllers
{
    public class UserController : Controller
    {
        //
        LuckyNumContext db = new LuckyNumContext();


        // GET: /User/
        public ActionResult Index()
        {

            if (Session["IDs"] == null)
            //string day = DateTime.Now.Day.ToString(new System.Globalization.CultureInfo("en-US"));
            //string month = DateTime.Now.Month.ToString(new System.Globalization.CultureInfo("en-US"));
            //string year = DateTime.Now.Year.ToString(new System.Globalization.CultureInfo("en-US"));
            //string timeNow = DateTime.Now.ToString("t", new System.Globalization.CultureInfo("en-US"));

            {
                string timeEnd = DateTime.Parse("11:50 PM").ToString("t");
                string timeStart = DateTime.Parse("11:58 PM").ToString("t");




                ////////////////////


                DateTime serverTime = DateTime.Now;
                DateTime utcTime = DateTime.UtcNow;

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
                string timeNow = localTime.ToString("t");

                ////////////////////////////////////

                string day = localTime.ToString("dd");
                string month = localTime.ToString("MM");
                string year = localTime.ToString("yyyy");


                Session["timenow"] = timeNow;




                DateTime datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));



                CuocChoi cuocchoi = db.CuocChois.FirstOrDefault(x => x.NgayDoanSo == datetime);
                //int mamax = db.CuocChois.Max(x => x.MaCuocChoi);
                //CuocChoi cuocchoi = db.CuocChois.SingleOrDefault(x => x.MaCuocChoi == mamax);

                int machoi;
                if (cuocchoi != null)

                {
                    if (cuocchoi.TrangThai == true)
                    {
                        machoi = cuocchoi.MaCuocChoi - 1;

                    }
                    //if (cuocchoi == null) machoi = 7;
                    //else machoi = cuocchoi.MaCuocChoi;

                    else machoi = cuocchoi.MaCuocChoi;
                }

                else
                {
                    CuocChoi cuocchoi1 = db.CuocChois.OrderByDescending(x => x.MaCuocChoi).Take(1).Single();
                    machoi = cuocchoi1.MaCuocChoi;
                }

                string dayNow = day + "/" + month + "/" + year;
                Session["dayNow"] = dayNow;
                ViewBag.dayNow = Session["dayNow"].ToString();


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
                if (cuocchoi != null)
                {

                    if (cuocchoi.TrangThai == true && ((DateTime.Compare(DateTime.Parse(timeNow), DateTime.Parse(timeEnd)) > 0) && (DateTime.Compare(DateTime.Parse(timeNow), DateTime.Parse(timeStart)) < 0)))
                        ketthucphien();
                    return View(model);

                }
                return View(model);
            }
            else return RedirectToAction("userProfile");
        }


        private void ketthucphien()
        {
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


            // ---------- End -------------------




            int maChoi = int.Parse(cuocchoi.MaCuocChoi.ToString()); // Lấy ra mã cuộc chơi từ ngày chơi

            // ----- Lấy ra danh sách theo mã cuộc chơi --------
            DanhSachTrungThuong danhsach = db.DanhSachTrungThuongs.SingleOrDefault(x => x.MaCuocChoi == maChoi);

            int maDS = int.Parse(danhsach.MaDSTrungThuong.ToString());

            // ----------- End --------------
            try
            {
                var tongSoLan = from u in db.ChiTietCuocChois
                                where u.MaCuocChoi == maChoi
                                group u by u.SoDuDoan into Counted
                                select new
                                {
                                    soDuDoan = Counted.Key,
                                    soLan = Counted.Count(),
                                    soTrongSo = Counted.Sum(u => u.TrongSo) ?? 0

                                };
                int? soLanItNhat = tongSoLan.Min(x => x.soLan);
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
                var selectlist = db.Users.ToList();
                foreach (var i in selectlist)
                {
                    i.diemdanh = 0;
                    db.SaveChanges();
                }
                cuocchoi.TrangThai = false;
                db.SaveChanges();
            }
            catch (Exception)
            {
                var selectlist = db.Users.ToList();
                foreach (var i in selectlist)
                {
                    i.diemdanh = 0;
                    db.SaveChanges();
                }
                cuocchoi.TrangThai = false;
                db.SaveChanges();
            }
        }

        public ActionResult AfterShare(User us)
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                var selectlist = db.Users.Where(a => a.ID == userID).ToList();
                foreach (var i in selectlist)
                {
                    i.checktt = 0;
                    db.SaveChanges();
                }

                return RedirectToAction("userProfile");
            }
            else return RedirectToAction("Login");
        }
        public ActionResult ShareTrungThuong()
        {
                return View();
        }
        public ActionResult DiemDanh()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                if (int.Parse(Session["diemdanh"].ToString()) == 1)
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
                    CuocChoi cuocchoi1 = db.CuocChois.OrderByDescending(x => x.MaCuocChoi).Take(1).Single();

                    DateTime? ngaychoi;
                    ngaychoi = cuocchoi1.NgayDoanSo;
                    DateTime datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

                    if (ngaychoi == datetime)
                    {
                        var select = from ds in db.CuocChois
                                     join tt in db.DanhSachTrungThuongs on ds.MaCuocChoi equals tt.MaCuocChoi
                                     where (ds.NgayDoanSo == datetime)
                                     select tt.TongTienThuong;
                        foreach (var i in select)
                        {
                            Session["tt"] = i;
                        }
                        string ttt = Session["tt"].ToString();
                        ViewBag.ttt = ttt;
                    }
                    else
                    {
                        Session["tt"] = null;
                        ViewBag.ttt = null;
                    }
                    return View();
                }
                else return RedirectToAction("DaDiemDanh");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult ThaoTacCongLuot(User us)
        {
            int userID = int.Parse(Session["IDs"].ToString());
            var selectlist = db.Users.Where(a => a.ID == userID).ToList();
            foreach (var i in selectlist)
            {
                i.diemdanh = 0;
                int temp_km = int.Parse(Session["soLuotChoi_km"].ToString()) + 5;
                if (temp_km < 30)
                {
                    i.soluotchoi_km = temp_km;
                }
                else i.soluotchoi_km = 30;
                db.SaveChanges();
            }
            Session["diemdanh"] = 0;
            return RedirectToAction("userProfile");
        }
        public ActionResult DaDiemDanh(User us)
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
                return View();
            else return RedirectToAction("Login");
        }

        public ActionResult CreateUser()
        {
            if (Session["IDs"] == null)
                return View();
            else if (Session["IDs"] != null && Session["Role"].ToString() == "User")
                return Redirect("~/User/userProfile");
            else
            {
                Session.Clear();
                return View();
            }
        }
        public ActionResult Create(User user)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            string c1;
            Random rand = new Random();
            for (int i = 0; i < 9; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(rand.Next(65, 87)));
                sb.Append(c);
            }
            c1 = sb.ToString();

            user.mamoi = c1;
            user.soluotchoi = 0;
            user.soluotchoi_km = 5;
            user.xacnhan = false;
            user.taikhoan = 0;
            user.online = 0;
            user.diemdanh = 0;
            user.fb = 0;
            user.diemdanh = 0;
            //user.token = null;
            user.status = false;
            user.checktt = 0;
            db.Users.Add(user);
            db.SaveChanges();
            return Redirect("~/User");
        }

        public ActionResult Login()
        {

            if(Session["IDs"]==null)
            return View();
            else if(Session["IDs"]!=null && Session["Role"].ToString() == "User")
                return Redirect("~/User/userProfile");
            else
            {
                Session.Clear();
                return View();
            }


        }

        public ActionResult LienHe()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string ttt = Session["tt"].ToString();
                ViewBag.ttt = ttt;
                string name = Session["userName"].ToString();
                ViewBag.Name = name;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public ActionResult Login(User user)
        {
            using (LuckyNumContext db = new LuckyNumContext())
            {
                User user2 = db.Users.SingleOrDefault(x => x.username == user.username && x.password == user.password);
                if (user2 != null)
                {
                    int online = int.Parse(user2.online.ToString());
                    Session["checktt"] = user2.checktt;
                    string Role = "User";
                    Session["Role"] = Role;
                    Session["userName"] = user2.nickname;
                    Session["IDs"] = user2.ID;
                    Session["eMail"] = user2.email;
                    Session["pHone"] = user2.phone;
                    Session["soLuotChoi"] = user2.soluotchoi.ToString();
                    Session["soLuotChoi_km"] = user2.soluotchoi_km.ToString();
                    Session["taiKhoan"] = user2.taikhoan.ToString();
                    Session["maMoi"] = user2.mamoi.ToString();
                    //Session["token"] = user2.token;
                    user2.online = 1;
                    db.SaveChanges();
                    Session["diemdanh"] = user2.diemdanh;
                    int diemdanh = user2.diemdanh.Value;
                    if (diemdanh == 1)
                    {
                        return RedirectToAction("DiemDanh");
                    }
                    return Redirect("~/User/userProfile");
                }
            }
            return Redirect("~/User/signError");
        }

        public ActionResult signError()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
                return View();
            else return RedirectToAction("Login");
        }

        public ActionResult CapNhatPass()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                string PassOld = Request.Form["txtPassOld"].ToString();
                string Pass = Request.Form["txtPass"].ToString();

                if (PassOld == user.password)
                {
                    user.password = Pass;
                    db.SaveChanges();
                    return Redirect("~/User/userProfile");
                }

                return Redirect("~/User/signError");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult NapThe()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                string Serial = Request.Form["txtSerial"].ToString();
                string Pin = Request.Form["txtPin"].ToString();

                if (Serial == "123" && Pin == "123")
                {
                    user.soluotchoi++;
                    Session["soLuotChoi"] = user.soluotchoi;
                    db.SaveChanges();
                    return Redirect("~/User/userProfile");
                }

                else return Redirect("~/User/userProfile");
            }
            else return RedirectToAction("Login");

        }

        public ActionResult Logout()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int temp = int.Parse(Session["IDs"].ToString());
                User us = db.Users.SingleOrDefault(x => x.ID == temp);
                if (us != null)
                {
                    //us.token = null;
                    us.online = 0;
                    db.SaveChanges();
                }
                Session.Clear();
                return Redirect("~/User/Index");
            }
            else
            {
                return Redirect("~/User/Index");
            }
        }

        public ActionResult NapThePage()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string name = Session["userName"].ToString();
                ViewBag.Name = name;
                return View();
            }
            else return RedirectToAction("Login");
        }



        public ActionResult ThongBao4()
        {
            return View();
        }
        public ActionResult confirm()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string name = Session["userName"].ToString();
                ViewBag.Name = name;
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                if (user.xacnhan == true)
                {
                    return Redirect("~/User/ThongBao_1");
                }

                else if (user.phone == null) return Redirect("~/User/ThongBao4");

                StringBuilder sb = new StringBuilder();
                char c;
                string c1;
                string phone = Session["pHone"].ToString();
                Random rand = new Random();
                for (int i = 0; i < 5; i++)
                {
                    c = Convert.ToChar(Convert.ToInt32(rand.Next(65, 87)));
                    sb.Append(c);
                }

                c1 = sb.ToString();
                Session["MaXacNhan"] = c1;

                SpeedSMSAPI api = new SpeedSMSAPI();
                String userInfo = api.getUserInfo();
                String response = api.sendSMS(phone, "Ma xac nhan cua ban la: " + c1, 2, "");
                return Redirect("~/User/confirmNum");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult confirmPhone()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                StringBuilder sb = new StringBuilder();
                char c;
                string c1;
                string phone = Session["pHone"].ToString();
                Random rand = new Random();
                for (int i = 0; i < 5; i++)
                {
                    c = Convert.ToChar(Convert.ToInt32(rand.Next(65, 87)));
                    sb.Append(c);
                }

                c1 = sb.ToString();
                Session["MaXacNhan"] = c1;

                SpeedSMSAPI api = new SpeedSMSAPI();
                String userInfo = api.getUserInfo();
                String response = api.sendSMS(phone, "Ma xac nhan cua ban la: " + c1, 2, "");
                return Redirect("~/User/confirmNum");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult confirmNum()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
                return View();
            else return RedirectToAction("Login");
        }
        public ActionResult confirmPhoneNum()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                string maxacnhan = Session["MaXacNhan"].ToString();
                string xacnhan = Request.Form["txtMaXacNhan"].ToString();

                if (maxacnhan == xacnhan)
                {
                    user.xacnhan = true;
                    db.SaveChanges();
                    return Redirect("~/User/userProfile");
                }

                return Redirect("~/User/Error1");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult Error1()
        {
            return View();
        }

        public ActionResult ThongBao_1()
        {
            return View();
        }
        public long InsertForFacebook(User user)
        {

            var user2 = db.Users.SingleOrDefault(x => x.username == user.username);
            if (user2 == null)
            {
                StringBuilder sb = new StringBuilder();
                char c;
                string c1;
                Random rand = new Random();
                for (int i = 0; i < 9; i++)
                {
                    c = Convert.ToChar(Convert.ToInt32(rand.Next(65, 87)));
                    sb.Append(c);
                }
                c1 = sb.ToString();
                user.mamoi = c1;
                user.phone = null;
                user.soluotchoi = 0;
                user.soluotchoi_km = 5;
                user.xacnhan = false;
                user.status = false;
                user.taikhoan = 0;
                user.fb = 1;
                user.online = 1;
                user.checktt = 0;
                user.diemdanh = 0;
                db.Users.Add(user);

                db.SaveChanges();


                return user.ID;
                //return Redirect("~/User");
            }
            else
            {
                return user.ID;
            }
        }

        public ActionResult LoginFacebook()
        {

            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = "1159603104149803",
                client_secret = "baaa2827a1a3de6d25ddab75d5344dd6",
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email",
            });
            return Redirect(loginUrl.AbsoluteUri);
        }

        public ActionResult FacebookCallback(string code)
        {
            try
            {
                var fb = new FacebookClient();
                dynamic result = fb.Post("oauth/access_token", new
                {
                    client_id = "1159603104149803",
                    client_secret = "baaa2827a1a3de6d25ddab75d5344dd6",
                    redirect_uri = RedirectUri.AbsoluteUri,
                    code = code
                });

                var accessToken = result.access_token;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    fb.AccessToken = accessToken;
                    dynamic me = fb.Get("me?fields=id, first_name, middle_name, last_name, email");
                    string id = me.id;
                    string email = me.email;
                    string firstname = me.first_name;
                    string midname = me.middle_name;
                    string lastname = me.last_name;

                    var user = new User();
                    user.username = id;
                    User dbUs = db.Users.SingleOrDefault(x => x.username == user.username);

                    user.email = email;
                    user.nickname = firstname + " " + midname + " " + lastname;
                    var resultInsert = new UserController().InsertForFacebook(user);
                    if (resultInsert > 0)
                    {
                        user.taikhoan = 0;
                        user.soluotchoi = 0;
                        user.phone = null;
                        user.fb = 1;
                        //user.token = null;
                        user.diemdanh = 0;
                        user.status = false;
                        user.xacnhan = false;
                        user.soluotchoi_km = 5;
                        Session["userName"] = user.username;
                        Session["IDs"] = user.ID;
                        Session["eMail"] = user.email;
                        Session["diemdanh"] = user.diemdanh;
                        Session["soLuotChoi"] = user.soluotchoi.ToString();
                        Session["soLuotChoi_km"] = user.soluotchoi_km.ToString();
                        Session["maMoi"] = user.mamoi;
                        Session["pass"] = user.password;
                        Session["taiKhoan"] = 0;
                        string Role = "User";
                        Session["Role"] = Role;
                        Session["maMoi"] = user.mamoi;
                        //Session["token"] = user.token;
                        Session["checktt"] = 0;
                        return Content("<script language='javascript' type='text/javascript'> " +
                            "alert('Đăng nhập bằng Facebook thành công!!!');" +
                            "window.location= '/User/userProfile';" +
                            "</script>");
                    }
                    else

                    {
                        user.ID = dbUs.ID;
                        user.email = email;
                        user.username = id;
                        user.taikhoan = dbUs.taikhoan;
                        user.phone = dbUs.phone;
                        user.soluotchoi = dbUs.soluotchoi;
                        user.soluotchoi_km = dbUs.soluotchoi_km;
                        user.mamoi = dbUs.mamoi;
                        user.nickname = firstname + " " + midname + " " + lastname;
                        user.fb = 1;
                        user.status = false;
                        user.xacnhan = false;
                        user.diemdanh = dbUs.diemdanh;
                        user.checktt = dbUs.checktt;
                        dbUs.online = 1;
                        db.SaveChanges();

                        Session["userName"] = user.username;
                        Session["IDs"] = user.ID;
                        Session["eMail"] = user.email;
                        Session["diemdanh"] = user.diemdanh;
                        Session["soLuotChoi"] = user.soluotchoi.ToString();
                        Session["soLuotChoi_km"] = user.soluotchoi_km.ToString();
                        Session["maMoi"] = user.mamoi;
                        Session["taiKhoan"] = user.taikhoan.ToString();
                        //Session["token"] = user.token;
                        Session["checktt"] = user.checktt;
                        string Role = "User";
                        Session["Role"] = Role;
                        int diemdanh = user.diemdanh.Value;
                        if (diemdanh == 1)
                        {
                            return Redirect("~/User/DiemDanh");
                        }
                        else
                        {
                            return Redirect("~/User/userProfile");
                        }
                    }

                }
                return Redirect("~/User/userProfile");
            }
            catch (Exception)
            {
                return Content("<script language='javascript' type='text/javascript'> " +
                            "alert('Lỗi lạ');" +
                            "window.location= '/User/Index';" +
                            "</script>");
            }
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }

        public ActionResult userProfile()
        {
            if (Session["IDs"] != null)
            {
                if (Session["Role"].ToString() == "User")
                {
                    int userID = int.Parse(Session["IDs"].ToString());
                    User user = db.Users.SingleOrDefault(x => x.ID == userID);

                    int online = int.Parse(user.online.ToString());
                    int? fb = int.Parse(user.fb.ToString());
                    if (int.Parse(Session["checktt"].ToString()) == 1)
                    {
                        Session["checktt"] = 0;
                        return RedirectToAction("ShareTrungThuong");
                    }
                    else
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
                        CuocChoi cuocchoi1 = db.CuocChois.OrderByDescending(x => x.MaCuocChoi).Take(1).Single();
                        DateTime? ngaychoi;
                        ngaychoi = cuocchoi1.NgayDoanSo;
                        DateTime datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

                        if (ngaychoi == datetime)
                        {
                            var select = from ds in db.CuocChois
                                         join tt in db.DanhSachTrungThuongs on ds.MaCuocChoi equals tt.MaCuocChoi
                                         where (ds.NgayDoanSo == datetime)
                                         select tt.TongTienThuong;
                            foreach (var i in select)
                            {
                                Session["tt"] = i;
                            }
                            string ttt = Session["tt"].ToString();
                            ViewBag.ttt = ttt;
                        }
                        else {
                            Session["tt"] = null;
                            ViewBag.ttt = null;
                        }
                        string name = Session["userName"].ToString();
                        ViewBag.Name = name;
                        string mail = Session["eMail"].ToString();
                        ViewBag.Mail = mail;
                        string id = Session["IDs"].ToString();
                        ViewBag.Id = id;
                        string luotchoi = user.soluotchoi.ToString();
                        ViewBag.LuotChoi = luotchoi;
                        string luotchoi_km = user.soluotchoi_km.ToString();
                        ViewBag.LuotChoi_km = luotchoi_km;
                        string mamoi = Session["maMoi"].ToString();
                        ViewBag.MaMoi = mamoi;
                        string sodu = user.taikhoan.ToString();
                        ViewBag.SoDu = sodu;

                        if (user.phone == null)
                        {
                            string phone = "Bạn vui lòng xác nhận số điện thoại";
                            ViewBag.phone = phone;
                        }
                        else
                        {
                            string phone = user.phone.ToString();
                            ViewBag.phone = phone;
                        }
                        return View();
                    }
                }
                else return Redirect("Login");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult CachChoi()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);


                string name = Session["userName"].ToString();
                ViewBag.Name = name;
                string mail = Session["eMail"].ToString();
                ViewBag.Mail = mail;
                string id = Session["IDs"].ToString();
                ViewBag.Id = id;
                string luotchoi = Session["soLuotChoi"].ToString();
                ViewBag.LuotChoi = luotchoi;
                string luotchoi_km = Session["soLuotChoi_km"].ToString();
                ViewBag.LuotChoi_km = luotchoi_km;
                string mamoi = Session["maMoi"].ToString();
                ViewBag.MaMoi = mamoi;



                if (String.IsNullOrWhiteSpace(user.phone))
                {
                    string phone = "Bạn vui lòng xác nhận số điện thoại";
                    //string phone = Session["pHone"].ToString();
                    ViewBag.phone = phone;
                }
                else
                {
                    string phone = user.phone.ToString();
                    ViewBag.phone = phone;
                }
                return View();
            }
            else return RedirectToAction("Login");
        }

        public ActionResult reDirect()
        {
            return Redirect("~/User/userProfile");
        }


        [AllowAnonymous]
        public ActionResult CheckUserName(string User_name)
        {
            User user = db.Users.SingleOrDefault(x => x.username == User_name);
            if (user == null)
                return Json("Bạn có thể dùng tài khoản này để đăng ký", JsonRequestBehavior.AllowGet);
            return Json("Đã có người sử dụng tài khoản này", JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult CheckPhone(string Phone)
        {
            int userID = int.Parse(Session["IDs"].ToString());
            User user = db.Users.SingleOrDefault(x => x.phone == Phone);
            if (user == null)
            {
                //string phone = Request.Form["txtPhone"].ToString();
                user = db.Users.SingleOrDefault(x => x.ID == userID);
                user.phone = Phone;
                Session["pHone"] = user.phone;
                db.SaveChanges();
                return Json("Cập nhật số điện thoại thành công", JsonRequestBehavior.AllowGet);
            }
            return Json("Số điện thoại này đã có người dùng", JsonRequestBehavior.AllowGet);
        }


        public ActionResult LichSuDoanSo()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string name = Session["userName"].ToString();
                ViewBag.Name = name;

                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);
                Session["km_view"] = user.soluotchoi_km.ToString();
                Session["ct_view"] = user.soluotchoi.ToString();

                List<ChiTietChoiViewModel> model = new List<ChiTietChoiViewModel>();
                var join = (from US in db.Users
                            join CTC in db.ChiTietCuocChois
                                on US.ID equals CTC.UserID
                            join CC in db.CuocChois on CTC.MaCuocChoi equals CC.MaCuocChoi
                            where US.ID == userID
                            select new
                            {
                                userName = US.username,
                                soDuDoan = CTC.SoDuDoan,
                                ngayDoanSo = CC.NgayDoanSo,
                                trongSo = CTC.TrongSo
                            }).ToList();
                foreach (var item in join)
                {
                    model.Add(new ChiTietChoiViewModel()
                    {
                        username = item.userName,
                        SoDuDoan = item.soDuDoan,
                        NgayDoanSo = item.ngayDoanSo,
                        TrongSo = item.trongSo
                    });
                }

                return View(model);
            }
            else return RedirectToAction("Login");
        }

        bool isNumber(string scr)
        {
            int strLength = scr.Length;
            for (int i = 0; i < strLength; i++)
            {
                if (scr[i] <= '0' || scr[i] > '9')
                    return false;
            }
            return true;
        }
        public ActionResult ThayDoiTrongSo()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string name = Session["userName"].ToString();
                ViewBag.Name = name;

                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);
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
                Session["km_view"] = user.soluotchoi_km.ToString();
                Session["ct_view"] = user.soluotchoi.ToString();
                int machoi = cuocchoi.MaCuocChoi;


                List<ChiTietChoiViewModel> model = new List<ChiTietChoiViewModel>();
                var join = (from US in db.Users
                            join CTC in db.ChiTietCuocChois
                                on US.ID equals CTC.UserID
                            join CC in db.CuocChois on CTC.MaCuocChoi equals CC.MaCuocChoi
                            where US.ID == userID && CTC.MaCuocChoi == machoi
                            select new
                            {
                                userID = US.ID,
                                macuocchoi = CC.MaCuocChoi,
                                userName = US.username,
                                soDuDoan = CTC.SoDuDoan,
                                ngayDoanSo = CC.NgayDoanSo,
                                trongSo = CTC.TrongSo,
                                id = CTC.id
                            }).ToList();
                foreach (var item in join)
                {
                    model.Add(new ChiTietChoiViewModel()
                    {
                        maCuocChoi = item.macuocchoi,
                        userID = userID,
                        username = item.userName,
                        SoDuDoan = item.soDuDoan,
                        NgayDoanSo = item.ngayDoanSo,
                        TrongSo = item.trongSo,
                        id = item.id
                    });


                }

                return View(model);
            }
            else return RedirectToAction("Login");
        }



        [HttpPost]
        public ActionResult ThayDoiTrongSo(int? ts)
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string trongso = Request.Form["ts"].ToString();
                if (isNumber(trongso) == true)
                {
                    LuckyNumContext db = new LuckyNumContext();
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

                    int machoi = cuocchoi.MaCuocChoi;
                    int userID = int.Parse(Session["IDs"].ToString());

                    var selectlist = db.ChiTietCuocChois.Where(a => a.UserID == userID && a.MaCuocChoi == machoi).ToList();
                    int count = selectlist.Count();
                    int? luotchotcu = selectlist.Sum(a => a.TrongSo);
                    int? luotchoimoi = count * ts;

                    User user = db.Users.SingleOrDefault(x => x.ID == userID);

                    int soluotchoi = int.Parse(user.soluotchoi.ToString());
                    int soluotchoi_km = int.Parse(user.soluotchoi_km.ToString());
                    int? sumluotchoi_conlai = soluotchoi + soluotchoi_km;

                    int luotchoiconlai = int.Parse(user.soluotchoi.ToString());
                    if (luotchoimoi > luotchotcu)
                    {
                        if (luotchoimoi <= sumluotchoi_conlai)

                        {
                            foreach (var i in selectlist)
                            {
                                i.TrongSo = ts;
                                db.SaveChanges();
                            }


                            sumluotchoi_conlai = sumluotchoi_conlai + luotchotcu - luotchoimoi;
                            if (sumluotchoi_conlai <= int.Parse(user.soluotchoi.ToString()))
                            {
                                user.soluotchoi = sumluotchoi_conlai;
                                user.soluotchoi_km = 0;
                            }
                            else
                            {
                                user.soluotchoi_km = sumluotchoi_conlai - int.Parse(user.soluotchoi.ToString());
                            }


                            db.SaveChanges();

                            List<ChiTietChoiViewModel> model = new List<ChiTietChoiViewModel>();
                            var join = (from US in db.Users
                                        join CTC in db.ChiTietCuocChois
                                            on US.ID equals CTC.UserID
                                        join CC in db.CuocChois on CTC.MaCuocChoi equals CC.MaCuocChoi
                                        where US.ID == userID && CTC.MaCuocChoi == machoi
                                        select new
                                        {
                                            userID = US.ID,
                                            macuocchoi = CC.MaCuocChoi,
                                            userName = US.username,
                                            soDuDoan = CTC.SoDuDoan,
                                            ngayDoanSo = CC.NgayDoanSo,
                                            trongSo = CTC.TrongSo,
                                            id = CTC.id
                                        }).ToList();
                            foreach (var item in join)
                            {
                                model.Add(new ChiTietChoiViewModel()
                                {
                                    maCuocChoi = item.macuocchoi,
                                    userID = userID,
                                    username = item.userName,
                                    SoDuDoan = item.soDuDoan,
                                    NgayDoanSo = item.ngayDoanSo,
                                    TrongSo = item.trongSo,
                                    id = item.id
                                });


                            }
                            return View(model);
                        }
                        else return Content("<script language='javascript' type='text/javascript'> " +
                            "alert('Tổng trọng số lớn hơn lượt chơi còn lại');" +
                            "window.location= '/User/ThayDoiTrongSo';" +
                            "</script>");
                    }
                    else return Content("<script language='javascript' type='text/javascript'> " +
                           "alert('Không thể thay đổi trọng số nhỏ hơn trọng số cũ.');" +
                           "window.location= '/User/ThayDoiTrongSo';" +
                           "</script>");
                }
                else return Content("<script language='javascript' type='text/javascript'> " +
                        "alert('Bạn phải nhập số mới hợp lệ!!!');" +
                        "window.location= '/User/ThayDoiTrongSo';" +
                        "</script>");
            }
            else return RedirectToAction("Login");

        }


        public ActionResult Edit(int? id)
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ChiTietCuocChoi ct = db.ChiTietCuocChois.SingleOrDefault(m => m.id == id);
                if (ct == null)
                {
                    return HttpNotFound();
                }
                return View(ct);
            }
            else return RedirectToAction ("Login");
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,MaCuocChoi,SoDuDoan,TrongSo,id")] ChiTietCuocChoi ct)
        {

            User user = db.Users.SingleOrDefault(x => x.ID == ct.UserID);

            int soluotchoi = int.Parse(user.soluotchoi.ToString());
            int soluotchoi_km = int.Parse(user.soluotchoi_km.ToString());
            int sumluotchoi = soluotchoi + soluotchoi_km;

            var join = (from chitiet in db.ChiTietCuocChois
                        where chitiet.id == ct.id
                        select new { trongso = chitiet.TrongSo }).ToList();

            foreach (var item in join)
            {
                Session["trongsoold"] = item.trongso;
            }

            int trongsoold = int.Parse(Session["trongsoold"].ToString());

            if (ModelState.IsValid)
            {

                db.Entry(ct).State = EntityState.Modified;

                int trongsonew = int.Parse((ct.TrongSo).ToString());
                int luotchoi = int.Parse(user.soluotchoi.ToString());

                sumluotchoi = sumluotchoi + trongsoold - trongsonew;
                if (trongsonew > trongsoold)
                {
                    if (sumluotchoi > 0)
                    {
                        if (sumluotchoi <= int.Parse(user.soluotchoi.ToString()))
                        {
                            user.soluotchoi = sumluotchoi;
                            user.soluotchoi_km = 0;
                        }
                        else
                        {
                            user.soluotchoi_km = sumluotchoi - int.Parse(user.soluotchoi.ToString());
                        }
                        db.SaveChanges();
                        return Redirect("~/User/ThayDoiTrongSo");
                    }
                    else return Content("<script language='javascript' type='text/javascript'> " +
                            "alert('Không thể thay đổi trọng số. Trọng số lớn hơn lượt còn lại');" +
                            "window.location= '/User/ThayDoiTrongSo';" +
                            "</script>");
                }
                else return Content("<script language='javascript' type='text/javascript'> " +
                        "alert('Trọng số không được nhỏ hơn trọng số cũ');" +
                        "window.location= '/User/ThayDoiTrongSo';" +
                        "</script>");
            }
            return View(ct);
        }


        public ActionResult LichSuTrungThuong()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                //User user = db.Users.SingleOrDefault(x => x.ID == userID);
                string name = Session["userName"].ToString();
                ViewBag.Name = name;


                List<LichSuTrungThuongViewModel> model = new List<LichSuTrungThuongViewModel>();
                var join = (from US in db.Users
                            join CTC in db.ChiTietCuocChois
                                on US.ID equals CTC.UserID
                            join CC in db.CuocChois on CTC.MaCuocChoi equals CC.MaCuocChoi
                            join DSTT in db.DanhSachTrungThuongs on CC.MaCuocChoi equals DSTT.MaCuocChoi
                            join CTTT in db.ChiTietTrungThuongs on DSTT.MaDSTrungThuong equals CTTT.MaDSTrungThuong
                            where US.ID == userID && CTTT.UserID == userID && CTTT.SoDuDoan == CTC.SoDuDoan
                            select new
                            {
                                userName = US.username,
                                soDuDoan = CTC.SoDuDoan,
                                ngayDoanSo = CC.NgayDoanSo,
                                tienThuong = CTTT.TienThuong
                            }).ToList();

                foreach (var item in join)
                {
                    model.Add(new LichSuTrungThuongViewModel()
                    {
                        username = item.userName,
                        NgayDoanSo = item.ngayDoanSo,
                        SoDuDoan = item.soDuDoan,
                        TienThuong = float.Parse(item.tienThuong.ToString())
                    });
                }

                return View(model);
            }
            else return RedirectToAction("Login");
        }

        public ActionResult ThongBao3()
        {
            return View();
        }
        public ActionResult MoiBanBe()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string name = Session["userName"].ToString();
                ViewBag.Name = name;
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                if (user.status == true) return Redirect("~/User/ThongBao3");

                else return View();
            }
            else return RedirectToAction("Login");
        }

        public ActionResult LoiMoiBan()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
                return View();
            else return RedirectToAction("Login");
        }

        public ActionResult MoiBan()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                string maMoi = Request.Form["MaMoi"];

                var list = from u in db.Users
                           select u;

                foreach (var i in list)
                {
                    if (i.mamoi == maMoi)
                    {
                        user.soluotchoi += 5;
                        user.status = true;
                        i.soluotchoi += 5;
                        Session["soLuotChoi"] = user.soluotchoi;
                        db.SaveChanges();
                        return Redirect("~/User/userProfile");
                    }
                }
                return Redirect("~/User/LoiMoiBan");
            }
            else return RedirectToAction("Login");
        }


        public ActionResult DoiThuongPage()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                string name = Session["userName"].ToString();
                ViewBag.Name = name;

                string id = Session["IDs"].ToString();
                ViewBag.Id = id;

                string taikhoan = Session["taiKhoan"].ToString();
                ViewBag.TaiKhoan = taikhoan;
                return View();
            }
            else return RedirectToAction("Login");
        }


        [AllowAnonymous]
        public ActionResult DoiThuong()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                int giatri = int.Parse(Request.Form["msi"].ToString());
                if (user.taikhoan >= giatri)
                {
                    user.taikhoan -= giatri;

                    StringBuilder sb = new StringBuilder();
                    char c;
                    string c1;
                    //string phone = Session["pHone"].ToString();
                    Random rand = new Random();
                    for (int i = 0; i < 10; i++)
                    {
                        c = Convert.ToChar(Convert.ToInt32(rand.Next(65, 87)));
                        sb.Append(c);
                    }

                    c1 = sb.ToString();

                    Session["taiKhoan"] = user.taikhoan;

                    db.SaveChanges();

                    return Json("Mã thẻ của bạn là: " + c1 +
                    " Bạn vui lòng ghi lại mã thẻ trước khi trở về!", JsonRequestBehavior.AllowGet);

                    //return Json("Tài khoản của bạn hiện tại bé hơn giá trị thẻ bạn chọn!", JsonRequestBehavior.AllowGet);
                }

                return Json("Tài khoản của bạn không đủ tiền", JsonRequestBehavior.AllowGet);
            }
            else return RedirectToAction("Login");
        }

        public ActionResult XacNhanSDT()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string name = Session["userName"].ToString();
                ViewBag.Name = name;
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);
                if (user.phone != null)
                {
                    return Redirect("~/User/ThongBao_1");
                }

                return View();
            }
            else return RedirectToAction("Login");
        }


        public ActionResult ThongBao2()
        {
            return View();
        }
        public ActionResult XacNhanSoDT()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                string sdt = Request.Form["sdt"].ToString();

                int userID = int.Parse(Session["IDs"].ToString());
                //User user = db.Users.SingleOrDefault(x => x.ID == userID);
                User user = db.Users.SingleOrDefault(x => x.phone == sdt);
                if (user == null)
                {
                    user = db.Users.SingleOrDefault(x => x.ID == userID);
                    user.phone = sdt;
                    Session["pHone"] = user.phone;
                    db.SaveChanges();
                    return Redirect("~/User/userProfile");
                }



                return Redirect("~/User/ThongBao2"); // chỗ này nhớ viết
            }
            else return RedirectToAction("Login");
        }

        public ActionResult NhapGioiThieu()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);
                string MaGT = Request.Form["MaGioiThieu"].ToString();
                var select = db.Users.ToList();
                int temp = 0;
                int count = select.Count();
                foreach (var i in select)
                {
                    if (i.mamoi == MaGT && user.mamoi != MaGT)
                    {
                        User us2 = db.Users.SingleOrDefault(x => x.mamoi == MaGT);
                        int temp_km = int.Parse(us2.soluotchoi_km.ToString()) + 5;
                        if (temp_km < 30)
                        {
                            us2.soluotchoi_km = temp_km;
                        }
                        else us2.soluotchoi_km = 30;

                        user.magioithieu = MaGT;
                        user.soluotchoi_km += 5;
                        user.status = true;
                        db.SaveChanges();
                    }
                    else if (i.mamoi != MaGT || user.mamoi == MaGT)
                    {
                        temp++;
                    }
                }

                if (temp == count)
                    return RedirectToAction("LoiMoiBan");
                return RedirectToAction("userProfile");
            }
            else return RedirectToAction("Login");
        }

        public ActionResult DoiMatKhau()
        {
            if (Session["userName"] != null && Session["Role"].ToString() == "User")
            {
                return View();
            }
            else return RedirectToAction("Login");
        }

        public ActionResult ChangePass()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);

                string PassOld = Request.Form["txtPassOld"].ToString();
                string Pass = Request.Form["txtPass"].ToString();
                string RePass = Request.Form["txtRePass"].ToString();
                if (user.password != PassOld)
                {
                    return Content("<script language='javascript' type='text/javascript'> " +
                        "alert('Mật khẩu cũ không chính xác. Vui lòng thử lại');" +
                        "window.location= '/User/userProfile';" +
                        "</script>");
                }
                else if (RePass != Pass)
                {
                    return Content("<script language='javascript' type='text/javascript'> " +
                        "alert('Vui lòng nhập lại mật khẩu trùng với mật khẩu mới.');" +
                        "window.location= '/User/userProfile';" +
                        "</script>");
                }
                else
                {
                    user.password = Pass;
                    db.SaveChanges();
                    return Redirect("~/User/userProfile");
                }
            }
            else return RedirectToAction("Login");
        }

        public ActionResult ChangeName()
        {
            if (Session["IDs"] != null && Session["Role"].ToString() == "User")
            {
                int userID = int.Parse(Session["IDs"].ToString());
                User user = db.Users.SingleOrDefault(x => x.ID == userID);
                string newname = Request.Form["newname"].ToString();

                user.nickname = newname;
                Session["userName"] = user.nickname;
                db.SaveChanges();
                return Redirect("~/User/userProfile");
            }
            else
                return RedirectToAction("Login");
        }

        public ActionResult ShareFacebook()
        {
                return View();
        }    
    }
}