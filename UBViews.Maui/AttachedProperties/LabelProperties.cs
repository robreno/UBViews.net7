using System;

namespace UBViews.AttachedProperties
{
    // See: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/attached-properties?view=net-maui-7.0
    // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/bindable-properties?view=net-maui-7.0#consume-a-bindable-property
    public class Audio
    {
        public static readonly BindableProperty StartMarkerProperty =
            BindableProperty.CreateAttached("StartMarker", typeof(string), typeof(Label), "0:0:0.0");
        public static readonly BindableProperty EndMarkerProperty =
            BindableProperty.CreateAttached("EndMarker", typeof(string), typeof(Label), "0:0:0.0");
        public static readonly BindableProperty TimeSpanProperty =
            BindableProperty.CreateAttached("TimeSpan", typeof(string), typeof(Label), "0:0:0.0_0:0:0.0");

        public static string GetStartMarker(BindableObject view)
        {
            return (string)view.GetValue(StartMarkerProperty);
        }

        public static void SetStartMarker(BindableObject view, string value)
        {
            view.SetValue(StartMarkerProperty, value);
        }

        public static string GetEndMarker(BindableObject view)
        {
            return (string)view.GetValue(EndMarkerProperty);
        }

        public static void SetEndMarker(BindableObject view, string value)
        {
            view.SetValue(EndMarkerProperty, value);
        }

        public static string GetTimeSpan(BindableObject view)
        {
            return (string)view.GetValue(TimeSpanProperty);
        }

        public static void SetTimeSpan(BindableObject view, string value)
        {
            view.SetValue(TimeSpanProperty, value);
        }
    }
    public class Ubml
    {
        public static readonly BindableProperty UniqueIdProperty =
            BindableProperty.CreateAttached("UniqueId", typeof(string), typeof(Label), "###.###.###.###");
        public static readonly BindableProperty SequenceIdProperty =
            BindableProperty.CreateAttached("SequenceId", typeof(string), typeof(Label), "#.#");

        public static string GetUniqueId(BindableObject view)
        {
            return (string)view.GetValue(UniqueIdProperty);
        }
        public static void SetUniqueId(BindableObject view, string value)
        {
            view.SetValue(UniqueIdProperty, value);
        }
        public static string GetSequenceId(BindableObject view)
        {
            return (string)view.GetValue(SequenceIdProperty);
        }
        public static void SetSequenceId(BindableObject view, string value)
        {
            view.SetValue(SequenceIdProperty, value);
        }

        public static readonly BindableProperty EraADProperty =
            BindableProperty.CreateAttached("EraAD", typeof(string), typeof(Label), "CE");
        public static readonly BindableProperty EraBCProperty =
            BindableProperty.CreateAttached("EraBC", typeof(string), typeof(Label), "BCE");

        public static string GetEraAD(BindableObject view)
        {
            return (string)view.GetValue(EraADProperty);
        }
        public static void SetEraAD(BindableObject view, string value)
        {
            view.SetValue(EraADProperty, value);
        }
        public static string GetEraBC(BindableObject view)
        {
            return (string)view.GetValue(EraBCProperty);
        }
        public static void SetEraBC(BindableObject view, string value)
        {
            view.SetValue(EraBCProperty, value);
        }
    }
    public class Paper
    {
        public static readonly BindableProperty IdProperty =
            BindableProperty.CreateAttached("Id", typeof(string), typeof(Label), "Id");
        public static readonly BindableProperty TitleProperty =
            BindableProperty.CreateAttached("Title", typeof(string), typeof(Label), "Title");
        public static readonly BindableProperty AuthorProperty =
            BindableProperty.CreateAttached("Author", typeof(string), typeof(Label), "Author");
        public static readonly BindableProperty PartIdProperty =
            BindableProperty.CreateAttached("PartId", typeof(string), typeof(Label), "PartId");
        public static readonly BindableProperty PartTitleProperty =
            BindableProperty.CreateAttached("PartTitle", typeof(string), typeof(Label), "PartTitle");

        public static string GetId(BindableObject view)
        {
            return (string)view.GetValue(IdProperty);
        }
        public static void SetId(BindableObject view, string value)
        {
            view.SetValue(IdProperty, value);
        }
        public static string GetTitle(BindableObject view)
        {
            return (string)view.GetValue(TitleProperty);
        }
        public static void SetTitle(BindableObject view, string value)
        {
            view.SetValue(TitleProperty, value);
        }
        public static string GetAuthor(BindableObject view)
        {
            return (string)view.GetValue(AuthorProperty);
        }
        public static void SetAuthor(BindableObject view, string value)
        {
            view.SetValue(AuthorProperty, value);
        }
        public static string GetPartId(BindableObject view)
        {
            return (string)view.GetValue(PartIdProperty);
        }
        public static void SetPartId(BindableObject view, string value)
        {
            view.SetValue(PartIdProperty, value);
        }
        public static string GetPartTitle(BindableObject view)
        {
            return (string)view.GetValue(PartTitleProperty);
        }
        public static void SetPartTitle(BindableObject view, string value)
        {
            view.SetValue(PartTitleProperty, value);
        }
    }
}
