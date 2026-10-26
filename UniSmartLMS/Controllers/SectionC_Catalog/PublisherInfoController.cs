using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionC_Catalog
{
    [Authorize(Roles = "Admin,Librarian")]
    public class PublisherInfoController : Controller
    {
        private readonly IGenericRepository<PublisherInfo> _repository;

        public PublisherInfoController(IGenericRepository<PublisherInfo> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var publishers = await _repository.GetAllAsync();
            return View(publishers);
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
            var publisher = await _repository.GetByIdAsync(id);
            if (publisher == null) return NotFound();
            return View(publisher);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublisherInfo publisher)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(publisher);
                TempData["Success"] = "Publisher created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var publisher = await _repository.GetByIdAsync(id);
            if (publisher == null) return NotFound();
            return View(publisher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PublisherInfo publisher)
        {
            if (id != publisher.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(publisher);
                TempData["Success"] = "Publisher updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var publisher = await _repository.GetByIdAsync(id);
            if (publisher == null) return NotFound();
            return View(publisher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Publisher deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}