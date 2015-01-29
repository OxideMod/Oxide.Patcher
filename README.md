Oxide Patcher [![Build Status](https://travis-ci.org/OxideMod/OxidePatcher.png)](https://travis-ci.org/OxideMod/OxidePatcher)
===============

Oxide 2 is a complete rewrite of the popular, original Oxide mod for the game Rust. Oxide 2 has a focus on modularity and extensibility. The core is highly abstracted and loosely coupled, and could be used to mod any game that uses .NET such as 7 Days to Die, The Forest, Space Engineers, and more. The patcher is only needed for those who want to experiment with adding new hooks or modding other games.

Patching Games
--------------

 1. Navigate to your installation of Oxide and locate Oxide.Core.dll. Copy it and paste it next to the freshly compiled OxidePatcher.exe.
 2. Navigate to .opj for the target game and open it in a plain text editor. It is formatted as a JSON file.
 3. Find the "TargetDirectory" field in the json text and change the value to be the "Managed" folder of your target game server installation.
 4. Launch the patcher. Go to File -> Open Project, and open the .opj.
 5. If all goes well, the hooks and assembly list should appear on the tree view to the left.
 6. To patch, click the wand icon on the toolbar.
 7. To add a hook, navigate to the desired method from the desired assembly and click the "Hook this Method" button.
 8. To include more assemblies, right click on any red-cross assembly and select "Add to Project".

Notes
-----

 * You should work on a vanilla version of the target game.
 * The patcher will make copies of the original DLLs and append "_Original" to them, and it will use them as the input when patching. This means it's safe to make a few changes and patch over and over again.
