﻿using System.Globalization;
using System.Text;

namespace UBViews.Converters
{
    public class DurationDisplayConverter : IValueConverter
    {
        #region Constructor(s) and Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="DurationDisplayConverter"/> class.
        /// </summary>
        public DurationDisplayConverter()
        {
            NoDurationText = "0.0.0";
            TimeSpanFormat = "g";
        } 

        #endregion

        /// <summary>
        /// Gets or sets the text to be displayed if there is no duration value.
        /// </summary>
        /// <value>
        /// The no duration text.
        /// </value>
        public String NoDurationText { get; set; }

        /// <summary>
        /// Gets or sets the time span format.
        /// </summary>
        /// <value>
        /// The time span format.
        /// </value>
        public String TimeSpanFormat { get; set; }

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The type of the target property. This uses a different type depending on whether you're programming with Microsoft .NET or Visual C++ component extensions (C++/CX). See Remarks.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Object Convert(Object value, Type targetType, Object parameter, String language)
        {
            Duration duration;
            TimeSpan timeSpanClass;
            string timeSpan = string.Empty;
            if (value.GetType().Name.Equals("Duration"))
            {
                duration = (Duration)value;
                timeSpanClass = duration.TimeSpan;
                var h = timeSpanClass.Hours;
                var m = timeSpanClass.Minutes;
                var s = timeSpanClass.Seconds;
                timeSpan = h + "." + m + "." + s;
            }
            if (value.GetType().Name.Equals("TimeSpan"))
            {
                var span = (TimeSpan)value;
            }
            if (duration != null) return timeSpan;
            return NoDurationText;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object. This method is called only in TwoWay bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Object ConvertBack(Object value, Type targetType, Object parameter, String language)
        {
            throw new NotImplementedException();
        }
    }
}