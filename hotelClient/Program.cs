using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; // NuGet NewtonSoft.Json

// adapted from http://www.dotnetperls.com/httpclient
// simplified by http://www.jayway.com/2012/03/13/httpclient-makes-get-and-post-very-simple/
namespace hotelClient
{
    class Program
    {
        private const string HotelsUri = "http://hotelapp1.azurewebsites.net/api/hotels/";

        static void Main()
        {
            //IList<Hotel> hotels = GetHotelsAsync().Result;
            //Console.WriteLine(string.Join("\n", hotels));

            Hotel newHotel = new Hotel { Hotel_No = 457, Name = "Anders2", Rating = "3.4", Address = "Hjemme" };
            //Hotel hot = AddHotelAsync(newHotel).Result;
            //Console.WriteLine(hot);

            //Hotel deletedHotel = DeleteHotelAsync(1234).Result;
            //if (deletedHotel == null)
            //{
            //    Console.WriteLine("No such hotel");
            //}
            //else
            //{
            //    Console.WriteLine(deletedHotel);
            //}

            IList<Hotel> hotels = GetHotelsAsync().Result;
            Console.WriteLine(string.Join("\n", hotels));

            newHotel.Name = newHotel.Name + "2";
            bool updated = UpdateHotelAsync(newHotel).Result;
            Console.WriteLine("Updated: " + updated);
        }

        private static async Task<IList<Hotel>> GetHotelsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string content = await client.GetStringAsync(HotelsUri);
                // NuGet NewtonSoft.Json
                IList<Hotel> hotels = JsonConvert.DeserializeObject<IList<Hotel>>(content);
                return hotels;
            }
        }

        private static async Task<Hotel> AddHotelAsync(Hotel hotel)
        {
            string jsonString = JsonConvert.SerializeObject(hotel);
            Console.WriteLine(jsonString);
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.PostAsync(HotelsUri, stringContent);
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new HotelException("Hotel already exists. Try another ID");
                }
                response.EnsureSuccessStatusCode();
                string str = await response.Content.ReadAsStringAsync();
                Hotel hot = JsonConvert.DeserializeObject<Hotel>(str);
                return hot;
            }
        }
        private static async Task<Hotel> AddHotelAsync2(Hotel hotel)
        {
            using (HttpClient client = new HttpClient())
            {
                // NuGet: System.Net.Http.Formatting
                // or https://www.nuget.org/packages/System.Net.Http.Formatting.Extension/ ??
                HttpResponseMessage response = await client.PostAsJsonAsync(HotelsUri, hotel);
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new HotelException("Hotel already exists. Try another ID");
                }
                response.EnsureSuccessStatusCode();
                string str = await response.Content.ReadAsStringAsync();
                Hotel hot = JsonConvert.DeserializeObject<Hotel>(str);
                return hot;
            }
        }

        private static async Task<bool> UpdateHotelAsync(Hotel hotel)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUri = HotelsUri + hotel.Hotel_No;
                HttpResponseMessage response = await client.PutAsJsonAsync(requestUri, hotel);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                return true;
            }
        }

        private static async Task<Hotel> DeleteHotelAsync(int hotelNo)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync(HotelsUri + "/" + hotelNo);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                string str = await response.Content.ReadAsStringAsync();
                Hotel hot = JsonConvert.DeserializeObject<Hotel>(str);
                return hot;
            }
        }
    }

    class HotelException : Exception
    {
        public HotelException(string message) : base(message)
        { }
    }

    public class Hotel
    {
        public int Hotel_No { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string HotelUrl { get; set; }
        public string Rating { get; set; }
        public string[] Room { get; set; }

        public override string ToString()
        {
            return Hotel_No + " " + Name + " " + Address;
        }
    }
}