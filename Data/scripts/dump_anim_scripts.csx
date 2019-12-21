
//
// Collects all Python script snippets used from within .ska files
//
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.IO;
using System.Collections.Generic;
using System.IO;

Print("Dumping Animation Scripts...");
var meshes = Tig.FS.ReadMesFile("art/meshes/meshes.mes");

var scripts = new HashSet<string>();

foreach (var path in meshes.Values) {
    var skeletonName = "art/meshes/" + path + ".ska";
    byte[] data;
    try {
        data = Tig.FS.ReadBinaryFile(skeletonName);
        
        var skeleton = new SpicyTemple.Core.AAS.Skeleton(data);

        foreach (var anim in skeleton.Animations) {
            foreach (var evt in anim.Events) {
                if (evt.Type == "script") {
                    scripts.Add(evt.Action);
                }
            }
        }
    } catch (Exception e) {
        Print("Failed to read skeleton '" + skeletonName + "'");
        continue;
    }

 }

using (var writer = new StreamWriter("animscripts.txt")) {
    foreach (var script in scripts) {
        writer.WriteLine(script);
    }
}

