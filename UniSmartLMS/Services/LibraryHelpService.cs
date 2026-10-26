using System.Text.RegularExpressions;

namespace UniSmartLMS.Services
{
    public interface ILibraryHelpService
    {
        Task<string> GetHelpAsync(string query);
        bool IsLibraryQuestion(string query);
    }

    public class LibraryHelpService : ILibraryHelpService
    {
        private readonly List<HelpIntent> _intents;

        public LibraryHelpService()
        {
            _intents = new List<HelpIntent>
            {
                // Search Books
                new HelpIntent
                {
                    Keywords = new[] { "search book", "find book", "look for book", "how to search", "find a book", "search for book" },
                    Title = "🔍 How to Search for Books",
                    Steps = new[] {
                        "Go to **Catalog → Books** in the navigation menu",
                        "Type the book title, author name, or ISBN in the search box",
                        "Press Enter or click the search icon",
                        "Click on any book to see details and availability"
                    },
                    Tips = new[] { "You can also ask me: 'Show me books about Python'", "Use filters by subject or language for better results" },
                    QuickLink = "/ItemInfo/Index"
                },
                
                // Borrow Books
                new HelpIntent
                {
                    Keywords = new[] { "borrow book", "issue book", "how to borrow", "take book", "loan book", "checkout book" },
                    Title = "📖 How to Borrow a Book",
                    Steps = new[] {
                        "First, find the book using the search feature",
                        "Go to **Circulation → Issuances**",
                        "Click **Create New** button",
                        "Select the member and book copy",
                        "Choose the loan duration",
                        "Click **Issue Book** to complete"
                    },
                    Tips = new[] { "Students can borrow 5 books for 14 days", "Faculty can borrow 15 books for 60 days", "Books can be renewed twice online" },
                    QuickLink = "/Issuance/Create"
                },
                
                // Return Books
                new HelpIntent
                {
                    Keywords = new[] { "return book", "how to return", "give back book", "submit book" },
                    Title = "🔄 How to Return a Book",
                    Steps = new[] {
                        "Go to **Circulation → Returns**",
                        "Click **Process Return**",
                        "Select the issuance ID (loan record)",
                        "Click **Process Return** to complete"
                    },
                    Tips = new[] { "Late returns incur a fine of Rs. 5 per day", "Maximum fine per book is Rs. 500", "Return on time to avoid fines!" },
                    QuickLink = "/ReturnItem/Create"
                },
                
                // Library Timings
                new HelpIntent
                {
                    Keywords = new[] { "library timing", "library hours", "open close", "timings", "open today", "closed" },
                    Title = "🕐 Library Timings",
                    TableData = new Dictionary<string, string>
                    {
                        { "Monday - Thursday", "8:00 AM - 8:00 PM" },
                        { "Friday", "2:00 PM - 8:00 PM" },
                        { "Saturday", "9:00 AM - 5:00 PM" },
                        { "Sunday", "Closed" }
                    },
                    ExtraInfo = "📞 For inquiries: +92 42 1234 5678 | 📧 library@unismartlms.edu.pk"
                },
                
                // Library Rules
                new HelpIntent
                {
                    Keywords = new[] { "library rules", "rules", "policy", "borrow limit", "max books", "loan limit", "how many books" },
                    Title = "📋 Library Rules & Policies",
                    Rules = new Dictionary<string, string>
                    {
                        { "👨‍🎓 Students", "5 books for 14 days" },
                        { "👨‍🏫 Faculty", "15 books for 60 days" },
                        { "👔 Staff", "8 books for 21 days" },
                        { "💰 Overdue Fine", "Rs. 5 per day (max Rs. 500)" },
                        { "🔄 Renewals", "Maximum 2 renewals per book" },
                        { "📚 Reservations", "Hold books for 7 days" }
                    }
                },
                
                // My Loans / My Fines
                new HelpIntent
                {
                    Keywords = new[] { "my loans", "my books", "what i borrowed", "current loans", "my fines", "my penalty", "how much fine" },
                    Title = "📊 Your Account Summary",
                    Steps = new[] {
                        "Go to **Dashboard** from the navigation menu",
                        "View your current loans in the 'My Loans' section",
                        "Check pending fines in the 'My Fines' section",
                        "Click 'Pay Fine' to clear any outstanding amount"
                    },
                    Tips = new[] { "Check due dates to avoid fines", "Renew books before they expire", "Pay fines online using JazzCash/EasyPaisa" },
                    QuickLink = "/Dashboard"
                },
                
                // Add Member
                new HelpIntent
                {
                    Keywords = new[] { "add member", "register member", "new member", "create member", "library card" },
                    Title = "👤 How to Register a New Library Member",
                    Steps = new[] {
                        "Go to **People → Library Members**",
                        "Click **Create New**",
                        "Select an existing Party or create a new one",
                        "Enter Card Number and expiry date",
                        "Select Membership Type (Student/Faculty/Staff)",
                        "Click **Create** to complete"
                    },
                    Tips = new[] { "Membership cards are valid for 1 year", "Students need valid university ID", "Faculty needs approval from HOD" },
                    QuickLink = "/LibraryMember/Create",
                    RequiredRole = "Admin,Librarian"
                },
                
                // Add Book
                new HelpIntent
                {
                    Keywords = new[] { "add book", "create book", "new book", "add to catalog", "catalog book" },
                    Title = "📚 How to Add a New Book",
                    Steps = new[] {
                        "Go to **Catalog → Books**",
                        "Click **Create New**",
                        "Fill in book details (Title, ISBN, Author, Publisher)",
                        "Select Subject and Language",
                        "Click **Create** to save"
                    },
                    Tips = new[] { "Add book copies after creating the book record", "ISBN is optional but recommended", "Add cover image for better visibility" },
                    QuickLink = "/ItemInfo/Create",
                    RequiredRole = "Admin,Librarian"
                },
                
                // Help / Support
                new HelpIntent
                {
                    Keywords = new[] { "help", "support", "what can you do", "how to use", "guide", "tutorial" },
                    Title = "🤖 AI Assistant Help",
                    Steps = new[] {
                        "📖 **Ask about documents** - 'What is Python?' or 'Explain AI'",
                        "🔍 **Search books** - 'Show me books about programming'",
                        "📊 **Check account** - 'My loans' or 'My fines'",
                        "📚 **Library info** - 'Library timings' or 'Library rules'",
                        "➕ **Manage data** - 'Add book' or 'Register member' (Librarian/Admin)"
                    },
                    Tips = new[] { "Upload PDF, DOCX, or PPTX files to enable document Q&A", "Type 'help' anytime to see this menu", "Questions are saved in your chat history" }
                }
            };
        }

