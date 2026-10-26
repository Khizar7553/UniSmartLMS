using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionG_Acquisitions;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionG_Acquisitions
{
    [Authorize(Roles = "Admin,Librarian")]
    public class PurchaseItemController : Controller
    {
        private readonly IGenericRepository<PurchaseItem> _purchaseRepo;
        private readonly IGenericRepository<ItemQuotation> _quotationRepo;
        private readonly IGenericRepository<ItemInfo> _itemRepo;
        private readonly IGenericRepository<Party> _partyRepo;

        public PurchaseItemController(
            IGenericRepository<PurchaseItem> purchaseRepo,
            IGenericRepository<ItemQuotation> quotationRepo,
            IGenericRepository<ItemInfo> itemRepo,
            IGenericRepository<Party> partyRepo)
        {
            _purchaseRepo = purchaseRepo;
            _quotationRepo = quotationRepo;
            _itemRepo = itemRepo;
            _partyRepo = partyRepo;
        }

        public async Task<IActionResult> Index()
        {
            var purchases = await _purchaseRepo.GetAllAsync();
            foreach (var purchase in purchases)
            {
                purchase.ItemQuotation = await _quotationRepo.GetByIdAsync(purchase.ItemQuotationId);
                purchase.ItemInfo = await _itemRepo.GetByIdAsync(purchase.LibraryItemId);
                purchase.Party = await _partyRepo.GetByIdAsync(purchase.PartyId);
            }
            return View(purchases);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _purchaseRepo.SearchAsync("InvoiceNumber", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var purchase = await _purchaseRepo.GetByIdAsync(id);
            if (purchase == null) return NotFound();
            purchase.ItemQuotation = await _quotationRepo.GetByIdAsync(purchase.ItemQuotationId);
            purchase.ItemInfo = await _itemRepo.GetByIdAsync(purchase.LibraryItemId);
            purchase.Party = await _partyRepo.GetByIdAsync(purchase.PartyId);
            return View(purchase);
        }

        public async Task<IActionResult> Create()
        {
            await LoadQuotationDropdown();
            await LoadItemDropdown();
            await LoadPartyDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseItem purchase)
        {
            ModelState.Remove("ItemQuotation");
            ModelState.Remove("ItemInfo");
            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _purchaseRepo.AddAsync(purchase);
                TempData["Success"] = "Purchase Item created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadQuotationDropdown(purchase.ItemQuotationId);
            await LoadItemDropdown(purchase.LibraryItemId);
            await LoadPartyDropdown(purchase.PartyId);
            return View(purchase);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var purchase = await _purchaseRepo.GetByIdAsync(id);
            if (purchase == null) return NotFound();
            await LoadQuotationDropdown(purchase.ItemQuotationId);
            await LoadItemDropdown(purchase.LibraryItemId);
            await LoadPartyDropdown(purchase.PartyId);
            return View(purchase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseItem purchase)
        {
            if (id != purchase.Id) return NotFound();

            ModelState.Remove("ItemQuotation");
            ModelState.Remove("ItemInfo");
            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _purchaseRepo.UpdateAsync(purchase);
                TempData["Success"] = "Purchase Item updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadQuotationDropdown(purchase.ItemQuotationId);
            await LoadItemDropdown(purchase.LibraryItemId);
            await LoadPartyDropdown(purchase.PartyId);
            return View(purchase);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var purchase = await _purchaseRepo.GetByIdAsync(id);
            if (purchase == null) return NotFound();
            purchase.ItemQuotation = await _quotationRepo.GetByIdAsync(purchase.ItemQuotationId);
            purchase.ItemInfo = await _itemRepo.GetByIdAsync(purchase.LibraryItemId);
            purchase.Party = await _partyRepo.GetByIdAsync(purchase.PartyId);
            return View(purchase);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _purchaseRepo.DeleteAsync(id);
            TempData["Success"] = "Purchase Item deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadQuotationDropdown(object selected = null)
        {
            var quotations = await _quotationRepo.GetAllAsync();
            ViewBag.QuotationList = new SelectList(quotations, "Id", "Id", selected);
        }

        private async Task LoadItemDropdown(object selected = null)
        {
            var items = await _itemRepo.GetAllAsync();
            ViewBag.ItemList = new SelectList(items, "Id", "Title", selected);
        }

        private async Task LoadPartyDropdown(object selected = null)
        {
            var parties = await _partyRepo.GetAllAsync();
            ViewBag.PartyList = new SelectList(parties, "Id", "PartyName", selected);
        }
    }
}