using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services.Interfaces;

namespace UniSmartLMS.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly IGenericRepository<ItemInfo> _bookRepo;
        private readonly IGenericRepository<AuthorInfo> _authorRepo;
        private readonly IGenericRepository<SubjectHeading> _subjectRepo;
        private readonly IGenericRepository<AuthorItemInfo> _authorItemRepo;
        private readonly IGenericRepository<ItemSubject> _itemSubjectRepo;
        private readonly IGenericRepository<ItemEdition> _editionRepo;

        public BookService(
            IGenericRepository<ItemInfo> bookRepo,
            IGenericRepository<AuthorInfo> authorRepo,
            IGenericRepository<SubjectHeading> subjectRepo,
            IGenericRepository<AuthorItemInfo> authorItemRepo,
            IGenericRepository<ItemSubject> itemSubjectRepo,
            IGenericRepository<ItemEdition> editionRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _subjectRepo = subjectRepo;
            _authorItemRepo = authorItemRepo;
            _itemSubjectRepo = itemSubjectRepo;
            _editionRepo = editionRepo;
        }

        public async Task<ItemInfo?> GetByIdAsync(int id) => await _bookRepo.GetByIdAsync(id);
        public async Task<IEnumerable<ItemInfo>> GetAllAsync() => await _bookRepo.GetAllAsync();

        public async Task<ItemInfo> AddBookWithDetailsAsync(ItemInfo book, List<int> authorIds, List<int> subjectIds, ItemEdition? edition = null)
        {
            var newBook = await _bookRepo.AddAsync(book);

            foreach (var authorId in authorIds)
            {
                await _authorItemRepo.AddAsync(new AuthorItemInfo
                {
                    AuthorId = authorId,
                    ItemInfoId = newBook.Id
                });
            }

            foreach (var subjectId in subjectIds)
            {
                await _itemSubjectRepo.AddAsync(new ItemSubject
                {
                    ItemInfoId = newBook.Id,
                    SubjectId = subjectId
                });
            }

            if (edition != null)
            {
                edition.ItemInfoId = newBook.Id;
                await _editionRepo.AddAsync(edition);
            }

            return newBook;
        }

        public async Task UpdateBookWithDetailsAsync(ItemInfo book, List<int> authorIds, List<int> subjectIds)
        {
            await _bookRepo.UpdateAsync(book);

            var oldAuthors = await _authorItemRepo.FindAsync(a => a.ItemInfoId == book.Id);
            foreach (var old in oldAuthors) await _authorItemRepo.DeleteAsync(old.Id);

            var oldSubjects = await _itemSubjectRepo.FindAsync(s => s.ItemInfoId == book.Id);
            foreach (var old in oldSubjects) await _itemSubjectRepo.DeleteAsync(old.Id);

            foreach (var authorId in authorIds)
                await _authorItemRepo.AddAsync(new AuthorItemInfo { AuthorId = authorId, ItemInfoId = book.Id });

            foreach (var subjectId in subjectIds)
                await _itemSubjectRepo.AddAsync(new ItemSubject { ItemInfoId = book.Id, SubjectId = subjectId });
        }

        // ✅ ADD THESE METHODS
        public async Task<List<AuthorInfo>> GetAuthorsByBookIdAsync(int bookId)
        {
            var links = await _authorItemRepo.FindAsync(a => a.ItemInfoId == bookId);
            var authors = new List<AuthorInfo>();
            foreach (var link in links)
            {
                var author = await _authorRepo.GetByIdAsync(link.AuthorId);
                if (author != null) authors.Add(author);
            }
            return authors;
        }

        public async Task<List<int>> GetAuthorIdsByBookIdAsync(int bookId)
        {
            var links = await _authorItemRepo.FindAsync(a => a.ItemInfoId == bookId);
            return links.Select(l => l.AuthorId).ToList();
        }

        // ✅ ADD THESE FOR COMPLETENESS
        public async Task<List<SubjectHeading>> GetSubjectsByBookIdAsync(int bookId)
        {
            var links = await _itemSubjectRepo.FindAsync(s => s.ItemInfoId == bookId);
            var subjects = new List<SubjectHeading>();
            foreach (var link in links)
            {
                var subject = await _subjectRepo.GetByIdAsync(link.SubjectId);
                if (subject != null) subjects.Add(subject);
            }
            return subjects;
        }

        public async Task<ItemEdition?> GetEditionByBookIdAsync(int bookId)
        {
            var editions = await _editionRepo.FindAsync(e => e.ItemInfoId == bookId);
            return editions.FirstOrDefault();
        }

        public async Task DeleteAsync(int id) => await _bookRepo.DeleteAsync(id);
        public async Task<bool> ExistsAsync(int id) => await _bookRepo.ExistsAsync(id);
        public async Task<ItemInfo> AddAsync(ItemInfo entity) => await _bookRepo.AddAsync(entity);
        public async Task UpdateAsync(ItemInfo entity) => await _bookRepo.UpdateAsync(entity);
    }


      
    
}