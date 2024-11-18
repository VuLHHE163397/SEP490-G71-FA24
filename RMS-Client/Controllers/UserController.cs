using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RMS_Client.Models; // Ensure that UserDTO is defined here or adjust the namespace
using RMS_API.DTOs;

namespace RMS_Client.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _client;
        private readonly string UserApiUri = "https://localhost:7056/api/User/GetAllLanlord";  // URI for getting users
        private readonly string UpdateStatusApiUri = "https://localhost:7056/api/User/UpdateStatus";  // URI for updating status

        public UserController()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IActionResult GetUser()
        {
            return View("~/Views/Profile/ViewProfile.cshtml");
            // Method to fetch and display the list of users

            
        }

        public async Task<IActionResult> ListUser()
        {
            var user = new List<UserDTO>();
            var res = await _client.GetAsync(UserApiUri);

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<List<UserDTO>>(json);
            }
            else
            {
                // Handle error while fetching the users
                ViewData["Error"] = "Failed to fetch user data.";
                return View(user);  // Return empty list if the API call fails
            }

            return View(user);
        }

        // Method to update the status of a user
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int newStatusId)
        {
            var request = new
            {
                Id = id,
                NewStatusId = newStatusId
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var updateRes = await _client.PostAsync(UpdateStatusApiUri, content);

            if (updateRes.IsSuccessStatusCode)
            {
                // Successfully updated status
                ViewData["SuccessMessage"] = "User status updated successfully.";
            }
            else
            {
                // Error updating status
                ViewData["Error"] = "Failed to update user status.";
            }

            // Redirect to the ListUser action to refresh the list of users
            return RedirectToAction("ListUser");
        }
    }
}
