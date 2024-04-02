# CSharp-SingleInstanceHandler
C# Application Single Instance
SingleInstanceHandler allows you to easily handle Single instances of your application.
By only a few lines of code you can allow only one Instance and if you want to you can pass on start arguments to the first instance of the program.

### Exampel 1
```csharp
using SingleInstance;

public MainWindow()
{

  // If this is not the first instance of the program, then exit (using Environment.Exit(0)) and send startup arguments to the first instance.
  // By adding "true", this instance will send all startup arguments to the first instance.
  SingleInstanceHandler.LaunchOrExit("MyApplicationName", true);

  // Adds an event listener if this instance is the first one to call LaunchOrExit or CheckAndLaunch.
  SingleInstanceHandler.OnReceiveArgsEvent += OnReceiveArgs;


  // Your Normal code..
  InitializeComponent();
}

private static void OnReceiveArgs(string[] SenderArgs)
{
  Console.WriteLine($"Received arguments from another instance. Arguments are '{string.Join(",", SenderArgs)}'");
}
```

### Exampel 2
```csharp
using SingleInstance;

public MainWindow()
{
  // Checks if this application is the first instance only.
  // You can add "true" if you want to send startup arguments to the first instance if the current instance is not the first.
  if (SingleInstanceHandler.CheckAndLaunch("MyApplicationName"))
  {
    //This is the first instance
  }

  // Your Normal code..
  InitializeComponent();
}
```
