using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionG_Acquisitions;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionG_Acquisitions
{
    [Authorize(Roles = "Admin,Librarian")]
    public class AcquisitionRecordController : Controller
    {
        private readonly IGenericRepository<AcquisitionRecord> _recordRepo;
        private readonly IGenericRepository<Party> _partyRepo;

        public AcquisitionRecordController(
            IGenericRepository<AcquisitionRecord> recordRepo,
            IGenericRepository<Party> partyRepo)
        {
            _recordRepo = recordRepo;
            _partyRepo = partyRepo;
        }

        public async Task<IActionResult> Index()
        {
            var records = await _recordRepo.GetAllAsync();
            foreach (var record in records)
            {
                record.Party = await _partyRepo.GetByIdAsync(record.PartyId);
            }
            return View(records);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _recordRepo.SearchAsync("Invoice", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null) return NotFound();
            record.Party = await _partyRepo.GetByIdAsync(record.PartyId);
            return View(record);
        }

        public async Task<IActionResult> Create()
        {
            await LoadPartyDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcquisitionRecord record)
        {
            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _recordRepo.AddAsync(record);
                TempData["Success"] = "Acquisition Record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(record.PartyId);
            return View(record);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null) return NotFound();
            await LoadPartyDropdown(record.PartyId);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcquisitionRecord record)
        {
            if (id != record.Id) return NotFound();

            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _recordRepo.UpdateAsync(record);
                TempData["Success"] = "Acquisition Record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(record.PartyId);
            return View(record);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var record = await _recordRepo.GetByIdAsync(id);
            if (record == null) return NotFound();
            record.Party = await _partyRepo.GetByIdAsync(record.PartyId);
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _recordRepo.DeleteAsync(id);
            TempData["Success"] = "Acquisition Record deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPartyDropdown(object selected = null)
        {
            var parties = await _partyRepo.GetAllAsync();
            ViewBag.PartyList = new SelectList(parties, "Id", "PartyName", selected);
        }
    }
}