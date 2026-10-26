using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionD_People
{
    [Authorize(Roles = "Admin,Librarian")]
    public class PartyController : Controller
    {
        private readonly IGenericRepository<Party> _partyRepo;
        private readonly IGenericRepository<Position> _positionRepo;

        public PartyController(
            IGenericRepository<Party> partyRepo,
            IGenericRepository<Position> positionRepo)
        {
            _partyRepo = partyRepo;
            _positionRepo = positionRepo;
        }

        public async Task<IActionResult> Index()
        {
            var parties = await _partyRepo.GetAllAsync();

            foreach (var party in parties)
            {
                if (party.PartyPositionId.HasValue)
                    party.Position = await _positionRepo.GetByIdAsync(party.PartyPositionId.Value);
            }
            return View(parties);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _partyRepo.SearchAsync("PartyName", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var party = await _partyRepo.GetByIdAsync(id);
            if (party == null) return NotFound();
            if (party.PartyPositionId.HasValue)
                party.Position = await _positionRepo.GetByIdAsync(party.PartyPositionId.Value);
            return View(party);
        }

        public async Task<IActionResult> Create()
        {
            await LoadPositionDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Party party)
        {
            // Remove navigation properties from ModelState
            ModelState.Remove("Position");

            // Validate Email is provided
            if (string.IsNullOrEmpty(party.Email))
            {
                ModelState.AddModelError("Email", "Email address is required");
                await LoadPositionDropdown(party.PartyPositionId);
                return View(party);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values if null
                    party.IsActive = true;
                    party.CreatedAt = DateTime.Now;

                    await _partyRepo.AddAsync(party);
                    TempData["Success"] = $"Party '{party.PartyName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving: {ex.Message}");
                }
            }

            await LoadPositionDropdown(party.PartyPositionId);
            return View(party);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var party = await _partyRepo.GetByIdAsync(id);
            if (party == null) return NotFound();
            await LoadPositionDropdown(party.PartyPositionId);
            return View(party);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Party party)
        {
            if (id != party.Id) return NotFound();

            ModelState.Remove("Position");

            if (string.IsNullOrEmpty(party.Email))
            {
                ModelState.AddModelError("Email", "Email address is required");
                await LoadPositionDropdown(party.PartyPositionId);
                return View(party);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    party.UpdatedAt = DateTime.Now;
                    await _partyRepo.UpdateAsync(party);
                    TempData["Success"] = $"Party '{party.PartyName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving: {ex.Message}");
                }
            }

            await LoadPositionDropdown(party.PartyPositionId);
            return View(party);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var party = await _partyRepo.GetByIdAsync(id);
            if (party == null) return NotFound();
            if (party.PartyPositionId.HasValue)
                party.Position = await _positionRepo.GetByIdAsync(party.PartyPositionId.Value);
            return View(party);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var party = await _partyRepo.GetByIdAsync(id);
            if (party != null)
            {
                await _partyRepo.DeleteAsync(id);
                TempData["Success"] = $"Party '{party.PartyName}' deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPositionDropdown(object selected = null)
        {
            var positions = await _positionRepo.GetAllAsync();
            ViewBag.PositionList = new SelectList(positions.Where(p => p.IsActive), "Id", "Category", selected);
        }
    }
}