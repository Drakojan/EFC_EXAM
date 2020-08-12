namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.EntityFrameworkCore.Internal;
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			//TODO: don't forget Each game must have at least one tag ON IMPORT
			var sb = new StringBuilder();

			var gamesRAW = JsonConvert.DeserializeObject<importGameDTO[]>(jsonString, new JsonSerializerSettings());

			var gamesFiltered = new List<Game>();
			var developers = new List<Developer>();
			var genres = new List<Genre>();
			var tags = new List<Tag>();

            foreach (var game in gamesRAW)
            {
                if (!IsValid(game))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				DateTime ReleaseDate;

				bool isDateValid = DateTime.TryParseExact(game.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out ReleaseDate);

				if (!isDateValid)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

                if (!game.Tags.Any())
                {
					sb.AppendLine("Invalid Data");
					continue;
				}

				if (!developers.Any(d=>d.Name== game.Developer))
                {
					var newDeveloper = new Developer(){ Name = game.Developer};
					developers.Add(newDeveloper);
                }


				if (!genres.Any(g => g.Name == game.Genre))
				{
					var newGenre = new Genre() { Name = game.Genre };
					genres.Add(newGenre);
				}

				var newGame = new Game()
				{
					Name = game.Name,
					Price = game.Price,
					ReleaseDate = ReleaseDate,
					Developer = developers.Where(d => d.Name == game.Developer).Single(),
					Genre = genres.Where(g => g.Name == game.Genre).Single()
				};

				foreach (var tag in game.Tags)
                {
                    if (!tags.Any(t=>t.Name == tag))
                    {
						var newTag = new Tag() { Name = tag };
						tags.Add(newTag);
					}

					newGame.GameTags.Add(new GameTag() 
					{ Tag = tags
						.Where(t => t.Name == tag)
						.Single() 
					});
                }

				gamesFiltered.Add(newGame);

				sb.AppendLine($"Added {newGame.Name} ({newGame.Genre.Name}) with {newGame.GameTags.Count()} tags"!);
			}
			
			context.Games.AddRange(gamesFiltered);
			context.SaveChanges();
			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			//TODO: not 100% sure if email prop of User should be validated as email

			var sb = new StringBuilder();

			var usersRAW = JsonConvert.DeserializeObject<importUsersDTO[]>(jsonString, new JsonSerializerSettings
			{ NullValueHandling = NullValueHandling.Ignore });

			var usersFiltered = new List<User>();
			var cards = new List<Card>();
			
            foreach (var user in usersRAW)
            {
                if (!IsValid(user))
                {
					sb.AppendLine("Invalid Data");
					continue;
				}

				var thisUserCards = new List<Card>();

                foreach (var card in user.Cards)
                {
					if (!IsValid(card))
					{
						sb.AppendLine("Invalid Data");
						continue;
					}

					object newCardType;

					bool isTypeValid = Enum.TryParse(typeof(CardType), card.Type, out newCardType);

					if (!isTypeValid)
					{
						sb.AppendLine("Invalid Data");
						continue;
					}

					var newCard = new Card()
					{
						Number = card.Number,
						Cvc = card.Cvc,
						Type = (CardType)newCardType
					};

					thisUserCards.Add(newCard);
					cards.Add(newCard);
				}

				var newUser = new User()
				{
					Age = user.Age,
					Email = user.Email,
					FullName = user.FullName,
					Username = user.Username,
					Cards = thisUserCards
				};

				usersFiltered.Add(newUser);
				sb.AppendLine($"Imported {newUser.Username} with {newUser.Cards.Count()} cards");
            }
            context.Users.AddRange(usersFiltered);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{

			var sb = new StringBuilder();
			var purchasesFiltered = new List<Purchase>();

			var validCards = context.Cards.Select(c => new { c.Number, c.Id});
			var validGames = context.Games.Select(g => new { g.Name, g.Id});

			var usersWithCards = context.Users
				.Select(u => new 
				{ 
					u.Username, 
					cardNumbers = u.Cards
						.Select(c => new 
						{
							c.Number 
						}) 
				});;

			XmlRootAttribute root = new XmlRootAttribute("Purchases");

			var serializer = new XmlSerializer(typeof(importPurchasesDTO[]), root);

			using (var stringReader = new StringReader(xmlString))
			{
				importPurchasesDTO[] purchasesRAW = (importPurchasesDTO[])serializer.Deserialize(stringReader);

                foreach (var purchase in purchasesRAW)
                {
					if (!IsValid(purchase))
					{
						sb.AppendLine("Invalid Data");
						continue;
					}

					DateTime purchaseDate;

					bool isDateValid = DateTime.TryParseExact(purchase.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out purchaseDate);

					if (!isDateValid)
					{
						sb.AppendLine("Invalid Data");
						continue;
					}

                    if (!validCards.Any(x=>x.Number==purchase.CardNumber))
                    {
						sb.AppendLine("Invalid Data");
						continue;
					}

                    if (!validGames.Any(x=>x.Name==purchase.GameName))
                    {
						sb.AppendLine("Invalid Data");
						continue;
					}

					object newPurchaseType;

					bool isTypeValid = Enum.TryParse(typeof(PurchaseType), purchase.Type, out newPurchaseType);

					if (!isTypeValid)
					{
						sb.AppendLine("Invalid Data");
						continue;
					}

					var newPurchase = new Purchase()
					{
						CardId = validCards.Where(c => c.Number == purchase.CardNumber).Single().Id,
						GameId = validGames.Where(x => x.Name == purchase.GameName).Single().Id,
						Date = purchaseDate,
						ProductKey = purchase.ProductKey,
						Type = (PurchaseType)newPurchaseType
					};

					purchasesFiltered.Add(newPurchase);

					var user = usersWithCards.Where(u => u.cardNumbers.Any(x => x.Number == purchase.CardNumber)).Single().Username;

					sb.AppendLine($"Imported {purchase.GameName} for {user}");
				}
			}

			context.Purchases.AddRange(purchasesFiltered);
			context.SaveChanges();

			return sb.ToString().TrimEnd();
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}