        public bool IsLibraryQuestion(string query)
        {
            var lowerQuery = query.ToLower();
            return _intents.Any(intent => intent.Keywords.Any(kw => lowerQuery.Contains(kw)));
        }

        public async Task<string> GetHelpAsync(string query)
        {
            await Task.Delay(1);
            var lowerQuery = query.ToLower();

            var matchedIntent = _intents.FirstOrDefault(intent =>
                intent.Keywords.Any(kw => lowerQuery.Contains(kw)));

            if (matchedIntent == null)
                return null;

            return GenerateHelpCard(matchedIntent);
        }

        private string GenerateHelpCard(HelpIntent intent)
        {
            var html = $@"
<div class='help-card'>
    <div class='help-header'>
        <i class='fas fa-{(intent.Title.Contains("Search") ? "search" :
                         intent.Title.Contains("Borrow") ? "hand-holding-heart" :
                         intent.Title.Contains("Return") ? "undo-alt" :
                         intent.Title.Contains("Timing") ? "clock" :
                         intent.Title.Contains("Rules") ? "gavel" :
                         intent.Title.Contains("Account") ? "chart-line" :
                         intent.Title.Contains("Register") ? "user-plus" :
                         intent.Title.Contains("Add") ? "plus-circle" : "question-circle")}'></i>
        <h3>{intent.Title}</h3>
    </div>
    <div class='help-content'>";

            // Steps
            if (intent.Steps != null && intent.Steps.Any())
            {
                html += "<div class='help-steps'>";
                for (int i = 0; i < intent.Steps.Length; i++)
                {
                    html += $@"
                <div class='help-step'>
                    <span class='step-number'>{i + 1}</span>
                    <div class='step-text'>{intent.Steps[i]}</div>
                </div>";
                }
                html += "</div>";
            }

            // Table Data (for timings)
            if (intent.TableData != null && intent.TableData.Any())
            {
                html += "<table class='timing-table'>";
                foreach (var item in intent.TableData)
                {
                    html += $@"
                <tr>
                    <td><strong>{item.Key}</strong></td>
                    <td>{item.Value}</td>
                </tr>";
                }
                html += "</table>";
            }

            // Rules
            if (intent.Rules != null && intent.Rules.Any())
            {
                html += "<div class='rules-list'>";
                foreach (var rule in intent.Rules)
                {
                    html += $@"
                <div class='rule-item'>
                    <i class='fas fa-check-circle'></i>
                    <span><strong>{rule.Key}:</strong> {rule.Value}</span>
                </div>";
                }
                html += "</div>";
            }

            // Tips
            if (intent.Tips != null && intent.Tips.Any())
            {
                html += "<div class='help-tips'>";
                html += "<div class='tips-header'><i class='fas fa-lightbulb'></i> Tips</div>";
                foreach (var tip in intent.Tips)
                {
                    html += $"<div class='tip-item'>💡 {tip}</div>";
                }
                html += "</div>";
            }

            // Extra Info
            if (!string.IsNullOrEmpty(intent.ExtraInfo))
            {
                html += $@"
            <div class='help-extra'>
                <i class='fas fa-info-circle'></i> {intent.ExtraInfo}
            </div>";
            }

            // Quick Link Button
            if (!string.IsNullOrEmpty(intent.QuickLink))
            {
                html += $@"
            <div class='help-action'>
                <a href='{intent.QuickLink}' class='help-btn'>
                    <i class='fas fa-arrow-right'></i> Go to {intent.Title.Split(' ')[0]} Page
                </a>
            </div>";
            }

            html += @"
    </div>
</div>";
            return html;
        }

        private class HelpIntent
        {
            public string[] Keywords { get; set; }
            public string Title { get; set; }
            public string[] Steps { get; set; }
            public string[] Tips { get; set; }
            public Dictionary<string, string> TableData { get; set; }
            public Dictionary<string, string> Rules { get; set; }
            public string ExtraInfo { get; set; }
            public string QuickLink { get; set; }
            public string RequiredRole { get; set; }
        }
    }
}