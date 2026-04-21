using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using Utilities.Responses;

namespace Business.IServices
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationUserRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<ApplicationUserRole> roleManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IResult> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Result.Fail(["User not found!"]);
                }

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                // Token'ı client'a gönder (email ile vs.)
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(["System error!", ex.Message]);
            }
        }

        public async Task<IResult> LoginAsync(LoginDto model)
        {
            try
            {
                var signInResult = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (signInResult.Succeeded)
                {
                    return Result.Success();
                }
                else if (signInResult.IsLockedOut)
                {
                    return Result.Fail(["Your account is locked out, please contact your superior"]);
                }
                else if (signInResult.IsNotAllowed)
                {
                    return Result.Fail(["Your account is not approved yet!"]);
                }
                else
                {
                    return Result.Fail(["Invalid login attempt!", "Password or email address not correct"]);
                }
            }
            catch (Exception ex)
            {
                return Result.Fail(["System error!", ex.Message]);
            }
        }

        public async Task LogOutAsync()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<IResult> RegisterAsync(RegisterDto model)
        {
            try
            {
                // Şifreler eşleşiyor mu kontrol et
                if (model.Password != model.ConfirmPassword)
                {
                    return Result.Fail(["Passwords do not match!"]);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                // Password ile user oluştur
                var identityResult = await userManager.CreateAsync(user, model.Password);
                if (identityResult.Succeeded)
                {
                    if (!roleManager.Roles.Any(x => x.Name == "Manager"))
                    {
                        await roleManager.CreateAsync(new ApplicationUserRole { Name = "Manager" });
                    }
                    if (!roleManager.Roles.Any(x => x.Name == "SalesPerson"))
                    {
                        await roleManager.CreateAsync(new ApplicationUserRole { Name = "SalesPerson" });
                    }
                    await userManager.AddToRoleAsync(user, "SalesPerson");
                    return Result.Success();
                }
                else
                {
                    return Result.Fail(identityResult.Errors.Select(x => x.Description));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            try
            {
                // ResetPasswordDto'da Email yok, sadece AuthToken var
                // AuthToken'dan user ID'yi çıkart (JWT parsing vb.)
                // Burada basit örnek: Token'dan email adresini al
                var claimsPrincipal = GetPrincipalFromToken(model.AuthToken);
                if (claimsPrincipal == null)
                {
                    return Result.Fail(["Invalid or expired token!"]);
                }

                var email = claimsPrincipal.FindFirst("email")?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return Result.Fail(["Token does not contain email information!"]);
                }

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Result.Fail(["User not found!"]);
                }

                // Şifreler eşleşiyor mu kontrol et
                if (model.NewPassword != model.ConfirmPassword)
                {
                    return Result.Fail(["Passwords do not match!"]);
                }

                var result = await userManager.ResetPasswordAsync(user, model.AuthToken, model.NewPassword);
                if (result.Succeeded)
                {
                    return Result.Success();
                }
                else
                {
                    return Result.Fail(result.Errors.Select(x => x.Description));
                }
            }
            catch (Exception ex)
            {
                return Result.Fail(["System error!", ex.Message]);
            }
        }

        public async Task<IResult> ChangePasswordAsync(ChangePasswordDto model)
        {
            try
            {
                // ChangePasswordDto'da Email yok, sadece şifreler var
                // Mevcut oturum açmış kullanıcıyı al (HttpContext ile)
                // Bu örnek - Controller'dan HttpContext almalısın
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Result.Fail(["User not found!"]);
                }

                // Şifreler eşleşiyor mu kontrol et
                if (model.NewPassword != model.ConfirmPassword)
                {
                    return Result.Fail(["Passwords do not match!"]);
                }

                var result = await userManager.ChangePasswordAsync(user, model.PreviousPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return Result.Success();
                }
                else
                {
                    return Result.Fail(result.Errors.Select(x => x.Description));
                }
            }
            catch (Exception ex)
            {
                return Result.Fail(["System error!", ex.Message]);
            }
        }

        // Helper Methods
        private System.Security.Claims.ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            try
            {
                // JWT token'ı parse et ve ClaimsPrincipal'ı döndür
                // Gerçek implementasyon için JwtSecurityTokenHandler kullan
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            // HttpContext.User üzerinden current user'ı al
            // Bu method Controller'dan injected olmalı
            // Geçici olarak null dön
            return null;
        }
    }
}