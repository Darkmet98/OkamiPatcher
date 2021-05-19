using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Mara.Lib;
using Mara.Lib.Common;
using Mara.Lib.Platforms.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace OkamiPatcher.Controllers
{
    public class HomeController : Controller
    {
        public static Main Core { get; set; }

        // GET: HomeController
        public ActionResult Index()
        {
            return View(this);
        }

        public ActionResult CheckInternet()
        {
            return Json(new { check = true });
        }

        public void OpenView(string url)
        {
            OpenAsync(url);
        }

        private async void OpenAsync(string url)
        {
            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Width = 1260,
                Height = 700,
                Show = false,
                MinWidth = 1260,
                MinHeight = 700,
            }, $"http://localhost:{BridgeSettings.WebPort}/{url}");

            browserWindow.OnReadyToShow += () =>
            {
                browserWindow.Show();
            };
        }
    }
}
