using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Protos;

namespace OpenTemple.Tests.Core.Systems.Proto
{
    public class EnumIntMappingTest
    {
        [Test]
        public void TestCaseInsensitiveEnumLiteralParsingForByteKeys()
        {
            var mapping = EnumIntMapping.Create(new Dictionary<string, WeaponType>
            {
                {"battleaxe", WeaponType.battleaxe},
                {"Longsword", WeaponType.longsword}
            });

            mapping.TryGetValueIgnoreCase(Encoding.ASCII.GetBytes("longsword"), out var mapped)
                .Should().BeTrue();
            mapped.Should().Be((int) WeaponType.longsword);

            mapping.TryGetValueIgnoreCase(Encoding.ASCII.GetBytes("Longsword"), out mapped)
                .Should().BeTrue();
            mapped.Should().Be((int) WeaponType.longsword);

            mapping.TryGetValueIgnoreCase(Encoding.ASCII.GetBytes("battleaxe"), out mapped)
                .Should().BeTrue();
            mapped.Should().Be((int) WeaponType.battleaxe);
        }
    }
}