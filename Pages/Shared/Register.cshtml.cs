using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gifters.Pages.Shared
{
    public class RegisterModel : PageModel
    {

        // global variable to carry the response message
        public string Message = string.Empty;

        // OnPost request method to get user's registration details and store in the database table
        public void OnPost()
        {
            // collect values from the form
            string fullName = Request.Form["FullName"].ToString();
            string email = Request.Form["Email"].ToString();
            string password = Request.Form["Password"].ToString();
            string confirmPassword = Request.Form["ConfirmPassword"].ToString();
            string role = Request.Form["Role"].ToString();

            // basic validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                Message = "Please fill in all required fields.";
                return;
            }

            if (password != confirmPassword)
            {
                Message = "Passwords do not match.";
                return;
            }

            // Example: send data to backend API or save to database
            using HttpClient send_request = new HttpClient();

            // Replace with your actual API endpoint or EF Core logic
            string url_endPoint = $"http://localhost:7260/api/Function1?name={fullName}&email={email}&password={password}&role={role}";

            var response = send_request.GetAsync(url_endPoint).Result;

            if (response.IsSuccessStatusCode)
            {
                Message = "Registration successful!";
            }
            else
            {
                Message = "Registration failed. Please try again.";
            }
        }
        public void OnGet()
        {
        }
    }
}
