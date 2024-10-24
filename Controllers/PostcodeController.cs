using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class PostcodeController : ControllerBase
{
    private static double AirportDistance(PostcodeObj position)
    {
        double R = 6371;
        var lat = toRadian(51.4700223 - position.latitude);
        var lng = toRadian(-0.4542955 - position.longitude);
        var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                    Math.Cos(toRadian(position.latitude)) * Math.Cos(toRadian(51.4700223)) *
                    Math.Sin(lng / 2) * Math.Sin(lng / 2);
        var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
        return R * h2;
    }

    private static double toRadian(double val)
    {
        return (Math.PI / 180) * val;
    }

    private readonly IHttpClientFactory _httpClientFactory;

    public PostcodeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostcodeRequest request)
    {
        if (request.Postcodes == null || request.Postcodes.Count == 0)
        {
            return BadRequest(new { message = "Please provide valid postcodes." });
        }

        var client = _httpClientFactory.CreateClient();
        var apiUrl = "https://api.postcodes.io/postcodes";

        var content = new StringContent(JsonSerializer.Serialize(new { 
            postcodes = request.Postcodes
             }),
            System.Text.Encoding.UTF8, "application/json");
            
        Console.WriteLine(content.ToString());
        Console.WriteLine("conteudo");
        var response = await client.PostAsync(apiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, new { message = "Error fetching data from postcodes.io" });
        }

        var result = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(result);
        foreach (Result postcode in apiResponse.result)
        {
            if(postcode.result != null){
            var dist = AirportDistance(postcode.result);
            postcode.result.airport_distance_km = dist;
            postcode.result.airport_distance_mi = dist / 1.6;

            }
        }
        return Ok(apiResponse.result);
    }
}

public class PostcodeRequest
{
    public List<string> Postcodes { get; set; }
    public List<string> Outcodes {get;set;}
}

public class ApiResponse
{
    public int status {get;set;}
    public List<Result> result {get;set;}
}

public class Result
{
    public string query {get;set;}
    public PostcodeObj result {get;set;}
}

public class PostcodeObj
{
    public float latitude{get;set;}
    public float longitude{get;set;}
    public double airport_distance_km {get;set;}
    public double airport_distance_mi {get;set;}
}