using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionH_Digital;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionH_Digital
{
    [Authorize(Roles = "Admin,Librarian")]
    public class OnlineAccessLogController : Controller
    {
        private readonly IGenericRepository<OnlineAccessLog> _logRepo;
        private readonly IGenericRepository<DigitalResource> _resourceRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;

        public OnlineAccessLogController(
            IGenericRepository<OnlineAccessLog> logRepo,
            IGenericRepository<DigitalResource> resourceRepo,
            IGenericRepository<LibraryMember> memberRepo)
        {
            _logRepo = logRepo;
            _resourceRepo = resourceRepo;
            _memberRepo = memberRepo;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _logRepo.GetAllAsync();
            foreach (var log in logs)
            {
                log.DigitalResource = await _resourceRepo.GetByIdAsync(log.DigitalResourceId);
                log.LibraryMember = await _memberRepo.GetByIdAsync(log.LibraryMemberId);
            }
            return View(logs);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _logRepo.SearchAsync("AccessType", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var log = await _logRepo.GetByIdAsync(id);
            if (log == null) return NotFound();
            log.DigitalResource = await _resourceRepo.GetByIdAsync(log.DigitalResourceId);
            log.LibraryMember = await _memberRepo.GetByIdAsync(log.LibraryMemberId);
            return View(log);
        }

        public async Task<IActionResult> Create()
        {
            await LoadResourceDropdown();
            await LoadMemberDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OnlineAccessLog log)
        {
            ModelState.Remove("DigitalResource");
            ModelState.Remove("LibraryMember");

            if (log.AccessDate == default)
            {
                log.AccessDate = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                await _logRepo.AddAsync(log);
                TempData["Success"] = "Access Log created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadResourceDropdown(log.DigitalResourceId);
            await LoadMemberDropdown(log.LibraryMemberId);
            return View(log);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var log = await _logRepo.GetByIdAsync(id);
            if (log == null) return NotFound();
            await LoadResourceDropdown(log.DigitalResourceId);
            await LoadMemberDropdown(log.LibraryMemberId);
            return View(log);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OnlineAccessLog log)
        {
            if (id != log.Id) return NotFound();

            ModelState.Remove("DigitalResource");
            ModelState.Remove("LibraryMember");

            if (ModelState.IsValid)
            {
                await _logRepo.UpdateAsync(log);
                TempData["Success"] = "Access Log updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadResourceDropdown(log.DigitalResourceId);
            await LoadMemberDropdown(log.LibraryMemberId);
            return View(log);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var log = await _logRepo.GetByIdAsync(id);
            if (log == null) return NotFound();
            log.DigitalResource = await _resourceRepo.GetByIdAsync(log.DigitalResourceId);
            log.LibraryMember = await _memberRepo.GetByIdAsync(log.LibraryMemberId);
            return View(log);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logRepo.DeleteAsync(id);
            TempData["Success"] = "Access Log deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadResourceDropdown(object selected = null)
        {
            var resources = await _resourceRepo.GetAllAsync();
            ViewBag.ResourceList = new SelectList(resources, "Id", "Id", selected);
        }

        private async Task LoadMemberDropdown(object selected = null)
        {
            var members = await _memberRepo.GetAllAsync();
            ViewBag.MemberList = new SelectList(members, "Id", "CardNo", selected);
        }
    }
}
