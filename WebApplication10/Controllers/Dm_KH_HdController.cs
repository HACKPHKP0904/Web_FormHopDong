using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebApplication10.Models;

namespace WebApplication10.Controllers
{
    public class Dm_KH_HdController : Controller
    {
        private readonly string connectionString = "Data Source=118.69.109.109;Initial Catalog=SAP_OPC;User ID=sa;Password=Hai@thong";

        // GET: Dm_KH_Hd
        public ActionResult Index()
        {
            List<HopDong> hopDongs = new List<HopDong>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM B20Dm_HopDong", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    HopDong hopDong = new HopDong
                    {
                        Ma_Dt = reader["Ma_Dt"].ToString(),
                        Ma_Dvcs = reader["Ma_Dvcs"].ToString(),
                        So_Hop_Dong = reader["So_Hop_Dong"].ToString(),
                    };

                    hopDongs.Add(hopDong);
                }

                reader.Close();
            }

            ViewBag.DvcsList = LoadChiNhanh(); // Load danh sách chi nhánh

            return View(hopDongs);
        }


        // GET: Dm_KH_Hd/Create
        public ActionResult Create(string maDvcs)
        {
            var model = new HopDong
            {
                Ma_Dvcs = maDvcs, // Truyền mã chi nhánh từ Index qua Create
                DvcsList = LoadChiNhanh(), // Load danh sách chi nhánh
                MaDtList = GetMaDtForBranch(maDvcs) // Load danh sách mã đối tượng dựa trên mã chi nhánh
            };

            return View(model);
        }

        // POST: Dm_KH_Hd/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HopDong model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string maDvcs = MapBranchNameToCode(model.Ma_Dvcs);
                    SqlCommand command = new SqlCommand("INSERT INTO B20Dm_HopDong (Ma_Dvcs, Ma_Dt, So_Hop_Dong) VALUES (@Ma_Dvcs, @Ma_Dt, @So_Hop_Dong)", connection);
                    command.Parameters.AddWithValue("@Ma_Dvcs", maDvcs);
                    command.Parameters.AddWithValue("@Ma_Dt", model.Ma_Dt);
                    command.Parameters.AddWithValue("@So_Hop_Dong", model.So_Hop_Dong);

                    command.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "Thêm hợp đồng thành công!";
                return RedirectToAction("Index");
            }

            model.DvcsList = LoadChiNhanh(); // Load lại danh sách chi nhánh
            model.MaDtList = GetMaDtForBranch(model.Ma_Dvcs); // Load lại danh sách mã đối tượng dựa trên mã chi nhánh

            return View(model);
        }

        // Method to get MaDtList for dropdown based on Ma_Dvcs
        private List<SelectListItem> GetMaDtForBranch(string maDvcs)
        {
            string dvcs = GetDvcs1FromDvcs(maDvcs); // Lấy Dvcs1 từ mã chi nhánh

            // Kiểm tra xem Dvcs của mã chi nhánh có giống với Dvcs trong cơ sở dữ liệu không
            if (!string.IsNullOrEmpty(dvcs))
            {
                List<SelectListItem> maDtList = new List<SelectListItem>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_DmDtTdv_SAP_MauIn", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@_Ma_Dvcs", dvcs);

                    connection.Open();
                    command.CommandTimeout = 950;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string maDt = reader["Ma_Dt"].ToString();
                        string tenDt = reader["Ten_Dt"].ToString();

                        maDtList.Add(new SelectListItem
                        {
                            Text = $"{maDt} - {tenDt}",
                            Value = maDt
                        });
                    }

                    reader.Close();
                }

                return maDtList;
            }

            return new List<SelectListItem>(); // Trả về danh sách rỗng nếu không tìm thấy Dvcs tương ứng
        }

        // Predefined method to load branches
        private List<SelectListItem> LoadChiNhanh()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Chi Nhánh Cần Thơ", Value = "Chi Nhánh Cần Thơ" },
                new SelectListItem { Text = "Chi Nhánh Miền Đông", Value = "Chi Nhánh Miền Đông" },
                new SelectListItem { Text = "Chi Nhánh Đà Nẵng", Value = "Chi Nhánh Đà Nẵng" },
                new SelectListItem { Text = "Chi Nhánh Hà Nội", Value = "Chi Nhánh Hà Nội" },
                new SelectListItem { Text = "Chi Nhánh Hồ Chí Minh", Value = "Chi Nhánh Hồ Chí Minh" },
                new SelectListItem { Text = "Chi Nhánh Nha Trang", Value = "Chi Nhánh Nha Trang" },
                new SelectListItem { Text = "Chi Nhánh Nghệ An", Value = "Chi Nhánh Nghệ An" },
                new SelectListItem { Text = "Chi Nhánh Tiền Giang", Value = "Chi Nhánh Tiền Giang" },
                new SelectListItem { Text = "Chi Nhánh Vũng Tàu", Value = "Chi Nhánh Vũng Tàu" }
            };
        }

        // Predefined method to map branch to Dvcs1
        private string GetDvcs1FromDvcs(string maDvcs)
        {
            Dictionary<string, string> chiNhanhMap = new Dictionary<string, string>
            {
                { "Chi Nhánh Cần Thơ", "OPCCNCT" },
                { "Chi Nhánh Miền Đông", "OPCCNMD" },
                { "Chi Nhánh Đà Nẵng", "OPCCNDN" },
                { "Chi Nhánh Hà Nội", "OPCCNHN" },
                { "Chi Nhánh Hồ Chí Minh", "OPCCNHCM" },
                { "Chi Nhánh Nha Trang", "OPCCNNT" },
                { "Chi Nhánh Nghệ An", "OPCCNNA" },
                { "Chi Nhánh Tiền Giang", "OPCCNTG" },
                { "Chi Nhánh Vũng Tàu", "OPCCNVT" }
            };

            return chiNhanhMap.ContainsKey(maDvcs) ? chiNhanhMap[maDvcs] : string.Empty;
        }

        public ActionResult LoadObjects(string maDvcs, string searchTerm)
        {
            List<SelectListItem> maDtList = new List<SelectListItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("usp_DmDtTdv_SAP_MauIn", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Thêm tham số vào stored procedure nếu cần thiết
                // command.Parameters.AddWithValue("@ParameterName", parameterValue);

                connection.Open();
                command.CommandTimeout = 950;
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string dvcs = reader["Dvcs"].ToString().Trim();
                    string maDt = reader["Ma_Dt"].ToString();
                    string tenDt = reader["Ten_Dt"].ToString();

                    // Lọc dữ liệu trong C#
                    if (dvcs.Equals(maDvcs.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        (string.IsNullOrEmpty(searchTerm) || maDt.ToLower().Contains(searchTerm.ToLower()) || tenDt.ToLower().Contains(searchTerm.ToLower())))
                    {
                        maDtList.Add(new SelectListItem
                        {
                            Text = $"{maDt} - {tenDt}",
                            Value = maDt
                        });
                    }
                }

                reader.Close();
            }

            return Json(maDtList.Select(item => new { item.Text, item.Value }), JsonRequestBehavior.AllowGet);
        }

        private string MapBranchNameToCode(string branchName)
        {
            switch (branchName)
            {
                case "Chi Nhánh Cần Thơ":
                    return "OPC_CT";
                case "Chi Nhánh Miền Đông":
                    return "OPC_MD";
                case "Chi Nhánh Đà Nẵng":
                    return "OPC_DN";
                case "Chi Nhánh Hà Nội":
                    return "OPC_HN";
                case "Chi Nhánh Hồ Chí Minh":
                    return "OPC_TP";
                case "Chi Nhánh Nha Trang":
                    return "OPC_NT";
                case "Chi Nhánh Nghệ An":
                    return "OPC_NA";
                case "Chi Nhánh Tiền Giang":
                    return "OPC_TG";
                case "Chi Nhánh Vũng Tàu":
                    return "OPC_VT";
                default:
                    return "";
            }
        }
        /// EDIT
        // GET: Dm_KH_Hd/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm hợp đồng cần sửa trong cơ sở dữ liệu theo id (ma_dt)
            HopDong hopDong;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM B20Dm_HopDong WHERE Ma_Dt = @Ma_Dt", conn);
                cmd.Parameters.AddWithValue("@Ma_Dt", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    hopDong = new HopDong
                    {
                        Id = (int)reader["Id"],
                        Ma_Dt = reader["Ma_Dt"].ToString(),
                        Ma_Dvcs = reader["Ma_Dvcs"].ToString(),
                        So_Hop_Dong = reader["So_Hop_Dong"].ToString()
                    };
                }
                else
                {
                    reader.Close();
                    return HttpNotFound();
                }

                reader.Close();
            }

            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HopDong model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("UPDATE B20Dm_HopDong SET So_Hop_Dong = @So_Hop_Dong WHERE Ma_Dt = @Ma_Dt", connection);
                    command.Parameters.AddWithValue("@So_Hop_Dong", model.So_Hop_Dong);
                    command.Parameters.AddWithValue("@Ma_Dt", model.Ma_Dt); // Ensure Ma_Dt is set correctly

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Sửa thành công hợp đồng!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sửa không thành công hợp đồng!";
                    }
                }

                // Redirect to the index page after successful update
                return RedirectToAction("Index");
            }

            // If ModelState is not valid, return the view with errors
            return View(model);
        }

        // GET: Dm_KH_Hd/Delete/5
        // Action để hiển thị form xóa hợp đồng
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            HopDong hopDong;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM B20Dm_HopDong WHERE Ma_Dt = @Ma_Dt", conn);
                cmd.Parameters.AddWithValue("@Ma_Dt", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    hopDong = new HopDong
                    {
                        Id = (int)reader["Id"],
                        Ma_Dt = reader["Ma_Dt"].ToString(),
                        Ma_Dvcs = reader["Ma_Dvcs"].ToString(),
                        So_Hop_Dong = reader["So_Hop_Dong"].ToString(),
                        IsActive = DBNull.Value.Equals(reader["IsActive"]) ? false : Convert.ToBoolean(reader["IsActive"])
                    };

                    reader.Close();

                    return View(hopDong); // Trả về view với dữ liệu của hợp đồng để xác nhận xóa
                }
                else
                {
                    reader.Close();
                    return HttpNotFound();
                }
            }
        }

        // Hành động xác nhận xóa hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM B20Dm_HopDong WHERE Ma_Dt = @Ma_Dt", conn);
                cmd.Parameters.AddWithValue("@Ma_Dt", id);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Hợp đồng đã được xóa thành công.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa hợp đồng thất bại.";
                    return RedirectToAction("Index");
                }
            }
        }
        // bộ lọc 
        // GET: Dm_KH_Hd/FilterByBranch
        // Action để lọc hợp đồng theo tên chi nhánh

    }

}