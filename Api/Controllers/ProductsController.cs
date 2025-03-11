// using Microsoft.AspNetCore.Mvc;
// using MyWebApiProject.Models;
// using System.Threading.Tasks;
// using System.Linq;

// namespace MyWebApiProject.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ProductsController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         // Constructor ที่รับ ApplicationDbContext มาใช้
//         public ProductsController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/products
//         [HttpGet]
//         public IActionResult GetProducts()
//         {
//             var products = _context.Products.ToList();
//             return Ok(products);
//         }

//         // POST: api/products
//         [HttpPost]
//         public async Task<IActionResult> CreateProduct(Product product)
//         {
//             _context.Products.Add(product);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
//         }
//     }
// }
