using FuzzySharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services.AI;

namespace UniSmartLMS.Services
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<ReserveItem> _reserveRepo;
        private readonly IGenericRepository<ItemInfo> _bookRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<CirculationRule> _ruleRepo;
        private readonly IGenericRepository<Party> _partyRepo;
        private readonly IIntentCacheService _intentCache;

        public AIAssistantService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<ReserveItem> reserveRepo,
            IGenericRepository<ItemInfo> bookRepo,
            IGenericRepository<ItemCopy> copyRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<CirculationRule> ruleRepo,
            IGenericRepository<Party> partyRepo,
            IIntentCacheService intentCache)
        {
            _context = context;
            _userManager = userManager;
            _issuanceRepo = issuanceRepo;
            _fineRepo = fineRepo;
            _reserveRepo = reserveRepo;
            _bookRepo = bookRepo;
            _copyRepo = copyRepo;
            _memberRepo = memberRepo;
            _ruleRepo = ruleRepo;
            _partyRepo = partyRepo;
            _intentCache = intentCache;
        }
        // Add this method after the constructor

private (bool isAction, string response) DetectIntentFromCache(string query, string role)
    {
        var intents = _intentCache.GetIntents();
        var lowerQuery = query.ToLower();

        // First try exact match
        foreach (var intent in intents)
        {
            if (lowerQuery.Contains(intent.IntentKeyword.ToLower()))
            {
                return (true, $"[[REDIRECT]]/{intent.Controller}/{intent.Action}");
            }
        }

        // ✅ Fuzzy match for spelling mistakes
        var bestMatch = intents
            .Select(intent => new {
                Intent = intent,
                Score = Fuzz.PartialRatio(lowerQuery, intent.IntentKeyword.ToLower())
            })
            .Where(x => x.Score > 65)  // 65% match threshold
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestMatch != null)
        {
            return (true, $"[[REDIRECT]]/{bestMatch.Intent.Controller}/{bestMatch.Intent.Action}");
        }

        return (false, null);
    }

    // ========================================
    // MULTI-LANGUAGE SUPPORT
    // ========================================

    private async Task<string> GetLocalizedResponse(string englishText, string urduText, string romanUrduText, AppUser user)
        {
            var language = user?.PreferredLanguage ?? "English";
            return language switch
            {
                "Urdu" => urduText,
                "RomanUrdu" => romanUrduText,
                _ => englishText
            };
        }

        private string GetUrduGreeting(int hour)
        {
            if (hour < 12) return "صبح بخیر";
            if (hour < 18) return "دوپہر بخیر";
            return "شام بخیر";
        }

        private string GetRomanUrduGreeting(int hour)
        {
            if (hour < 12) return "Subah Bakhair";
            if (hour < 18) return "Dopahar Bakhair";
            return "Sham Bakhair";
        }

        public async Task<string> GetGreetingAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Guest";
            var hour = DateTime.Now.Hour;
            var language = user?.PreferredLanguage ?? "English";

            if (language == "Urdu")
            {
                var urduGreeting = GetUrduGreeting(hour);
                var roleGreetings = new Dictionary<string, string>
                {
                    { "Student", $"{urduGreeting}، {user?.FullName}! {await GetAvailableBooksCountAsync()} کتابیں دستیاب ہیں" },
                    { "Faculty", $"{urduGreeting}، پروفیسر {user?.FullName}! آپ کے تحقیقی وسائل تیار ہیں" },
                    { "Librarian", $"{urduGreeting}، لائبریرین {user?.FullName}! آج {await GetTodaysActivitySummaryUrduAsync()}" },
                    { "Admin", $"{urduGreeting}، ایڈمن {user?.FullName}! سسٹم بہتر طریقے سے چل رہا ہے" },
                    { "Visitor", $"{urduGreeting}، یونی سمارٹ ایل ایم ایس میں خوش آمدید!" }
                };
                return roleGreetings.ContainsKey(role) ? roleGreetings[role] : $"{urduGreeting}! یونی سمارٹ ایل ایم ایس میں خوش آمدید";
            }

            if (language == "RomanUrdu")
            {
                var romanGreeting = GetRomanUrduGreeting(hour);
                var roleGreetings = new Dictionary<string, string>
                {
                    { "Student", $"{romanGreeting}, {user?.FullName}! {await GetAvailableBooksCountAsync()} kitabein dastiyab hain" },
                    { "Faculty", $"{romanGreeting}, Professor {user?.FullName}! Aap ke tehqeeqi wasail tayar hain" },
                    { "Librarian", $"{romanGreeting}, Librarian {user?.FullName}! Aaj {await GetTodaysActivitySummaryRomanUrduAsync()}" },
                    { "Admin", $"{romanGreeting}, Admin {user?.FullName}! System behtar tareeqay se chal raha hai" }
                };
                return roleGreetings.ContainsKey(role) ? roleGreetings[role] : $"{romanGreeting}! UniSmart LMS mein khush amdeed";
            }

            // English (default)
            var timeOfDay = hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
            var englishGreetings = new Dictionary<string, string>
            {
                { "Student", $"📚 {timeOfDay}, {user?.FullName}! Ready to explore {await GetAvailableBooksCountAsync()} books available?" },
                { "Faculty", $"🎓 {timeOfDay}, Professor {user?.FullName}! Your research resources are ready." },
                { "Librarian", $"📖 {timeOfDay}, Librarian {user?.FullName}! {await GetTodaysActivitySummaryAsync()}" },
                { "Admin", $"⚙️ {timeOfDay}, Admin {user?.FullName}! System is running smoothly." }
            };
            return englishGreetings.ContainsKey(role) ? englishGreetings[role] : $"👋 {timeOfDay}, welcome to UniSmartLMS!";
        }

        public async Task<string> GetRoleBasedHelpAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Guest";

            var helps = new Dictionary<string, string>
            {
                { "Student", GetStudentHelp() },
                { "Faculty", GetFacultyHelp() },
                { "Staff", GetStaffHelp() },
                { "Librarian", await GetLibrarianHelpAsync() },
                { "Admin", await GetAdminHelpAsync() },
                { "Visitor", GetVisitorHelp() }
            };
            return helps.ContainsKey(role) ? helps[role] : "Type 'help' to see what I can do for you!";
        }

        public async Task<string> GetResponseAsync(string userQuery, AppUser user)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
                return "Please ask me something!";

            var language = user?.PreferredLanguage ?? "English";
            var lowerQuery = userQuery.ToLower().Trim();
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Guest";
            // ========================================
            // ACTION INTENT DETECTION (NEW)
            // ========================================
            var (isAction, actionResponse) = DetectIntentFromCache(lowerQuery, role);
            if (isAction) return actionResponse;

            // ========================================
            // BOOK LOCATION & FINE PREDICTION
            // ========================================
            if (lowerQuery.Contains("where is") || lowerQuery.Contains("location of"))
                return await GetBookLocationAsync(userQuery, user);

            if (lowerQuery.Contains("predict fine") || lowerQuery.Contains("if i return") || lowerQuery.Contains("days late"))
                return await CalculatePredictedFineAsync(userQuery, user);

            // ========================================
            // LANGUAGE-SPECIFIC QUERIES
            // ========================================
            if (language == "Urdu")
            {
                if ((lowerQuery.Contains("کتاب") && (lowerQuery.Contains("ڈھونڈو") || lowerQuery.Contains("تلاش"))) || lowerQuery.Contains("سرچ"))
                    return await SearchBooksAsync(userQuery, user, language);

                if (lowerQuery.Contains("میرا قرض") || lowerQuery.Contains("میری کتابیں"))
                    return await GetStudentLoansAsync(user);

                if (lowerQuery.Contains("جرمانہ") || lowerQuery.Contains("رقم"))
                    return await GetStudentFinesAsync(user);
            }

            if (language == "RomanUrdu")
            {
                if ((lowerQuery.Contains("kitab") && (lowerQuery.Contains("dhoondo") || lowerQuery.Contains("talash"))) || lowerQuery.Contains("search"))
                    return await SearchBooksAsync(userQuery, user, language);

                if (lowerQuery.Contains("mera qarz") || lowerQuery.Contains("meri kitabein"))
                    return await GetStudentLoansAsync(user);

                if (lowerQuery.Contains("jurmana") || lowerQuery.Contains("raqam"))
                    return await GetStudentFinesAsync(user);
            }

            // Language Switch Help
            if (lowerQuery.Contains("urdu mein") || lowerQuery.Contains("language change") || lowerQuery.Contains("change language"))
            {
                return await GetLocalizedResponse(
                    "To change language, click the 'Language' button in the top menu and select your preferred language.",
                    "زبان تبدیل کرنے کے لیے، اوپر والے مینو میں 'Language' بٹن پر کلک کریں اور اپنی پسندیدہ زبان منتخب کریں۔",
                    "Zaban tabdeel karne ke liye, upar wale menu mein 'Language' button par click karein aur apni pasandeeda zaban muntakhib karein.",
                    user);
            }

            // ========================================
            // GREETINGS & GENERAL
            // ========================================
            if (lowerQuery.Contains("hello") || lowerQuery.Contains("hi") || lowerQuery.Contains("salam") || lowerQuery.Contains("assalam"))
                return await GetGreetingAsync(user);

            if (lowerQuery.Contains("help") || lowerQuery.Contains("what can you do") || lowerQuery.Contains("how to use"))
                return await GetRoleBasedHelpAsync(user);

            if (lowerQuery.Contains("thank") || lowerQuery.Contains("shukriya") || lowerQuery.Contains("thanks"))
                return "You're welcome! 😊 Is there anything else I can help you with?";

            if (lowerQuery.Contains("bye") || lowerQuery.Contains("goodbye") || lowerQuery.Contains("allah hafiz"))
                return "Goodbye! 👋 Come back anytime. Happy reading! 📚";

            // ========================================
            // LIBRARY INFO (All Roles)
            // ========================================
            if (lowerQuery.Contains("timing") || lowerQuery.Contains("open") || lowerQuery.Contains("closed") || lowerQuery.Contains("hours"))
                return GetLibraryTimings();

            if (lowerQuery.Contains("contact") || lowerQuery.Contains("phone") || lowerQuery.Contains("email"))
                return GetLibraryContact();

            if (lowerQuery.Contains("location") || lowerQuery.Contains("address") || lowerQuery.Contains("where"))
                return GetLibraryAddress();

            if (lowerQuery.Contains("rules") || lowerQuery.Contains("policy") || lowerQuery.Contains("regulation"))
                return await GetLibraryRulesAsync(role);

            // ========================================
            // SEARCH BOOKS (All Roles)
            // ========================================
            if (lowerQuery.Contains("search") || lowerQuery.Contains("find book") || lowerQuery.Contains("look for") || lowerQuery.Contains("about"))
                return await SearchBooksAsync(userQuery, user, language);

            if (lowerQuery.Contains("available books") || lowerQuery.Contains("total books") || lowerQuery.Contains("how many books"))
                return await GetAvailableBooksDetailsAsync();

            if (lowerQuery.Contains("popular") || lowerQuery.Contains("bestseller") || lowerQuery.Contains("most borrowed"))
                return await GetPopularBooksAsync();

            if (lowerQuery.Contains("new arrivals") || lowerQuery.Contains("new books") || lowerQuery.Contains("latest"))
                return await GetNewArrivalsAsync();

            // ========================================
            // STUDENT SPECIFIC
            // ========================================
            if (role == "Student")
            {
                if (lowerQuery.Contains("my loan") || lowerQuery.Contains("my book") || lowerQuery.Contains("what i borrowed") || lowerQuery.Contains("current loan"))
                    return await GetStudentLoansAsync(user);

                if (lowerQuery.Contains("my fine") || lowerQuery.Contains("fine amount") || lowerQuery.Contains("how much fine") || lowerQuery.Contains("pending fine"))
                    return await GetStudentFinesAsync(user);

                if (lowerQuery.Contains("due") || lowerQuery.Contains("return") || lowerQuery.Contains("when to return"))
                    return await GetStudentDueDatesAsync(user);

                if (lowerQuery.Contains("max loan") || lowerQuery.Contains("how many book") || lowerQuery.Contains("borrow limit"))
                    return await GetStudentBorrowLimitAsync(user);

                if (lowerQuery.Contains("reservation") || lowerQuery.Contains("hold") || lowerQuery.Contains("my reservation"))
                    return await GetStudentReservationsAsync(user);

                if (lowerQuery.Contains("renew") || lowerQuery.Contains("extend"))
                    return await GetStudentRenewalHelpAsync(user);

                if (lowerQuery.Contains("recommend") || lowerQuery.Contains("suggest") || lowerQuery.Contains("similar books"))
                    return await GetRecommendationsAsync(user);
            }

            // ========================================
            // FACULTY SPECIFIC
            // ========================================
            if (role == "Faculty")
            {
                if (lowerQuery.Contains("my loan") || lowerQuery.Contains("my book") || lowerQuery.Contains("faculty loans"))
                    return await GetFacultyLoansAsync(user);

                if (lowerQuery.Contains("course reserve") || lowerQuery.Contains("course material"))
                    return await GetCourseReservesAsync(user);

                if (lowerQuery.Contains("research") || lowerQuery.Contains("journal") || lowerQuery.Contains("paper"))
                    return await GetResearchResourcesAsync();

                if (lowerQuery.Contains("recommend book") || lowerQuery.Contains("suggest purchase"))
                    return GetBookRecommendationHelp();
            }

            // ========================================
            // LIBRARIAN SPECIFIC
            // ========================================
            if (role == "Librarian")
            {
                if (lowerQuery.Contains("today") || lowerQuery.Contains("today's") || lowerQuery.Contains("daily"))
                    return await GetDailyReportAsync();

                if (lowerQuery.Contains("overdue") || lowerQuery.Contains("late book"))
                    return await GetOverdueReportAsync();

                if (lowerQuery.Contains("pending fine") || lowerQuery.Contains("unpaid fine"))
                    return await GetPendingFinesReportAsync();

                if (lowerQuery.Contains("popular") || lowerQuery.Contains("most borrowed"))
                    return await GetPopularBooksReportAsync();

                if (lowerQuery.Contains("member activity") || lowerQuery.Contains("active members"))
                    return await GetMemberActivityReportAsync();

                if (lowerQuery.Contains("add member") || lowerQuery.Contains("register member"))
                    return "➕ To add a new member:\n1. Go to 'People' → 'Library Members'\n2. Click 'Create New'\n3. Select Party (person)\n4. Enter Card No and expiry date\n5. Click 'Create'";

                if (lowerQuery.Contains("issue book") || lowerQuery.Contains("borrow book"))
                    return "📖 To issue a book:\n1. Go to 'Circulation' → 'Issuances'\n2. Click 'Create New'\n3. Select member and book copy\n4. Select duration\n5. Click 'Issue Book'";

                if (lowerQuery.Contains("return book"))
                    return "🔄 To return a book:\n1. Go to 'Circulation' → 'Returns'\n2. Click 'Process Return'\n3. Select the issuance ID\n4. Click 'Process Return'";
            }

            // ========================================
            // ADMIN SPECIFIC
            // ========================================
            if (role == "Admin")
            {
                if (lowerQuery.Contains("stats") || lowerQuery.Contains("statistics") || lowerQuery.Contains("overview"))
                    return await GetSystemStatisticsAsync();

                if (lowerQuery.Contains("users") || lowerQuery.Contains("total users") || lowerQuery.Contains("user count"))
                    return await GetUserStatisticsAsync();

                if (lowerQuery.Contains("backup") || lowerQuery.Contains("database"))
                    return "💾 Database backup is configured daily at 2 AM. Manual backup can be taken from Admin → System Settings.";

                if (lowerQuery.Contains("audit") || lowerQuery.Contains("log"))
                    return "📋 Audit logs track all user actions. Go to Admin → Audit Logs to view complete history.";

                if (lowerQuery.Contains("create user") || lowerQuery.Contains("add user"))
                    return "👤 To create a user:\n1. Go to 'Admin' → 'User Management'\n2. Click 'Create User'\n3. Fill details and select role\n4. Click 'Create'";

                if (lowerQuery.Contains("assign role"))
                    return "🎭 To assign a role:\n1. Go to 'Admin' → 'Assign Role'\n2. Select user and role\n3. Click 'Assign'";
            }


            // ========================================
            // GUEST/VISITOR SPECIFIC
            // ========================================
            if (role == "Visitor")
            {
                if (lowerQuery.Contains("register") || lowerQuery.Contains("sign up") || lowerQuery.Contains("become member"))
                    return GetRegistrationHelp();

                if (lowerQuery.Contains("borrow") || lowerQuery.Contains("loan"))
                    return "📖 Visitors cannot borrow books directly. Please register as Student or Faculty member first.";
            }
            // Detect action from cached intents
            //var (isAction, actionResponse) = DetectIntentFromCache(lowerQuery, role);
            //if (isAction) return actionResponse;
            // ========================================
            // DEFAULT RESPONSE
            // ========================================
            return $@"I understand you're asking about: ""{userQuery}""

