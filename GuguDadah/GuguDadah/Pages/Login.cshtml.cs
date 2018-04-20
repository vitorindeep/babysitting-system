﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using GuguDadah.Data;
using GuguDadah.Includes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GuguDadah.Pages {

    [AllowAnonymous]
    public class Login : PageModel {

        [Required]
        [BindProperty]
        [Display(Name = "Nickname")]
        public string UserName { get; set; }

        [Required]
        [BindProperty]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [BindProperty]
        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }

        //instanciando uma classe de serviço por injeção
        private ILoginService _loginService;

        public Login(ILoginService loginService) {

            _loginService = loginService;
        }

        //metodo post do formulario
        public IActionResult OnPostLoginUser() {

            TryUpdateModelAsync(this);

            // para o caso de as credenciais não terem sido preenchidas. Mostra mensagens de erro...
            if (!ModelState.IsValid) return Page();

            // verificação dupla
            if (UserName == null || Password == null) {
                ModelState.AddModelError("", "Preencha os campos");
                return Page();
            }

            // desta forma, não mais é possível alterar o valor das strings
            const string AdminUsername = "admin";
            const string PasswordUsername = "securityhole";

            var user = (dynamic)null;
            var claims = (dynamic)null;

            if (UserName.Equals(AdminUsername) && Password.Equals(PasswordUsername)) {
                claims = new[]
{
                     new Claim(ClaimTypes.Name, "Administrador"),
                     new Claim(ClaimTypes.Role, "Admin")
                };
                goto Checked;
            }

            //faz a busca do usuário e verifica se existe
            Client client = _loginService.AuthenticateClient(UserName, Password);
            Professional professional = _loginService.AuthenticateProfessional(UserName, Password);

            if (client == null && professional == null) {
                ModelState.AddModelError("", "Username ou Password inválidas.");
                return Page();
            }

            else if (client == null) {
                user = professional;

                claims = new[]
            {
                 new Claim(ClaimTypes.Name, user.UserName),
                 new Claim(ClaimTypes.Role, "Professional")
            };
            }

            else {
                user = client;

                claims = new[]
            {
                     new Claim(ClaimTypes.Name, user.UserName),
                     new Claim(ClaimTypes.Role, "Client")
                };
            }

            Checked:
            //faz autenticação via Cookie
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties {
                    IsPersistent = this.RememberMe,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30)
                });

            // redireciona para a Index novamente, porém já autenticado
            return RedirectToPage("/Index").WithSuccess("Login", "efetuado com sucesso.", "3000");
        }

        public IActionResult OnGetLogout() {

            Response.Cookies.Delete("GuguDadahLogin");

            return RedirectToPage("/Index");
        }

    }
}