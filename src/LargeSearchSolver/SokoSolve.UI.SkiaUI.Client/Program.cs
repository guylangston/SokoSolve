using System;
using Gtk;
using SkiaUI.Core;
using SkiaUI.Gtk;

namespace SkiaUI.Gtk.Example.Host;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.Init();

        Console.WriteLine($"PID: {Environment.ProcessId}");

        var app = new Application("org.gtk.gtk", GLib.ApplicationFlags.None);
        app.Register(GLib.Cancellable.Current);


        SkiaUI.Core.ISkiaApp skiaApp = new SkiaAppExample()
        {
            HostCallback = (x) =>
            {
                if (x != null && x.Equals("Quit"))
                {
                    Application.Quit();
                    return true;
                }
                Console.WriteLine($"Unknown Host Action: {x}");
                return false;
            }
        };

        var win = new MainWindow(skiaApp);
        app.AddWindow(win);

        win.Show();
        Application.Run();
    }
}
