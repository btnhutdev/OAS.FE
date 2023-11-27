using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Utilities.Constant;

namespace Admin.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = PermissionNameConst.Admin)]
    public class ProductController : Controller
    {
        #region global variables
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiGetwayBaseAddress"]);
        }
        #endregion

        #region Index
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            HttpResponseMessage response = new HttpResponseMessage();
            // Get Data from API
            response = await _httpClient.GetAsync($"Product/GetListProcessingProductByAdmin").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                products = JsonConvert.DeserializeObject<IList<ProductViewModel>>(responseData);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Empty List");
            }

            return View(products);
        }
        #endregion

        #region Approve
        [HttpGet]
        [Route("Approve/{id}")]
        public async Task<IActionResult> Approve(Guid id)
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            HttpResponseMessage response = new HttpResponseMessage();

            // Get Data from API
            response = await _httpClient.GetAsync($"Product/Approve/{id}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Approve Success";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred, please try again";
                return RedirectToAction("Index", "Product");
            }
        }
        #endregion

        #region Reject
        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] RejectViewModel rejectModel)
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            HttpResponseMessage response = new HttpResponseMessage();

            Guid? id = rejectModel.ProductId;
            string message = rejectModel.Message;

            // Get Data from API
            response = await _httpClient.GetAsync($"Product/Reject/{id}/{message}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Reject Success";
                return new OkResult();
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred, please try again";
                return new BadRequestResult();
            }
        }
        #endregion
    }
}
