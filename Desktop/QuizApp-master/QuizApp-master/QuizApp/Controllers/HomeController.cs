using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> _logger)
    {
        this._logger = _logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // ErrorViewModel'i sildiğimiz için sadece View'ı döndürüyoruz
        return View();
    }
}