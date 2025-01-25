using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing; 
using System.Linq;
using Backend.Models;
using Backend.Data;


namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context, ILogger<ArticlesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Получить все статьи
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.articles.ToListAsync();
        }

        // Получить статью по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await _context.articles.FindAsync(id);

            if (article == null)
                return NotFound();

            return article;
        }

        // Создать статью
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            _context.articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArticle), new { id = article.id }, article);
        }

        // Обновить статью
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            if (id != article.id)
                return BadRequest();

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.articles.Any(e => e.id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // Удалить статью
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.articles.FindAsync(id);
            if (article == null)
                return NotFound();

            _context.articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Article>>> SearchArticles([FromQuery] string? author, [FromQuery] string? title)
        {
            var query = _context.articles.AsQueryable();

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(a => a.author.Contains(author));
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(a => a.title.Contains(title));
            }

            var results = await query.ToListAsync();

            return Ok(results);
        }

        [HttpGet("pdf")]
        public async Task<IActionResult> ExportToPdf()
        {
            try
            {
                var articles = await _context.articles.ToListAsync();

                using (MemoryStream stream = new MemoryStream())
                {
                    // Создание нового PDF документа
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "Articles Report";

                    // Создание пустой страницы
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // Шрифты
                    var headerFont = new XFont("DejaVu Sans", 8, XFontStyle.Bold); // Уменьшенный размер шрифта
                    var rowFont = new XFont("DejaVu Sans", 6, XFontStyle.Regular);   // Уменьшенный размер шрифта

                    // Настройка таблицы
                    double margin = 40;
                    double yPoint = margin;
                    double tableWidth = page.Width - 2 * margin;
                    double rowHeight = 25; // Увеличенный высота строки для переноса текста

                    // Определение ширины колонок (уменьшены для вмещения таблицы)
                    double[] columnWidths = { 30, 100, 80, 150, 80, 70 };
                    string[] headers = { "ID", "Title", "Author", "Content", "Annotation", "Published Date" };

                    // Рисование заголовков таблицы
                    double xPoint = margin;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, xPoint, yPoint, columnWidths[i], rowHeight);
                        gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                            new XRect(xPoint + 2, yPoint + 2, columnWidths[i] - 4, rowHeight - 4),
                            XStringFormats.TopLeft);
                        xPoint += columnWidths[i];
                    }

                    yPoint += rowHeight;

                    // Рисование строк таблицы
                    foreach (var article in articles)
                    {
                        // Проверка на перенос страницы
                        if (yPoint + rowHeight > page.Height - margin)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPoint = margin;

                            // Рисование заголовков снова на новой странице
                            xPoint = margin;
                            for (int i = 0; i < headers.Length; i++)
                            {
                                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, xPoint, yPoint, columnWidths[i], rowHeight);
                                gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                                    new XRect(xPoint + 2, yPoint + 2, columnWidths[i] - 4, rowHeight - 4),
                                    XStringFormats.TopLeft);
                                xPoint += columnWidths[i];
                            }

                            yPoint += rowHeight;
                        }

                        xPoint = margin;

                        string[] rowData = {
                            article.id.ToString(),
                            article.title,
                            article.author,
                            article.content,
                            article.annotation ?? "",
                            article.published_date.ToString("g")
                        };

                        for (int i = 0; i < rowData.Length; i++)
                        {
                            gfx.DrawRectangle(XPens.Black, xPoint, yPoint, columnWidths[i], rowHeight);

                            // Создание области для текста с учетом отступов
                            var rect = new XRect(xPoint + 2, yPoint + 2, columnWidths[i] - 4, rowHeight - 4);

                            // Рисование текста с переносом
                            gfx.DrawString(rowData[i], rowFont, XBrushes.Black,
                                rect,
                                XStringFormats.TopLeft);

                            xPoint += columnWidths[i];
                        }

                        yPoint += rowHeight;
                    }

                    // Сохранение документа в MemoryStream
                    document.Save(stream, false);
                    stream.Position = 0;

                    // Возвращение PDF файла
                    return File(stream.ToArray(), "application/pdf", "articles.pdf");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:", ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}

