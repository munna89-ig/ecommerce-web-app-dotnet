using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;

namespace Pearl_Poetry.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly EcommerceSystemContext _context;

        public AuditLogController(EcommerceSystemContext context)
        {
            _context = context;
        }

        // =========================================================
        // AUDIT LOG LIST
        // Supports:
        // /Admin/Audit
        // /Admin/AuditLog
        // =========================================================

        [HttpGet]
        [Route("Admin/Audit")]
        [Route("Admin/AuditLog")]
        [Route("Admin/AuditLog/Index")]
        public async Task<IActionResult> Index(
            string? search,
            [FromQuery(Name = "action")] string? actionFilter,
            string? entityType,
            int page = 1)
        {
            // "action" is a reserved route name (it always contains "Index"),
            // so the dropdown value is read from the query string only.
            string? action = actionFilter;

            const int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            var query = _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .AsQueryable();

            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    (a.Action != null && a.Action.Contains(search)) ||
                    (a.EntityType != null && a.EntityType.Contains(search)) ||
                    (a.IpAddress != null && a.IpAddress.Contains(search)) ||
                    (a.User != null && a.User.Name != null &&
                     a.User.Name.Contains(search)) ||
                    (a.User != null && a.User.Email != null &&
                     a.User.Email.Contains(search))
                );
            }

            // -----------------------------------------------------
            // ACTION FILTER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(a => a.Action == action);
            }

            // -----------------------------------------------------
            // ENTITY TYPE FILTER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            // -----------------------------------------------------
            // TOTAL ITEMS
            // -----------------------------------------------------

            int totalItems = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize
            );

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // -----------------------------------------------------
            // GET LOGS
            // -----------------------------------------------------

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // -----------------------------------------------------
            // ACTION DROPDOWN
            // -----------------------------------------------------

            var actions = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Action != null)
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            // -----------------------------------------------------
            // ENTITY TYPE DROPDOWN
            // -----------------------------------------------------

            var entityTypes = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityType != null)
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            // -----------------------------------------------------
            // VIEW DATA
            // -----------------------------------------------------

            ViewBag.Search = search;
            ViewBag.Action = action;
            ViewBag.EntityType = entityType;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            ViewBag.Actions = actions;
            ViewBag.EntityTypes = entityTypes;

            return View(logs);
        }


        // =========================================================
        // AUDIT LOG DETAILS
        // Supports:
        // /Admin/Audit/Details/1
        // /Admin/AuditLog/Details/1
        // =========================================================

        [HttpGet]
        [Route("Admin/Audit/Details/{id:int}")]
        [Route("Admin/AuditLog/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var auditLog = await _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auditLog == null)
            {
                TempData["Error"] = "Audit log not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(auditLog);
        }
    }
}