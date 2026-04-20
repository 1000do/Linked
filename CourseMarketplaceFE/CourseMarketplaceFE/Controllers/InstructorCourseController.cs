using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceFE.Models;
using System;
using System.Collections.Generic;

namespace CourseMarketplaceFE.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorCourseController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Fetch courses from backend API using HttpClient or IServices
            var mockCourses = new List<CourseListViewModel> {
                new CourseListViewModel { Id = 1, Title = "Advanced React Patterns", Students = 4290, Rating = 4.9, Status = "Published", ThumbnailUrl = "", UpdatedAt = DateTime.Now.AddDays(-10) },
                new CourseListViewModel { Id = 2, Title = "Data Science with Python", Students = 8102, Rating = 4.8, Status = "Published", ThumbnailUrl = "", UpdatedAt = DateTime.Now.AddDays(-20) },
                new CourseListViewModel { Id = 3, Title = "UI/UX Design Systems", Students = 0, Rating = 0.0, Status = "Draft", ThumbnailUrl = "", UpdatedAt = DateTime.Now.AddDays(-2) }
            };
            return View(mockCourses);
        }

        public IActionResult Create()
        {
            return View(new CreateCourseViewModel());
        }

        [HttpPost]
        public IActionResult Create(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle course creation logic
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editor(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
    }
}
