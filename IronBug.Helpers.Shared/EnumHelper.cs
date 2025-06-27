﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IronBug.Helpers
{
    public static class EnumHelper
    {
        public static Enum GetValue(this int value, Type enumType)
        {
            return (Enum)Enum.ToObject(enumType, value);
        }

        public static TAttribute Attribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
                return null;

            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        public static string DisplayName(this Enum value)
        {
            var attribute = value.Attribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }

        public static object ToViewModel<TEnum>() where TEnum : Enum
        {
            var json = new Dictionary<string, object>();

            var enums = new List<object>();
            var names = new Dictionary<int, string>();

            foreach (var e in Enum.GetValues(typeof(TEnum)))
            {
                var value = (int)e;
                var displayName = ((TEnum)e).DisplayName();

                enums.Add(new
                {
                    Value = value,
                    Name = e.ToString(),
                    Display = displayName
                });

                names.Add(value, displayName);
            }

            json.Add("enums", enums.ToArray());
            json.Add("names", names);

            foreach (var e in Enum.GetValues(typeof(TEnum)))
            {
                json.Add(e.ToString(), (int)e);
            }

            return json;
        }
    }
}