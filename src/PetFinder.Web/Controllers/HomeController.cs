using Microsoft.AspNetCore.Mvc;

namespace PetFinder.Web.Controllers;

/// <summary>Acciones públicas de inicio de PetFinder.</summary>
public sealed class HomeController : Controller
{
    /// <summary>Presenta la landing pública.</summary>
    public IActionResult Index() => View();
}
