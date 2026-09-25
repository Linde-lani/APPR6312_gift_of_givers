using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace gifters.Pages
{
    // PageModel for Pages/Profile/Donations.cshtml
    // NOTE: namespace changed to 'gifters.Pages' (lowercase) to match the rest of the project.
    // This resolves Razor compilation errors where the view could not find the PageModel type.
    public class DonationsModel : PageModel
    {
        private readonly ILogger<DonationsModel> _logger;

        // In-memory store for demo purposes. Replace with DB/service when ready.
        private static readonly List<DonationItem> s_donationsStore = new();

        public DonationsModel(ILogger<DonationsModel> logger)
        {
            _logger = logger;
        }

        // Optional status message shown after a successful post (persisted across redirect)
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        // List rendered by the Razor page
        public List<DonationItem> Donations { get; private set; } = new();

        // Bound property for the add-donation form (asp-for="NewDonation.*")
        [BindProperty]
        public DonationItem NewDonation { get; set; } = new DonationItem();

        // Simple view model used by this page. Kept local to avoid touching shared Models.
        public class DonationItem
        {
            public int DonationId { get; set; }

            // If you collect UserId from the form or from the logged-in user, set it here.
            public int UserId { get; set; }

            [Required(ErrorMessage = "Amount is required.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
            public decimal Amount { get; set; }

            [Required(ErrorMessage = "Purpose is required.")]
            public string Purpose { get; set; } = string.Empty;

            public DateTime Date { get; set; } = DateTime.UtcNow;
        }

        // GET: populate the Donations list from the in-memory store
        public void OnGet()
        {
            Donations = new List<DonationItem>(s_donationsStore);
        }

        // POST handler that matches the form's asp-page-handler="Add"
        // Validates, adds to the in-memory store and redirects to GET to avoid double-post.
        public IActionResult OnPostAdd()
        {
            // If validation fails, re-render the page with validation messages.
            if (!ModelState.IsValid)
            {
                Donations = new List<DonationItem>(s_donationsStore);
                return Page();
            }

            // Assign id and timestamp, store the donation (replace with real persistence)
            NewDonation.DonationId = s_donationsStore.Count + 1;
            NewDonation.Date = DateTime.UtcNow;

            // Insert at top so newest appears first
            s_donationsStore.Insert(0, NewDonation);

            StatusMessage = "Donation added successfully.";

            // Redirect to GET so the form is cleared and browser refresh won't repost the form
            return RedirectToPage();
        }
    }
}