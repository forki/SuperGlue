﻿using System;

namespace Jajo.Web.ModelBinding
{
    public interface IValueConverter
    {
        bool Matches(Type destinationType);
        object Convert(Type destinationType, object value);
    }
}