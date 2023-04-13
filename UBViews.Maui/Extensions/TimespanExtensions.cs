using System;

namespace UBViews.Extensions;
public static class TimespanExtensions
{
    // See: https://youtu.be/sX9InsNMN8U?t=592
    // Usage: https://youtu.be/sX9InsNMN8U?t=708
    public static string ToShortTimeString(this TimeSpan t)
    {
        string ret = "";
        if (t.Hours > 0)
		{
			ret = $"{t.Hours}:";
		}

		if (t.TotalMinutes > 0)
        {
            if (t.Hours == 0)
			{
				ret += $"{t.Minutes}:";
			}
			else
			{
				ret += $"{t.Minutes.ToString("D2")}:";
			}
		}

        if (t.TotalSeconds > 0)
		{
			ret += $"{t.Seconds.ToString("D2")}";
		}
		else
		{
			ret += "00";
		}

		return ret;
    }

	public static string ToMediumTimeString(this TimeSpan t)
	{
		string ret = "";

		if (t.Hours > 0)
		{
			ret = $"{t.Hours}:";
		}

		if (t.TotalMinutes > 0)
		{
			if (t.Hours == 0)
			{
				ret += $"{t.Minutes}:";
			}
			else
			{
				ret += $"{t.Minutes.ToString("D2")}:";
			}
		}

		if (t.TotalSeconds > 0)
		{
			ret += $"{t.Seconds.ToString("D2")}";
		}
		else
		{
			ret += "00";
		}

		return ret;
	}

	public static string ToLongTimeString(this TimeSpan t)
	{
		string ret = "";
		if (t.Hours > 0)
		{
			ret = $"{t.Hours}:";
		}

		if (t.TotalMinutes > 0)
		{
			if (t.Hours == 0)
			{
				ret += $"{t.Minutes}:";
			}
			else
			{
				ret += $"{t.Minutes.ToString("D2")}:";
			}
		}

		if (t.TotalSeconds > 0)
		{
			ret += $"{t.Seconds.ToString("D2")}";
		}
		else
		{
			ret += "00";
		}

		return ret;
	}
}
