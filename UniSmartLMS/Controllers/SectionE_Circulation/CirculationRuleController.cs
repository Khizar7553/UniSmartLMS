using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian")]
    public class CirculationRuleController : Controller
    {
        private readonly IGenericRepository<CirculationRule> _ruleRepo;
        private readonly IGenericRepository<Party> _partyRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;

        public CirculationRuleController(
            IGenericRepository<CirculationRule> ruleRepo,
            IGenericRepository<Party> partyRepo,
            IGenericRepository<LibraryBranch> branchRepo)
        {
            _ruleRepo = ruleRepo;
            _partyRepo = partyRepo;
            _branchRepo = branchRepo;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _ruleRepo.GetAllAsync();
            foreach (var rule in rules)
            {
                rule.Party = await _partyRepo.GetByIdAsync(rule.PartyId);
                rule.LibraryBranch = await _branchRepo.GetByIdAsync(rule.LibraryBranchId);
            }
            return View(rules);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _ruleRepo.SearchAsync("PartyId", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var rule = await _ruleRepo.GetByIdAsync(id);
            if (rule == null) return NotFound();
            rule.Party = await _partyRepo.GetByIdAsync(rule.PartyId);
            rule.LibraryBranch = await _branchRepo.GetByIdAsync(rule.LibraryBranchId);
            return View(rule);
        }

        public async Task<IActionResult> Create()
        {
            await LoadPartyDropdown();
            await LoadBranchDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CirculationRule rule)
        {
            ModelState.Remove("Party");
            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _ruleRepo.AddAsync(rule);
                TempData["Success"] = "Circulation Rule created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(rule.PartyId);
            await LoadBranchDropdown(rule.LibraryBranchId);
            return View(rule);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var rule = await _ruleRepo.GetByIdAsync(id);
            if (rule == null) return NotFound();
            await LoadPartyDropdown(rule.PartyId);
            await LoadBranchDropdown(rule.LibraryBranchId);
            return View(rule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CirculationRule rule)
        {
            if (id != rule.Id) return NotFound();

            ModelState.Remove("Party");
            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _ruleRepo.UpdateAsync(rule);
                TempData["Success"] = "Circulation Rule updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(rule.PartyId);
            await LoadBranchDropdown(rule.LibraryBranchId);
            return View(rule);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var rule = await _ruleRepo.GetByIdAsync(id);
            if (rule == null) return NotFound();
            rule.Party = await _partyRepo.GetByIdAsync(rule.PartyId);
            rule.LibraryBranch = await _branchRepo.GetByIdAsync(rule.LibraryBranchId);
            return View(rule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _ruleRepo.DeleteAsync(id);
            TempData["Success"] = "Circulation Rule deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPartyDropdown(object selected = null)
        {
            var parties = await _partyRepo.GetAllAsync();
            ViewBag.PartyList = new SelectList(parties, "Id", "PartyName", selected);
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }
    }
}
