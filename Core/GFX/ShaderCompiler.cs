using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace OpenTemple.Core.GFX
{
    internal class VfsIncludeHandler : Include
    {
        private readonly IFileSystem _fs;

        public VfsIncludeHandler(IFileSystem fs)
        {
            _fs = fs;
        }

        public ShadowContainer Shadow { get; set; }

        public void Dispose()
        {
            Shadow.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            var filename = $"shaders/{fileName}";
            var content = _fs.ReadBinaryFile(filename);
            return new MemoryStream(content);
        }

        public void Close(Stream stream)
        {
            stream.Close();
        }
    }

    internal class ShaderCompiler
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly IFileSystem _fs;

        public ShaderCompiler(IFileSystem fs)
        {
            _fs = fs;
        }

        public Dictionary<string, string> Defines { get; set; }
        public string SourceCode { get; set; }
        public string Name { get; set; }

        public bool DebugMode { get; set; }

        public ResourceRef<VertexShader> CompileVertexShader(RenderingDevice device)
        {
            var code = CompileShaderCode("vs_4_0");
            return new ResourceRef<VertexShader>(new VertexShader(device, Name, code));
        }

        public ResourceRef<PixelShader> CompilePixelShader(RenderingDevice device)
        {
            var code = CompileShaderCode("ps_4_0");
            return new ResourceRef<PixelShader>(new PixelShader(device, Name, code));
        }

        private byte[] CompileShaderCode(string profile)
        {
            using var includeHandler = new VfsIncludeHandler(_fs);

            // Convert the defines
            List<ShaderMacro> macros = new List<ShaderMacro>(Defines.Count + 1);
            foreach (var (key, value) in Defines)
            {
                macros.Add(new ShaderMacro(key, value));
            }

            // Add a D3D11 define if we compile for newer targets
            if (profile == "vs_4_0" || profile == "ps_4_0")
            {
                macros.Add(new ShaderMacro("D3D11", "1"));
            }

            // Debug flags
            var flags = ShaderFlags.SkipOptimization;
            if (DebugMode)
            {
                flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.PreferFlowControl;
            }

            var err = Compiler.Compile(
                SourceCode,
                macros.ToArray(),
                includeHandler,
                "main",
                Name + ".hlsl",
                profile,
                flags,
                EffectFlags.None,
                out var blob,
                out var errorBlob
            );

            try
            {
                if (err.Failure)
                {
                    // Unable to compile the actual shader
                    throw new GfxException($"Unable to compile shader {Name}: {err} {errorBlob.ConvertToString()}");
                }

                if (errorBlob != null)
                {
                    Logger.Warn("Errors/Warnings compiling shader {0}: {1}", Name, errorBlob.ConvertToString());
                }

                return blob.GetBytes();
            }
            finally
            {
                errorBlob?.Dispose();
                blob?.Dispose();
            }
        }
    }
}