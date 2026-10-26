using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionD_People
{
    [Authorize(Roles = "Admin,Librarian")]
    public class StudentController : Controller
    {
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<Party> _partyRepo;

        public StudentController(
            IGenericRepository<Student> studentRepo,
            IGenericRepository<Party> partyRepo)
        {
            _studentRepo = studentRepo;
            _partyRepo = partyRepo;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _studentRepo.GetAllAsync();
            foreach (var student in students)
            {
                student.Party = await _partyRepo.GetByIdAsync(student.PartyId);
            }
            return View(students);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _studentRepo.SearchAsync("RegistrationNumber", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null) return NotFound();
            student.Party = await _partyRepo.GetByIdAsync(student.PartyId);
            return View(student);
        }

        public async Task<IActionResult> Create()
        {
            await LoadPartyDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _studentRepo.AddAsync(student);
                TempData["Success"] = "Student created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(student.PartyId);
            return View(student);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null) return NotFound();
            await LoadPartyDropdown(student.PartyId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _studentRepo.UpdateAsync(student);
                TempData["Success"] = "Student updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadPartyDropdown(student.PartyId);
            return View(student);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null) return NotFound();
            student.Party = await _partyRepo.GetByIdAsync(student.PartyId);
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentRepo.DeleteAsync(id);
            TempData["Success"] = "Student deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPartyDropdown(object selected = null)
        {
            var parties = await _partyRepo.GetAllAsync();
            ViewBag.PartyList = new SelectList(parties, "Id", "PartyName", selected);
        }
    }
}