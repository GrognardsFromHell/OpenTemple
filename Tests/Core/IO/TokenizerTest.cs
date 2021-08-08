using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using OpenTemple.Core.IO;
using Xunit;

namespace OpenTemple.Tests.Core.IO
{
    public class TokenizerTest
    {
        [Fact]
        public void CanParseSimpleTexturedMdf()
        {
            var tokenizer = new Tokenizer("Textured\r\nTexture \"folder/filename\"");
            tokenizer.NextToken().Should().BeTrue();
            tokenizer.IsNamedIdentifier("textured").Should().BeTrue();
            tokenizer.NextToken().Should().BeTrue();
            tokenizer.IsNamedIdentifier("texture").Should().BeTrue();
            tokenizer.NextToken().Should().BeTrue();
            tokenizer.IsIdentifier.Should().BeFalse();
            tokenizer.IsQuotedString.Should().BeTrue();
            tokenizer.TokenText.ToString().Should().Be("folder/filename");
        }

        [Fact]
        public void CanParseChainOfNumbers()
        {
            Tokenize("268 0 0 0 0").Should()
                .Equal(268f, 0f, 0f, 0f, 0f);
        }

        [Fact]
        public void UnexpectedPunctuationIsSkipped()
        {
            Tokenize("TAG_PROTECTION_D TAG_RANGER_2, TAG_SORCERER_3").Should()
                .Equal("tag_protection_d", "tag_ranger_2", "UNKNOWN(,)", "tag_sorcerer_3");
        }

        private List<object> Tokenize(string input)
        {
            var result = new List<object>();
            var tokenizer = new Tokenizer(input);
            while (tokenizer.NextToken())
            {
                if (tokenizer.IsNumber)
                {
                    result.Add(tokenizer.TokenFloat);
                }
                else
                {
                    if (tokenizer.IsIdentifier)
                    {
                        result.Add(tokenizer.TokenText.ToString());
                    }
                    else if (tokenizer.IsQuotedString)
                    {
                        result.Add(tokenizer.TokenText.ToString());
                    }
                    else
                    {
                        result.Add("UNKNOWN(" + tokenizer.TokenText.ToString() + ")");
                    }
                }
            }

            return result;
        }
    }
}