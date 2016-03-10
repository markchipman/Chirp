﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Chirp.Database;
using Chirp.ViewModels;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net;
using Chirp.Models;
using Microsoft.AspNet.Authorization;
using Chirp.PageModels;
using SendGridMessenger;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Chirp.Controllers.Web
{
    [Route("[action]")]
    public class AuthController : Controller
    {
        private UserManager<ChirpUser> m_userManager;
        private IEmailSender m_emailSender;

        public AuthController(UserManager<ChirpUser> a_userManager, IEmailSender a_emailSender)
        {
            m_userManager = a_userManager;
            m_emailSender = a_emailSender;
        }

        public IActionResult Login(string email)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "App");
            }

            LoginPageModel pageModel = new LoginPageModel();
            pageModel.SetMessage(email);

            return View(pageModel);
        }

        public IActionResult Signup()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "App");
            }

            return View();
        }

        public IActionResult ConfirmEmailSent()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> EmailConfirmed(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await m_userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await m_userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "EmailConfirmed" : "Error");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult ResetPassword(string userId, string code)
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost()]
        public async Task<IActionResult> SendResetPasswordEmail(ForgotPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = await m_userManager.FindByEmailAsync(vm.Email);
                if (user != null)
                {
                    // Send an email with this link
                    var code = await m_userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Auth", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await m_emailSender.SendEmailAsync(vm.Email, "Reset Password",
                       "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");

                    Response.StatusCode = (int)HttpStatusCode.SeeOther;
                    return RedirectToAction("Login", "Auth", new { email = vm.Email });
                }
            }

            Response.StatusCode = (int)HttpStatusCode.SeeOther;
            return RedirectToAction("Login", "Auth", new { email = vm.Email });
        }
    }
}
