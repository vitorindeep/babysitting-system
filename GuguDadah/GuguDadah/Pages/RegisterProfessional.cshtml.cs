﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using System.Diagnostics;
using ImageMagick;

using GuguDadah.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GuguDadah.Includes;
using Microsoft.EntityFrameworkCore;

namespace GuguDadah.Pages {

    [Authorize(Roles = "Admin")]
    public class RegisterProfessional : PageModel {

        [Required]
        [BindProperty]
        [Display(Name = "Confirmar Password")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public IFormFile Avatar { get; set; }

        [BindProperty]
        public Professional Professional { get; set; }

        private readonly AppDbContext dbContext;

        public RegisterProfessional(AppDbContext context) {

            dbContext = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult OnPostCreateAccount() {

            TryUpdateModelAsync(this);

            ModelState.Remove("Professional.Avatar");
            ModelState.Remove("Professional.Rating");
            ModelState.Remove("Professional.RegistrationDate");

            // retorna erros se os campos foram incorretamente preenchidos
            if (!ModelState.IsValid) return Page();

            if (ModelState.IsValid) {

                // verifica se o username já existe
                bool clientUsernameAlreadyExists = dbContext.Clients.Any(o => o.UserName == Professional.UserName);
                bool professionalUsernameAlreadyExists = dbContext.Professionals.Any(o => o.UserName == Professional.UserName);

                // retorna erro se já existir
                if (clientUsernameAlreadyExists || professionalUsernameAlreadyExists || Professional.UserName.Equals("admin")) {

                    ModelState.AddModelError(string.Empty, "Este nickname já existe no sistema.");

                    return Page();
                }

                // verifica se o email já existe
                bool clientEmailAlreadyExists = dbContext.Clients.Any(o => o.Email == Professional.Email);
                bool professionalEmailAlreadyExists = dbContext.Professionals.Any(o => o.Email == Professional.Email);

                // retorna erro se já existe
                if (clientEmailAlreadyExists || professionalEmailAlreadyExists) {

                    ModelState.AddModelError(string.Empty, "Este email já existe no sistema.");

                    return Page();
                }

                // retorna erro se as passwords não coincidirem
                if (ConfirmPassword != Professional.Password) {

                    ModelState.AddModelError(string.Empty, "Passwords não coincidem");

                    return Page();
                }

                // retorna erro se tentar ir para um turno inválido
                if (Professional.Shift != "M" && Professional.Shift != "T" && Professional.Shift != "N") {

                    ModelState.AddModelError(string.Empty, "Turno inválido.");

                    return Page();
                }
            }


            // define data atual (data de registo)
            var dateAndTime = DateTime.Now;
            var date = dateAndTime.Date;

            // encripta a password
            var hash = BCrypt.Net.BCrypt.HashPassword(Professional.Password);

            // instancia a classe de registo de clientes para aceder ao método GetAvatar
            Register register = new Register(dbContext);

            Professional newProfessional = new Professional() {
                UserName = Professional.UserName,
                Avatar = register.GetAvatar(Avatar).ToArray(),
                Password = hash,
                Email = Professional.Email,
                Contact = Professional.Contact,
                Name = Professional.Name,
                Shift = Professional.Shift,
                Rating = 3,
                RegistrationDate = date
            };

            // adiciona o profissional à BD
            dbContext.Professionals.Add(newProfessional);

            // guarda as alterações
            dbContext.SaveChanges();

            return RedirectToPage("./AdminArea").WithSuccess("Profissional", "registado com sucesso.", "3000");

        }

    }

}