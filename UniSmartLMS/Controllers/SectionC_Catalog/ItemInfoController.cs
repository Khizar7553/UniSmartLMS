using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services;
using UniSmartLMS.Services.Implementations;
using UniSmartLMS.Services.Interfaces;  // ✅ ADD THIS

namespace UniSmartLMS.Controllers.SectionC_Catalog
{
    [Authorize(Roles = "Admin,Librarian")]
    public class ItemInfoController : Controller
    {
        private readonly IGenericRepository<ItemInfo> _itemRepo;
        private readonly IGenericRepository<SubjectHeading> _subjectRepo;
        private readonly IGenericRepository<Language> _languageRepo;
        private readonly IGenericRepository<PublisherInfo> _publisherRepo;
        private readonly IGenericRepository<AuthorInfo> _authorRepo;
        private readonly IBookService _bookService;

        public ItemInfoController(
            IGenericRepository<ItemInfo> itemRepo,
            IGenericRepository<SubjectHeading> subjectRepo,
            IGenericRepository<Language> languageRepo,
            IGenericRepository<PublisherInfo> publisherRepo,
            IGenericRepository<AuthorInfo> authorRepo,
            IBookService bookService)
        {
            _itemRepo = itemRepo;
            _subjectRepo = subjectRepo;
            _languageRepo = languageRepo;
            _publisherRepo = publisherRepo;
            _authorRepo = authorRepo;
            _bookService = bookService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _itemRepo.GetAllAsync();
            foreach (var item in items)
            {
                if (item.SubjectHeadingId.HasValue)
                    item.SubjectHeading = await _subjectRepo.GetByIdAsync(item.SubjectHeadingId.Value);
                if (item.LanguageId.HasValue)
                    item.Language = await _languageRepo.GetByIdAsync(item.LanguageId.Value);
                if (item.PublisherInfoId.HasValue)
                    item.PublisherInfo = await _publisherRepo.GetByIdAsync(item.PublisherInfoId.Value);
            }
            return View(items);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _itemRepo.SearchAsync("Title", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _itemRepo.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (item.SubjectHeadingId.HasValue)
                item.SubjectHeading = await _subjectRepo.GetByIdAsync(item.SubjectHeadingId.Value);
            if (item.LanguageId.HasValue)
                item.Language = await _languageRepo.GetByIdAsync(item.LanguageId.Value);
            if (item.PublisherInfoId.HasValue)
                item.PublisherInfo = await _publisherRepo.GetByIdAsync(item.PublisherInfoId.Value);

            // Load authors for this book
            var authorLinks = await _bookService.GetAuthorsByBookIdAsync(id);
            ViewBag.Authors = authorLinks;

            return View(item);
        }

        public async Task<IActionResult> Create()
        {
            await LoadSubjectDropdown();
            await LoadLanguageDropdown();
            await LoadPublisherDropdown();

            var authors = await _authorRepo.GetAllAsync();
            ViewBag.AuthorList = new MultiSelectList(authors, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemInfo item, List<int> authorIds, List<int> subjectIds, int editionYear = 0, string editionNumber = "")
        {
            ModelState.Remove("SubjectHeading");
            ModelState.Remove("Language");
            ModelState.Remove("PublisherInfo");
            ModelState.Remove("AuthorItemInfos");
            ModelState.Remove("ItemCopies");
            ModelState.Remove("ItemEditions");

            if (ModelState.IsValid)
            {
                // Create edition if provided
                ItemEdition edition = null;
                if (editionYear > 0 || !string.IsNullOrEmpty(editionNumber))
                {
                    edition = new ItemEdition
                    {
                        Year = editionYear,
                        EditionNumber = editionNumber
                    };
                }

                var newBook = await _bookService.AddBookWithDetailsAsync(
                    item,
                    authorIds ?? new List<int>(),
                    subjectIds ?? new List<int>(),
                    edition);

                TempData["Success"] = "Book created successfully with authors and subjects!";
                return RedirectToAction(nameof(Index));
            }

            await LoadSubjectDropdown(item.SubjectHeadingId);
            await LoadLanguageDropdown(item.LanguageId);
            await LoadPublisherDropdown(item.PublisherInfoId);

            var authors = await _authorRepo.GetAllAsync();
            ViewBag.AuthorList = new MultiSelectList(authors, "Id", "Name", authorIds);

            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _itemRepo.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadSubjectDropdown(item.SubjectHeadingId);
            await LoadLanguageDropdown(item.LanguageId);
            await LoadPublisherDropdown(item.PublisherInfoId);

            var authors = await _authorRepo.GetAllAsync();
            var existingAuthorIds = await _bookService.GetAuthorIdsByBookIdAsync(id);
            ViewBag.AuthorList = new MultiSelectList(authors, "Id", "Name", existingAuthorIds);

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemInfo item, List<int> authorIds, List<int> subjectIds)
        {
            if (id != item.Id) return NotFound();

            ModelState.Remove("SubjectHeading");
            ModelState.Remove("Language");
            ModelState.Remove("PublisherInfo");
            ModelState.Remove("AuthorItemInfos");
            ModelState.Remove("ItemCopies");
            ModelState.Remove("ItemEditions");

            if (ModelState.IsValid)
            {
                await _bookService.UpdateBookWithDetailsAsync(item, authorIds ?? new List<int>(), subjectIds ?? new List<int>());
                TempData["Success"] = "Book updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadSubjectDropdown(item.SubjectHeadingId);
            await LoadLanguageDropdown(item.LanguageId);
            await LoadPublisherDropdown(item.PublisherInfoId);

            var authors = await _authorRepo.GetAllAsync();
            ViewBag.AuthorList = new MultiSelectList(authors, "Id", "Name", authorIds);

            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _itemRepo.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (item.SubjectHeadingId.HasValue)
                item.SubjectHeading = await _subjectRepo.GetByIdAsync(item.SubjectHeadingId.Value);
            if (item.LanguageId.HasValue)
                item.Language = await _languageRepo.GetByIdAsync(item.LanguageId.Value);
            if (item.PublisherInfoId.HasValue)
                item.PublisherInfo = await _publisherRepo.GetByIdAsync(item.PublisherInfoId.Value);

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bookService.DeleteAsync(id);
            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSubjectDropdown(object selected = null)
        {
            var subjects = await _subjectRepo.GetAllAsync();
            ViewBag.SubjectList = new SelectList(subjects, "Id", "Name", selected);
        }

        private async Task LoadLanguageDropdown(object selected = null)
        {
            var languages = await _languageRepo.GetAllAsync();
            ViewBag.LanguageList = new SelectList(languages, "Id", "LanguageCategory", selected);
        }

        private async Task LoadPublisherDropdown(object selected = null)
        {
            var publishers = await _publisherRepo.GetAllAsync();
            ViewBag.PublisherList = new SelectList(publishers, "Id", "Name", selected);
        }
    }
}