As a {role}, here's what I can help you with:

{await GetRoleBasedHelpAsync(user)}

💡 Try:
• 'search for [book name]'
• 'my loans' (if you're a member)
• 'library timings' (for library hours)
• 'how to issue a book' (for librarian)
• 'system statistics' (for admin)";
        }

        // ========================================
        // HELPER METHODS
        // ========================================

        private async Task<int> GetAvailableBooksCountAsync()
        {
            return await _context.ItemInfos.CountAsync();
        }

        private async Task<string> GetTodaysActivitySummaryAsync()
        {
            var today = DateTime.Today;
            var todaysIssues = await _context.Issuances.CountAsync(i => i.IssueDate.Date == today);
            var overdue = await _context.Issuances.CountAsync(i => i.IsActive && i.IssueDate.AddDays(i.DurationNumber).Date < today);
            return $"{todaysIssues} issues today, {overdue} overdue books";
        }

        private async Task<string> GetTodaysActivitySummaryUrduAsync()
        {
            var today = DateTime.Today;
            var todaysIssues = await _context.Issuances.CountAsync(i => i.IssueDate.Date == today);
            var overdue = await _context.Issuances.CountAsync(i => i.IsActive && i.IssueDate.AddDays(i.DurationNumber).Date < today);
            return $"آج {todaysIssues} کتابیں جاری ہوئیں، {overdue} کتابیں اوور ڈیو ہیں";
        }

        private async Task<string> GetTodaysActivitySummaryRomanUrduAsync()
        {
            var today = DateTime.Today;
            var todaysIssues = await _context.Issuances.CountAsync(i => i.IssueDate.Date == today);
            var overdue = await _context.Issuances.CountAsync(i => i.IsActive && i.IssueDate.AddDays(i.DurationNumber).Date < today);
            return $"aaj {todaysIssues} kitabein jari huin, {overdue} kitabein over due hain";
        }

        private async Task<string> GetSystemHealthAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var activeLoans = await _context.Issuances.CountAsync(i => i.IsActive);
            return $"System healthy! {totalUsers} users, {activeLoans} active loans.";
        }

        private string GetStudentHelp() => @"📚 **What I can do for STUDENTS:**

