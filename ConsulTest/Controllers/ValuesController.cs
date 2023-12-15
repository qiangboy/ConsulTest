using Consul;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using RestSharp;

namespace ConsulTest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController(IConsulServerManager consulServerManager, IConsulClient consulClient, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public Task<string> Get()
    {
        return consulServerManager.GetServerAsync("MS.IPCS");
    }

    [HttpGet("GetService")]
    public async Task GetServiceAsync()
    {
        var result = await consulClient.Health.Service("MS.IPCS");
    }

    [HttpGet("GetInfo")]
    public string GetInfo()
    {
        return "请求成功";
    }

    [HttpGet("GetConfiguration")]
    public string GetConfiguration()
    {
        var age = configuration["Data:Age"];

        var redis = configuration["Redis:ConnectionString"];

        return age + "|" + redis;
    }

    [HttpGet("do")]
    public IActionResult Do(string url)
    {
        _ = Do(0, url);

        return Ok();
    }

    private async Task Do(int i, string realUrl)
    {
        var counter = 0;
        var sw2 = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        while (true)
        {
            await Task.Delay(100);
            sw.Restart();

            using RestClient client = new(realUrl);
            RestRequest request = new();
            RestResponse response = await client.ExecuteGetAsync(request);

            sw.Stop();

            var isSuccessful = response.IsSuccessful ? "成功" : "失败";

            Console.WriteLine($"并发请求 {isSuccessful}  {i}  耗时:{sw.ElapsedMilliseconds} ms -------- 已执行{sw2.ElapsedMilliseconds / 1000}秒 超过300ms的次数:{counter}");

            if (sw.ElapsedMilliseconds > 300)
            {
                counter++;
                Console.WriteLine("监测到耗时超过300ms的请求");
                await Task.Delay(3000);
            }
        }

    }
}