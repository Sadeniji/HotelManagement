using Microsoft.JSInterop;

namespace HotelManagement.Extensions;

public static class JsRuntimeExtensions
{
    public static async Task AlertAsync(this IJSRuntime jsRuntime, string message) 
        => await jsRuntime.InvokeVoidAsync("windows.alert", message);    

    public static async Task<bool> ConfirmAsync(this IJSRuntime jsRuntime, string message) 
        => await jsRuntime.InvokeAsync<bool>("windows.confirm", message);

    public static async Task<string?> PromptAsync(this IJSRuntime jsRuntime, string message)
        => await jsRuntime.InvokeAsync<string?>("windows.prompt", message);
}