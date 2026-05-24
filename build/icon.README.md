# Package icon

Drop a `128x128` (or larger) PNG at `build/icon.png`.

[Directory.Build.props](../Directory.Build.props) detects the file on disk and
automatically wires it into every packable project as the NuGet package icon
(`<PackageIcon>icon.png</PackageIcon>`). No further changes needed.

NuGet.org recommends 128x128 transparent PNG, max 1 MB.