1. 🔍 **Search Books** - 'search for C# programming'
2. 📖 **My Loans** - 'my loans' or 'what books I borrowed'
3. 💰 **My Fines** - 'my fines' or 'how much do I owe'
4. 📅 **Due Dates** - 'when to return my books'
5. ⭐ **Reserve Books** - 'reserve [book name]'
6. 🔄 **Renew Books** - 'renew my book'
7. 📊 **Borrow Limit** - 'how many books can I borrow'
8. ✨ **Recommendations** - 'recommend books for me'
9. 📚 **New Arrivals** - 'new books in library'
10. 🏆 **Popular Books** - 'most borrowed books'";

        private string GetFacultyHelp() => @"🎓 **What I can do for FACULTY:**

1. 🔬 **Research Resources** - 'research papers in my field'
2. 📖 **My Loans** - 'my faculty loans'
3. 📝 **Course Reserves** - 'my course reserves'
4. ⭐ **Book Recommendations** - 'recommend book for purchase'
5. 📊 **Department Statistics** - 'most borrowed books in my department'
6. 📚 **Journals** - 'access academic journals'
7. 🔍 **Advanced Search** - 'search by subject or author'";

        private string GetStaffHelp() => @"👔 **What I can do for STAFF:**

1. 📖 **My Loans** - 'my current loans'
2. 💰 **My Fines** - 'check my fines'
3. 🔍 **Search Books** - 'search for [book name]'
4. 📅 **Due Dates** - 'when are my books due'
5. 📚 **Library Info** - 'library timings'";

        private async Task<string> GetLibrarianHelpAsync()
        {
            var today = DateTime.Today;
            var overdueCount = await _context.Issuances.CountAsync(i => i.IsActive && i.IssueDate.AddDays(i.DurationNumber).Date < today);
            var pendingFines = await _context.UserFines.SumAsync(f => f.FineAccrued);

            return $@"📖 **What I can do for LIBRARIANS:**

📊 **Reports:**
• 'daily report' - today's activity
• 'overdue books' - list of overdue items
• 'pending fines' - unpaid fines summary
• 'popular books' - most borrowed this month
• 'member activity' - active members report

🔧 **Operations:**
• 'issue book' - how to issue a book
• 'return book' - how to process returns
• 'add member' - how to register new member
• 'collect fine' - how to record payment

⚠️ **Current Status:**
• Overdue Books: {overdueCount}
• Pending Fines: Rs. {pendingFines:N2}

Type 'help' for all commands or ask specific questions!";
        }

        private async Task<string> GetAdminHelpAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalBooks = await _context.ItemInfos.CountAsync();

            return $@"⚙️ **What I can do for ADMIN:**

