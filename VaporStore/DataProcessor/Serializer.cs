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

            //        var resultArray = context.Genres
            //            .Include(g => g.Games).ThenInclude(g => g.Developer)
            //            .Include(g => g.Games).ThenInclude(g => g.Purchases)
            //            .ToArray()
            //            .Where(g => genreNames.Any(gn => gn == g.Name))
            //.Select(g => new //ExportGame()

            //            {
            //	Id = g.Id,
            //	Genre = g.Name,
            //                Games = g.Games
            //                .Where(game => game.Purchases.Any())
            //                .Select(game => new //GameDTO
            //                {
            //                    Id = game.Id,
            //                    Title = game.Name,
            //                    Developer = game.Developer.Name,
            //                    Tags = string.Join(", ", game.GameTags.Select(gt => gt.Tag.Name)).TrimEnd(),
            //                    Players = game.Purchases.Count
            //                })
            //                .OrderByDescending(x => x.Players)
            //                .ThenBy(x => x.Id)
            //                .ToArray(),

            //                TotalPlayers = g.Games.Sum(g => g.Purchases.Count) //null
            //            })
            //            .OrderByDescending(x => x.TotalPlayers)
            //            .ThenBy(x => x.Id)
            //            .ToArray();
            var resultArray = context.Genres
                        .Where(g => genreNames.Contains(g.Name))
                        .AsEnumerable()
                        .Select(g => new
                        {
                            Id = g.Id,
                            Genre = g.Name,
                            Games = g.Games
                                        .Where(ga => ga.Purchases.Any())
                                        .Select(ga => new
                                        {
                                            Id = ga.Id,
                                            Title = ga.Name,
                                            Developer = ga.Developer.Name,
                                            Tags = string.Join(", ", ga.GameTags.Select(gt => gt.Tag.Name)),
                                            Players = ga.Purchases.Count
                                        })
                                        .OrderByDescending(ga => ga.Players)
                                        .ThenBy(ga => ga.Id),
                            TotalPlayers = g.Games.Sum(ga => ga.Purchases.Count)
                        })
                        .OrderByDescending(g => g.TotalPlayers)
                        .ThenBy(g => g.Id)
                        .ToArray();
            ;
            //foreach (var result in resultArray)
            //{
            //    result.TotalPlayers = context.Games
            //        .ToArray()
            //        .Where(g => g.Genre.Name == result.Genre)
            //        .Select(g => g.Purchases.Count)
            //        .ToArray()
            //        .Sum();
            //}
            //resultArray
            //    .OrderByDescending(x => x.TotalPlayers)
            //    .ThenBy(x => x.Id)
            //    .ToArray();

            string json = JsonConvert.SerializeObject(resultArray, Formatting.Indented);
			return json;
		}
        public class ExportGame
        {
            public int Id { get; set; }

            public string Genre { get; set; }

            public GameDTO[] Games { get; set; }

            public int? TotalPlayers { get; set; }
        }
        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            object newPurchaseType;

            bool isTypeValid = Enum.TryParse(typeof(PurchaseType), storeType, out newPurchaseType);

            var users = context.Users
                .Where(u => u.Cards.Any(c => c.Purchases.Any()))
                .Include(p=>p.Cards)
                .ThenInclude(c=>c.Purchases)
                .ThenInclude(p=>p.Game)
                .ThenInclude(g=>g.Genre)
                .ToArray()
                .OrderByDescending(u => u.Cards.Sum(c => c.Purchases.Where(p=>p.Type == (PurchaseType)newPurchaseType).Sum(p => p.Game.Price)))
                .ThenBy(u => u.Username)
                .Select(u => new ExportUsersDTO()
                {
                    username = u.Username,
                    TotalSpent = u.Cards.Sum(c => c.Purchases.Where(p => p.Type == (PurchaseType)newPurchaseType).Sum(p => p.Game.Price)).ToString(),

                    Purchases = u.Cards.SelectMany(c => c.Purchases)
                    .Where(p => p.Type == (PurchaseType)newPurchaseType)
                    .Select(p => new ExportPurchaseDTO
                    {
                        Card = p.Card.Number,
                        Cvc = p.Card.Cvc,
                        Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                        Game = new ExportGameDTO()
                        {
                            Genre = p.Game.Genre.Name,
                            Price = p.Game.Price!=0 ? p.Game.Price.ToString("f2") : Math.Round(p.Game.Price).ToString(),
                            title = p.Game.Name
                        }
                    })
                    .OrderBy(p=>p.Date)
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