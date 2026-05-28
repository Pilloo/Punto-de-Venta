using Microsoft.JSInterop;

namespace Client.Services
{
    public class LocalStorageService(IJSRuntime JS) : IAsyncDisposable
    {
        private IJSObjectReference? _module;

        private async ValueTask<IJSObjectReference> GetModuleAsync()
        {
            if (_module is null)
            {
                _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/storage.js");
            }

            return _module;
        }

        public async Task<int> GetLengthAsync()
        {
            IJSObjectReference module = await GetModuleAsync();

            return await module.InvokeAsync<int>("length");
        }

        public async Task<string> GetKeyAsync(int index)
        {
            IJSObjectReference module = await GetModuleAsync();

            return await module.InvokeAsync<string>("key", index);
        }

        public async Task<T?> GetItemAsync<T>(string keyName)
        {
            IJSObjectReference module = await GetModuleAsync();

            return await module.InvokeAsync<T?>("getItem", keyName);
        }

        public async Task SetItemAsync<T>(string keyName, T value)
        {
            IJSObjectReference module = await GetModuleAsync();

            await module.InvokeVoidAsync("setItem", keyName, value);
        }

        public async Task RemoveItemAsync(string keyName)
        {
            IJSObjectReference module = await GetModuleAsync();

            await module.InvokeVoidAsync("removeItem", keyName);
        }

        public async Task ClearAsync()
        {
            IJSObjectReference module = await GetModuleAsync();

            await module.InvokeVoidAsync("clear");
        }

        public async ValueTask DisposeAsync()
        {
            if (_module is not null)
            {
                try
                {
                    await _module.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {

                }
            }
        }
    }
}
