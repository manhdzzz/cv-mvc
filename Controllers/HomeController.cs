using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using trangcv.Models;
using trangcv.Services;

namespace trangcv.Controllers
{
    public class HomeController : Controller
    {
        private readonly CVService _cvService;

        public HomeController(CVService cvService)
        {
            _cvService = cvService;
        }

        /// <summary>
        /// Trang chủ - Hiển thị CV
        /// </summary>
        public IActionResult Index()
        {
            var cv = _cvService.GetCV();
            ViewBag.NCKHList = _cvService.GetNCKHList(cv.NCKH);
            ViewBag.ProjectList = _cvService.GetProjectList(cv.DuAn);
            return View(cv);
        }

        /// <summary>
        /// Form chỉnh sửa CV
        /// </summary>
        public IActionResult Edit()
        {
            var cv = _cvService.GetCV();
            ViewBag.NCKHList = _cvService.GetNCKHList(cv.NCKH);
            ViewBag.ProjectList = _cvService.GetProjectList(cv.DuAn);
            return View(cv);
        }

        /// <summary>
        /// Lưu CV từ form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CVModel cv, string NCKHJson, string DuAnJson)
        {
            // Lưu JSON cho NCKH và Dự án
            cv.NCKH = NCKHJson ?? "[]";
            cv.DuAn = DuAnJson ?? "[]";

            _cvService.SaveCV(cv);
            TempData["Message"] = "Lưu CV thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
