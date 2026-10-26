using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Models.SectionG_Acquisitions;
using UniSmartLMS.Models.SectionH_Digital;


namespace UniSmartLMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // SECTION A: UNIVERSITY (15)
        public DbSet<University> Universities { get; set; }
        public DbSet<Campus> Campuses { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<RoomTypeAllocation> RoomTypeAllocations { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<RoomFeatureAllocation> RoomFeatureAllocations { get; set; }
        public DbSet<Institute> Institutes { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Position> Positions { get; set; }

        // SECTION B: LIBRARY (8)
        public DbSet<LibraryType> LibraryTypes { get; set; }
        public DbSet<LibraryBranch> LibraryBranches { get; set; }
        public DbSet<LibraryBranchLocation> LibraryBranchLocations { get; set; }
        public DbSet<LibraryManagementSection> LibraryManagementSections { get; set; }
        public DbSet<LibraryBranchManagement> LibraryBranchManagements { get; set; }
        public DbSet<LibraryItemSection> LibraryItemSections { get; set; }
        public DbSet<LibraryRack> LibraryRacks { get; set; }
        public DbSet<LibraryShelf> LibraryShelves { get; set; }

        // SECTION C: CATALOG (11)
        public DbSet<AuthorInfo> AuthorInfos { get; set; }
        public DbSet<PublisherInfo> PublisherInfos { get; set; }
        public DbSet<SubjectHeading> SubjectHeadings { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<ItemInfo> ItemInfos { get; set; }
        public DbSet<AuthorItemInfo> AuthorItemInfos { get; set; }
        public DbSet<ItemEdition> ItemEditions { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<AccessionPattern> AccessionPatterns { get; set; }
        public DbSet<ItemCopy> ItemCopies { get; set; }
        public DbSet<ItemCondition> ItemConditions { get; set; }

        // SECTION D: PEOPLE (10)
        public DbSet<Party> Parties { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeRelationHistory> EmployeeRelationHistories { get; set; }
        public DbSet<LibraryMember> LibraryMembers { get; set; }
        public DbSet<MemberBlock> MemberBlocks { get; set; }
        public DbSet<MemberQuotaOverride> MemberQuotaOverrides { get; set; }
        public DbSet<StructureType> StructureTypes { get; set; }
        public DbSet<StructureUnit> StructureUnits { get; set; }
        public DbSet<StructureDesignation> StructureDesignations { get; set; }
        public DbSet<LibrarianInfo> LibrarianInfos { get; set; }

        // SECTION E: CIRCULATION (7)
        public DbSet<CirculationRule> CirculationRules { get; set; }
        public DbSet<Duration> Durations { get; set; }
        public DbSet<Issuance> Issuances { get; set; }
        public DbSet<ReserveItem> ReserveItems { get; set; }
        public DbSet<ReturnItem> ReturnItems { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<ItemRequisition> ItemRequisitions { get; set; }

        // SECTION F: FINES (4)
        public DbSet<FineCategory> FineCategories { get; set; }
        public DbSet<FineDefinition> FineDefinitions { get; set; }
        public DbSet<UserFine> UserFines { get; set; }
        public DbSet<FineTransaction> FineTransactions { get; set; }

        // SECTION G: ACQUISITIONS (4)
        public DbSet<AcquisitionRecord> AcquisitionRecords { get; set; }
        public DbSet<ItemQuotation> ItemQuotations { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<LibraryRequisition> LibraryRequisitions { get; set; }

        // SECTION H: DIGITAL (2)
        public DbSet<DigitalResource> DigitalResources { get; set; }
        public DbSet<OnlineAccessLog> OnlineAccessLogs { get; set; }
        public DbSet<BorrowRequest> BorrowRequests { get; set; }
        //Section I
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Add this line in your ApplicationDbContext class
        public DbSet<AIActionIntent> AIActionIntents { get; set; }



       
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }


        public DbSet<ConversationLog> ConversationLogs { get; set; }







        public DbSet<ChatConversation> ChatConversations { get; set; }
        //public DbSet<ChatMessage> ChatMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}