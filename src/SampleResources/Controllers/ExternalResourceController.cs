using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleResources.Controllers
{
    public class ExternalResourceController : Controller
    {
        private readonly IStringLocalizer<ExternalResourceController> localizer;

        public ExternalResourceController(IStringLocalizer<ExternalResourceController> localizer)
        {
            this.localizer = localizer;
        }

        public IActionResult Index()
        {
            ViewBag.ControllerLocalizer = this.localizer["ExternalResourceController"];
            return View();
        }
    }
}
