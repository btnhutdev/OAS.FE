using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using Utilities.Constant;
using Utilities.ViewModel;

namespace Auctioneer.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = PermissionNameConst.Auctioneer)]
    public class ProductController : Controller
    {
        #region global variables
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;
        #endregion

        #region Constructor
        public ProductController(ILogger<ProductController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiGetwayBaseAddress"]);
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IList<ProductViewModel> products = new List<ProductViewModel>();
            HttpResponseMessage response = new HttpResponseMessage();

            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? id = null;
            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                id = userId;
            }

            // Get Data from API
            response = await _httpClient.GetAsync($"Product/GetListProductAllStatusByAuctioneerID/{id}").ConfigureAwait(false);

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

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        #endregion

        #region CreateProduct
        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductViewModel product)
        {
            var content = new MultipartFormDataContent();

            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? id = null;
            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                id = userId;
            }

            // Set default value
            product.UserId = id;
            product.IsApprove = false;
            product.IsSold = false;
            product.IsPayment = false;
            product.IsReject = false;
            product.CategoryName = "CategoryName";
            product.CategoryId = new Guid("33CEDB5B-DF86-4ED0-9BBE-DF0425D35C13");

            // Add properties as StringContent to content
            content.Add(new StringContent(product.IdProduct.ToString()), "IdProduct");
            content.Add(new StringContent(product.ProductName), "ProductName");
            content.Add(new StringContent(product.InitPrice.ToString()), "InitPrice");
            content.Add(new StringContent(product.StepPrice.ToString()), "StepPrice");
            content.Add(new StringContent(product.Description), "Description");
            content.Add(new StringContent(product.StartDate.ToString("yyyy-MM-ddTHH:mm:ss")), "StartDate");
            content.Add(new StringContent(product.EndDate?.ToString("yyyy-MM-ddTHH:mm:ss")), "EndDate");
            content.Add(new StringContent(product.IsApprove.ToString()), "IsApprove");
            content.Add(new StringContent(product.IsSold.ToString()), "IsSold");
            content.Add(new StringContent(product.IsPayment.ToString()), "IsPayment");
            content.Add(new StringContent(product.IsReject.ToString()), "IsReject");
            content.Add(new StringContent(product.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(product.UserId.ToString()), "UserId");
            content.Add(new StringContent(product.CategoryName.ToString()), "CategoryName");

            // Add IFormFile to content
            foreach (var image in product.ImageFiles)
            {
                var fileContent = new StreamContent(image.OpenReadStream())
                {
                    Headers =
                    {
                        ContentLength = image.Length,
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType)
                    }
                };
                content.Add(fileContent, "ImageFiles", image.FileName);
            }

            var response = await _httpClient.PostAsync("Product/Create", content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Register Success";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred, please try again";
            }

            var redirectUrl = Url.Action("Index", "Product");
            return Json(new { RedirectUrl = redirectUrl });
        }
        #endregion

        #region Update
        [HttpGet("{id}")]
        public async Task<IActionResult> Update(Guid id)
        {
            // Get Data from API
            var response = await _httpClient.GetAsync($"Product/GetDetailProductForAuctioneer/{id}").ConfigureAwait(false);
            ProductViewModel model = new ProductViewModel();
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                model = JsonConvert.DeserializeObject<ProductViewModel>(responseData);

                if (model.Images != null)
                {
                    foreach (var item in model.Images)
                    {
                        // Create file name
                        string fileName = model.IdProduct + "_" + item.IdImage + item.Extension;

                        // Path in folder wwwroot/imgProduct
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "imgProduct", fileName);

                        // Check file Exist, if do not Exist, create new
                        if (!System.IO.File.Exists(filePath))
                        {
                            using (FileStream fs = new FileStream(filePath, FileMode.Create))
                            {
                                IFormFile formFile = new FormFile(new MemoryStream(item.Data), 0, item.Data.Length, "image", fileName);
                                formFile.CopyTo(fs);
                            }
                        }
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred, please try again";
            }

            return View(model);
        }
        #endregion

        #region UpdateProduct
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(ProductViewModel product)
        {
            var content = new MultipartFormDataContent();

            var claimSid = User.FindFirstValue(ClaimTypes.Sid);
            Guid? id = null;
            if (!string.IsNullOrEmpty(claimSid) && Guid.TryParse(claimSid, out Guid userId))
            {
                id = userId;
            }

            // Set default value
            product.UserId = id;
            product.IsApprove = false;
            product.IsSold = false;
            product.IsPayment = false;
            product.IsReject = false;
            product.CategoryName = "CategoryName";
            product.CategoryId = new Guid("33CEDB5B-DF86-4ED0-9BBE-DF0425D35C13");

            // Add properties as StringContent to content
            content.Add(new StringContent(product.IdProduct.ToString()), "IdProduct");
            content.Add(new StringContent(product.ProductName.ToString()), "ProductName");
            content.Add(new StringContent(product.InitPrice.ToString()), "InitPrice");
            content.Add(new StringContent(product.StepPrice.ToString()), "StepPrice");
            content.Add(new StringContent(product.Description.ToString()), "Description");
            content.Add(new StringContent(product.StartDate.ToString("yyyy-MM-ddTHH:mm:ss")), "StartDate");
            content.Add(new StringContent(product.EndDate?.ToString("yyyy-MM-ddTHH:mm:ss")), "EndDate");
            content.Add(new StringContent(product.IsApprove.ToString()), "IsApprove");
            content.Add(new StringContent(product.IsSold.ToString()), "IsSold");
            content.Add(new StringContent(product.IsPayment.ToString()), "IsPayment");
            content.Add(new StringContent(product.IsReject.ToString()), "IsReject");
            content.Add(new StringContent(product.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(product.UserId.ToString()), "UserId");
            content.Add(new StringContent(product.CategoryName.ToString()), "CategoryName");

            // Add IFormFile to content
            foreach (var image in product.ImageFiles)
            {
                var fileContent = new StreamContent(image.OpenReadStream())
                {
                    Headers =
                    {
                        ContentLength = image.Length,
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType)
                    }
                };
                content.Add(fileContent, "ImageFiles", image.FileName);
            }

            var response = await _httpClient.PostAsync("Product/Update", content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Update Success";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred, please try again";
            }

            var redirectUrl = Url.Action("Index", "Product");
            return Json(new { RedirectUrl = redirectUrl });
        }
        #endregion
    }
}
