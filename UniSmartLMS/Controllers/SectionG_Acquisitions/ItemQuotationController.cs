using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionG_Acquisitions;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionG_Acquisitions
{
    [Authorize(Roles = "Admin,Librarian")]
    public class ItemQuotationController : Controller
    {
        private readonly IGenericRepository<ItemQuotation> _quotationRepo;
        private readonly IGenericRepository<Party> _partyRepo;
        private readonly IGenericRepository<LibraryRack> _rackRepo;

        public ItemQuotationController(
            IGenericRepository<ItemQuotation> quotationRepo,
            IGenericRepository<Party> partyRepo,
            IGenericRepository<LibraryRack> rackRepo)
        {
            _quotationRepo = quotationRepo;
            _partyRepo = partyRepo;
            _rackRepo = rackRepo;
        }

        public async Task<IActionResult> Index()
        {
            var quotations = await _quotationRepo.GetAllAsync();
            foreach (var quotation in quotations)
            {
                quotation.Party = await _partyRepo.GetByIdAsync(quotation.PartyId);
                quotation.LibraryRack = await _rackRepo.GetByIdAsync(quotation.LibraryRackId);
            }
            return View(quotations);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _quotationRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null) return NotFound();
            quotation.Party = await _partyRepo.GetByIdAsync(quotation.PartyId);
            quotation.LibraryRack = await _rackRepo.GetByIdAsync(quotation.LibraryRackId);
            return View(quotation);
        }

        public async Task<IActionResult> Create()
        {
            await LoadPartyDropdown();
            await LoadRackDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemQuotation quotation)
        {
            ModelState.Remove("Party");
            ModelState.Remove("LibraryRack");

            if (ModelState.IsValid)
            {
                await _quotationRepo.AddAsync(quotation);
                TempData["Success"] = "Item Quotation created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(quotation.PartyId);
            await LoadRackDropdown(quotation.LibraryRackId);
            return View(quotation);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null) return NotFound();
            await LoadPartyDropdown(quotation.PartyId);
            await LoadRackDropdown(quotation.LibraryRackId);
            return View(quotation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemQuotation quotation)
        {
            if (id != quotation.Id) return NotFound();

            ModelState.Remove("Party");
            ModelState.Remove("LibraryRack");

            if (ModelState.IsValid)
            {
                await _quotationRepo.UpdateAsync(quotation);
                TempData["Success"] = "Item Quotation updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(quotation.PartyId);
            await LoadRackDropdown(quotation.LibraryRackId);
            return View(quotation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null) return NotFound();
            quotation.Party = await _partyRepo.GetByIdAsync(quotation.PartyId);
            quotation.LibraryRack = await _rackRepo.GetByIdAsync(quotation.LibraryRackId);
            return View(quotation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _quotationRepo.DeleteAsync(id);
            TempData["Success"] = "Item Quotation deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPartyDropdown(object selected = null)
        {
            var parties = await _partyRepo.GetAllAsync();
            ViewBag.PartyList = new SelectList(parties, "Id", "PartyName", selected);
        }

        private async Task LoadRackDropdown(object selected = null)
        {
            var racks = await _rackRepo.GetAllAsync();
            ViewBag.RackList = new SelectList(racks, "Id", "Code", selected);
        }
    }
}