👑 **User Management:**
• 'create user' - add new user accounts
• 'assign role' - change user roles
• 'user statistics' - view all user counts
• 'reset password' - help with password reset

📊 **System Management:**
• 'system statistics' - complete system overview
• 'audit logs' - view all user actions
• 'backup' - database backup info
• 'system health' - check system status

📈 **Current Status:**
• Total Users: {totalUsers}
• Total Books: {totalBooks}

Type 'help' for all commands or ask specific questions!";
        }

        private string GetVisitorHelp() => @"👤 **What I can do for VISITORS:**

1. 🔍 **Search Books** - 'search for [book name]'
2. 📚 **Library Info** - 'library timings', 'library contact'
3. 📖 **About Membership** - 'how to become a member'
4. 📊 **Catalog** - 'total books available'

💡 **Tip:** Register as Student or Faculty to borrow books!";

        private string GetLibraryTimings() => "🕐 **Library Timings:**\nMonday - Saturday: 8:00 AM - 8:00 PM\nFriday: 2:00 PM - 8:00 PM\nSunday: Closed";

        private string GetLibraryContact() => "📞 **Contact Info:**\nPhone: +92 42 1234 5678\nEmail: library@unismartlms.edu.pk\nEmergency: 1122";

        private string GetLibraryAddress() => "📍 **Address:**\nMain Library Building, Ground Floor\nUniversity Campus, Lahore, Pakistan";

        private async Task<string> GetLibraryRulesAsync(string role)
        {
            var rules = await _ruleRepo.GetAllAsync();
            var defaultRule = rules.FirstOrDefault();

            return $@"📋 **Library Rules:**

• Maximum Books: {defaultRule?.MaxCurrentLoans ?? 5} at a time
• Loan Period: {defaultRule?.LoanDays ?? 14} days
• Overdue Fine: Rs. {defaultRule?.OverdueFinePerDay ?? 5}/day
• Maximum Fine: Rs. {defaultRule?.MaxFinePerItem ?? 500} per item
• Renewals: {defaultRule?.MaxRenewals ?? 2} times allowed

{GetRoleSpecificRules(role)}";

            string GetRoleSpecificRules(string userRole)
            {
                if (userRole == "Faculty") return "• Faculty can borrow for 30 days\n• 15 books maximum";
                if (userRole == "Staff") return "• Staff can borrow for 21 days\n• 8 books maximum";
                if (userRole == "Student") return "• Students can borrow for 14 days\n• 5 books maximum";
                return "";
            }
        }

        private async Task<string> SearchBooksAsync(string userQuery, AppUser user, string language)
        {
            var keywords = new[] { "search", "find", "look for", "about", "سرچ", "ڈھونڈو", "تلاش", "dhoondo", "talash" };
            string searchTerm = null;

            foreach (var kw in keywords)
            {
                if (userQuery.ToLower().Contains(kw))
                {
                    var parts = userQuery.ToLower().Split(new[] { kw }, StringSplitOptions.None);
                    if (parts.Length > 1)
                        searchTerm = parts[1].Trim();
                    break;
                }
            }

            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 2)
            {
                return await GetLocalizedResponse(
                    "What book would you like to search for? Please tell me the title, author, or subject.\n\nExample: 'search for C# programming' or 'find books by John Doe'",
                    "آپ کون سی کتاب تلاش کرنا چاہیں گے؟ براہ کرم مجھے عنوان، مصنف یا موضوع بتائیں۔\n\nمثال: 'C# پروگرامنگ کے لیے تلاش کریں' یا 'جان ڈو کی کتابیں تلاش کریں'",
                    "Aap kaun si kitab talash karna chahein ge? Barah karam mujhe unwaan, musannif ya mawzu bataein.\n\nMisaal: 'C# programming ke liye talash karein' ya 'John Doe ki kitabein talash karein'",
                    user);
            }

            var books = await _bookRepo.SearchAsync("Title", searchTerm);
            books = books.Take(5);

            if (!books.Any())
            {
                return await GetLocalizedResponse(
                    $"I couldn't find any books matching '{searchTerm}'. Try different keywords or check spelling.",
                    $"معاف کیجیے، '{searchTerm}' کے بارے میں کوئی کتاب نہیں ملی۔ مختلف کلیدی الفاظ آزمائیں۔",
                    $"Maaf kijiye, '{searchTerm}' ke bare mein koi kitab nahi milli. Mukhtalif kalime alfaaz azmaein.",
                    user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Guest";
            var response = await GetLocalizedResponse(
                $"📚 **Found {books.Count()} book(s) matching '{searchTerm}':**\n\n",
                $"📚 **'{searchTerm}' سے ملنے والی {books.Count()} کتابیں:**\n\n",
                $"📚 **'{searchTerm}' se milne wali {books.Count()} kitabein:**\n\n",
                user);

            foreach (var book in books)
            {
                var copies = await _context.ItemCopies.CountAsync(c => c.ItemInfoId == book.Id && c.IsActive);
                var available = copies > 0 ? (language == "Urdu" ? $"✅ {copies} کاپیاں دستیاب" : language == "RomanUrdu" ? $"✅ {copies} copyein dastiyab" : $"✅ {copies} copy(s) available") : (language == "Urdu" ? "❌ فی الحال دستیاب نہیں" : language == "RomanUrdu" ? "❌ filhal dastiyab nahi" : "❌ Currently unavailable");
                var isAvailable = copies > 0 && (role == "Student" || role == "Faculty" || role == "Staff" || role == "Librarian");
                var borrowText = isAvailable ? (language == "Urdu" ? "\n🔹 *لائبریری سے لے سکتے ہیں*" : language == "RomanUrdu" ? "\n🔹 *Library se le sakte hain*" : "\n🔹 *Can borrow from library*") : "";

                response += $"**{book.Title}**\n";
                response += $"ISBN: {book.ISBN}\n";
                response += $"Status: {available}\n";
                response += $"{borrowText}\n\n";
            }
            response += await GetLocalizedResponse("Type 'help' for more assistance!", "مزید مدد کے لیے 'help' ٹائپ کریں!", "Mazid madad ke liye 'help' type karein!", user);
            return response;
        }

        private async Task<string> GetAvailableBooksDetailsAsync()
        {
            var totalBooks = await _context.ItemInfos.CountAsync();
            var totalCopies = await _context.ItemCopies.CountAsync();
            var availableCopies = await _context.ItemCopies.CountAsync(c => c.IsActive);
            var totalIssuances = await _context.Issuances.CountAsync();

            return $"📊 **Library Collection Status:**\n\n• Total Unique Books: {totalBooks}\n• Total Physical Copies: {totalCopies}\n• Available Copies: {availableCopies}\n• Books Currently Issued: {totalIssuances}\n• Collection Updated: Daily";
        }

        private async Task<string> GetPopularBooksAsync()
        {
            var popular = await _context.Issuances
                .GroupBy(i => i.ItemCopyId)
                .Select(g => new { CopyId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            if (!popular.Any()) return "No borrowing data available yet.";

            var response = "🏆 **Most Popular Books:**\n\n";
            foreach (var item in popular)
            {
                var copy = await _context.ItemCopies.FindAsync(item.CopyId);
                if (copy != null)
                {
                    var book = await _context.ItemInfos.FindAsync(copy.ItemInfoId);
                    if (book != null)
                        response += $"• {book.Title} - Borrowed {item.Count} times\n";
                }
            }
            return response;
        }

        private async Task<string> GetNewArrivalsAsync()
        {
            var newBooks = await _context.ItemInfos
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            if (!newBooks.Any()) return "No new arrivals in the past month.";

            var response = "🆕 **Latest Arrivals:**\n\n";
            foreach (var book in newBooks)
                response += $"• {book.Title} - Added {book.CreatedAt:dd MMM yyyy}\n";
            return response;
        }

        private async Task<string> GetStudentLoansAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);

            if (libraryMember == null) return "You don't have a library membership. Please visit the library to register.";

            var loans = await _issuanceRepo.GetAllAsync();
            var activeLoans = loans.Where(l => l.LibraryMemberId == libraryMember.Id && l.IsActive).ToList();

            if (!activeLoans.Any()) return "📖 You don't have any active loans. Visit the library to borrow books!";

            var response = $"📚 **You have {activeLoans.Count} active loan(s):**\n\n";
            foreach (var loan in activeLoans)
            {
                var copy = await _copyRepo.GetByIdAsync(loan.ItemCopyId);
                var book = copy != null ? await _bookRepo.GetByIdAsync(copy.ItemInfoId) : null;
                var dueDate = loan.IssueDate.AddDays(loan.DurationNumber);
                var daysLeft = (dueDate - DateTime.Now).Days;
                var status = daysLeft < 0 ? "⚠️ OVERDUE" : $"{daysLeft} days left";

                response += $"**{book?.Title}**\n";
                response += $"Barcode: {copy?.Barcode}\n";
                response += $"Due: {dueDate:dd MMM yyyy} ({status})\n";
                response += $"Renewals used: {loan.RenewalsUsed}/2\n\n";
            }
            return response;
        }

        private async Task<string> GetStudentFinesAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);
            if (libraryMember == null) return "You don't have a library membership.";

            var fines = await _fineRepo.GetAllAsync();
            var userFines = fines.Where(f => f.LibraryMemberId == libraryMember.Id && f.IsActive).ToList();
            var totalFine = userFines.Sum(f => f.FineAccrued);

            if (totalFine <= 0) return "✅ Good news! You have no pending fines. Keep up the good work!";
            return $"⚠️ You have pending fines of **Rs. {totalFine:N2}**.\n\nPlease pay at the library counter or use JazzCash/EasyPaisa.";
        }

        private async Task<string> GetStudentDueDatesAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);
            if (libraryMember == null) return "You don't have any active loans.";

            var loans = await _issuanceRepo.GetAllAsync();
            var activeLoans = loans.Where(l => l.LibraryMemberId == libraryMember.Id && l.IsActive).ToList();

            if (!activeLoans.Any()) return "You have no books to return.";

            var soonestDue = activeLoans.Min(l => l.IssueDate.AddDays(l.DurationNumber));
            var daysLeft = (soonestDue - DateTime.Now).Days;

            if (daysLeft < 0) return $"⚠️ Your book was due on {soonestDue:dd MMM yyyy}. It is {Math.Abs(daysLeft)} days overdue. Please return immediately!";
            if (daysLeft <= 3) return $"📅 Your book is due in {daysLeft} days on {soonestDue:dd MMM yyyy}. Please return on time to avoid fines.";
            return $"📅 Your next due date is {soonestDue:dd MMM yyyy} ({daysLeft} days from now).";
        }

        private async Task<string> GetStudentBorrowLimitAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);

            var maxLoans = 5;
            var currentLoansCount = 0;
            if (libraryMember != null)
            {
                var allIssuances = await _issuanceRepo.GetAllAsync();
                currentLoansCount = allIssuances.Count(l => l.LibraryMemberId == libraryMember.Id && l.IsActive);
            }
            return $"📚 As a Student, you can borrow up to **{maxLoans} books** at a time.\nYou currently have **{currentLoansCount}** book(s) borrowed.\nRemaining: {maxLoans - currentLoansCount}";
        }

        private async Task<string> GetStudentReservationsAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);
            if (libraryMember == null) return "You don't have a library membership.";

            var reservations = await _reserveRepo.GetAllAsync();
            var userReservations = reservations.Where(r => r.LibraryMemberId == libraryMember.Id && r.IsActive).ToList();

            if (!userReservations.Any()) return "⭐ You have no active reservations.\n\nYou can reserve books that are currently issued to others.";

            var response = "⭐ **Your Reservations:**\n\n";
            foreach (var res in userReservations)
            {
                var book = await _bookRepo.GetByIdAsync(res.ItemInfoId);
                response += $"• {book?.Title}\n";
                response += $"  Expires: {res.ExpiryDate:dd MMM yyyy}\n";
                response += $"  Status: {(res.IsAvailable ? "✅ Available for pickup" : "⏳ Waiting")}\n\n";
            }
            return response;
        }

        private async Task<string> GetStudentRenewalHelpAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);
            if (libraryMember == null) return "You don't have a library membership.";

            var loans = await _issuanceRepo.GetAllAsync();
            var activeLoans = loans.Where(l => l.LibraryMemberId == libraryMember.Id && l.IsActive).ToList();

            if (!activeLoans.Any()) return "You have no active loans to renew.";

            var renewable = activeLoans.Where(l => l.RenewalsUsed < 2 && l.IssueDate.AddDays(l.DurationNumber) > DateTime.Now).ToList();

            if (!renewable.Any()) return "No books are eligible for renewal. Either you've reached max renewals or books are overdue.";

            var response = "🔄 **Books eligible for renewal:**\n\n";
            foreach (var loan in renewable)
            {
                var copy = await _copyRepo.GetByIdAsync(loan.ItemCopyId);
                var book = copy != null ? await _bookRepo.GetByIdAsync(copy.ItemInfoId) : null;
                response += $"• {book?.Title} - {loan.RenewalsUsed}/2 renewals used\n";
            }
            response += "\nTo renew, go to Student Dashboard → My Loans → Click 'Renew' button.";
            return response;
        }

        private async Task<string> GetRecommendationsAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);

            if (libraryMember == null) return "Login to get personalized recommendations.";

            var userLoans = await _issuanceRepo.GetAllAsync();
            var userSubjectIds = new HashSet<int>();

            foreach (var loan in userLoans)
            {
                var copy = await _copyRepo.GetByIdAsync(loan.ItemCopyId);
                if (copy != null)
                {
                    var book = await _bookRepo.GetByIdAsync(copy.ItemInfoId);
                    if (book?.SubjectHeadingId != null)
                        userSubjectIds.Add(book.SubjectHeadingId.Value);
                }
            }

            var recommendations = await _bookRepo.GetAllAsync();
            if (userSubjectIds.Any())
                recommendations = recommendations.Where(b => b.SubjectHeadingId.HasValue && userSubjectIds.Contains(b.SubjectHeadingId.Value)).Take(5);
            else
                recommendations = recommendations.Take(5);

            if (!recommendations.Any()) return "Based on your reading history, we recommend exploring our 'Computer Science' or 'Literature' sections.";

            var response = "✨ **Recommended for you:**\n\n";
            foreach (var book in recommendations)
                response += $"• {book.Title} - {book.ISBN}\n";
            return response;
        }

        private async Task<string> GetFacultyLoansAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);
            if (libraryMember == null) return "Faculty membership not found. Please contact librarian.";

            var loans = await _issuanceRepo.GetAllAsync();
            var activeLoans = loans.Where(l => l.LibraryMemberId == libraryMember.Id && l.IsActive).ToList();

            if (!activeLoans.Any()) return "📖 You have no active faculty loans.";

            var response = $"📚 **Faculty Loans ({activeLoans.Count} active):**\n\n";
            foreach (var loan in activeLoans)
            {
                var copy = await _copyRepo.GetByIdAsync(loan.ItemCopyId);
                var book = copy != null ? await _bookRepo.GetByIdAsync(copy.ItemInfoId) : null;
                var dueDate = loan.IssueDate.AddDays(loan.DurationNumber);
                response += $"• {book?.Title} - Due: {dueDate:dd MMM yyyy}\n";
            }
            return response;
        }

        private async Task<string> GetCourseReservesAsync(AppUser user)
        {
            var party = await GetPartyAsync(user);
            var libraryMember = await GetLibraryMemberAsync(party);
            if (libraryMember == null) return "Faculty membership not found.";

            var reserves = await _reserveRepo.GetAllAsync();
            var courseReserves = reserves.Where(r => r.LibraryMemberId == libraryMember.Id && r.IsActive).ToList();

            if (!courseReserves.Any()) return "📝 You have no active course reserves.\n\nTo add course reserves, contact the circulation desk.";

            var response = "📝 **Your Course Reserves:**\n\n";
            foreach (var reserve in courseReserves)
            {
                var book = await _bookRepo.GetByIdAsync(reserve.ItemInfoId);
                response += $"• {book?.Title}\n";
            }
            return response;
        }

        private async Task<string> GetResearchResourcesAsync()
        {
            var journals = await _context.ItemInfos.CountAsync();
            return $"🔬 **Research Resources Available:**\n\n• Digital Journals: {journals}\n• Research Papers: Access via digital library\n• Databases: JSTOR, IEEE, Springer\n• **Note:** Faculty can request inter-library loans for unavailable resources.";
        }

        private string GetBookRecommendationHelp() => "📚 To recommend a book for purchase:\n1. Go to 'Circulation' → 'Item Requisitions'\n2. Click 'Create New'\n3. Select the book and branch\n4. Enter quantity and estimated cost\n5. Submit for approval";

        private async Task<string> GetDailyReportAsync()
        {
            var today = DateTime.Today;
            var todaysIssues = await _context.Issuances.CountAsync(i => i.IssueDate.Date == today);
            var todaysReturns = await _context.ReturnItems.CountAsync(r => r.ReturnDate.Date == today);
            var newMembers = await _context.LibraryMembers.CountAsync(m => m.CreatedAt.Date == today);
            var collectedFines = await _context.FineTransactions.SumAsync(t => t.Amount);

            return $"📊 **Today's Report ({today:dd MMM yyyy}):**\n\n• Books Issued: {todaysIssues}\n• Books Returned: {todaysReturns}\n• New Members: {newMembers}\n• Fines Collected: Rs. {collectedFines:N2}";
        }

        private async Task<string> GetOverdueReportAsync()
        {
            var today = DateTime.Today;
            var overdueLoans = await _context.Issuances
                .Where(i => i.IsActive && i.IssueDate.AddDays(i.DurationNumber).Date < today)
                .ToListAsync();

            if (!overdueLoans.Any()) return "✅ No overdue books today! All members are on time.";

            var response = $"⚠️ **Overdue Books: {overdueLoans.Count}**\n\n";
            foreach (var loan in overdueLoans.Take(10))
            {
                var member = await _memberRepo.GetByIdAsync(loan.LibraryMemberId);
                var copy = await _copyRepo.GetByIdAsync(loan.ItemCopyId);
                var book = copy != null ? await _bookRepo.GetByIdAsync(copy.ItemInfoId) : null;
                var dueDate = loan.IssueDate.AddDays(loan.DurationNumber);
                var daysOverdue = (today - dueDate.Date).Days;
                response += $"• {book?.Title} - Member: {member?.CardNo} - {daysOverdue} days overdue\n";
            }
            if (overdueLoans.Count > 10) response += $"\n... and {overdueLoans.Count - 10} more";
            return response;
        }

        private async Task<string> GetPendingFinesReportAsync()
        {
            var pendingFines = await _context.UserFines
                .Where(f => f.IsActive && f.FineAccrued > 0)
                .ToListAsync();

            var totalPending = pendingFines.Sum(f => f.FineAccrued);
            var uniqueMembers = pendingFines.Select(f => f.LibraryMemberId).Distinct().Count();

            if (!pendingFines.Any()) return "✅ No pending fines! All members have cleared their dues.";

            return $"💰 **Pending Fines Report:**\n\n• Total Pending: Rs. {totalPending:N2}\n• Members with Fines: {uniqueMembers}\n• Total Fine Records: {pendingFines.Count}\n\nPlease send reminders to members with pending fines.";
        }

        private async Task<string> GetPopularBooksReportAsync()
        {
            var popular = await _context.Issuances
                .Where(i => i.IssueDate >= DateTime.Now.AddMonths(-1))
                .GroupBy(i => i.ItemCopyId)
                .Select(g => new { CopyId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            if (!popular.Any()) return "No borrowing data for this month.";

            var response = "🏆 **Most Popular Books (Last 30 Days):**\n\n";
            foreach (var item in popular)
            {
                var copy = await _context.ItemCopies.FindAsync(item.CopyId);
                if (copy != null)
                {
                    var book = await _context.ItemInfos.FindAsync(copy.ItemInfoId);
                    if (book != null)
                        response += $"• {book.Title} - {item.Count} times\n";
                }
            }
            return response;
        }

        private async Task<string> GetMemberActivityReportAsync()
        {
            var activeMembers = await _context.LibraryMembers.CountAsync(m => m.IsActive && m.ExpiredOn > DateTime.Now);
            var totalMembers = await _context.LibraryMembers.CountAsync();
            var expiringSoon = await _context.LibraryMembers.CountAsync(m => m.ExpiredOn <= DateTime.Now.AddDays(30) && m.ExpiredOn > DateTime.Now);

            return $"👥 **Member Activity Report:**\n\n• Active Members: {activeMembers}\n• Total Registered: {totalMembers}\n• Expiring in 30 days: {expiringSoon}\n• Renewal Rate: {(totalMembers > 0 ? (activeMembers * 100 / totalMembers) : 0)}%";
        }

        private async Task<string> GetSystemStatisticsAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalBooks = await _context.ItemInfos.CountAsync();
            var totalCopies = await _context.ItemCopies.CountAsync();
            var totalMembers = await _context.LibraryMembers.CountAsync();
            var activeLoans = await _context.Issuances.CountAsync(i => i.IsActive);
            var totalFines = await _context.UserFines.SumAsync(f => (decimal?)f.FineAccrued) ?? 0;
            var totalTransactions = await _context.FineTransactions.SumAsync(t => (decimal?)t.Amount) ?? 0;

            return $"📊 **System Statistics:**\n\n• Users: {totalUsers}\n• Books: {totalBooks}\n• Book Copies: {totalCopies}\n• Library Members: {totalMembers}\n• Active Loans: {activeLoans}\n• Total Fines Applied: Rs. {totalFines:N2}\n• Total Fines Collected: Rs. {totalTransactions:N2}";
        }

        private async Task<string> GetUserStatisticsAsync()
        {
            var students = await _userManager.Users.CountAsync(u => u.UserType == "Student");
            var faculty = await _userManager.Users.CountAsync(u => u.UserType == "Faculty");
            var staff = await _userManager.Users.CountAsync(u => u.UserType == "Staff");
            var librarians = await _userManager.Users.CountAsync(u => u.UserType == "Librarian");
            var admins = await _userManager.Users.CountAsync(u => u.UserType == "Admin");
            var visitors = await _userManager.Users.CountAsync(u => u.UserType == "Visitor");

            return $"👥 **User Statistics:**\n\n• Students: {students}\n• Faculty: {faculty}\n• Staff: {staff}\n• Librarians: {librarians}\n• Admins: {admins}\n• Visitors: {visitors}\n\n**Total: {students + faculty + staff + librarians + admins + visitors}**";
        }

        private string GetRegistrationHelp() => "📝 **How to become a member:**\n\n1. Click 'Register' on the homepage\n2. Select 'Student' or 'Faculty'\n3. Fill in your details\n4. Submit registration\n5. Visit library to get your membership card\n\n**Documents Required:**\n• Valid CNIC/B-Form\n• University ID card\n• Recent photograph";

        private async Task<string> GetBookLocationAsync(string query, AppUser user)
        {
            var bookName = query.Replace("where is", "").Replace("location of", "").Replace("کہاں ہے", "").Trim();
            var books = await _bookRepo.SearchAsync("Title", bookName);
            var book = books.FirstOrDefault();

            if (book == null)
                return await GetLocalizedResponse(
                    $"Sorry, I couldn't find '{bookName}' in our catalog.",
                    $"معاف کیجیے، '{bookName}' ہمارے کیٹلاگ میں نہیں ملی۔",
                    $"Maaf kijiye, '{bookName}' hamare catalog mein nahi milli.",
                    user);

            var copy = await _context.ItemCopies.FirstOrDefaultAsync(c => c.ItemInfoId == book.Id);
            var location = copy != null ? $"Almari: {copy.LibraryRack?.Code ?? "Unknown"}, Row: {copy.LibraryShelf?.Code ?? "Unknown"}" : "Location not assigned";

            return await GetLocalizedResponse(
                $"📍 **'{book.Title}' Location:**\n{location}\n\nPlease visit this section to find your book.",
                $"📍 **'{book.Title}' کا مقام:**\n{location}\n\nبراہ کرم یہ سیکشن دیکھیں۔",
                $"📍 **'{book.Title}' ka maqam:**\n{location}\n\nBarah karam yeh section dekhein.",
                user);
        }

        private async Task<string> CalculatePredictedFineAsync(string query, AppUser user)
        {
            var match = System.Text.RegularExpressions.Regex.Match(query, @"(\d+)\s*days?");
            int days = match.Success ? int.Parse(match.Groups[1].Value) : 5;

            var rules = await _ruleRepo.GetAllAsync();
            var rule = rules.FirstOrDefault();
            var finePerDay = rule?.OverdueFinePerDay ?? 5;
            var maxFine = rule?.MaxFinePerItem ?? 500;
            var calculatedFine = Math.Min(days * finePerDay, maxFine);

            return await GetLocalizedResponse(
                $"💰 **Fine Prediction:**\nIf you return {days} days late:\nFine per day: Rs. {finePerDay}\nTotal Fine: Rs. {calculatedFine}\nMaximum fine: Rs. {maxFine}\n\n💡 Tip: Return on time to avoid fines!",
                $"💰 **جرمانے کی پیش گوئی:**\nاگر آپ {days} دن دیر سے واپس کریں گے:\nفی دن جرمانہ: روپے {finePerDay}\nکل جرمانہ: روپے {calculatedFine}\nزیادہ سے زیادہ جرمانہ: روپے {maxFine}\n\n💡 مشورہ: جرمانے سے بچنے کے لیے وقت پر واپس کریں!",
                $"💰 **Jurmany ki pesh goi:**\nAgar aap {days} din der se wapas karein ge:\nFi din jurmana: Rupees {finePerDay}\nTotal jurmana: Rupees {calculatedFine}\nZyada se zyada jurmana: Rupees {maxFine}\n\n💡 Mashwara: Jurmanay se bachnay ke liye waqt par wapas karein!",
                user);
        }

        private async Task<Party> GetPartyAsync(AppUser user)
        {
            return await _context.Parties.FirstOrDefaultAsync(p => p.PartyName == user.Email);
        }

        private async Task<LibraryMember> GetLibraryMemberAsync(Party party)
        {
            if (party == null) return null;
            return await _context.LibraryMembers.FirstOrDefaultAsync(m => m.PartyId == party.Id);
        }



        // Add this method:
        //private (bool isAction, string response) DetectIntentFromCache(string query, string role)
        //{
        //    var intent = _intentCache.GetMatchingIntent(query, role);

        //    if (intent != null)
        //    {
        //        return (true, $"[[REDIRECT]]/{intent.Controller}/{intent.Action}");
        //    }

        //    return (false, null);
        //}
    }
}