namespace VaporStore.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var resultArray = context.Genres
                .Where(g => genreNames.Any(gn => gn == g.Name))
                .Include(g => g.Games).ThenInclude(g => g.Developer)
                .Include(g => g.Games).ThenInclude(g => g.Purchases)
                .Include(g => g.Games).ThenInclude(g => g.GameTags).ThenInclude(gt => gt.Tag)
                .ToArray()
                //using Eager loading and materialization here because TotalPlayers Property of anonymous object throws EFC Exception
                .Select(g => new
                {
                    Id = g.Id,
                    Genre = g.Name,
                    Games = g.Games
                                .Where(game => game.Purchases.Any())
                                .Select(game => new
                                {
                                    Id = game.Id,
                                    Title = game.Name,
                                    Developer = game.Developer.Name,
                                    Tags = string.Join(", ", game.GameTags.Select(gt => gt.Tag.Name)).TrimEnd(),
                                    Players = game.Purchases.Count
                                })
                                .OrderByDescending(x => x.Players)
                                .ThenBy(x => x.Id)
                                .ToArray(),

                    TotalPlayers = g.Games.Sum(g => g.Purchases.Count)
                })
                .OrderByDescending(x => x.TotalPlayers)
                .ThenBy(x => x.Id)
                .ToArray();

            string json = JsonConvert.SerializeObject(resultArray, Formatting.Indented);
            return json;
        }
        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            object newPurchaseType;

            bool isTypeValid = Enum.TryParse(typeof(PurchaseType), storeType, out newPurchaseType);

            var parsedPurchaseType = (PurchaseType)newPurchaseType;
            ;
            var users = context.Users
                .Where(u => u.Cards
                    .Any(c => c.Purchases
                    .Any(p => p.Type == parsedPurchaseType)))
                .Include(p => p.Cards)
                    .ThenInclude(c => c.Purchases)
                    .ThenInclude(p => p.Game)
                    .ThenInclude(g => g.Genre)
                .ToArray() // materialize and explicit load because LINQ query is too complex for EFC
                .OrderByDescending(u => u.Cards
                        .Sum(c => c.Purchases
                        .Where(p => p.Type == parsedPurchaseType)
                        .Sum(p => p.Game.Price)))
                .ThenBy(u => u.Username)
                .Select(u => new ExportUsersDTO()
                {
                    Username = u.Username,
                    TotalSpent = u.Cards
                            .Sum(c => c.Purchases
                            .Where(p => p.Type == parsedPurchaseType)
                            .Sum(p => p.Game.Price)).ToString(),

                    Purchases = u.Cards.SelectMany(c => c.Purchases)
                    .Where(p => p.Type == parsedPurchaseType)
                    .Select(p => new ExportPurchaseDTO
                    {
                        Card = p.Card.Number,
                        Cvc = p.Card.Cvc,
                        Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                        Game = new ExportGameDTO()
                        {
                            Genre = p.Game.Genre.Name,
                            Price = p.Game.Price.ToString(),
                            Title = p.Game.Name
                        }
                    })
                    .OrderBy(p => p.Date)
                    .ToArray()
                })
                .ToArray();

            var sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportUsersDTO[]), new XmlRootAttribute("Users"));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            xmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}