﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using GuguDadah.Data;
using System.ComponentModel.DataAnnotations;

namespace GuguDadah.Pages {
    public class Babysitters : PageModel {

        private readonly AppDbContext dbContext;

        public Babysitters(AppDbContext context) {

            dbContext = context;
            onGet();
        }

        public List<Professional> lista = new List<Professional>();

        public void onGet() {

            var query = (from p in dbContext.Professionals
                         orderby p.userName
                         select p).ToList();

            foreach (var item in query) //retrieve each item and assign to model
            {
                lista.Add(new Professional() {
                    userName = item.userName,
                    contact = item.contact,
                    eMail = item.eMail,
                    avatar = item.avatar,
                    shift = item.shift,
                    rating = item.rating
                });
            }
        }
    }
}