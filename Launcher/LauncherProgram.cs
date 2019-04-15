using System;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO.TroikaArchives;
using System.Windows.Forms;

namespace Launcher
{
    public static class LauncherProgram
    {
        public static void Main(string[] args)
        {
            using var form = new Form {Width = 800, Height = 600};
            form.Show();

            using (var fs = TroikaVfs.CreateFromInstallationDir(@"C:\templeplus\ToEE-Vanilla-Patch2"))
            {
                var meshIndex = fs.ReadMesFile("art/meshes/meshes.mes");

                using (var aasSystem = new AnimatedModelFactory(
                    fs,
                    meshIndex,
                    RunScript,
                    ResolveMaterial
                ))
                {
                    var animParams = new AnimatedModelParams();

                    aasSystem.FromIds(
                        1000,
                        1000,
                        new EncodedAnimId(WeaponAnim.Idle),
                        animParams
                    );
                }
            }
        }

        private static int ResolveMaterial(string materialPath)
        {
            Console.WriteLine("Resolving material " + materialPath);
            return 1;
        }

        private static void RunScript(ReadOnlySpan<char> script)
        {
            throw new NotImplementedException();
        }
    }
}