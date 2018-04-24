using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MusicPlayer.Presentation.Web.Controllers
{
    public class PlayerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PlayPause()
        {
            return View();
        }

        public IActionResult SkipForward()
        {
            return View();
        }

        public IActionResult SkipBackward()
        {
            return View();
        }

        public IActionResult ToggleShuffle()
        {
            return View();
        }
    }
}