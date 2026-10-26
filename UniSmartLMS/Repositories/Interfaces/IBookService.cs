using UniSmartLMS.Models.SectionC_Catalog;

namespace UniSmartLMS.Services.Interfaces
{
    public interface IBookService : IService<ItemInfo>
    {
        Task<ItemInfo> AddBookWithDetailsAsync(ItemInfo book, List<int> authorIds, List<int> subjectIds, ItemEdition? edition = null);
        Task UpdateBookWithDetailsAsync(ItemInfo book, List<int> authorIds, List<int> subjectIds);
        Task<List<AuthorInfo>> GetAuthorsByBookIdAsync(int bookId);
        Task<List<int>> GetAuthorIdsByBookIdAsync(int bookId);
        Task<List<SubjectHeading>> GetSubjectsByBookIdAsync(int bookId);
        Task<ItemEdition?> GetEditionByBookIdAsync(int bookId);
    }
}