# AcrylicHelper [![NuGet](https://img.shields.io/nuget/v/PicaPico.AcrylicHelper.svg)](https://nuget.org/packages/PicaPico.AcrylicHelper) [![Build AutoUpdate](https://github.com/HeHang0/AcrylicHelper/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/HeHang0/AcrylicHelper/actions/workflows/library.nuget.yml)

AcrylicHelper is an easy-to-use library for apply acrylic window.

## Usage

-------

AcrylicHelper is available as [NuGet package](https://www.nuget.org/packages/PicaPico.AcrylicHelper).

```csharp
using PicaPico;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AcrylicHelper.Apply(this, DragHelper);
    }
}
```

## Repository

-------

The source code for AcrylicHelper is hosted on GitHub. You can find it at the following URL: [https://github.com/HeHang0/AcrylicHelper](https://github.com/HeHang0/AcrylicHelper)

## License

-------

AcrylicHelper is released under the MIT license. This means you are free to use and modify it, as long as you comply with the terms of the license.
