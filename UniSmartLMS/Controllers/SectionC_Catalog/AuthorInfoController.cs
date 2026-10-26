using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionC_Catalog
{
    [Authorize(Roles = "Admin,Librarian")]
    public class AuthorInfoController : Controller
    {
        private readonly IGenericRepository<AuthorInfo> _repository;

        public AuthorInfoController(IGenericRepository<AuthorInfo> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _repository.GetAllAsync();
            return View(authors);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _repository.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var author = await _repository.GetByIdAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorInfo author)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(author);
                TempData["Success"] = "Author created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var author = await _repository.GetByIdAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuthorInfo author)
        {
            if (id != author.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(author);
                TempData["Success"] = "Author updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var author = await _repository.GetByIdAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Author deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}