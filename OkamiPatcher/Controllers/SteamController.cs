using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using System.IO;
using Mara.Lib.Common;
using Mara.Lib.Platforms.Generic;

namespace OkamiPatcher.Controllers
{
    public class SteamController : Controller
    {
        public static string Folder { get; set; }
        public static bool ListenerEnabled { get; set; }
        private string url = "https://tradusquare.es/okami/okamipc.zip";
        private static Main patchProcess;
        private string downloadPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}Okamipc.zip";

        public ActionResult Index()
        {
            if (HybridSupport.IsElectronActive && !ListenerEnabled)
            {
                Electron.IpcMain.On("select-gameFolder", async (args) => {
                    var mainWindow = Electron.WindowManager.BrowserWindows.Last();
                    var options = new OpenDialogOptions
                    {
                        Properties = new OpenDialogProperty[] {
                            OpenDialogProperty.openDirectory
                        }
                    };

                    string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                    if (files.Length != 0)
                        Folder = files[0];
                    Electron.IpcMain.Send(mainWindow, "select-game-reply", files);
                });
                ListenerEnabled = true;
            }

            return View(this);
        }

        

        public ActionResult DownloadPatch()
        {
            try
            {
                if (System.IO.File.Exists(downloadPath))
                    System.IO.File.Delete(downloadPath);

                Internet.GetFile(url, downloadPath);
            }
            catch (Exception e)
            {
                return Problem($"Se ha producido un error descargando los archivos.\n{e.Message}\n{e.StackTrace}");
            }
            return Ok();
        }

        public ActionResult ExtractPatch()
        {
            try
            {
                patchProcess = new Main(Folder, Folder, downloadPath);
            }
            catch (Exception e)
            {
                return Problem($"Se ha producido un error extrayendo los archivos.\n{e.Message}\n{e.StackTrace}");
            }
            return Ok();
        }

        public ActionResult PatchGame()
        {
            try
            {
                var result = patchProcess.ApplyTranslation();
                if (result.Item1 == 0)
                    return Ok();
                return Problem(
                    $"Se ha producido un error aplicando la traducción.\nError: {result.Item1}\nMensaje: {result.Item2}");
            }
            catch (Exception e)
            {
                return Problem($"Se ha producido un error aplicando la traducción.\nError: INTERNAL CRASH\nMensaje:\n{e.Message}\n{e.StackTrace}");
            }
        }


        public ActionResult CheckGame()
        {
            var files = new string[]
            {
                "steam_api64.dll",
                "okami.exe",
                "main.dll",
                "flower_kernel.dll",
                "app_install.vdf"
            };

            if (files.Any(file => !System.IO.File.Exists($"{Folder}{Path.DirectorySeparatorChar}{file}")))
            {
                return Problem();
            }

            if (!Directory.Exists($"{Folder}{Path.DirectorySeparatorChar}data_pc"))
                return Problem();

            return Ok();
        }
    }
}
