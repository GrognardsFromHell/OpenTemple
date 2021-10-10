### Integration Tests

To run the tests that require an actual game installation, you need to set the
`TOEE_DIR` environment variable to your Temple of Elemental Evil installation
directory before running the tests.

The tests support recording video of the test run, and making screenshots at the 
end of the test. They will be attached to the test report. You need to download
[ffmpeg](https://github.com/GyanD/codexffmpeg/releases/download/4.4/ffmpeg-4.4-full_build-shared.7z) 
and place it in the `ffmpeg` directory at the repository root to enable 
video recording during tests.

### Features

#### Scripts for Debugging

Any `.csx` files you place in `Data/scripts` can be run using the scripts menu of the debug
toolbar at the top of the screen. You can use any C# code within these scripts. Some
example scripts are already included.

#### Startup Script

Place a startup script file in `Data/scripts/startup.csx` to run C# script code immediately
after entering the main menu. This is very effective for quickly testing certain scenarios.

For example, the following script would immediately enter the tutorial after launching
the game:

```csharp
await UiSystems.MainMenu.LaunchTutorial();
```

#### Patching MES files

MES files in the game directory can be patched easily by placing additional
files into a `.d` subdirectory. When the game reads a `<filename>.mes` file, it'll
search for all files in the `<filename>.d` directory next to the file. The files
will be sorted alphabetically and then merged into the original files content
in order.

**Example**

When reading `mes\combat.mes`, the files `mes\combat.d\00-spell_levels.mes` and 
`mes\combat.d\99-more-things.mes` would also be merged into the resulting dictionary
in order. Due to the alphabetical order, keys from `99-more-things.mes` would overwrite
keys from `00-spell_levels.mes`.
