using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using MusicApp.UI;

namespace MusicApp.Test
{
    public static class AvaloniaHelper
    {
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UseHeadless()
                .UseReactiveUI();
    }
}