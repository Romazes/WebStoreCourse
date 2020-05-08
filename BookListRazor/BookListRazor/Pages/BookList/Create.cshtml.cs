﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListRazor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookListRazor.Pages.BookList
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _appDbContext;

        public CreateModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Book Book { get; set; }

        public void OnGet()
        {

        }
    }
}