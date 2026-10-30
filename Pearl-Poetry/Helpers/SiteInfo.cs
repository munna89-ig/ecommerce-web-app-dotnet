using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;

namespace Pearl_Poetry.Helpers
{
    // Site name from Admin > Settings. Read once per request, then reused.
    public class SiteInfo
    {
        private const string DefaultName = "Pearl Poetry";

        private readonly EcommerceSystemContext _context;
        private string? _name;

        public SiteInfo(EcommerceSystemContext context)
        {
            _context = context;
        }

        public string Name
        {
            get
            {
                if (_name != null) return _name;

                var fromDb = _context.Settings
                    .AsNoTracking()
                    .OrderBy(s => s.Id)
                    .Select(s => s.SiteName)
                    .FirstOrDefault();

                _name = string.IsNullOrWhiteSpace(fromDb) ? DefaultName : fromDb.Trim();
                return _name;
            }
        }
    }
}