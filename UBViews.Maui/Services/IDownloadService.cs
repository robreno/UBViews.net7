namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models;

public interface IDownloadService
{
    Task InitializeDataAsync(ContentPage contentPage, PaperDto dto);
    Task<bool> DownloadAudioFileAsync();
    Task<bool> DownloadAudioFileAsync(string fileName, string audioDir);
    Task<bool> DownloadAudioFileAsync(Uri uri, string audioDir);
}
