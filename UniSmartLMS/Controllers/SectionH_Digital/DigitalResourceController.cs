using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionH_Digital;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionH_Digital
{
    [Authorize]
    public class DigitalResourceController : Controller
    {
        private readonly IGenericRepository<DigitalResource> _resourceRepo;
        private readonly IGenericRepository<ItemInfo> _itemRepo;

        public DigitalResourceController(
            IGenericRepository<DigitalResource> resourceRepo,
            IGenericRepository<ItemInfo> itemRepo)
        {
            _resourceRepo = resourceRepo;
            _itemRepo = itemRepo;
        }

        public async Task<IActionResult> Index()
        {
            var resources = await _resourceRepo.GetAllAsync();
            foreach (var resource in resources)
            {
                resource.ItemInfo = await _itemRepo.GetByIdAsync(resource.ItemInfoId);
            }
            return View(resources);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _resourceRepo.SearchAsync("FileFormat", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var resource = await _resourceRepo.GetByIdAsync(id);
            if (resource == null) return NotFound();
            resource.ItemInfo = await _itemRepo.GetByIdAsync(resource.ItemInfoId);
            return View(resource);
        }

        public async Task<IActionResult> Create()
        {
            await LoadItemDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DigitalResource resource)
        {
            ModelState.Remove("ItemInfo");

            if (ModelState.IsValid)
            {
                await _resourceRepo.AddAsync(resource);
                TempData["Success"] = "Digital Resource created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadItemDropdown(resource.ItemInfoId);
            return View(resource);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _resourceRepo.GetByIdAsync(id);
            if (resource == null) return NotFound();
            await LoadItemDropdown(resource.ItemInfoId);
            return View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DigitalResource resource)
        {
            if (id != resource.Id) return NotFound();

            ModelState.Remove("ItemInfo");

            if (ModelState.IsValid)
            {
                await _resourceRepo.UpdateAsync(resource);
                TempData["Success"] = "Digital Resource updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadItemDropdown(resource.ItemInfoId);
            return View(resource);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _resourceRepo.GetByIdAsync(id);
            if (resource == null) return NotFound();
            resource.ItemInfo = await _itemRepo.GetByIdAsync(resource.ItemInfoId);
            return View(resource);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _resourceRepo.DeleteAsync(id);
            TempData["Success"] = "Digital Resource deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadItemDropdown(object selected = null)
        {
            var items = await _itemRepo.GetAllAsync();
            ViewBag.ItemList = new SelectList(items, "Id", "Title", selected);
        }
    }
}
