using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace CityPlacesApi
{
    class Program
    {
        private const string GeoapifyApiKey = "c91ab8f84d2a42e985f6989c202900bb"; // Замените на свой API-ключ

        static async Task Main(string[] args)
        {
            string cityName = "Berlin"; // Замените на желаемый город

            try
            {
                var places = await GetCityPlacesAsync(cityName);
                if (places.Any())
                {
                    Console.WriteLine($"Самые популярные места в городе {cityName}:");
                    foreach (var place in places)
                    {
                        Console.WriteLine($"Name: {place.Name}");
                        Console.WriteLine($"Categories: {string.Join(", ", place.Categories)}"); // Преобразуем массив категорий в строку
                        Console.WriteLine($"Address: {place.AddressLine2}");
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine($"Не удалось найти популярные места в городе {cityName}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static async Task<Place[]> GetCityPlacesAsync(string cityName)
        {
            using var httpClient = new HttpClient();
            var encodedCityName = Uri.EscapeDataString(cityName);
            var response = await httpClient.GetAsync($"https://api.geoapify.com/v2/places?categories=catering.restaurant&filter=rect:11.549881365729718,48.15114774722076,11.58831616443861,48.12837326392079&limit=10&apiKey={GeoapifyApiKey}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var placeResponse = JsonSerializer.Deserialize<PlaceResponse>(content, options);
            if (placeResponse != null && placeResponse.Features != null)
            {
                // Преобразуем ответ в массив объектов Place
                return placeResponse.Features.Select(f => new Place
                {
                    Name = f.Properties.Name,
                    Categories = f.Properties.Categories, // Десериализуем JSON-массив категорий в массив строк
                    AddressLine2 = f.Properties.address_line2,
                    //Url = f.Properties.datasource
                }).ToArray();
            }
            return new Place[0];
        }
    }

    // Класс, представляющий место
    public class Place
    {
        public string Name { get; set; } // Название места
        public string[] Categories { get; set; } // Категория места
        public string Street { get; set; } // Улица места
        public string City { get; set; } // Город места
        public string AddressLine2 { get; set; } // Дополнительная строка адреса
        public string Url { get; set; } // URL места
    }

    // Класс для десериализации JSON-ответа от API
    public class PlaceResponse
    {
        public Feature[] Features { get; set; }
    }

    // Класс для десериализации свойства Features из JSON-ответа от API
    public class Feature
    {
        public Properties Properties { get; set; }
    }
    // Класс для десериализации свойства Properties из JSON-ответа от API
    public class Properties
    {
        public string Name { get; set; } // Название места
        public string[] Categories { get; set; } // Категория места
        public string address_line2 { get; set; } // Дополнительная строка адреса
    }
}