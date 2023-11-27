using Bidder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constant;
using Utilities.ViewModel;

namespace Bidder.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = PermissionNameConst.Bidder)]
    public class MyProductController : Controller
    {
        #region Global Variables
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IVnPayService _vnPayService;
        #endregion

        #region Constructor
        public MyProductController(IConfiguration configuration, IVnPayService vnPayService)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiGetwayBaseAddress"]);
            _vnPayService = vnPayService;
        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IList<DetailProductBidderViewModel> products = new List<DetailProductBidderViewModel>();
            HttpResponseMessage response = new HttpResponseMessage();
            string responseData = string.Empty;

            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? id = null;
            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                id = userId;
            }

            // Get Data from API
            response = await _httpClient.GetAsync($"Product/GetListWinnerProductByBidder/{id}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                products = JsonConvert.DeserializeObject<IList<DetailProductBidderViewModel>>(responseData);
            }

            return View(products);
        }
        #endregion

        #region Detail
        [HttpGet("{productId}")]
        public async Task<IActionResult> Detail(Guid productId)
        {
            #region get bidder id from token
            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? bidderId = null;
            #endregion

            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                bidderId = userId;
                TempData[TempDataNameConst.BidderIdMessage] = bidderId;
            }

            // Get Data from API
            var response = await _httpClient.GetAsync($"Product/GetDetailWinnerProductByBidder/{bidderId}/{productId}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                TempData[TempDataNameConst.ErrorMessage] = "An error occurred, please try again";
                return RedirectToAction("Index", "MyProduct");
            }

            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            DetailProductBidderViewModel model = JsonConvert.DeserializeObject<DetailProductBidderViewModel>(responseData);

            return View(model);
        }
        #endregion

        #region Checkout
        [HttpGet("{productId}")]
        public async Task<IActionResult> Checkout(Guid productId)
        {
            #region get bidder id from token
            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? bidderId = null;
            #endregion

            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                bidderId = userId;
                TempData[TempDataNameConst.BidderIdMessage] = bidderId;
            }

            // Get Data from API
            var response = await _httpClient.GetAsync($"Product/GetDetailWinnerProductByBidder/{bidderId}/{productId}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                TempData[TempDataNameConst.ErrorMessage] = "An error occurred, please try again";
                return RedirectToAction("Index", "MyProduct");
            }

            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            DetailProductBidderViewModel model = JsonConvert.DeserializeObject<DetailProductBidderViewModel>(responseData);
            TempData[TempDataNameConst.ProductPrice] = model.PriceCurrentMax;
            TempData[TempDataNameConst.ProductName] = model.ProductName;
            TempData[TempDataNameConst.ProductId] = productId;
            return View();
        }
        #endregion

        #region Payment
        [HttpPost]
        public async Task<IActionResult> Payment(PaymentInformationModel payment)
        {
            var url = _vnPayService.CreatePaymentUrl(payment, HttpContext);
            return Redirect(url);
        }
        #endregion

        #region PaymentCallback
        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            string[] data = response.OrderInfo.Split(new string[] { "|*|" }, StringSplitOptions.None);

            #region get bidder id from token
            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? bidderId = null;
            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                bidderId = userId;
            }
            #endregion

            PaymentViewModel payment = new PaymentViewModel
            {
                FirstName = data[0],
                LastName = data[1],
                Email = data[2],
                ShipingAddress = data[3],
                ZIPCode = data[4],
                OrderNotes = data[5],
                Telephone = data[6],
                TotalPrice = Convert.ToDouble(data[7]),
                IdProduct = Guid.Parse(data[8]),
                OrderType = "VNPay",
                IdUser = bidderId
            };

            var json = JsonConvert.SerializeObject(payment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync("Payment/Create", content);

            if (httpResponse.IsSuccessStatusCode)
            {
                TempData[TempDataNameConst.SuccessMessage] = "Payment success";
            }
            else
            {
                TempData[TempDataNameConst.ErrorMessage] = "An error occurred, please try again";
            }

            return RedirectToAction("Index", "MyProduct");
        }
        #endregion
    }
}

