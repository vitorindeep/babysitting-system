﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using GuguDadah.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace GuguDadah.Pages
{
    [AllowAnonymous]
    public class Login : PageModel
    {
        [BindProperty]
        [Display(Name = "Username")]
        public string userName { get; set; }

        [BindProperty]
        [Display(Name = "Password")]
        public string password { get; set; }

        [BindProperty]
        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }

        //instanciando uma classe de serviço por injeção
        private IUserService _userService;
        public string Message { get; private set; } = string.Empty;

        public Login(IUserService userService) {

            _userService = userService;
        }

        //metodo Get inicial
        public void OnGet() {
            //verifica se o usuário está autenticado
            if (User.Identity.IsAuthenticated) {
                Message += "Olá User, você está autenticado";

            }
            else {
                Message += "Você não está autenticado";
            }
        }

        //metodo post do formulario
        public IActionResult OnPost() {
            if (!ModelState.IsValid) {
                return RedirectToPage("/Index");
            }

            //faz a busca do usuário e verifica se existe
            Client client = _userService.AuthenticateClient(userName, password);
            Professional professional = _userService.AuthenticateProfessional(userName, password);

            var user = (dynamic)null;
            var claims = (dynamic)null;

            if (client == null && professional == null)
                return Unauthorized();

            else if (client == null) {
                user = professional;

                claims = new[]
            {
                 new Claim(ClaimTypes.Name, user.userName),
                 new Claim(ClaimTypes.Role, "Professional")
            };
            }

                else {
                    user = client;

                    claims = new[]
                {
                     new Claim(ClaimTypes.Name, user.userName),
                     new Claim(ClaimTypes.Role, "Client")
                };
            }

            //faz autenticação via Cookie
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties {
                    IsPersistent = this.RememberMe,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30)
                });

            // redireciona para a Index novamente, porém já autenticado
            return RedirectToPage("/Index");
        }

        public IActionResult OnGetLogout() {

            Response.Cookies.Delete("GuguDadahLogin");

            return RedirectToPage("/Index");
        }

    }
}