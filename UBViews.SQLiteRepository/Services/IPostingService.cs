using UBViews.SQLiteRepository.Models;

namespace UBViews.SQLiteRepository.Services
{
    public interface IPostingService
    {
        Task<List<PostingList>> GetPostingsAsync();
        Task<PostingList> GetPostingByLexemeAsync(string lexeme);
        Task<PostingList> GetPostingByStableIdAsync(string stableId);
        Task<List<TokenOccurrence>> GetTokenOccurrencesAsync(int postingListId);
        Task<int> SavePostingAsync(PostingList postingList);
        Task<int> SaveTokenOccurenceAsync(TokenOccurrence tokenOccurrence);
        Task<int> SaveTokenOccurrencesAsync(IEnumerable<TokenOccurrence> occurrences);
    }
}
