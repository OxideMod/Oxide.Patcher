Oxide 2 Patcher [![Build Status](https://travis-ci.org/RustOxide/Oxide-2-Patcher.png)](https://travis-ci.org/RustOxide/Oxide-2-Patcher)
===============

Oxide 2 is a complete rewrite of the original popular Oxide mod for the game Rust.
Oxide 2 has a focus on modularity and extensibility.
The patcher is responsible for hooking Oxide into the target game's binaries, allowing the mod to work.
The patcher will work on any game made using .Net.
Oxide 2 will be bundled with patched DLLs for Rust already, so this is only needed for those who want to experiment with adding new hooks or modding other games.

Usage for Rust Server Users
----------------------------------

 1. Clone the git repository locally.
 2. Open the solution in Visual Studio (2013 is recommended, but it should work on earlier versions).
 3. Build the project.
 4. Navigate to your installation of Oxide 2 and locate Oxide.Core.dll. Copy it and paste it next to the freshly compiled OxidePatcher.exe.
 5. Navigate to RustExperimental.opj and open it in a plain text editor. It is formatted as a json file.
 6. Find the "TargetDirectory" field in the json text and change the value to be the "RustDedicated_Data/Managed" folder of your Rust server installation.
 7. Launch the patcher. Go to File -> Open Project, and open RustExperimental.opj.
 8. If all goes well, the hooks and assembly list should appear on the tree view to the left.
 9. To patch, click the wand icon on the toolbar.
 10. To add a hook, navigate to the desired method from the desired assembly and click the "Hook this Method" button.
 11. To include more assemblies, right click on any red-cross assembly and select "Add to Project".

Notes
-----

You should work on a vanilla version of the target game.
It's probably worth installing the rust server in a different directory to your working one and pointing your patcher at that instead.
If you run the patcher on already patched DLLs, Oxide is going to try and load twice and bad things will happen.
The patcher will make copies of the original DLLs and append "_Original" to them, and it will use them as the input when patching.
This means it's safe to make a few changes and patch over and over again.
