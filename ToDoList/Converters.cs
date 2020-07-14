/*
*---------------------------------
*|		All rights reserved.
*|		author: lizhanping
*|		version:1.0
*|		File: Converters.cs
*|		Summary: 
*|		Date: 2020/2/28 17:39:08
*---------------------------------
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ToDoList
{
    public class Int2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            if (value == null)
                return string.Empty;
            if (parameter == null)
                return string.Empty;
            return parameter.ToString() + ":" + value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
