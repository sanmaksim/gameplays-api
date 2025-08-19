using GameplaysApi.Data;
using GameplaysApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Services
{
    public class GameService
    {
        private readonly ApplicationDbContext _context;

        public GameService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultsWrapperDto?> GetExistingGame(int gameId)
        {
            return await _context.Games
                .Include(game => game.Developers)
                .Include(game => game.Franchises)
                .Include(game => game.Genres)
                .Include(game => game.Platforms)
                .Include(game => game.Publishers)
                .Where(game => game.GameId == gameId)
                .Select(game => new ResultsWrapperDto
                {
                    Results = new GameResponseDto
                    {
                        Name = game.Name,
                        Deck = game.Deck,
                        DateLastUpdated = game.DateLastUpdated,
                        Developers = game.Developers!.Select(
                            developer => new DeveloperDto
                            {
                                DeveloperId = developer.DeveloperId,
                                Name = developer.Name
                            }).ToList(),
                        Franchises = game.Franchises!.Select(
                            franchise => new FranchiseDto
                            {
                                FranchiseId = franchise.FranchiseId,
                                Name = franchise.Name
                            }).ToList(),
                        Genres = game.Genres!.Select(
                            genre => new GenreDto
                            {
                                GenreId = genre.GenreId,
                                Name = genre.Name
                            }).ToList(),
                        Image = game.Image,
                        OriginalReleaseDate = game.OriginalReleaseDate,
                        Platforms = game.Platforms!.Select(
                            platform => new PlatformDto
                            {
                                PlatformId = platform.PlatformId,
                                Name = platform.Name
                            }).ToList(),
                        Publishers = game.Publishers!.Select(
                            publisher => new PublisherDto
                            {
                                PublisherId = publisher.PublisherId,
                                Name = publisher.Name
                            }).ToList(),
                        UpdatedAt = game.UpdatedAt
                    }
                })
                .FirstOrDefaultAsync();
        }
    }
}
