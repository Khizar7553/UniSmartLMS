using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class CampusController : Controller
    {
        private readonly IGenericRepository<Campus> _campusRepo;
        private readonly IGenericRepository<University> _universityRepo;

        public CampusController(
            IGenericRepository<Campus> campusRepo,
            IGenericRepository<University> universityRepo)
        {
            _campusRepo = campusRepo;
            _universityRepo = universityRepo;
        }

        // GET: Campus
        public async Task<IActionResult> Index()
        {
            var campuses = await _campusRepo.GetAllAsync();
            foreach (var campus in campuses)
            {
                campus.University = await _universityRepo.GetByIdAsync(campus.UniversityId);
            }
            return View(campuses);
        }

        // GET: Campus/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _campusRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var campus = await _campusRepo.GetByIdAsync(id);
            if (campus == null) return NotFound();
            // Manually load University
            campus.University = await _universityRepo.GetByIdAsync(campus.UniversityId);
            return View(campus);
        }

        public async Task<IActionResult> Create()
        {
            await LoadUniversityDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Campus campus)
        {
            ModelState.Remove("University");

            if (ModelState.IsValid)
            {
                await _campusRepo.AddAsync(campus);
                TempData["Success"] = "Campus created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadUniversityDropdown(campus.UniversityId);
            return View(campus);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var campus = await _campusRepo.GetByIdAsync(id);
            if (campus == null) return NotFound();
            await LoadUniversityDropdown(campus.UniversityId);
            return View(campus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Campus campus)
        {
            if (id != campus.Id) return NotFound();

            ModelState.Remove("University");

            if (ModelState.IsValid)
            {
                await _campusRepo.UpdateAsync(campus);
                TempData["Success"] = "Campus updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadUniversityDropdown(campus.UniversityId);
            return View(campus);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var campus = await _campusRepo.GetByIdAsync(id);
            if (campus == null) return NotFound();
            campus.University = await _universityRepo.GetByIdAsync(campus.UniversityId);

            return View(campus);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _campusRepo.DeleteAsync(id);
            TempData["Success"] = "Campus deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadUniversityDropdown(object selectedUniversity = null)
        {
            var universities = await _universityRepo.GetAllAsync();
            ViewBag.UniversityList = new SelectList(universities, "Id", "Name", selectedUniversity);
        }
    }
}