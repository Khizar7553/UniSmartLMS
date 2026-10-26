using Microsoft.AspNetCore.Identity;
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
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            // ========================================
            // STEP 1: CREATE ROLES
            // ========================================
            string[] roles = { "Student", "Faculty", "Staff", "Librarian", "Admin", "Visitor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ========================================
            // STEP 2: SECTION A - UNIVERSITY HIERARCHY
            // ========================================

            // Universities
            if (!context.Universities.Any())
            {
                var universities = new[]
                {
                    new University { Name = "MNS-University of Agriculture, Multan", Code = "MNS-UAM", Description = "Leading agricultural university in South Punjab", IsActive = true },
                    new University { Name = "Bahauddin Zakariya University", Code = "BZU", Description = "Multi-campus university in Multan", IsActive = true },
                    new University { Name = "University of the Punjab", Code = "PU", Description = "Largest university in Pakistan", IsActive = true },
                    new University { Name = "University of Engineering & Technology", Code = "UET", Description = "Top engineering university", IsActive = true },
                    new University { Name = "Government College University", Code = "GCU", Description = "Historic institution", IsActive = true }
                };
                await context.Universities.AddRangeAsync(universities);
                await context.SaveChangesAsync();
            }

            // Campuses (using FirstOrDefaultAsync with null checks)
            if (!context.Campuses.Any())
            {
                var mns = await context.Universities.FirstOrDefaultAsync(u => u.Code == "MNS-UAM");
                var bzu = await context.Universities.FirstOrDefaultAsync(u => u.Code == "BZU");
                var pu = await context.Universities.FirstOrDefaultAsync(u => u.Code == "PU");
                var uet = await context.Universities.FirstOrDefaultAsync(u => u.Code == "UET");
                var gcu = await context.Universities.FirstOrDefaultAsync(u => u.Code == "GCU");

                var campusesList = new List<Campus>();
                if (mns != null) campusesList.Add(new Campus { Name = "Main Campus MNS", Code = "MNS-MAIN", UniversityId = mns.Id, Description = "Old Shujabad Road, Multan", IsActive = true });
                if (mns != null) campusesList.Add(new Campus { Name = "City Campus MNS", Code = "MNS-CITY", UniversityId = mns.Id, Description = "Near Multan Cantt", IsActive = true });
                if (bzu != null) campusesList.Add(new Campus { Name = "BZU Main Campus", Code = "BZU-MAIN", UniversityId = bzu.Id, Description = "Multan", IsActive = true });
                if (pu != null) campusesList.Add(new Campus { Name = "PU Quaid-e-Azam Campus", Code = "PU-MAIN", UniversityId = pu.Id, Description = "Lahore", IsActive = true });
                if (uet != null) campusesList.Add(new Campus { Name = "UET Main Campus", Code = "UET-MAIN", UniversityId = uet.Id, Description = "Lahore", IsActive = true });
                if (gcu != null) campusesList.Add(new Campus { Name = "GCU Main Campus", Code = "GCU-MAIN", UniversityId = gcu.Id, Description = "Lahore", IsActive = true });

                if (campusesList.Any())
                {
                    await context.Campuses.AddRangeAsync(campusesList);
                    await context.SaveChangesAsync();
                }
            }

            // Blocks
            if (!context.Blocks.Any())
            {
                var mnsMain = await context.Campuses.FirstOrDefaultAsync(c => c.Code == "MNS-MAIN");
                if (mnsMain != null)
                {
                    var blocks = new[]
                    {
                        new Block { Name = "Agriculture Block", Code = "AGRI-BLK", CampusId = mnsMain.Id, IsActive = true },
                        new Block { Name = "Science Block", Code = "SCI-BLK", CampusId = mnsMain.Id, IsActive = true },
                        new Block { Name = "Library Block", Code = "LIB-BLK", CampusId = mnsMain.Id, IsActive = true },
                        new Block { Name = "Admin Block", Code = "ADMIN-BLK", CampusId = mnsMain.Id, IsActive = true },
                        new Block { Name = "Engineering Block", Code = "ENG-BLK", CampusId = mnsMain.Id, IsActive = true }
                    };
                    await context.Blocks.AddRangeAsync(blocks);
                    await context.SaveChangesAsync();
                }
            }

            // Buildings
            if (!context.Buildings.Any())
            {
                var libBlock = await context.Blocks.FirstOrDefaultAsync(b => b.Code == "LIB-BLK");
                var agriBlock = await context.Blocks.FirstOrDefaultAsync(b => b.Code == "AGRI-BLK");
                var sciBlock = await context.Blocks.FirstOrDefaultAsync(b => b.Code == "SCI-BLK");

                var buildingsList = new List<Building>();
                if (libBlock != null) buildingsList.Add(new Building { Name = "Central Library Building", Code = "CENTRAL-LIB", BlockId = libBlock.Id, Description = "Main Library", IsActive = true });
                if (agriBlock != null) buildingsList.Add(new Building { Name = "Faculty of Agriculture", Code = "AGRI-BLD", BlockId = agriBlock.Id, Description = "Agriculture Department", IsActive = true });
                if (sciBlock != null) buildingsList.Add(new Building { Name = "Computer Science Department", Code = "CS-BLD", BlockId = sciBlock.Id, Description = "IT Department", IsActive = true });
                if (sciBlock != null) buildingsList.Add(new Building { Name = "Physics Department", Code = "PHY-BLD", BlockId = sciBlock.Id, Description = "Physics Department", IsActive = true });
                if (sciBlock != null) buildingsList.Add(new Building { Name = "Chemistry Department", Code = "CHEM-BLD", BlockId = sciBlock.Id, Description = "Chemistry Department", IsActive = true });

                if (buildingsList.Any())
                {
                    await context.Buildings.AddRangeAsync(buildingsList);
                    await context.SaveChangesAsync();
                }
            }

            // Floors
            if (!context.Floors.Any())
            {
                var centralLib = await context.Buildings.FirstOrDefaultAsync(b => b.Code == "CENTRAL-LIB");
                if (centralLib != null)
                {
                    var floors = new[]
                    {
                        new Floor { Name = "Ground Floor", Code = "GF", BuildingId = centralLib.Id, IsActive = true },
                        new Floor { Name = "First Floor", Code = "FF", BuildingId = centralLib.Id, IsActive = true },
                        new Floor { Name = "Second Floor", Code = "SF", BuildingId = centralLib.Id, IsActive = true },
                        new Floor { Name = "Third Floor", Code = "TF", BuildingId = centralLib.Id, IsActive = true },
                        new Floor { Name = "Basement", Code = "BSMT", BuildingId = centralLib.Id, IsActive = true }
                    };
                    await context.Floors.AddRangeAsync(floors);
                    await context.SaveChangesAsync();
                }
            }

            // Rooms
            if (!context.Rooms.Any())
            {
                var gf = await context.Floors.FirstOrDefaultAsync(f => f.Code == "GF");
                var ff = await context.Floors.FirstOrDefaultAsync(f => f.Code == "FF");
                var sf = await context.Floors.FirstOrDefaultAsync(f => f.Code == "SF");

                var roomsList = new List<Room>();
                if (gf != null) roomsList.Add(new Room { Name = "Main Reading Hall", Code = "RH-101", FloorId = gf.Id, Description = "200 seats, Silent Zone", IsActive = true });
                if (gf != null) roomsList.Add(new Room { Name = "Reference Section", Code = "REF-101", FloorId = gf.Id, Description = "Reference books only", IsActive = true });
                if (gf != null) roomsList.Add(new Room { Name = "Children Section", Code = "CH-102", FloorId = gf.Id, Description = "Kids books", IsActive = true });
                if (ff != null) roomsList.Add(new Room { Name = "Computer Lab", Code = "CMP-201", FloorId = ff.Id, Description = "50 computers", IsActive = true });
                if (ff != null) roomsList.Add(new Room { Name = "Periodicals Section", Code = "PER-202", FloorId = ff.Id, Description = "Journals & Magazines", IsActive = true });
                if (sf != null) roomsList.Add(new Room { Name = "Research Carrels", Code = "RSH-301", FloorId = sf.Id, Description = "PhD research area", IsActive = true });
                if (sf != null) roomsList.Add(new Room { Name = "Group Study Room", Code = "GRP-302", FloorId = sf.Id, Description = "6-8 persons", IsActive = true });

                if (roomsList.Any())
                {
                    await context.Rooms.AddRangeAsync(roomsList);
                    await context.SaveChangesAsync();
                }
            }

            // Institutes
            if (!context.Institutes.Any())
            {
                var institutes = new[]
                {
                    new Institute { Name = "Institute of Agricultural Sciences", Code = "IAS", IsActive = true },
                    new Institute { Name = "Institute of Computer Science", Code = "ICS", IsActive = true },
                    new Institute { Name = "Institute of Food Science", Code = "IFS", IsActive = true },
                    new Institute { Name = "Institute of Engineering", Code = "IE", IsActive = true },
                    new Institute { Name = "Institute of Business Administration", Code = "IBA", IsActive = true }
                };
                await context.Institutes.AddRangeAsync(institutes);
                await context.SaveChangesAsync();
            }

            // Faculties
            if (!context.Faculties.Any())
            {
                var ias = await context.Institutes.FirstOrDefaultAsync(i => i.Code == "IAS");
                var ics = await context.Institutes.FirstOrDefaultAsync(i => i.Code == "ICS");
                var ifs = await context.Institutes.FirstOrDefaultAsync(i => i.Code == "IFS");
                var ie = await context.Institutes.FirstOrDefaultAsync(i => i.Code == "IE");
                var iba = await context.Institutes.FirstOrDefaultAsync(i => i.Code == "IBA");
                var mnsMain = await context.Campuses.FirstOrDefaultAsync(c => c.Code == "MNS-MAIN");

                var facultiesList = new List<Faculty>();
                if (ias != null && mnsMain != null) facultiesList.Add(new Faculty { Name = "Faculty of Agriculture", Code = "AGRI", InstituteId = ias.Id, CampusId = mnsMain.Id, Description = "Agriculture Sciences", IsActive = true });
                if (ics != null && mnsMain != null) facultiesList.Add(new Faculty { Name = "Faculty of Computer Science", Code = "CS", InstituteId = ics.Id, CampusId = mnsMain.Id, Description = "Computer Science & IT", IsActive = true });
                if (ifs != null && mnsMain != null) facultiesList.Add(new Faculty { Name = "Faculty of Food Science", Code = "FOOD", InstituteId = ifs.Id, CampusId = mnsMain.Id, Description = "Food Technology", IsActive = true });
                if (ie != null && mnsMain != null) facultiesList.Add(new Faculty { Name = "Faculty of Engineering", Code = "ENG", InstituteId = ie.Id, CampusId = mnsMain.Id, Description = "Engineering Programs", IsActive = true });
                if (iba != null && mnsMain != null) facultiesList.Add(new Faculty { Name = "Faculty of Business", Code = "BUS", InstituteId = iba.Id, CampusId = mnsMain.Id, Description = "Business Administration", IsActive = true });

                if (facultiesList.Any())
                {
                    await context.Faculties.AddRangeAsync(facultiesList);
                    await context.SaveChangesAsync();
                }
            }

            // Departments
            if (!context.Departments.Any())
            {
                var agri = await context.Faculties.FirstOrDefaultAsync(f => f.Code == "AGRI");
                var cs = await context.Faculties.FirstOrDefaultAsync(f => f.Code == "CS");
                var eng = await context.Faculties.FirstOrDefaultAsync(f => f.Code == "ENG");

                var departmentsList = new List<Department>();
                if (agri != null)
                {
                    departmentsList.Add(new Department { Name = "Plant Sciences", Code = "PS", FacultyId = agri.Id, InstituteId = agri.InstituteId, IsActive = true });
                    departmentsList.Add(new Department { Name = "Soil Sciences", Code = "SS", FacultyId = agri.Id, InstituteId = agri.InstituteId, IsActive = true });
                    departmentsList.Add(new Department { Name = "Horticulture", Code = "HORT", FacultyId = agri.Id, InstituteId = agri.InstituteId, IsActive = true });
                }
                if (cs != null)
                {
                    departmentsList.Add(new Department { Name = "Computer Science", Code = "CS", FacultyId = cs.Id, InstituteId = cs.InstituteId, IsActive = true });
                    departmentsList.Add(new Department { Name = "Information Technology", Code = "IT", FacultyId = cs.Id, InstituteId = cs.InstituteId, IsActive = true });
                    departmentsList.Add(new Department { Name = "Software Engineering", Code = "SE", FacultyId = cs.Id, InstituteId = cs.InstituteId, IsActive = true });
                }
                if (eng != null)
                {
                    departmentsList.Add(new Department { Name = "Civil Engineering", Code = "CE", FacultyId = eng.Id, InstituteId = eng.InstituteId, IsActive = true });
                    departmentsList.Add(new Department { Name = "Electrical Engineering", Code = "EE", FacultyId = eng.Id, InstituteId = eng.InstituteId, IsActive = true });
                    departmentsList.Add(new Department { Name = "Mechanical Engineering", Code = "ME", FacultyId = eng.Id, InstituteId = eng.InstituteId, IsActive = true });
                }

                if (departmentsList.Any())
                {
                    await context.Departments.AddRangeAsync(departmentsList);
                    await context.SaveChangesAsync();
                }
            }

            // Designations
            if (!context.Designations.Any())
            {
                var designations = new[]
                {
                    new Designation { Name = "Chief Librarian", Code = "CH-LIB", Description = "Head of Library", IsActive = true },
                    new Designation { Name = "Deputy Librarian", Code = "D-LIB", Description = "Assistant Head", IsActive = true },
                    new Designation { Name = "Assistant Librarian", Code = "A-LIB", Description = "Section Incharge", IsActive = true },
                    new Designation { Name = "Cataloguer", Code = "CAT", Description = "Book Cataloging", IsActive = true },
                    new Designation { Name = "Digital Librarian", Code = "DIG-LIB", Description = "E-resources Manager", IsActive = true }
                };
                await context.Designations.AddRangeAsync(designations);
                await context.SaveChangesAsync();
            }

            // Positions
            if (!context.Positions.Any())
            {
                var positions = new[]
                {
                    new Position { Category = "Librarian", Code = "LIB", Description = "Professional Librarian", IsActive = true },
                    new Position { Category = "Assistant", Code = "ASST", Description = "Library Assistant", IsActive = true },
                    new Position { Category = "Manager", Code = "MGR", Description = "Library Manager", IsActive = true },
                    new Position { Category = "Technician", Code = "TECH", Description = "Library Technician", IsActive = true }
                };
                await context.Positions.AddRangeAsync(positions);
                await context.SaveChangesAsync();
            }

            // ========================================
            // STEP 3: SECTION B - LIBRARY STRUCTURE
            // ========================================

            // Library Types
            if (!context.LibraryTypes.Any())
            {
                var libTypes = new[]
                {
                    new LibraryType { Name = "Main Library", Code = "MAIN", Description = "Central Library", IsActive = true },
                    new LibraryType { Name = "Departmental Library", Code = "DEPT", Description = "Department-wise library", IsActive = true },
                    new LibraryType { Name = "Digital Library", Code = "DIG", Description = "E-resources", IsActive = true },
                    new LibraryType { Name = "Reference Library", Code = "REF", Description = "Reference only", IsActive = true }
                };
                await context.LibraryTypes.AddRangeAsync(libTypes);
                await context.SaveChangesAsync();
            }

            // Library Branches
            if (!context.LibraryBranches.Any())
            {
                var mainType = await context.LibraryTypes.FirstOrDefaultAsync(t => t.Code == "MAIN");
                var deptType = await context.LibraryTypes.FirstOrDefaultAsync(t => t.Code == "DEPT");
                var digType = await context.LibraryTypes.FirstOrDefaultAsync(t => t.Code == "DIG");

                var branchesList = new List<LibraryBranch>();
                if (mainType != null) branchesList.Add(new LibraryBranch { Name = "Central Library", Code = "LIB-CENTRAL", LibraryTypeId = mainType.Id, Description = "Main Library Building", IsActive = true });
                if (digType != null) branchesList.Add(new LibraryBranch { Name = "Digital Library", Code = "LIB-DIGITAL", LibraryTypeId = digType.Id, Description = "E-Library Section", IsActive = true });
                if (deptType != null) branchesList.Add(new LibraryBranch { Name = "CS Department Library", Code = "LIB-CS", LibraryTypeId = deptType.Id, Description = "Computer Science Library", IsActive = true });
                if (deptType != null) branchesList.Add(new LibraryBranch { Name = "Engineering Library", Code = "LIB-ENG", LibraryTypeId = deptType.Id, Description = "Engineering Section", IsActive = true });

                if (branchesList.Any())
                {
                    await context.LibraryBranches.AddRangeAsync(branchesList);
                    await context.SaveChangesAsync();
                }
            }

            // Library Sections
            if (!context.LibraryItemSections.Any())
            {
                var centralLib = await context.LibraryBranches.FirstOrDefaultAsync(b => b.Code == "LIB-CENTRAL");
                var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "CS");

                if (centralLib != null && csDept != null)
                {
                    var sections = new[]
                    {
                        new LibraryItemSection { Name = "Reference Section", Code = "REF", LibraryBranchId = centralLib.Id, DepartmentId = csDept.Id, IsActive = true },
                        new LibraryItemSection { Name = "General Books", Code = "GEN", LibraryBranchId = centralLib.Id, DepartmentId = csDept.Id, IsActive = true },
                        new LibraryItemSection { Name = "Urdu Section", Code = "URD", LibraryBranchId = centralLib.Id, DepartmentId = csDept.Id, IsActive = true },
                        new LibraryItemSection { Name = "English Literature", Code = "ENG-LIT", LibraryBranchId = centralLib.Id, DepartmentId = csDept.Id, IsActive = true },
                        new LibraryItemSection { Name = "Periodicals", Code = "PER", LibraryBranchId = centralLib.Id, DepartmentId = csDept.Id, IsActive = true }
                    };
                    await context.LibraryItemSections.AddRangeAsync(sections);
                    await context.SaveChangesAsync();
                }
            }

            // Library Racks
            if (!context.LibraryRacks.Any())
            {
                var refSection = await context.LibraryItemSections.FirstOrDefaultAsync(s => s.Code == "REF");
                var genSection = await context.LibraryItemSections.FirstOrDefaultAsync(s => s.Code == "GEN");
                var urdSection = await context.LibraryItemSections.FirstOrDefaultAsync(s => s.Code == "URD");

                var racksList = new List<LibraryRack>();
                if (refSection != null) racksList.Add(new LibraryRack { Code = "RACK-A1", Description = "Computer Science Books", LibraryItemSectionId = refSection.Id, IsActive = true });
                if (refSection != null) racksList.Add(new LibraryRack { Code = "RACK-A2", Description = "Mathematics Books", LibraryItemSectionId = refSection.Id, IsActive = true });
                if (refSection != null) racksList.Add(new LibraryRack { Code = "RACK-A3", Description = "Physics Books", LibraryItemSectionId = refSection.Id, IsActive = true });
                if (genSection != null) racksList.Add(new LibraryRack { Code = "RACK-B1", Description = "Novels", LibraryItemSectionId = genSection.Id, IsActive = true });
                if (genSection != null) racksList.Add(new LibraryRack { Code = "RACK-B2", Description = "Poetry", LibraryItemSectionId = genSection.Id, IsActive = true });
                if (urdSection != null) racksList.Add(new LibraryRack { Code = "RACK-C1", Description = "Urdu Novels", LibraryItemSectionId = urdSection.Id, IsActive = true });
                if (urdSection != null) racksList.Add(new LibraryRack { Code = "RACK-C2", Description = "Urdu Poetry", LibraryItemSectionId = urdSection.Id, IsActive = true });

                if (racksList.Any())
                {
                    await context.LibraryRacks.AddRangeAsync(racksList);
                    await context.SaveChangesAsync();
                }
            }

            // Library Shelves
            if (!context.LibraryShelves.Any())
            {
                var rackA1 = await context.LibraryRacks.FirstOrDefaultAsync(r => r.Code == "RACK-A1");
                var rackB1 = await context.LibraryRacks.FirstOrDefaultAsync(r => r.Code == "RACK-B1");
                var rackC1 = await context.LibraryRacks.FirstOrDefaultAsync(r => r.Code == "RACK-C1");

                var shelvesList = new List<LibraryShelf>();
                if (rackA1 != null) shelvesList.Add(new LibraryShelf { Code = "SHELF-1", LibraryRackId = rackA1.Id, MaxCapacity = 100, NoOfItems = 45, IsActive = true });
                if (rackA1 != null) shelvesList.Add(new LibraryShelf { Code = "SHELF-2", LibraryRackId = rackA1.Id, MaxCapacity = 100, NoOfItems = 38, IsActive = true });
                if (rackA1 != null) shelvesList.Add(new LibraryShelf { Code = "SHELF-3", LibraryRackId = rackA1.Id, MaxCapacity = 100, NoOfItems = 52, IsActive = true });
                if (rackB1 != null) shelvesList.Add(new LibraryShelf { Code = "SHELF-1", LibraryRackId = rackB1.Id, MaxCapacity = 150, NoOfItems = 80, IsActive = true });
                if (rackB1 != null) shelvesList.Add(new LibraryShelf { Code = "SHELF-2", LibraryRackId = rackB1.Id, MaxCapacity = 150, NoOfItems = 65, IsActive = true });
                if (rackC1 != null) shelvesList.Add(new LibraryShelf { Code = "SHELF-1", LibraryRackId = rackC1.Id, MaxCapacity = 80, NoOfItems = 40, IsActive = true });

                if (shelvesList.Any())
                {
                    await context.LibraryShelves.AddRangeAsync(shelvesList);
                    await context.SaveChangesAsync();
                }
            }

            // ========================================
            // STEP 4: SECTION C - CATALOG
            // ========================================

            // Languages
            if (!context.Languages.Any())
            {
                var languages = new[]
                {
                    new Language { LanguageCategory = "Urdu", LanguageCode = "UR", IsActive = true },
                    new Language { LanguageCategory = "English", LanguageCode = "EN", IsActive = true },
                    new Language { LanguageCategory = "Arabic", LanguageCode = "AR", IsActive = true },
                    new Language { LanguageCategory = "Sindhi", LanguageCode = "SD", IsActive = true },
                    new Language { LanguageCategory = "Punjabi", LanguageCode = "PN", IsActive = true },
                    new Language { LanguageCategory = "Pashto", LanguageCode = "PS", IsActive = true },
                    new Language { LanguageCategory = "Balochi", LanguageCode = "BL", IsActive = true }
                };
                await context.Languages.AddRangeAsync(languages);
                await context.SaveChangesAsync();
            }

            // Subjects
            if (!context.SubjectHeadings.Any())
            {
                var subjects = new[]
                {
                    new SubjectHeading { Name = "Computer Science", Code = "CS", IsActive = true },
                    new SubjectHeading { Name = "Mathematics", Code = "MATH", IsActive = true },
                    new SubjectHeading { Name = "Physics", Code = "PHY", IsActive = true },
                    new SubjectHeading { Name = "Chemistry", Code = "CHEM", IsActive = true },
                    new SubjectHeading { Name = "Biology", Code = "BIO", IsActive = true },
                    new SubjectHeading { Name = "Urdu Literature", Code = "URD-LIT", IsActive = true },
                    new SubjectHeading { Name = "English Literature", Code = "ENG-LIT", IsActive = true },
                    new SubjectHeading { Name = "Islamic Studies", Code = "ISL", IsActive = true },
                    new SubjectHeading { Name = "Pakistan Studies", Code = "PK-ST", IsActive = true },
                    new SubjectHeading { Name = "Agriculture", Code = "AGRI", IsActive = true }
                };
                await context.SubjectHeadings.AddRangeAsync(subjects);
                await context.SaveChangesAsync();
            }

            // Publishers
            if (!context.PublisherInfos.Any())
            {
                var publishers = new[]
                {
                    new PublisherInfo { Name = "Oxford University Press", IsActive = true },
                    new PublisherInfo { Name = "Cambridge University Press", IsActive = true },
                    new PublisherInfo { Name = "Springer", IsActive = true },
                    new PublisherInfo { Name = "Pearson", IsActive = true },
                    new PublisherInfo { Name = "Sang-e-Meel Publications", IsActive = true },
                    new PublisherInfo { Name = "Ferozsons", IsActive = true },
                    new PublisherInfo { Name = "Ilmi Kitab Khana", IsActive = true },
                    new PublisherInfo { Name = "National Book Foundation", IsActive = true }
                };
                await context.PublisherInfos.AddRangeAsync(publishers);
                await context.SaveChangesAsync();
            }

            // Authors
            if (!context.AuthorInfos.Any())
            {
                var authors = new[]
                {
                    new AuthorInfo { Name = "Dr. Atta-ur-Rahman", IsActive = true },
                    new AuthorInfo { Name = "Dr. Abdul Qadeer Khan", IsActive = true },
                    new AuthorInfo { Name = "Robert C. Martin", IsActive = true },
                    new AuthorInfo { Name = "Andrew S. Tanenbaum", IsActive = true },
                    new AuthorInfo { Name = "Saadat Hasan Manto", IsActive = true },
                    new AuthorInfo { Name = "Allama Iqbal", IsActive = true },
                    new AuthorInfo { Name = "Faiz Ahmed Faiz", IsActive = true },
                    new AuthorInfo { Name = "Bano Qudsia", IsActive = true },
                    new AuthorInfo { Name = "Ashfaq Ahmed", IsActive = true },
                    new AuthorInfo { Name = "Parveen Shakir", IsActive = true }
                };
                await context.AuthorInfos.AddRangeAsync(authors);
                await context.SaveChangesAsync();
            }

            // Books (ItemInfo)
            if (!context.ItemInfos.Any())
            {
                var urduLang = await context.Languages.FirstOrDefaultAsync(l => l.LanguageCode == "UR");
                var engLang = await context.Languages.FirstOrDefaultAsync(l => l.LanguageCode == "EN");
                var csSubject = await context.SubjectHeadings.FirstOrDefaultAsync(s => s.Code == "CS");
                var mathSubject = await context.SubjectHeadings.FirstOrDefaultAsync(s => s.Code == "MATH");
                var urduSubject = await context.SubjectHeadings.FirstOrDefaultAsync(s => s.Code == "URD-LIT");
                var islSubject = await context.SubjectHeadings.FirstOrDefaultAsync(s => s.Code == "ISL");
                var oxford = await context.PublisherInfos.FirstOrDefaultAsync(p => p.Name == "Oxford University Press");
                var cambridge = await context.PublisherInfos.FirstOrDefaultAsync(p => p.Name == "Cambridge University Press");
                var sangMeel = await context.PublisherInfos.FirstOrDefaultAsync(p => p.Name == "Sang-e-Meel Publications");
                var ilmi = await context.PublisherInfos.FirstOrDefaultAsync(p => p.Name == "Ilmi Kitab Khana");

                if (urduLang != null && engLang != null && csSubject != null)
                {
                    var books = new List<ItemInfo>();

                    // English Books
                    if (oxford != null && csSubject != null && engLang != null)
                    {
                        books.Add(new ItemInfo { Title = "Clean Code", ISBN = "9780132350884", SubjectHeadingId = csSubject.Id, LanguageId = engLang.Id, PublisherInfoId = oxford.Id, TotalPages = 464, BindingType = "Paperback", Description = "Best practices for writing clean code", Keywords = "programming", IsActive = true });
                        books.Add(new ItemInfo { Title = "Introduction to Algorithms", ISBN = "9780262033848", SubjectHeadingId = csSubject.Id, LanguageId = engLang.Id, PublisherInfoId = oxford.Id, TotalPages = 1312, BindingType = "Hardcover", Description = "Algorithm design", Keywords = "algorithms", IsActive = true });
                        books.Add(new ItemInfo { Title = "The C++ Programming Language", ISBN = "9780321563842", SubjectHeadingId = csSubject.Id, LanguageId = engLang.Id, PublisherInfoId = oxford.Id, TotalPages = 1376, BindingType = "Hardcover", Description = "C++ by its creator", Keywords = "c++", IsActive = true });
                    }
                    if (cambridge != null && mathSubject != null && engLang != null)
                    {
                        books.Add(new ItemInfo { Title = "Calculus", ISBN = "9781337613927", SubjectHeadingId = mathSubject.Id, LanguageId = engLang.Id, PublisherInfoId = cambridge.Id, TotalPages = 1312, BindingType = "Hardcover", Description = "Calculus textbook", Keywords = "calculus", IsActive = true });
                    }
                    // Urdu Books
                    if (sangMeel != null && urduSubject != null && urduLang != null)
                    {
                        books.Add(new ItemInfo { Title = "Toba Tek Singh", ISBN = "9789693525801", SubjectHeadingId = urduSubject.Id, LanguageId = urduLang.Id, PublisherInfoId = sangMeel.Id, TotalPages = 128, BindingType = "Paperback", Description = "Famous Urdu short story by Manto", Keywords = "urdu literature", IsActive = true });
                        books.Add(new ItemInfo { Title = "Bang-e-Dara", ISBN = "9789693512344", SubjectHeadingId = urduSubject.Id, LanguageId = urduLang.Id, PublisherInfoId = sangMeel.Id, TotalPages = 300, BindingType = "Hardcover", Description = "Poetry by Allama Iqbal", Keywords = "urdu poetry", IsActive = true });
                        books.Add(new ItemInfo { Title = "Raja Gidh", ISBN = "9789693501232", SubjectHeadingId = urduSubject.Id, LanguageId = urduLang.Id, PublisherInfoId = sangMeel.Id, TotalPages = 400, BindingType = "Paperback", Description = "Novel by Bano Qudsia", Keywords = "urdu novel", IsActive = true });
                        books.Add(new ItemInfo { Title = "Nuskha Hae Wafa", ISBN = "9789693515673", SubjectHeadingId = urduSubject.Id, LanguageId = urduLang.Id, PublisherInfoId = sangMeel.Id, TotalPages = 250, BindingType = "Paperback", Description = "Poetry by Faiz Ahmed Faiz", Keywords = "urdu poetry", IsActive = true });
                    }
                    if (ilmi != null && islSubject != null && urduLang != null)
                    {
                        books.Add(new ItemInfo { Title = "Zavia", ISBN = "9789693517899", SubjectHeadingId = islSubject.Id, LanguageId = urduLang.Id, PublisherInfoId = ilmi.Id, TotalPages = 600, BindingType = "Hardcover", Description = "Islamic thought by Ashfaq Ahmed", Keywords = "islamic", IsActive = true });
                    }

                    if (books.Any())
                    {
                        await context.ItemInfos.AddRangeAsync(books);
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Book Copies
            if (!context.ItemCopies.Any())
            {
                var cleanCode = await context.ItemInfos.FirstOrDefaultAsync(i => i.Title == "Clean Code");
                var tobaTek = await context.ItemInfos.FirstOrDefaultAsync(i => i.Title == "Toba Tek Singh");
                var bangEDara = await context.ItemInfos.FirstOrDefaultAsync(i => i.Title == "Bang-e-Dara");
                var centralLib = await context.LibraryBranches.FirstOrDefaultAsync(b => b.Code == "LIB-CENTRAL");
                var rackA1 = await context.LibraryRacks.FirstOrDefaultAsync(r => r.Code == "RACK-A1");
                var rackB1 = await context.LibraryRacks.FirstOrDefaultAsync(r => r.Code == "RACK-B1");
                var rackC1 = await context.LibraryRacks.FirstOrDefaultAsync(r => r.Code == "RACK-C1");
                var shelf1 = await context.LibraryShelves.FirstOrDefaultAsync(s => s.Code == "SHELF-1");
                var shelf2 = await context.LibraryShelves.FirstOrDefaultAsync(s => s.Code == "SHELF-2");

                var copiesList = new List<ItemCopy>();

                if (cleanCode != null && centralLib != null && rackA1 != null && shelf1 != null)
                {
                    copiesList.Add(new ItemCopy { ItemInfoId = cleanCode.Id, Barcode = "BAR-001", AccessionNumber = "ACC-001", LibraryBranchId = centralLib.Id, LibraryRackId = rackA1.Id, LibraryShelfId = shelf1.Id, UnitCost = 4500, ConditionRemarks = "New", Status = "Available", IsActive = true });
                    copiesList.Add(new ItemCopy { ItemInfoId = cleanCode.Id, Barcode = "BAR-002", AccessionNumber = "ACC-002", LibraryBranchId = centralLib.Id, LibraryRackId = rackA1.Id, LibraryShelfId = shelf1.Id, UnitCost = 4500, ConditionRemarks = "New", Status = "Available", IsActive = true });
                    copiesList.Add(new ItemCopy { ItemInfoId = cleanCode.Id, Barcode = "BAR-003", AccessionNumber = "ACC-003", LibraryBranchId = centralLib.Id, LibraryRackId = rackA1.Id, LibraryShelfId = shelf2.Id, UnitCost = 4500, ConditionRemarks = "Good", Status = "Available", IsActive = true });
                }
                if (tobaTek != null && centralLib != null && rackB1 != null && shelf1 != null)
                {
                    copiesList.Add(new ItemCopy { ItemInfoId = tobaTek.Id, Barcode = "BAR-004", AccessionNumber = "ACC-004", LibraryBranchId = centralLib.Id, LibraryRackId = rackB1.Id, LibraryShelfId = shelf1.Id, UnitCost = 500, ConditionRemarks = "New", Status = "Issued", IsActive = true });
                    copiesList.Add(new ItemCopy { ItemInfoId = tobaTek.Id, Barcode = "BAR-005", AccessionNumber = "ACC-005", LibraryBranchId = centralLib.Id, LibraryRackId = rackB1.Id, LibraryShelfId = shelf1.Id, UnitCost = 500, ConditionRemarks = "Good", Status = "Available", IsActive = true });
                }
                if (bangEDara != null && centralLib != null && rackC1 != null && shelf1 != null)
                {
                    copiesList.Add(new ItemCopy { ItemInfoId = bangEDara.Id, Barcode = "BAR-006", AccessionNumber = "ACC-006", LibraryBranchId = centralLib.Id, LibraryRackId = rackC1.Id, LibraryShelfId = shelf1.Id, UnitCost = 800, ConditionRemarks = "New", Status = "Available", IsActive = true });
                }

                if (copiesList.Any())
                {
                    await context.ItemCopies.AddRangeAsync(copiesList);
                    await context.SaveChangesAsync();
                }
            }

            // Author-Item links
            if (!context.AuthorItemInfos.Any())
            {
                var cleanCode = await context.ItemInfos.FirstOrDefaultAsync(i => i.Title == "Clean Code");
                var uncleBob = await context.AuthorInfos.FirstOrDefaultAsync(a => a.Name == "Robert C. Martin");
                var bangEDara = await context.ItemInfos.FirstOrDefaultAsync(i => i.Title == "Bang-e-Dara");
                var iqbal = await context.AuthorInfos.FirstOrDefaultAsync(a => a.Name == "Allama Iqbal");
                var tobaTek = await context.ItemInfos.FirstOrDefaultAsync(i => i.Title == "Toba Tek Singh");
                var manto = await context.AuthorInfos.FirstOrDefaultAsync(a => a.Name == "Saadat Hasan Manto");

                var linksList = new List<AuthorItemInfo>();
                if (cleanCode != null && uncleBob != null) linksList.Add(new AuthorItemInfo { AuthorId = uncleBob.Id, ItemInfoId = cleanCode.Id, IsActive = true });
                if (bangEDara != null && iqbal != null) linksList.Add(new AuthorItemInfo { AuthorId = iqbal.Id, ItemInfoId = bangEDara.Id, IsActive = true });
                if (tobaTek != null && manto != null) linksList.Add(new AuthorItemInfo { AuthorId = manto.Id, ItemInfoId = tobaTek.Id, IsActive = true });

                if (linksList.Any())
                {
                    await context.AuthorItemInfos.AddRangeAsync(linksList);
                    await context.SaveChangesAsync();
                }
            }

            // ========================================
            // STEP 5: SECTION D - PEOPLE
            // ========================================

            // Parties
            if (!context.Parties.Any())
            {
                var parties = new[]
                {
                    new Party { PartyName = "Admin User", PartyContact = "03001234567", Email = "admin@mnsuam.edu.pk", IsActive = true },
                    new Party { PartyName = "Librarian User", PartyContact = "03007654321", Email = "librarian@mnsuam.edu.pk", IsActive = true },
                    new Party { PartyName = "Ahmad Raza", PartyContact = "03001112222", Email = "student@mnsuam.edu.pk", IsActive = true },
                    new Party { PartyName = "Prof. Dr. Saleem", PartyContact = "03003334444", Email = "faculty@mnsuam.edu.pk", IsActive = true },
                    new Party { PartyName = "Fatima Akhtar", PartyContact = "03005556666", Email = "staff@mnsuam.edu.pk", IsActive = true }
                };
                await context.Parties.AddRangeAsync(parties);
                await context.SaveChangesAsync();
            }

            // Library Members
            if (!context.LibraryMembers.Any())
            {
                var centralLib = await context.LibraryBranches.FirstOrDefaultAsync(b => b.Code == "LIB-CENTRAL");
                var ahmadRaza = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Ahmad Raza");
                var profSaleem = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Prof. Dr. Saleem");
                var fatima = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Fatima Akhtar");

                var membersList = new List<LibraryMember>();
                if (centralLib != null && ahmadRaza != null) membersList.Add(new LibraryMember { PartyId = ahmadRaza.Id, LibraryBranchId = centralLib.Id, CardNo = "LIB-000001", IssuedOn = DateTime.Now.AddDays(-30), ExpiredOn = DateTime.Now.AddYears(1), MembershipType = 1, IsActive = true });
                if (centralLib != null && profSaleem != null) membersList.Add(new LibraryMember { PartyId = profSaleem.Id, LibraryBranchId = centralLib.Id, CardNo = "LIB-000002", IssuedOn = DateTime.Now.AddDays(-30), ExpiredOn = DateTime.Now.AddYears(1), MembershipType = 2, IsActive = true });
                if (centralLib != null && fatima != null) membersList.Add(new LibraryMember { PartyId = fatima.Id, LibraryBranchId = centralLib.Id, CardNo = "LIB-000003", IssuedOn = DateTime.Now.AddDays(-30), ExpiredOn = DateTime.Now.AddYears(1), MembershipType = 3, IsActive = true });

                if (membersList.Any())
                {
                    await context.LibraryMembers.AddRangeAsync(membersList);
                    await context.SaveChangesAsync();
                }
            }

            // Students
            if (!context.Students.Any())
            {
                var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "CS");
                var ahmadRaza = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Ahmad Raza");

                if (csDept != null && ahmadRaza != null)
                {
                    var students = new[]
                    {
                        new Student { RegistrationNumber = "2024-CS-101", PartyId = ahmadRaza.Id, CandidateId = 12345, DegreeProgramId = 1, DegreeLevelId = 1, Quota = "Open Merit", TimingId = 1, ProgramSessionId = 1, IsActive = true }
                    };
                    await context.Students.AddRangeAsync(students);
                    await context.SaveChangesAsync();
                }
            }

            // Employees (Staff)
            if (!context.Employees.Any())
            {
                var fatima = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Fatima Akhtar");
                if (fatima != null)
                {
                    var employees = new[]
                    {
                        new Employee { Name = "Fatima Akhtar", Contact = "03005556666", Code = "EMP-001", JoiningDate = DateTime.Now.AddYears(-2), IsActive = true }
                    };
                    await context.Employees.AddRangeAsync(employees);
                    await context.SaveChangesAsync();
                }
            }

            // ========================================
            // STEP 6: SECTION E - CIRCULATION
            // ========================================

            // Durations
            if (!context.Durations.Any())
            {
                var durations = new[]
                {
                    new Duration { Name = "Standard Loan", Code = "14", Description = "14 Days Loan", IsActive = true },
                    new Duration { Name = "Extended Loan", Code = "30", Description = "30 Days Loan", IsActive = true },
                    new Duration { Name = "Faculty Loan", Code = "60", Description = "60 Days Loan", IsActive = true },
                    new Duration { Name = "Short Loan", Code = "7", Description = "7 Days Loan", IsActive = true }
                };
                await context.Durations.AddRangeAsync(durations);
                await context.SaveChangesAsync();
            }

            // Issuances (Loans)
            if (!context.Issuances.Any())
            {
                var studentMember = await context.LibraryMembers.FirstOrDefaultAsync(m => m.CardNo == "LIB-000001");
                var tobaTekCopy = await context.ItemCopies.FirstOrDefaultAsync(c => c.Barcode == "BAR-004");
                var stdDuration = await context.Durations.FirstOrDefaultAsync(d => d.Code == "14");

                if (studentMember != null && tobaTekCopy != null && stdDuration != null)
                {
                    var issuances = new[]
                    {
                        new Issuance { LibraryMemberId = studentMember.Id, ItemCopyId = tobaTekCopy.Id, DurationId = stdDuration.Id, IssueDate = DateTime.Now.AddDays(-10), DueDate = DateTime.Now.AddDays(4), DurationNumber = 14, FineAccrued = 0, RenewalsUsed = 0, IsActive = true }
                    };
                    await context.Issuances.AddRangeAsync(issuances);
                    await context.SaveChangesAsync();
                }
            }

            // Circulation Rules
            if (!context.CirculationRules.Any())
            {
                var studentParty = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Ahmad Raza");
                var facultyParty = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Prof. Dr. Saleem");
                var staffParty = await context.Parties.FirstOrDefaultAsync(p => p.PartyName == "Fatima Akhtar");
                var centralLib = await context.LibraryBranches.FirstOrDefaultAsync(b => b.Code == "LIB-CENTRAL");

                var rulesList = new List<CirculationRule>();
                if (studentParty != null && centralLib != null) rulesList.Add(new CirculationRule { PartyId = studentParty.Id, MaxCurrentLoans = 5, MaxCurrentReserves = 3, LoanDays = 14, GraceDay = 0, MaxRenewals = 2, OverdueFinePerDay = 5, MaxFinePerItem = 500, DamageValue = 200, LibraryBranchId = centralLib.Id, LostChargesItem = "Replacement Cost", IsActive = true });
                if (facultyParty != null && centralLib != null) rulesList.Add(new CirculationRule { PartyId = facultyParty.Id, MaxCurrentLoans = 15, MaxCurrentReserves = 5, LoanDays = 60, GraceDay = 0, MaxRenewals = 3, OverdueFinePerDay = 2, MaxFinePerItem = 1000, DamageValue = 500, LibraryBranchId = centralLib.Id, LostChargesItem = "Replacement Cost", IsActive = true });
                if (staffParty != null && centralLib != null) rulesList.Add(new CirculationRule { PartyId = staffParty.Id, MaxCurrentLoans = 8, MaxCurrentReserves = 4, LoanDays = 21, GraceDay = 0, MaxRenewals = 2, OverdueFinePerDay = 5, MaxFinePerItem = 500, DamageValue = 200, LibraryBranchId = centralLib.Id, LostChargesItem = "Replacement Cost", IsActive = true });

                if (rulesList.Any())
                {
                    await context.CirculationRules.AddRangeAsync(rulesList);
                    await context.SaveChangesAsync();
                }
            }

            // ========================================
            // STEP 7: SECTION F - FINES
            // ========================================

            // Fine Categories
            if (!context.FineCategories.Any())
            {
                var categories = new[]
                {
                    new FineCategory { FineCategoryName = "Overdue", IsActive = true },
                    new FineCategory { FineCategoryName = "Lost", IsActive = true },
                    new FineCategory { FineCategoryName = "Damaged", IsActive = true }
                };
                await context.FineCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // ========================================
            // STEP 8: CREATE APP USERS
            // ========================================

            // Admin User
            var adminEmail = "admin@mnsuam.edu.pk";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminParty = await context.Parties.FirstOrDefaultAsync(p => p.Email == adminEmail);
                var admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    UserType = "Admin",
                    PartyId = adminParty?.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    PreferredLanguage = "English"
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Librarian User
            var librarianEmail = "librarian@mnsuam.edu.pk";
            if (await userManager.FindByEmailAsync(librarianEmail) == null)
            {
                var librarianParty = await context.Parties.FirstOrDefaultAsync(p => p.Email == librarianEmail);
                var librarian = new AppUser
                {
                    UserName = librarianEmail,
                    Email = librarianEmail,
                    FullName = "Library Staff",
                    UserType = "Librarian",
                    PartyId = librarianParty?.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    PreferredLanguage = "English"
                };
                await userManager.CreateAsync(librarian, "Librarian@123");
                await userManager.AddToRoleAsync(librarian, "Librarian");
            }

            // Student User
            var studentEmail = "student@mnsuam.edu.pk";
            if (await userManager.FindByEmailAsync(studentEmail) == null)
            {
                var studentParty = await context.Parties.FirstOrDefaultAsync(p => p.Email == studentEmail);
                var student = new AppUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FullName = "Ahmad Raza",
                    UserType = "Student",
                    PartyId = studentParty?.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    PreferredLanguage = "Urdu"
                };
                await userManager.CreateAsync(student, "Student@123");
                await userManager.AddToRoleAsync(student, "Student");
            }

            // Faculty User
            var facultyEmail = "faculty@mnsuam.edu.pk";
            if (await userManager.FindByEmailAsync(facultyEmail) == null)
            {
                var facultyParty = await context.Parties.FirstOrDefaultAsync(p => p.Email == facultyEmail);
                var faculty = new AppUser
                {
                    UserName = facultyEmail,
                    Email = facultyEmail,
                    FullName = "Prof. Dr. Saleem",
                    UserType = "Faculty",
                    PartyId = facultyParty?.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    PreferredLanguage = "English"
                };
                await userManager.CreateAsync(faculty, "Faculty@123");
                await userManager.AddToRoleAsync(faculty, "Faculty");
            }

            // Staff User
            var staffEmail = "staff@mnsuam.edu.pk";
            if (await userManager.FindByEmailAsync(staffEmail) == null)
            {
                var staffParty = await context.Parties.FirstOrDefaultAsync(p => p.Email == staffEmail);
                var staff = new AppUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    FullName = "Fatima Akhtar",
                    UserType = "Staff",
                    PartyId = staffParty?.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    PreferredLanguage = "English"
                };
                await userManager.CreateAsync(staff, "Staff@123");
                await userManager.AddToRoleAsync(staff, "Staff");
            }

            Console.WriteLine("✅ Seed data completed successfully!");
            Console.WriteLine($"📚 Universities: {context.Universities.Count()}");
            Console.WriteLine($"📚 Books: {context.ItemInfos.Count()}");
            Console.WriteLine($"📚 Copies: {context.ItemCopies.Count()}");
            Console.WriteLine($"👥 Users: {userManager.Users.Count()}");
        }
    }
}