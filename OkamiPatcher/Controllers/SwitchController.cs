using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Mara.Lib.Common;
using Mara.Lib.Platforms.Switch;

namespace OkamiPatcher.Controllers
{
    public class SwitchController : Controller
    {
        public static string Folder { get; set; }
        public static string Keys { get; set; }
        public static bool ListenerEnabled { get; set; }
        private bool piracy;
        private string url = "https://github.com/TraduSquare/Parches/releases/download/Okami/okamiswitch.zip";
        private string downloadPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}Okamiswitch.zip";
        private static Main patchProcess;

        public static string[] titleIds { get; } = {
            "0100F10009870000",
            "0100276009872000"
        };

        public ActionResult Index()
        {
            Folder = string.Empty;
            Keys = string.Empty;
            if (!HybridSupport.IsElectronActive) 
                return View(this);

            if (ListenerEnabled)
                return View(this);

            Electron.IpcMain.On("select-gamePath", async (args) => {
                var mainWindow = Electron.WindowManager.BrowserWindows.Last();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openFile
                    },
                    Filters = new []
                    {
                        new FileFilter()
                        {
                            Name = "Juego Ōkami HD",
                            Extensions = new []
                            {
                                "nsp",
                                "xci"
                            }
                        }
                    }
                };

                string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                if (files.Length != 0)
                    Folder = files[0];
                Electron.IpcMain.Send(mainWindow, "select-game-reply", files);
            });

            Electron.IpcMain.On("select-keys", async (args) => {
                var mainWindow = Electron.WindowManager.BrowserWindows.Last();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openFile
                    },
                    Filters = new[]
                    {
                        new FileFilter()
                        {
                            Name = "Keys Nintendo Switch",
                            Extensions = new []
                            {
                                "txt",
                                "dat",
                                "ini",
                                "keys"
                            }
                        }
                    }
                };

                string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                if (files.Length != 0)
                    Keys = files[0];
                Electron.IpcMain.Send(mainWindow, "select-keys-reply", files);
            });
            ListenerEnabled = true;
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
            var outPath = Path.GetDirectoryName(Folder);
            try
            {
                patchProcess = new Main(Folder, outPath, downloadPath, Keys,
                    Folder.Contains(".xci") ? titleIds[0] : titleIds[1]);
            }
            catch (Exception e)
            {
                if (e.Message == "Invalid Cert Signature." && e.Message == "Invalid ticket Signature.")
                    return Problem($"Se ha producido un error extrayendo los archivos.\n{e.Message}\n{e.StackTrace}");

                try
                {
                    piracy = true;
                    patchProcess = new Main(Folder, outPath, downloadPath, Keys, Folder.Contains(".xci") ? titleIds[0] : titleIds[1], false, false);
                    return Ok("piracy");
                }
                catch (Exception exception)
                {
                    return Problem($"Se ha producido un error extrayendo los archivos.\n{exception.Message}\n{exception.StackTrace}");
                }
               
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
            if (string.IsNullOrWhiteSpace(Folder) || string.IsNullOrWhiteSpace(Keys))
                return Problem();
            return Ok();
        }
    }
}
