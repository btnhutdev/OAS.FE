using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using Utilities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Utilities.Constant;
using System.Security.Claims;
using Hangfire;
using System.Linq;
using StackExchange.Redis;

namespace Bidder.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = PermissionNameConst.Bidder)]
    public class ProductController : Controller
    {
        #region Global Variables
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConnectionMultiplexer _redisConnection;

        #endregion

        #region Constructor
        public ProductController(IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _redisConnection = connectionMultiplexer;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiGatewayBaseAddress"]);
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            IList<CategoryViewModel> categories = new List<CategoryViewModel>();
            IList<string> productNames = new List<string>();
            HttpResponseMessage productResponse = new HttpResponseMessage();
            HttpResponseMessage categoryResponse = new HttpResponseMessage();
            HttpResponseMessage productNameResponse = new HttpResponseMessage();
            string productResponseData = string.Empty;
            string categoriesResponseData = string.Empty;
            string productNamesResponseData = string.Empty;

            // Get Data from API
            productResponse = await _httpClient.GetAsync("Product/GetListProductAuctioning").ConfigureAwait(false);
            categoryResponse = await _httpClient.GetAsync("Search/GetListCategory").ConfigureAwait(false);
            productNameResponse = await _httpClient.GetAsync("Search/SearchNameProduct").ConfigureAwait(false);

            if (productResponse.IsSuccessStatusCode)
            {
                productResponseData = await productResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                products = JsonConvert.DeserializeObject<IList<ProductViewModel>>(productResponseData);
            }

            if (categoryResponse.IsSuccessStatusCode)
            {
                categoriesResponseData = await categoryResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                categories = JsonConvert.DeserializeObject<IList<CategoryViewModel>>(categoriesResponseData);
                categories.Insert(0, new CategoryViewModel { IdCategory = Guid.NewGuid(), CategoryName = "All" });
                TempData[TempDataNameConst.Categories] = categories;
            }

            if (productNameResponse.IsSuccessStatusCode)
            {
                productNamesResponseData = await productNameResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                productNames = JsonConvert.DeserializeObject<IList<string>>(productNamesResponseData);
                TempData[TempDataNameConst.ProductName] = productNames;
            }

            return View(products);
        }

        #endregion

        #region Detail
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            // Get Data from API
            var response = await _httpClient.GetAsync($"Product/GetDetailProductForBidder/{id}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                TempData[TempDataNameConst.ErrorMessage] = "The auction for this product has ended";
                return RedirectToAction("Index", "Product");
            }

            #region get bidder id from token

            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? bidderId = null;

            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                bidderId = userId;
                TempData[TempDataNameConst.BidderIdMessage] = bidderId;
            }

            #endregion

            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            DetailProductBidderViewModel model = JsonConvert.DeserializeObject<DetailProductBidderViewModel>(responseData);

            return View(model);
        }
        #endregion

        #region GetProductAuctioningByCategory
        [HttpGet("{idCategory}")]
        public async Task<IActionResult> GetProductAuctioningByCategory(string idCategory)
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            string productResponseData = string.Empty;

            // Get Data from API
            Guid id = new Guid(idCategory);
            var response = await _httpClient.GetAsync($"Search/GetListProductAuctioningByCategory/{id}").ConfigureAwait(false);

            productResponseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            products = JsonConvert.DeserializeObject<IList<ProductViewModel>>(productResponseData);

            return PartialView("_ProductAuctioningView", products);
        }
        #endregion

        #region IsProductAuctioning
        [HttpGet("{auctionId}")]
        public async Task<IActionResult> IsProductAuctioning(Guid auctionId)
        {
            var response = await _httpClient.GetAsync($"Product/IsProductAuctioning/{auctionId}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return new OkResult();
            }

            return new BadRequestResult();
        }
        #endregion

        #region CheckAutoBidder
        [HttpGet("{auctionId}")]
        public async Task<IActionResult> CheckAutoBidder(Guid auctionId)
        {
            await CreateTaskUpdateBidder(auctionId);
            return new OkResult();
        }
        #endregion

        #region PushRecordToRedis
        public async Task PushRecordToRedis(Message message)
        {
            //var hubUrl = @"http://localhost:40754/AuctionHub"; old config
            var hubUrl = @"http://localhost:5030/AuctionHub";
            var redisConnectionString = @"127.0.0.1:6379";

            SignalRAndRedisClient signalRAndRedisClient = new SignalRAndRedisClient(hubUrl, redisConnectionString);

            await signalRAndRedisClient.SendMessageAsync(message.AuctionId, message.ProductId, message.BidderId,
                message.CurrentPrice, message.MyPrice, message.AuctionType, message.MyMaxPriceAuto,
                message.StepPrice);
        }
        #endregion

        #region HandleAutoBidder
        public async Task HandleAutoBidder(Guid auctionId)
        {
            var allRecords = GetHistoryMessages(auctionId);
            var newestRecord = allRecords.LastOrDefault();
            List<Message> topRecordUser = new List<Message>();
            var filteredByAuctionType = allRecords.Where(msg => msg.AuctionType == 1).ToList();
            var groupedByBidder = filteredByAuctionType.GroupBy(msg => msg.BidderId);

            foreach (var group in groupedByBidder)
            {
                var bidderId = group.Key;
                var recordsForBidder = group.ToList();

                var maxMyPrice = recordsForBidder.Max(msg => msg.MyPrice);
                var recordWithMaxPrice = recordsForBidder.FirstOrDefault(msg => msg.MyPrice == maxMyPrice);
                topRecordUser.Add(recordWithMaxPrice);
            }

            foreach (var item in topRecordUser)
            {
                float currPrice = newestRecord.CurrentPrice;
                float myPrice = item.MyPrice;
                float stepPrice = item.StepPrice;
                float maxPrice = item.MyMaxPriceAuto;

                if (myPrice < currPrice)
                {
                    if (currPrice + stepPrice <= maxPrice)
                    {
                        item.MyPrice = currPrice + stepPrice;
                        item.CurrentPrice = currPrice + stepPrice;
                        await PushRecordToRedis(item).ConfigureAwait(false);
                    }
                }
            }
        }
        #endregion

        #region CreateTaskUpdateBidder
        public async Task CreateTaskUpdateBidder(Guid auctionId)
        {
            BackgroundJob.Enqueue(() => HandleAutoBidder(auctionId));
        }
        #endregion

        #region GetHistoryMessages
        public List<Message> GetHistoryMessages(Guid auctionId)
        {
            var redisDb = _redisConnection.GetDatabase();

            var groupKey = $"{KeyRedisNameConst.GroupMessageHistory}:{auctionId}";
            var messages = redisDb.ListRange(groupKey, start: 0, stop: -1);

            // Đảo ngược thứ tự các phần tử để lấy ra price cao nhất
            messages = messages.Reverse().ToArray();

            return messages.Select(m => JsonConvert.DeserializeObject<Message>(m)).ToList();
        }
        #endregion

        #region Search - GetListNameProduct
        [HttpGet("{categoryId}/{productName}")]
        public async Task<JsonResult> GetListNameProduct(Guid categoryId, string productName)
        {
            IList<string> products = new List<string>();
            HttpResponseMessage productResponse = new HttpResponseMessage();
            string productResponseData = string.Empty;

            // Get Data from API
            productResponse = await _httpClient.GetAsync($"Search/SearchNameProduct/{categoryId}/{productName}").ConfigureAwait(false);

            if (productResponse.IsSuccessStatusCode)
            {
                productResponseData = await productResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return new JsonResult(productResponseData);
        }
        #endregion

        #region SearchProductByProductName
        [HttpGet("{productName}")]
        public async Task<IActionResult> SearchProductByProductName(string productName)
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            HttpResponseMessage productResponse = new HttpResponseMessage();
            string productResponseData = string.Empty;

            // Get Data from API
            productResponse = await _httpClient.GetAsync($"Search/SearchProductByProductName/{productName}").ConfigureAwait(false);

            if (productResponse.IsSuccessStatusCode)
            {
                productResponseData = await productResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                products = JsonConvert.DeserializeObject<IList<ProductViewModel>>(productResponseData);
            }

            return PartialView("_ProductAuctioningView", products);
        }

        #endregion

        #region SearchByProductNameAndCategory
        [HttpGet("{categoryId}/{selectedOptions}")]
        public async Task<IActionResult> SearchByProductNameAndCategory(Guid categoryId, string selectedOptions)
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            HttpResponseMessage productResponse = new HttpResponseMessage();
            string productResponseData = string.Empty;

            // Get Data from API
            productResponse = await _httpClient.GetAsync($"Search/SearchByProductNameAndCategory/{categoryId}/{selectedOptions}").ConfigureAwait(false);

            if (productResponse.IsSuccessStatusCode)
            {
                productResponseData = await productResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                products = JsonConvert.DeserializeObject<IList<ProductViewModel>>(productResponseData);
            }

            return PartialView("_ProductAuctioningView", products);
        }
        #endregion

        #region SendMailNotHighestPrice
        [HttpGet("{productId}/{currentPrice}/{yourPrice}")]
        public async Task<IActionResult> SendMailNotHighestPrice(Guid productId, float currentPrice, float yourPrice)
        {
            HttpResponseMessage productResponse = new HttpResponseMessage();

            string claimEmail = User.FindFirstValue(ClaimTypes.Email);
            string email = string.Empty;
            if (!string.IsNullOrEmpty(claimEmail))
            {
                email = claimEmail;
            }

            // Get Data from API
            productResponse = await _httpClient.GetAsync($"Product/SendMailNotHighestPrice/{email}/{productId}/{currentPrice}/{yourPrice}").ConfigureAwait(false);

            if (productResponse.IsSuccessStatusCode)
            {
                return new OkResult();
            }

            return new BadRequestResult();
        }
        #endregion

    }
}
