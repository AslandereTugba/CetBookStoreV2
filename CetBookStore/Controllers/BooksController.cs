using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CetBookStore.Data;
using CetBookStore.Models;
using CetBookStore.Services;
using Microsoft.AspNetCore.Authorization;

namespace CetBookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ImageService _imageService;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, ImageService imageService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _imageService = imageService;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Books.Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Author,Publisher,PageCount,Price,IsInSale,PreviousPrice,PublicationDate,CategoryId,ImageFile")] Book book)
        {
            if (ModelState.IsValid)
            {
                if (book.ImageFile != null)
                {
                    try
                    {
                        book.ImageUrl = _imageService.SaveImage(book.ImageFile);
                        _context.Add(book);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Resim seçiniz");
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Author,Publisher,PageCount,Price,IsInSale,PreviousPrice,PublicationDate,CreatedDate,CategoryId,ImageFile")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Books.FindAsync(id);
                    if (existing == null)
                        return NotFound();

                    if (book.ImageFile != null)
                    {
                        try
                        {
                            _imageService.DeleteImage(existing.ImageUrl);
                            book.ImageUrl = _imageService.SaveImage(book.ImageFile);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("ImageFile", ex.Message);
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                            return View(book);
                        }
                    }
                    else
                    {
                        book.ImageUrl = existing.ImageUrl;
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        [Authorize]
        public async Task<IActionResult> Buy(int id, int count=1)
        {
            if(count<1)
            {
                return BadRequest();
            }
            var book = await _context.Books.FindAsync(id);
            if(book==null)
            {                
                return NotFound();
            }
            
           // Sale sale = new Sale();
           // sale.CetUserId =_context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            //sale.BookId = id;

           // sale.TotalPrice = book.Price*count;
           // sale.SalesDate = DateTime.Now;
           // sale.Count = count;
           // _context.Sales.Add(sale);
           // await _context.SaveChangesAsync();
            return RedirectToAction( "Index", "Home");

        }
        public async Task<IActionResult> CommentCreate([Bind("Id,UserName,Content,BookId")] Comment comment)
        {

            comment.CreatedDate = DateTime.Now;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                comment.UserName = HttpContext.User.Identity.Name;

            }
           

           
                _context.Add(comment);
                await _context.SaveChangesAsync();
                
            

            return RedirectToAction(nameof(Details),new { id = comment.BookId });
        }
    }
}
