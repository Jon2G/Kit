﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Services.Interfaces
{
    public interface IPrintHTML
    {
        public bool Print(string Html, string Printer);
    }
}