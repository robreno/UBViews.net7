namespace UBViews.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class HttpClientUtils
{
	public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
	{
		using (var s = await client.GetStreamAsync(uri))
		{
			using (var fs = new FileStream(FileName, FileMode.CreateNew))
			{
				await s.CopyToAsync(fs);
			}
		}
	}
}
