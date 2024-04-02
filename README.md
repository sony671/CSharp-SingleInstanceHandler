# CSharp-SingleInstanceHandler
C# Application Single Instance
SingleInstanceHandler allows you to easly handle Single instance of your application.
By only few lines of code you can allow only one Instance and if you want to you can pass on start arguments to the first instance of the program.

### Exampel 1
```csharp
using SingleInstance;

public MainWindow()
{

  //if not first instance of the program then exit (using Environment.Exit(0)) and sends start args to the first Instance
  // By adding "true" this instance whill send all start arguments to the first Instance.
  SingleInstanceHandler.LaunchOrExit("MyApplicationName", true);

  // Adds a event lisiner it this instance is the first istanee in order to resive start args.
  SingleInstanceHandler.OnReceiveArgsEvent += OnReceiveArgs;


  // Your Normal code..
  InitializeComponent();
}

private static void OnReceiveArgs(string[] SenderArgs)
{
  Console.WriteLine($"Received args from other instance. Args are '{string.Join(",", SenderArgs)}'");
}
```

### Exampel 2
```csharp
using SingleInstance;

public MainWindow()
{
  // Checks only if this application is the first istance.
  // You can add "true" if you want to send start arguments to the first instance if curent instance are not the first.
  if (SingleInstanceHandler.CheckAndLaunch("MyApplicationName"))
  {
    //This is the first instance
  }

  // Your Normal code..
  InitializeComponent();
}
```
