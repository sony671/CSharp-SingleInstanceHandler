# CSharp-SingleInstanceHandler
SingleInstanceHandler allows you to easily handle Single instances of your application.
By only a few lines of code you can allow only one Instance and if you want to you can pass on start arguments to the first instance of the program.

### Exampel 1
```csharp
using SingleInstance;

public MainWindow()
{

  // If this is not the first instance of the program, then exit (using Environment.Exit(0))
  // By adding "true", this instance will send all startup arguments to the first instance.
  SingleInstanceHandler.LaunchOrExit("MyApplicationName", true);

  // Adds an event listener. If this instance is the first one to call LaunchOrExit or CheckAndLaunch.
  // This event is triggered when other instance are calling LaunchOrExit or CheckAndLaunch
  // and are passing on start arguments.
  SingleInstanceHandler.OnReceiveArgsEvent += OnReceiveArgs;


  // Your Normal code..
  InitializeComponent();
}


// Warning this code is not running on the main thread! UI calls will not working unless invoked.
private static void OnReceiveArgs(string[] SenderArgs)
{
  Console.WriteLine($"Received arguments from another instance. Arguments are '{string.Join(",", SenderArgs)}'");

  // Runs your code on the main thread. 
  Dispatcher.Invoke(() =>
  {
      //Calls your function on the main thread. UI code works inside the Dispatcher
      MyUIFunction(SenderArgs);
  });
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
