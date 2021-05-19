using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OkamiPatcher.Controllers
{
    public class EpilogueController : Controller
    {
        public ActionResult Index()
        {
            return View(this);
        }
    }
}
