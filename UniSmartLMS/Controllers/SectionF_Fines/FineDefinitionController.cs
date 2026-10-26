using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionF_Fines
{
    [Authorize(Roles = "Admin,Librarian")]
    public class FineDefinitionController : Controller
    {
        private readonly IGenericRepository<FineDefinition> _definitionRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IGenericRepository<FineCategory> _categoryRepo;

        public FineDefinitionController(
            IGenericRepository<FineDefinition> definitionRepo,
            IGenericRepository<ItemCopy> copyRepo,
            IGenericRepository<FineCategory> categoryRepo)
        {
            _definitionRepo = definitionRepo;
            _copyRepo = copyRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index()
        {
            var definitions = await _definitionRepo.GetAllAsync();
            foreach (var def in definitions)
            {
                def.ItemCopy = await _copyRepo.GetByIdAsync(def.ItemCopyId);
                def.FineCategory = await _categoryRepo.GetByIdAsync(def.FineCategoryId);
            }
            return View(definitions);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _definitionRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var definition = await _definitionRepo.GetByIdAsync(id);
            if (definition == null) return NotFound();
            definition.ItemCopy = await _copyRepo.GetByIdAsync(definition.ItemCopyId);
            definition.FineCategory = await _categoryRepo.GetByIdAsync(definition.FineCategoryId);
            return View(definition);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCopyDropdown();
            await LoadCategoryDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FineDefinition definition)
        {
            ModelState.Remove("ItemCopy");
            ModelState.Remove("FineCategory");

            if (ModelState.IsValid)
            {
                await _definitionRepo.AddAsync(definition);
                TempData["Success"] = "Fine Definition created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadCopyDropdown(definition.ItemCopyId);
            await LoadCategoryDropdown(definition.FineCategoryId);
            return View(definition);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var definition = await _definitionRepo.GetByIdAsync(id);
            if (definition == null) return NotFound();
            await LoadCopyDropdown(definition.ItemCopyId);
            await LoadCategoryDropdown(definition.FineCategoryId);
            return View(definition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FineDefinition definition)
        {
            if (id != definition.Id) return NotFound();

            ModelState.Remove("ItemCopy");
            ModelState.Remove("FineCategory");

            if (ModelState.IsValid)
            {
                await _definitionRepo.UpdateAsync(definition);
                TempData["Success"] = "Fine Definition updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadCopyDropdown(definition.ItemCopyId);
            await LoadCategoryDropdown(definition.FineCategoryId);
            return View(definition);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var definition = await _definitionRepo.GetByIdAsync(id);
            if (definition == null) return NotFound();
            definition.ItemCopy = await _copyRepo.GetByIdAsync(definition.ItemCopyId);
            definition.FineCategory = await _categoryRepo.GetByIdAsync(definition.FineCategoryId);
            return View(definition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _definitionRepo.DeleteAsync(id);
            TempData["Success"] = "Fine Definition deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCopyDropdown(object selected = null)
        {
            var copies = await _copyRepo.GetAllAsync();
            ViewBag.CopyList = new SelectList(copies, "Id", "Barcode", selected);
        }

        private async Task LoadCategoryDropdown(object selected = null)
        {
            var categories = await _categoryRepo.GetAllAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "FineCategoryName", selected);
        }
    }
}