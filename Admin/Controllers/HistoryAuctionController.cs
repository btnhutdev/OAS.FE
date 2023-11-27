using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities.ViewModel;

namespace Admin.Controllers
{
    public class HistoryAuctionController : Controller
    {
        #region global variables
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public HistoryAuctionController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiGetwayBaseAddress"]);
        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get Data from API
            var response = await _httpClient.GetAsync($"Product/GetListHistoryAuctionByAdmin").ConfigureAwait(false);
            IList<ProductViewModel> model = new List<ProductViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                model = JsonConvert.DeserializeObject<IList<ProductViewModel>>(responseData);
                return View(model);
            }
            else
            {
                return View(model);
            }
        }
        #endregion

        #region Detail
        [HttpGet]
        [Route("Detail/{id}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            // Get Data from API
            var response = await _httpClient.GetAsync($"Product/GetDetaiHistoryAuctionByAdmin/{id}").ConfigureAwait(false);
            IList<HistoryAuctionViewModel> model = new List<HistoryAuctionViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var tempData = JsonConvert.DeserializeObject<IList<HistoryAuctionViewModel>>(responseData);
                model = tempData.Reverse().ToList();
                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred, please try again";
                return View(model);
            }
        }
        #endregion
    }
}
