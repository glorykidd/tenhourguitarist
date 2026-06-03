using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Controllers;

[Route("account")]
public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet("do-logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User signed out");
        return Redirect("/");
    }

    [HttpPost("do-login")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] bool rememberMe,
        [FromForm] string? returnUrl)
    {
        var loginUrl = string.IsNullOrEmpty(returnUrl)
            ? "/account/login"
            : $"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || user.IsDeleted)
            return Redirect($"{loginUrl}&error=invalid");

        if (!await userManager.IsEmailConfirmedAsync(user))
            return Redirect($"{loginUrl}&error=unconfirmed");

        if (await userManager.IsLockedOutAsync(user))
            return Redirect($"{loginUrl}&error=locked");

        var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User {Email} signed in successfully", email);
            return LocalRedirect(returnUrl ?? "/");
        }

        if (result.IsLockedOut)
            return Redirect($"{loginUrl}&error=locked");

        return Redirect($"{loginUrl}&error=invalid");
    }
}
