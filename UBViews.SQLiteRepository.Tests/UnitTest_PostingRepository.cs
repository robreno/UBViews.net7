namespace UBViews.SQLiteRepository.Tests;

using UBViews.SQLiteRepository.Dtos;
using UBViews.SQLiteRepository.Models;
using UBViews.SQLiteRepository.Services;

public class UnitTest_PostingRepository
{
    [Fact]
    public async void Test_Not_Initialized_DBExists()
    {
        var result = await PostingRepository.DatabaseExistsAsync();
        Assert.True(result);
    }
}