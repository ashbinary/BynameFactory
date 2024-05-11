# BynameFactory
A Splashtag Title Order auto-analyzer that lets dataminers find data the developers left in the game through Splashtags. Sounds absolutely insane, I know.

# Usage
## Populating the Asset Folder
The tool will not function without the proper game files added to the Asset folder. The folder's structure is shown in the table below.

├── Asset
│   ├── Data
|   |   ├── Version (ex. "1.0.0")
|   |   |   ├── Mals (language files, taken from the game - requires file of language wanted to parse)
|   |   |   ├── Pack (parameter files, taken from the game - requires the Bootup.Nin_NX_NVN.pack.zs file)
└── ─── Export (will be created automatically, where the byname output files are stored)

The proper files have to be placed in the Mals and Pack folders accordingly, and the Asset folder has to be placed in the same area as Program.cs and the .csproj to function correctly, or the program will error.

## Using the Program

Currently, there is no way to use it as a command line tool. To modify the output, the user has to go into Program.cs and find the last line in the `public static void Main` method.
The subject, language, and gender are all Enums which can be changed to any of the valid inputs. Gender also has a "null" value, which is used when there is no gendered Splashtag Titles within said file.
The version number is a string, and should mirror the version placed in the Data folder. Version mismatching will likely create a corrupted output or cause issues.

For example, finding the Splashtag Title Order for the adjectives in Version 1.2.0 in American English would be:
```
PrintBynameText("1.2.0", OrderKind.Adjective, Language.USen, null);
```

# Credits & Special Thanks
[Fushigi](https://github.com/shibbo/Fushigi) for the modified Byml and SARC libraries used in the tool.
[NinLib.MessageStudio](https://github.com/OatmealDome/OatmealDome.NinLib.MessageStudio) for the MSBT library.

💖 Diam for teaching me what Bynames originally were oh-so long ago.
💖 Shadów for answering all of my questions when I was running into brick walls.
