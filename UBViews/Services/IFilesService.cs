﻿using UBViews.Models;
using UBViews.Models.Ubml;

namespace UBViews.Services;

/// <summary>
/// 
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePathName"></param>
    /// <returns></returns>
    Task<string> LoadAsset(string filePathName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<string> LoadAsset(string rootPath, string fileName);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePathName"></param>
    /// <returns></returns>
    Task<List<T>> LoadAsset_JsonSerializer<T>(string filePathName);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<List<PaperDto>> GetPaperDtosAsync();

    /// <summary>
    /// Get PaperDto by Paper ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<PaperDto> GetPaperDtoAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ContentDto> GetContentDtoAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<Paragraph>> GetParagraphsAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <param name="seqId"></param>
    /// <returns></returns>
    Task<Paragraph> GetParagraphAsync(int paperId, int seqId);
}