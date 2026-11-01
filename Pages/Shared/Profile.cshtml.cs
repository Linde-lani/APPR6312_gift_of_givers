using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;

namespace Gifters.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(ILogger<ProfileModel> logger)
        {
            _logger = logger;
        }

        // Global variable to carry the response message
        public string Message = string.Empty;

        // Properties bound to the Razor Page
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
       
        public DateTime JoinDate { get; set; }


        // GET handler
        public void OnGet(int userId)
        {
            // Example: call your Azure Function to fetch user profile
            using HttpClient send_request = new HttpClient();

            // Replace with your Function1 endpoint
            string url_endPoint = $"http://localhost:7298/api/Function1?userId={userId}";

            try
            {
                // Call the Azure Function and get result
                Message = send_request.GetStringAsync(url_endPoint).Result;

                // TODO: parse JSON response into properties
                // For example, if Function1 returns JSON:
                // { "FullName":"Energy", "Email":"energy@example.com", "Role":"Donor", "JoinDate":"2026-09-20" }
                // You would deserialize it here into the properties.
            }
            catch (Exception ex)
            {
                Message = $"Error loading profile: {ex.Message}";
            }
        }

        // POST handler (optional, e.g. update profile)
        public void OnPost()
        {
            string fullName = Request.Form["FullName"].ToString();
            string email = Request.Form["Email"].ToString();
            string role = Request.Form["Role"].ToString();

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                using HttpClient send_request = new HttpClient();

                string url_endPoint = $"http://localhost:7260/api/Function1?FullName={fullName}&Email={email}&Role={role}";

                Message = send_request.GetStringAsync(url_endPoint).Result;
            }
            else
            {
                Message = "Please enter your full name.";
            }
        }
    }
}
