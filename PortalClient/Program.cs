using System.Net.Http.Json;
using PortalContracts;

Console.WriteLine("Portal Service Test Client");
Console.WriteLine("==========================");

var baseUrl = Environment.GetEnvironmentVariable("PORTAL_BASE_URL") ?? "https://localhost:5001";

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

// Test Health Check
Console.WriteLine("\n1. Testing Health Check...");
try
{
    var healthResponse = await httpClient.GetAsync("/api/portal/health");
    if (healthResponse.IsSuccessStatusCode)
    {
        var healthData = await healthResponse.Content.ReadFromJsonAsync<dynamic>();
        Console.WriteLine("✅ Health Check Passed");
        Console.WriteLine($"   Status: {healthData?.Status}");
        Console.WriteLine($"   Timestamp: {healthData?.Timestamp}");
    }
    else
    {
        Console.WriteLine($"❌ Health Check Failed: {healthResponse.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Health Check Error: {ex.Message}");
    Console.WriteLine("   Make sure PortalServices is running on https://localhost:5001");
    return;
}

// Test Process Endpoint with different actions
var testRequests = new[]
{
    new PortalRequest("CUST001", "Create"),
    new PortalRequest("CUST002", "Update"),
    new PortalRequest("CUST003", "Delete")
};

Console.WriteLine("\n2. Testing Process Endpoint...");

foreach (var request in testRequests)
{
    Console.WriteLine($"\n   Testing {request.Action} for {request.CustomerId}...");

    try
    {
        var response = await httpClient.PostAsJsonAsync("/api/portal/process", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PortalResponse>();
            Console.WriteLine("   ✅ Process Successful");
            Console.WriteLine($"      Success: {result?.Success}");
            Console.WriteLine($"      Message: {result?.Message}");
            if (result?.Data != null)
            {
                Console.WriteLine($"      Data ID: {result.Data.Id}");
                Console.WriteLine($"      Data Name: {result.Data.Name}");
                Console.WriteLine($"      Data Status: {result.Data.Status}");
            }
            Console.WriteLine($"      Timestamp: {result?.Timestamp}");
        }
        else
        {
            Console.WriteLine($"   ❌ Process Failed: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"      Error: {errorContent}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Process Error: {ex.Message}");
    }
}

Console.WriteLine("\n3. Testing OpenAPI Documentation...");
try
{
    var openApiResponse = await httpClient.GetAsync("/openapi/v1.json");
    if (openApiResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("✅ OpenAPI Documentation Available");
        Console.WriteLine("   Visit: https://localhost:5001/openapi/v1.json");
    }
    else
    {
        Console.WriteLine($"❌ OpenAPI Documentation Not Available: {openApiResponse.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ OpenAPI Documentation Error: {ex.Message}");
}

Console.WriteLine("\n🎉 Testing Complete!");
Console.WriteLine("\nTo run PortalServices:");
Console.WriteLine("   cd PortalServices");
Console.WriteLine("   dotnet run --launch-profile https");
Console.WriteLine("\nTo run this test client:");
Console.WriteLine("   dotnet run --project PortalClient");

