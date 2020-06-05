﻿using System;

namespace WebStore.UI.Models
{
    public class PagingInfo
    {
        public int TotalItem { get; set; }

        public int ItemsPerPage { get; set; }

        public int CurrentPage { get; set; }

        public int totalPage => (int)Math.Ceiling((decimal)TotalItem / ItemsPerPage);

        public string UrlParam { get; set; }
    }
}