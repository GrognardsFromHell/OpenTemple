using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.Materials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Clipping
{
    public class ClippingSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();
        private List<ClippingMesh> _clippingMeshes = new List<ClippingMesh>();

        private readonly RenderingDevice _device;
        private ResourceRef<Material> _material;
        private ResourceRef<Material> _debugMaterial;
        private ResourceRef<BufferBinding> _bufferBinding;

        public bool Debug { get; set; }

        public int Rendered { get; private set; }

        public ClippingSystem(RenderingDevice device)
        {
            _device = device;
            _material = Tig.RenderingDevice.LoadNewMaterial("materials/internal/clipping.json");
            _debugMaterial = Tig.RenderingDevice.LoadNewMaterial("materials/internal/clipping_debug.json");
            _bufferBinding = new BufferBinding(device, _material.Resource.VertexShader).Ref();

            _bufferBinding.Resource.AddBuffer<Vector3>(null, 0)
                .AddElement(VertexElementType.Float3, VertexElementSemantic.Position);
        }

        public void Dispose()
        {
            _clippingMeshes.DisposeAndClear();
            _material.Dispose();
            _debugMaterial.Dispose();
            _bufferBinding.Dispose();
        }


        public void Load(string dataDir)
        {
            Unload();

            LoadMeshes(dataDir);
            LoadObjects(dataDir);
        }

        private void Unload()
        {
            _clippingMeshes.DisposeAndClear();
        }

        public int Total
        {
            get
            {
                var total = 0;
                foreach (var mesh in _clippingMeshes)
                {
                    if (mesh != null)
                    {
                        total += mesh.Instances.Count;
                    }
                }

                return total;
            }
        }


        private void LoadMeshes(string directory)
        {
            var filename = $"{directory}/clipping.cgf";
            using var reader = Tig.FS.OpenBinaryReader(filename);

            var count = reader.ReadInt32();
            _clippingMeshes.DisposeAndClear();
            _clippingMeshes.Capacity = count;

            for (var i = 0; i < count; ++i)
            {
                var meshFilename = reader.ReadFixedString(260);
                if (meshFilename.Length <= 0)
                {
                    Logger.Error("Failed to read filename of clipping mesh #{0} from {1}", i, filename);
                    _clippingMeshes.Add(null);
                    continue;
                }

                try
                {
                    _clippingMeshes.Add(
                        new ClippingMesh(_device, meshFilename)
                    );
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to load clipping mesh {0}: {1}", meshFilename, e);
                    _clippingMeshes.Add(null);
                }
            }
        }

        private void LoadObjects(string directory)
        {
            var indexFileName = $"{directory}/clipping.cif";
            using var reader = Tig.FS.OpenBinaryReader(indexFileName);

            var count = reader.ReadInt32();

            for (var i = 0; i < count; ++i)
            {
                LoadObject(reader);
            }
        }

        private void LoadObject(BinaryReader reader)
        {
            var meshIdx = reader.ReadInt32();

            var mesh = _clippingMeshes[meshIdx];

            var obj = new ClippingMeshObj
            {
                posX = reader.ReadSingle(),
                posY = reader.ReadSingle(),
                posZ = reader.ReadSingle(),
                scaleX = reader.ReadSingle(),
                scaleY = reader.ReadSingle(),
                scaleZ = reader.ReadSingle(),
                rotation = reader.ReadSingle()
            };

            if (mesh == null)
            {
                Logger.Warn("Discarding clipping instance for invalid mesh #{0}", meshIdx);
                return;
            }

            mesh.AddInstance(obj);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ClippingGlobals
        {
            public Matrix4x4 viewProj;
            public Vector4 rotCos;
            public Vector4 rotSin;
            public Vector4 pos;
            public Vector4 scale;
        }

        [TempleDllLocation(0x100A4FB0)]
        public void Render()
        {
            var perfGroup = _device.CreatePerfGroup("Clipping");

            Rendered = 0;

            if (_clippingMeshes.Count == 0)
            {
                return;
            }

            if (Debug)
            {
                _device.SetMaterial(_debugMaterial);
            }
            else
            {
                _device.SetMaterial(_material);
            }

            var camera = _device.GetCamera();

            var globals = new ClippingGlobals();
            globals.viewProj = camera.GetViewProj();

            // For clipping purposes
            var screenCenterWorld = camera.ScreenToWorld(
                camera.GetScreenWidth() * 0.5f,
                camera.GetScreenHeight() * 0.5f
            );

            foreach (var mesh in _clippingMeshes)
            {
                if (mesh == null)
                {
                    continue; // This is a mesh that failed to load
                }

                _bufferBinding.Resource.SetBuffer(0, mesh.VertexBuffer);
                _bufferBinding.Resource.Bind();
                _device.SetIndexBuffer(mesh.IndexBuffer);

                foreach (var obj in mesh.Instances)
                {
                    var sphereCenter = mesh.BoundingSphereOrigin;

                    // Sphere pos relative to computed screen center
                    var relXPos = obj.posX - sphereCenter.X - screenCenterWorld.X;
                    var relZPos = obj.posZ - sphereCenter.Y - screenCenterWorld.Z;

                    // Distance of the sphere center in screen coordinates from the screen center
                    var distX = MathF.Abs(relZPos * 0.70710599f - relXPos * 0.70710599f);
                    var distY = MathF.Abs(relZPos * 0.50497407f + relXPos * 0.50497407f -
                                          (sphereCenter.Z + obj.posY) * 0.7f);

                    var maxScale = Math.Max(Math.Max(obj.scaleX, obj.scaleY), obj.scaleZ);
                    var scaledRadius = maxScale * mesh.BoundingSphereRadius;
                    var culled = (distX > camera.GetScreenWidth() / 2.0f + scaledRadius
                                  || distY > (camera.GetScreenHeight() / 2.0f) + scaledRadius);

                    if (culled)
                    {
                        continue;
                    }

                    Rendered++;

                    globals.rotCos.X = MathF.Cos(obj.rotation);
                    globals.rotSin.X = MathF.Sin(obj.rotation);

                    globals.pos.X = obj.posX;
                    globals.pos.Y = obj.posY;
                    globals.pos.Z = obj.posZ;

                    globals.scale.X = obj.scaleX;
                    globals.scale.Y = obj.scaleY;
                    globals.scale.Z = obj.scaleZ;

                    _device.SetVertexShaderConstants(0, ref globals);

                    _device.DrawIndexed(
                        PrimitiveType.TriangleList,
                        mesh.VertexCount,
                        mesh.TriCount * 3
                    );
                }
            }
        }
    }
}