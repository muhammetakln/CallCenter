using Core.Abstracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UIWeb.Areas.Identity.Pages
{
    public class LogOutModel(IAuthService service) : PageModel
    {
        public IActionResult OnGet()
        {
            return NotFound();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await service.LogOutAsync();
            return LocalRedirect("/identity/login");
        }
    }
}
