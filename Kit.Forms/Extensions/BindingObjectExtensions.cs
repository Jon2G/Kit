﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

// ReSharper disable once CheckNamespace
namespace Kit.Forms
{
    public static class BindingObjectExtensions
    {
        public static string GetBindingPath(this BindableObject self, BindableProperty property)
        {
            BindingBase binding = GetBinding(self, property);
            if (binding is Xamarin.Forms.Internals.TypedBindingBase)
            {
                var _handlersField = binding.GetType().GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_handlersField is not null)
                {
                    //PropertyChangedProxy
                    if (_handlersField.GetValue(binding) is Array array)
                    {
                        if (array.Length > 1)
                        {
                            if (Debugger.IsAttached)
                                Debugger.Break();
                        }
                        if (array.Length > 0)
                        {
                            var propertyChangedProxy = array.GetValue(0);
                            var PropertyName = propertyChangedProxy.GetType().GetProperty("PropertyName", BindingFlags.Instance | BindingFlags.Public);
                            return PropertyName.GetValue(propertyChangedProxy)?.ToString();
                        }
                    }
                }
            }
            return null;
        }
        public static BindingBase GetBinding(this BindableObject self, BindableProperty property)
        {
            var methodInfo = typeof(BindableObject).GetTypeInfo().GetDeclaredMethod("GetContext");
            var context = methodInfo?.Invoke(self, new[] { property });

            var propertyInfo = context?.GetType().GetTypeInfo().GetDeclaredField("Binding");
            var binding = propertyInfo?.GetValue(context);
            return binding as BindingBase;
        }

        public static object GetBindingExpression(this Binding self)
        {
            var fieldInfo = self?.GetType().GetTypeInfo().GetDeclaredField("_expression");
            return fieldInfo?.GetValue(self);


        }
    }
}
