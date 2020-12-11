﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Forms.Controls.Pages.PinView
{
    public class PinSubmitEventArg
    {
        public PinSubmitEventArg(object sender, string pin)
        {
            Source = sender;
            Pin = pin;
        }
        public object Source { get; private set; }
        public string Pin { get; private set; }
    }
}