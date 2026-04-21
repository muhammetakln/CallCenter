using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UIWeb.Areas.Identity.Pages
{
    public class LoginModel(IAuthService service) : PageModel
    {

        // Sayafya baðlamak için yazýlýr.
        [BindProperty]
        public LoginDto Input { get; set; }
        public void OnGet()
        {
        }

        //http post yerine onpost yazýyoruz.Gönder tuþuna bastýðýmzda gittiðimiz yer post.bununla çalýþýr.
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {

                var result = await service.LoginAsync(Input);
                if (result.IsSuccess)
                {
                    return LocalRedirect("/");

                }

                foreach (var err in result.Messages)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
            }
            return Page();
        }
    }
}
